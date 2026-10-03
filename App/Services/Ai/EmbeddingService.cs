using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace App.Services.Ai;

public interface IEmbeddingService
{
    bool IsAvailable { get; }
    int Dimension { get; }
    Task<float[]?> EmbedAsync(string text, bool isQuery = false, CancellationToken ct = default);
}

public class OnnxE5EmbeddingService : IEmbeddingService, IDisposable
{
    private const int EmbeddingDim = 384;
    private const int MaxSeqLength = 256;

    private readonly ILogger<OnnxE5EmbeddingService> _log;
    private readonly InferenceSession? _session;
    private readonly Tokenizer? _tokenizer;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly string _modelsDir;

    public bool IsAvailable => _session is not null && _tokenizer is not null;
    public int Dimension => EmbeddingDim;

    public OnnxE5EmbeddingService(IHostEnvironment env, ILogger<OnnxE5EmbeddingService> log)
    {
        _log = log;
        _modelsDir = Path.Combine(env.ContentRootPath, "wwwroot", "models", "e5-small");

        var modelPath = Path.Combine(_modelsDir, "model.onnx");
        var tokenizerPath = Path.Combine(_modelsDir, "sentencepiece.bpe.model");

        if (!File.Exists(modelPath) || !File.Exists(tokenizerPath))
        {
            _log.LogWarning(
                "Model embeddingów e5-small nie został znaleziony w {Dir}. " +
                "Platforma będzie używać fallbacku tokenowego dla matchmakingu. " +
                "Instrukcja pobrania modelu znajduje się w {Dir}/README.md.",
                _modelsDir, _modelsDir);
            return;
        }

        try
        {
            var options = new Microsoft.ML.OnnxRuntime.SessionOptions
            {
                IntraOpNumThreads = Math.Min(Environment.ProcessorCount, 4)
            };
            _session = new InferenceSession(modelPath, options);
            using var fs = File.OpenRead(tokenizerPath);
            // LlamaTokenizer używa tego samego formatu SentencePiece BPE co XLM-RoBERTa (baza e5-small).
            // Specjalne tokeny XLM-R: <s>=0, <pad>=1, </s>=2, <unk>=3 — tylko <unk> musi być zmapowane,
            // bo <s>/</s> dodajemy ręcznie wokół sekwencji w metodzie Infer.
            var specialTokens = new Dictionary<string, int> { ["<unk>"] = 3 };
            _tokenizer = LlamaTokenizer.Create(fs, addBeginOfSentence: false, addEndOfSentence: false, specialTokens: specialTokens);
            _log.LogInformation("Załadowano model embeddingów e5-small z {Dir}.", _modelsDir);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Nie udało się załadować modelu embeddingów. Fallback tokenowy.");
            _session?.Dispose();
            _session = null;
            _tokenizer = null;
        }
    }

    public async Task<float[]?> EmbedAsync(string text, bool isQuery = false, CancellationToken ct = default)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(text)) return null;

        var prefixed = (isQuery ? "query: " : "passage: ") + text;

        await _lock.WaitAsync(ct);
        try
        {
            return Infer(prefixed);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Błąd inferencji embeddingu dla tekstu długości {Len}.", text.Length);
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private float[] Infer(string text)
    {
        var ids = _tokenizer!.EncodeToIds(text);
        var tokenIds = new List<long> { 0 };
        tokenIds.AddRange(ids.Take(MaxSeqLength - 2).Select(i => (long)i));
        tokenIds.Add(2);

        int seqLen = tokenIds.Count;
        var inputIds = new DenseTensor<long>(new[] { 1, seqLen });
        var attentionMask = new DenseTensor<long>(new[] { 1, seqLen });

        for (int i = 0; i < seqLen; i++)
        {
            inputIds[0, i] = tokenIds[i];
            attentionMask[0, i] = 1;
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
        };

        using var results = _session!.Run(inputs);
        var lastHidden = results.First().AsTensor<float>();

        var pooled = new float[EmbeddingDim];
        for (int i = 0; i < seqLen; i++)
            for (int d = 0; d < EmbeddingDim; d++)
                pooled[d] += lastHidden[0, i, d];
        for (int d = 0; d < EmbeddingDim; d++)
            pooled[d] /= seqLen;

        double norm = 0;
        for (int d = 0; d < EmbeddingDim; d++) norm += pooled[d] * pooled[d];
        norm = Math.Sqrt(norm);
        if (norm > 1e-9)
            for (int d = 0; d < EmbeddingDim; d++)
                pooled[d] = (float)(pooled[d] / norm);

        return pooled;
    }

    public static float Cosine(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0;
        double sum = 0;
        for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
        return (float)sum;
    }

    public void Dispose()
    {
        _session?.Dispose();
        _lock.Dispose();
    }
}
