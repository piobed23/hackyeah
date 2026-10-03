using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
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

    public List<IGrouping<string, ProblemReport>> KlasteryLuk { get; private set; } = new();
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

        KlasteryLuk = wszystkie
            .Where(p => p.Status == StatusProblemu.Luka)
            .GroupBy(p => KluczKlastra(p))
            .OrderByDescending(g => g.Count())
            .ToList();
    }

    private static string KluczKlastra(ProblemReport p)
    {
        var tagi = (p.Tagi ?? "").ToLowerInvariant()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(t => t)
            .FirstOrDefault() ?? "brak-tagu";
        return tagi;
    }
}
