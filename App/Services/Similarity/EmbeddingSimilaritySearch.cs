using System.Collections.Concurrent;
using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Models;
using App.Services.Ai;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Similarity;

public class EmbeddingSimilaritySearch : ISimilaritySearch
{
    private readonly AppDbContext _db;
    private readonly IEmbeddingService _embed;
    private readonly EmbeddingCache _cache;
    private readonly InMemorySimilaritySearch _fallback;

    public EmbeddingSimilaritySearch(
        AppDbContext db,
        IEmbeddingService embed,
        EmbeddingCache cache,
        InMemorySimilaritySearch fallback)
    {
        _db = db;
        _embed = embed;
        _cache = cache;
        _fallback = fallback;
    }

    public async Task<IReadOnlyList<SimilarityResult>> ZnajdzPodobneAsync(IdeaCard karta, int topN = 5)
    {
        if (!_embed.IsAvailable) return await _fallback.ZnajdzPodobneAsync(karta, topN);

        var zapytanie = BudujTekstPomyslu(karta);
        var vec = await _embed.EmbedAsync(zapytanie, isQuery: true);
        if (vec is null) return await _fallback.ZnajdzPodobneAsync(karta, topN);

        var kandydaci = await _db.Ideas
            .Include(i => i.Karta)
            .Where(i => i.Karta != null
                && i.Id != karta.IdeaId
                && (i.Karta.Status == StatusFiszki.Opublikowany
                    || i.Karta.Status == StatusFiszki.Zaakceptowany))
            .ToListAsync();

        var wyniki = new List<SimilarityResult>();
        foreach (var i in kandydaci)
        {
            var k = i.Karta!;
            var kandVec = await _cache.PobierzLubWygenerujIdeaAsync(i.Id, BudujTekstPomyslu(k), _embed);
            if (kandVec is null) continue;

            var sim = OnnxE5EmbeddingService.Cosine(vec, kandVec);
            if (sim < 0.65f) continue;

            var kategoria = i.EtapInnowacji == EtapInnowacji.SprawdzonaInnowacja
                ? KategoriaPodobienstwa.SprawdzonaInnowacja
                : KategoriaPodobienstwa.PodobnyPomysl;

            var wspolneTagi = TagiJakoZbior(karta.Tagi).Intersect(TagiJakoZbior(k.Tagi)).ToList();
            wyniki.Add(new SimilarityResult(i.Id, k.Tytul, k.Streszczenie, sim, kategoria, i.Rodzaj, i.EtapInnowacji, wspolneTagi));
        }

        var problemy = await _db.ProblemReports.ToListAsync();
        foreach (var p in problemy)
        {
            var pVec = await _cache.PobierzLubWygenerujProblemAsync(p.Id, BudujTekstProblemu(p), _embed);
            if (pVec is null) continue;
            var sim = OnnxE5EmbeddingService.Cosine(vec, pVec);
            if (sim < 0.65f) continue;

            wyniki.Add(new SimilarityResult(
                -p.Id, p.Tytul, p.Opis, sim,
                KategoriaPodobienstwa.TenSamProblem,
                RodzajPomyslu.Innowacja, EtapInnowacji.Pomysl,
                TagiJakoZbior(karta.Tagi).Intersect(TagiJakoZbior(p.Tagi)).ToList()));
        }

        return wyniki.OrderByDescending(w => w.Wynik).Take(topN).ToList();
    }

