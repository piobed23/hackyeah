using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Rops;

public class ProblemyModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public ProblemyModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public List<ClasterLuki> KlasteryLuk { get; private set; } = new();
    public List<ProblemReport> Dopasowane { get; private set; } = new();
    public List<ProblemReport> Nowe { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var wszystkie = await _db.ProblemReports
            .Include(p => p.Mieszkaniec)
            .Include(p => p.PowiazanaIdea).ThenInclude(i => i!.Karta)
            .OrderByDescending(p => p.UtworzonoUtc)
            .ToListAsync();

        Nowe = wszystkie.Where(p => p.Status == StatusProblemu.Zgloszony).ToList();
        Dopasowane = wszystkie.Where(p => p.Status == StatusProblemu.DopasowanoRozwiazanie).ToList();

        var luki = wszystkie.Where(p => p.Status == StatusProblemu.Luka).ToList();
        KlasteryLuk = BudujKlastry(luki);
    }

    private const double ProgKlastrowania = 0.30;

    private static List<ClasterLuki> BudujKlastry(List<ProblemReport> luki)
    {
        if (luki.Count == 0) return new();

        var tokeny = luki.Select(TokensOf).ToArray();
        var uf = new UnionFind(luki.Count);

        for (int i = 0; i < luki.Count; i++)
        for (int j = i + 1; j < luki.Count; j++)
        {
            if (Jaccard(tokeny[i], tokeny[j]) >= ProgKlastrowania)
                uf.Union(i, j);
        }

        var grupy = luki
            .Select((p, i) => new { p, root = uf.Find(i), tokens = tokeny[i] })
            .GroupBy(x => x.root)
            .Select(g => g.ToList())
            .OrderByDescending(g => g.Count)
            .ToList();

        return grupy.Select(g => new ClasterLuki
        {
            Nazwa = WyznaczNazweKlastra(g.Select(x => x.tokens).ToList(), g.Select(x => x.p).ToList()),
            Problemy = g.Select(x => x.p).OrderByDescending(p => p.UtworzonoUtc).ToList(),
            Lokalizacje = g.Select(x => x.p)
                           .Where(p => !string.IsNullOrWhiteSpace(p.Lokalizacja))
                           .Select(p => p.Lokalizacja!)
                           .Distinct()
                           .ToList()
        }).ToList();
    }

    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "i","oraz","lub","albo","że","aby","dla","do","na","od","po","we","w","z","ze","się","nie","to","jest","są",
        "a","o","u","by","czy","jak","ale","tak","też","już","oni","ona","być","gdyż","który","która","które","kiedy",
        "mój","moja","moje","mnie","mi","mam","ma","mają","ten","ta","ci","tego","tej","naszym","nasza","nasz","problem",
        "problemu","nic","ma","mają","gdzie","czym","czego","tylko"
    };

    private static HashSet<string> TokensOf(ProblemReport p)
    {
        var text = $"{p.Tytul} {p.Opis} {p.KogoDotyczy} {p.OczekiwanyEfekt}".ToLowerInvariant();
        var cleaned = new string(text.Select(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) ? c : ' ').ToArray());
        return cleaned
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length >= 4 && !Stopwords.Contains(t))
            .ToHashSet();
    }

    private static double Jaccard(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 || b.Count == 0) return 0;
        var intersection = a.Intersect(b).Count();
        var union = a.Count + b.Count - intersection;
        return union == 0 ? 0 : (double)intersection / union;
    }

    private static string WyznaczNazweKlastra(List<HashSet<string>> tokeny, List<ProblemReport> grupa)
    {
        if (grupa.Count == 1) return grupa[0].Tytul;

        var wspolne = tokeny.Aggregate((HashSet<string>?)null, (acc, s) => acc is null ? new HashSet<string>(s) : acc.Intersect(s).ToHashSet())
                      ?? new HashSet<string>();

        if (wspolne.Count > 0)
        {
            var kluczowe = wspolne.OrderByDescending(w => w.Length).Take(3).ToList();
            return string.Join(" ", kluczowe);
        }

        return grupa[0].Tytul;
    }

    public class ClasterLuki
    {
        public string Nazwa { get; set; } = "";
        public List<ProblemReport> Problemy { get; set; } = new();
        public List<string> Lokalizacje { get; set; } = new();
    }

    private class UnionFind
    {
        private readonly int[] _parent;
        private readonly int[] _rank;

        public UnionFind(int n)
        {
            _parent = Enumerable.Range(0, n).ToArray();
            _rank = new int[n];
        }

        public int Find(int x)
        {
            while (_parent[x] != x)
            {
                _parent[x] = _parent[_parent[x]];
                x = _parent[x];
            }
            return x;
        }

        public void Union(int a, int b)
        {
            var ra = Find(a); var rb = Find(b);
            if (ra == rb) return;
            if (_rank[ra] < _rank[rb]) (ra, rb) = (rb, ra);
            _parent[rb] = ra;
            if (_rank[ra] == _rank[rb]) _rank[ra]++;
        }
    }
}
