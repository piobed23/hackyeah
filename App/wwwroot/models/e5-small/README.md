# Model embeddingów — multilingual-e5-small

Katalog oczekuje dwóch plików z HuggingFace (razem ~470 MB):

1. **`model.onnx`** — wagi modelu w formacie ONNX
2. **`sentencepiece.bpe.model`** — tokenizer SentencePiece

## Jak pobrać

### Wariant A: HuggingFace Hub (ręcznie)

Źródło: https://huggingface.co/intfloat/multilingual-e5-small

Pobierz dwa pliki do tego katalogu:
- https://huggingface.co/intfloat/multilingual-e5-small/resolve/main/onnx/model.onnx  → zapisz jako `model.onnx`
- https://huggingface.co/intfloat/multilingual-e5-small/resolve/main/sentencepiece.bpe.model  → zapisz jako `sentencepiece.bpe.model`

### Wariant B: `huggingface-cli` (jeśli masz zainstalowane)

```bash
cd App/wwwroot/models/e5-small
huggingface-cli download intfloat/multilingual-e5-small onnx/model.onnx --local-dir .
mv onnx/model.onnx .
huggingface-cli download intfloat/multilingual-e5-small sentencepiece.bpe.model --local-dir .
```

### Wariant C: PowerShell (Windows)

```powershell
cd App\wwwroot\models\e5-small
Invoke-WebRequest -Uri "https://huggingface.co/intfloat/multilingual-e5-small/resolve/main/onnx/model.onnx" -OutFile "model.onnx"
Invoke-WebRequest -Uri "https://huggingface.co/intfloat/multilingual-e5-small/resolve/main/sentencepiece.bpe.model" -OutFile "sentencepiece.bpe.model"
```

## Weryfikacja

Po uruchomieniu aplikacji w logach powinno pojawić się:
```
info: App.Services.Ai.OnnxE5EmbeddingService[0]
      Załadowano model embeddingów e5-small z <ścieżka>.
```

Jeśli zamiast tego widzisz warning `Model embeddingów e5-small nie został znaleziony` — aplikacja działa, ale matchmaking używa prostego fallbacku tokenowego (gorsze dopasowania).

## Licencja

Model `intfloat/multilingual-e5-small` jest wydany na licencji **MIT** (Microsoft Corporation).
Model jest zbyt duży, aby trzymać go w repo — każdy developer / instancja demo pobiera go lokalnie.
