using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Models;
using App.Services.Similarity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Problemy;

[AllowAnonymous]
public class WynikiModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ISimilaritySearch _search;

    public WynikiModel(AppDbContext db, ISimilaritySearch search)
    {
        _db = db;
        _search = search;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public ProblemReport Problem { get; private set; } = null!;
    public List<SimilarityResult> Sprawdzone { get; private set; } = new();
    public List<SimilarityResult> WTestach { get; private set; } = new();
    public List<SimilarityResult> PoszukujaceFinansowania { get; private set; } = new();
    public List<ProblemReport> PodobneProblemy { get; private set; } = new();
    public bool Luka => Sprawdzone.Count == 0 && WTestach.Count == 0 && PoszukujaceFinansowania.Count == 0;

    public async Task<IActionResult> OnGetAsync()
    {
        var p = await _db.ProblemReports.FirstOrDefaultAsync(x => x.Id == Id);
        if (p is null) return NotFound();
        Problem = p;

        var wszystkie = await _search.ZnajdzDlaProblemuAsync(p, topN: 10);
        foreach (var w in wszystkie)
        {
            if (w.Etap == EtapInnowacji.SprawdzonaInnowacja) Sprawdzone.Add(w);
            else if (w.Etap == EtapInnowacji.WTestach || w.Etap == EtapInnowacji.WRealizacji
                     || w.Etap == EtapInnowacji.PoszukujeTesterow || w.Etap == EtapInnowacji.WPrzygotowaniu
                     || w.Etap == EtapInnowacji.TestyZakonczone) WTestach.Add(w);
            else if (w.Etap == EtapInnowacji.PoszukujeFinansowania || w.Etap == EtapInnowacji.Pomysl)
                PoszukujaceFinansowania.Add(w);
        }

        PodobneProblemy = (await _search.ZnajdzPodobneProblemyAsync(p)).ToList();

        if (Sprawdzone.Count > 0 && p.Status == StatusProblemu.Zgloszony)
        {
            p.Status = StatusProblemu.DopasowanoRozwiazanie;
            p.PowiazanaIdeaId = Sprawdzone.First().IdeaId;
            await _db.SaveChangesAsync();
        }
        else if (Luka && p.Status == StatusProblemu.Zgloszony)
        {
            p.Status = StatusProblemu.Luka;
            await _db.SaveChangesAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostObserwujAsync(int ideaId)
    {
        var p = await _db.ProblemReports.FirstOrDefaultAsync(x => x.Id == Id);
        if (p is null) return NotFound();
        p.PowiazanaIdeaId = ideaId;
        p.PowiadamiajORozwiazaniu = true;
        await _db.SaveChangesAsync();
        TempData["Komunikat"] = "Zapisaliśmy Twoje zainteresowanie. Powiadomimy, gdy pojawi się zmiana.";
        return RedirectToPage(new { id = Id });
    }
}
