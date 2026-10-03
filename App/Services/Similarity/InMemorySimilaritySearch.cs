using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Similarity;

public class InMemorySimilaritySearch : ISimilaritySearch
{
    private readonly AppDbContext _db;
    public InMemorySimilaritySearch(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<SimilarityResult>> ZnajdzPodobneAsync(IdeaCard karta, int topN = 5)
    {
        var tekstKandydat = Normalizuj($"{karta.Tytul} {karta.Streszczenie} {karta.Problem} {karta.Rozwiazanie} {karta.GrupaDocelowa}");
        var tokenyKandydat = Tokenizuj(tekstKandydat);
        var tagiKandydat = RozbijTagi(karta.Tagi);

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
            var tekstKand = Normalizuj($"{k.Tytul} {k.Streszczenie} {k.Problem} {k.Rozwiazanie} {k.GrupaDocelowa}");
            var tokenyKand = Tokenizuj(tekstKand);
            var tagiKand = RozbijTagi(k.Tagi);

            var fts = PodobienstwoTokenow(tokenyKandydat, tokenyKand);
            var tagi = Jaccard(tagiKandydat, tagiKand);
            var rodzaj = i.Rodzaj == (karta.Idea?.Rodzaj ?? default) ? 1.0 : 0.0;

            var wynik = 0.60 * fts + 0.30 * tagi + 0.10 * rodzaj;
            if (wynik <= 0) continue;

            var kategoria = i.EtapInnowacji == EtapInnowacji.SprawdzonaInnowacja
                ? KategoriaPodobienstwa.SprawdzonaInnowacja
                : KategoriaPodobienstwa.PodobnyPomysl;

            var wspolne = tagiKandydat.Intersect(tagiKand).ToList();

            wyniki.Add(new SimilarityResult(i.Id, k.Tytul, k.Streszczenie, wynik, kategoria, i.Rodzaj, i.EtapInnowacji, wspolne));
        }

        var problemy = await _db.ProblemReports.ToListAsync();
        foreach (var p in problemy)
        {
            var tekstP = Normalizuj($"{p.Tytul} {p.Opis}");
            var tokenyP = Tokenizuj(tekstP);
            var tagiP = RozbijTagi(p.Tagi);

            var fts = PodobienstwoTokenow(tokenyKandydat, tokenyP);
            var tagi = Jaccard(tagiKandydat, tagiP);
            var wynik = 0.70 * fts + 0.30 * tagi;
            if (wynik < 0.1) continue;

            wyniki.Add(new SimilarityResult(
                -p.Id, p.Tytul, p.Opis, wynik,
                KategoriaPodobienstwa.TenSamProblem,
                RodzajPomyslu.Innowacja, EtapInnowacji.Pomysl,
                tagiKandydat.Intersect(tagiP).ToList()));
        }

        return wyniki.OrderByDescending(w => w.Wynik).Take(topN).ToList();
    }

    public async Task<IReadOnlyList<SimilarityResult>> ZnajdzDlaProblemuAsync(ProblemReport problem, int topN = 5)
    {
        var tekstKand = Normalizuj($"{problem.Tytul} {problem.Opis} {problem.KogoDotyczy} {problem.OczekiwanyEfekt}");
        var tokenyKand = Tokenizuj(tekstKand);
        var tagiKand = RozbijTagi(problem.Tagi);

        var innowacje = await _db.Ideas
            .Include(i => i.Karta)
            .Where(i => i.Karta != null
                && (i.Karta.Status == StatusFiszki.Opublikowany
                    || i.Karta.Status == StatusFiszki.Zaakceptowany))
            .ToListAsync();

        var wyniki = new List<SimilarityResult>();
        foreach (var i in innowacje)
        {
            var k = i.Karta!;
            var tekstInnowacji = Normalizuj($"{k.Tytul} {k.Streszczenie} {k.Problem} {k.Rozwiazanie} {k.GrupaDocelowa}");
            var tokenyInnowacji = Tokenizuj(tekstInnowacji);
            var tagiInnowacji = RozbijTagi(k.Tagi);

            var fts = PodobienstwoTokenow(tokenyKand, tokenyInnowacji);
            var tagi = Jaccard(tagiKand, tagiInnowacji);
            var wynik = 0.70 * fts + 0.30 * tagi;
            if (wynik < 0.05) continue;

            var kategoria = i.EtapInnowacji switch
            {
                EtapInnowacji.SprawdzonaInnowacja => KategoriaPodobienstwa.SprawdzonaInnowacja,
                EtapInnowacji.WRealizacji or EtapInnowacji.WTestach or EtapInnowacji.PoszukujeTesterow
                    or EtapInnowacji.WPrzygotowaniu or EtapInnowacji.FinansowaniePrzyznane
                    or EtapInnowacji.TestyZakonczone => KategoriaPodobienstwa.PodobnyPomysl,
                _ => KategoriaPodobienstwa.PodobnyPomysl
            };

            wyniki.Add(new SimilarityResult(i.Id, k.Tytul, k.Streszczenie, wynik, kategoria, i.Rodzaj, i.EtapInnowacji, tagiKand.Intersect(tagiInnowacji).ToList()));
        }

        return wyniki.OrderByDescending(w => w.Wynik).Take(topN).ToList();
    }

    public async Task<IReadOnlyList<ProblemReport>> ZnajdzPodobneProblemyAsync(ProblemReport problem, int topN = 5)
    {
        var tekstKand = Normalizuj($"{problem.Tytul} {problem.Opis}");
        var tokenyKand = Tokenizuj(tekstKand);
        var tagiKand = RozbijTagi(problem.Tagi);

        var inne = await _db.ProblemReports.Where(p => p.Id != problem.Id).ToListAsync();
        var wyniki = new List<(ProblemReport p, double w)>();
        foreach (var p in inne)
        {
            var tekstP = Normalizuj($"{p.Tytul} {p.Opis}");
            var fts = PodobienstwoTokenow(tokenyKand, Tokenizuj(tekstP));
            var tagi = Jaccard(tagiKand, RozbijTagi(p.Tagi));
            var w = 0.70 * fts + 0.30 * tagi;
            if (w > 0.1) wyniki.Add((p, w));
        }
        return wyniki.OrderByDescending(x => x.w).Take(topN).Select(x => x.p).ToList();
    }

    private static string Normalizuj(string? s)
    {
        s = (s ?? "").ToLowerInvariant();
        return new string(s.Select(ch => char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch) ? ch : ' ').ToArray());
    }

    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "i","oraz","lub","albo","że","aby","dla","do","na","od","po","we","w","z","ze","się","nie","to","jest","są",
        "a","o","u","by","czy","jak","ale","tak","też","już","oni","ona","oni","być","gdyż","który","która","które"
    };

    private static HashSet<string> Tokenizuj(string s) =>
        new(s.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
             .Where(t => t.Length >= 3 && !Stopwords.Contains(t)));

    private static HashSet<string> RozbijTagi(string? tagi)
    {
        if (string.IsNullOrWhiteSpace(tagi)) return new();
        return new(tagi.ToLowerInvariant()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static double PodobienstwoTokenow(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 || b.Count == 0) return 0;
        var czesc = a.Intersect(b).Count();
        return (double)czesc / Math.Max(a.Count, b.Count);
    }

    private static double Jaccard(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 0;
        var unia = a.Union(b).Count();
        return unia == 0 ? 0 : (double)a.Intersect(b).Count() / unia;
    }
}