    public async Task<IReadOnlyList<SimilarityResult>> ZnajdzDlaProblemuAsync(ProblemReport problem, int topN = 5)
    {
        if (!_embed.IsAvailable) return await _fallback.ZnajdzDlaProblemuAsync(problem, topN);

        var vec = await _embed.EmbedAsync(BudujTekstProblemu(problem), isQuery: true);
        if (vec is null) return await _fallback.ZnajdzDlaProblemuAsync(problem, topN);

        var innowacje = await _db.Ideas
            .Include(i => i.Karta)
            .Where(i => i.Karta != null
                && (i.Karta.Status == StatusFiszki.Opublikowany
                    || i.Karta.Status == StatusFiszki.Zaakceptowany))
            .ToListAsync();

        var wyniki = new List<SimilarityResult>();
        foreach (var i in innowacje)
        {
            var kandVec = await _cache.PobierzLubWygenerujIdeaAsync(i.Id, BudujTekstPomyslu(i.Karta!), _embed);
            if (kandVec is null) continue;

            var sim = OnnxE5EmbeddingService.Cosine(vec, kandVec);
            if (sim < 0.62f) continue;

            var kategoria = i.EtapInnowacji switch
            {
                EtapInnowacji.SprawdzonaInnowacja => KategoriaPodobienstwa.SprawdzonaInnowacja,
                _ => KategoriaPodobienstwa.PodobnyPomysl
            };

            wyniki.Add(new SimilarityResult(
                i.Id, i.Karta!.Tytul, i.Karta.Streszczenie, sim, kategoria,
                i.Rodzaj, i.EtapInnowacji,
                TagiJakoZbior(problem.Tagi).Intersect(TagiJakoZbior(i.Karta.Tagi)).ToList()));
        }

        return wyniki.OrderByDescending(w => w.Wynik).Take(topN).ToList();
    }

    public async Task<IReadOnlyList<ProblemReport>> ZnajdzPodobneProblemyAsync(ProblemReport problem, int topN = 5)
    {
        if (!_embed.IsAvailable) return await _fallback.ZnajdzPodobneProblemyAsync(problem, topN);

        var vec = await _embed.EmbedAsync(BudujTekstProblemu(problem), isQuery: true);
        if (vec is null) return await _fallback.ZnajdzPodobneProblemyAsync(problem, topN);

        var inne = await _db.ProblemReports.Where(p => p.Id != problem.Id).ToListAsync();
        var wyniki = new List<(ProblemReport p, float sim)>();
        foreach (var p in inne)
        {
            var pVec = await _cache.PobierzLubWygenerujProblemAsync(p.Id, BudujTekstProblemu(p), _embed);
            if (pVec is null) continue;
            var sim = OnnxE5EmbeddingService.Cosine(vec, pVec);
            if (sim >= 0.70f) wyniki.Add((p, sim));
        }
        return wyniki.OrderByDescending(x => x.sim).Take(topN).Select(x => x.p).ToList();
    }

    private static string BudujTekstPomyslu(IdeaCard k) =>
        $"{k.Tytul}. {k.Streszczenie} {k.Problem} {k.Rozwiazanie} {k.GrupaDocelowa}".Trim();

    private static string BudujTekstProblemu(ProblemReport p) =>
        $"{p.Tytul}. {p.Opis} {p.KogoDotyczy} {p.OczekiwanyEfekt}".Trim();

    private static HashSet<string> TagiJakoZbior(string? tagi) =>
        new((tagi ?? "").ToLowerInvariant()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}

public class EmbeddingCache
{
    private readonly ConcurrentDictionary<string, float[]> _store = new();
    private readonly ConcurrentDictionary<string, string> _textHash = new();

    public async Task<float[]?> PobierzLubWygenerujIdeaAsync(int ideaId, string tekst, IEmbeddingService embed)
        => await WezAsync($"idea:{ideaId}", tekst, embed, isQuery: false);

    public async Task<float[]?> PobierzLubWygenerujProblemAsync(int problemId, string tekst, IEmbeddingService embed)
        => await WezAsync($"problem:{problemId}", tekst, embed, isQuery: false);

    private async Task<float[]?> WezAsync(string klucz, string tekst, IEmbeddingService embed, bool isQuery)
    {
        var hash = tekst.GetHashCode().ToString();
        if (_store.TryGetValue(klucz, out var zapisany) &&
            _textHash.TryGetValue(klucz, out var zapisanyHash) &&
            zapisanyHash == hash)
            return zapisany;

        var vec = await embed.EmbedAsync(tekst, isQuery);
        if (vec is null) return null;

        _store[klucz] = vec;
        _textHash[klucz] = hash;
        return vec;
    }

    public void Invalidate(string klucz) { _store.TryRemove(klucz, out _); _textHash.TryRemove(klucz, out _); }
}
