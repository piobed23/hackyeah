using App.Data;
using App.Data.Entities;
using App.Models;
using App.Services.Similarity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Kreator;

public class PodobneModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ISimilaritySearch _search;

    public PodobneModel(AppDbContext db, ISimilaritySearch search)
    {
        _db = db;
        _search = search;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public Idea? Idea { get; private set; }
    public IReadOnlyList<SimilarityResult> Wyniki { get; private set; } = Array.Empty<SimilarityResult>();

    public async Task<IActionResult> OnGetAsync()
    {
        Idea = await _db.Ideas.Include(i => i.Karta).FirstOrDefaultAsync(i => i.Id == Id);
        if (Idea?.Karta is null) return NotFound();
        Wyniki = await _search.ZnajdzPodobneAsync(Idea.Karta);
        return Page();
    }

    public static string Etykieta(KategoriaPodobienstwa k) => k switch
    {
        KategoriaPodobienstwa.SprawdzonaInnowacja => "Sprawdzona innowacja",
        KategoriaPodobienstwa.PodobnyPomysl => "Podobny pomysł",
        KategoriaPodobienstwa.TenSamProblem => "Ten sam problem",
        _ => k.ToString()
    };

    public static string Klasa(KategoriaPodobienstwa k) => k switch
    {
        KategoriaPodobienstwa.SprawdzonaInnowacja => "bg-success",
        KategoriaPodobienstwa.PodobnyPomysl => "bg-info text-dark",
        KategoriaPodobienstwa.TenSamProblem => "bg-warning text-dark",
        _ => "bg-light text-dark"
    };
}
