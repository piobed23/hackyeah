using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Models;
using App.Services.Completeness;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Kreator;

public class PodgladModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICompletenessChecker<IdeaCard> _checker;

    public PodgladModel(AppDbContext db, ICompletenessChecker<IdeaCard> checker)
    {
        _db = db;
        _checker = checker;
    }

    public Idea? Idea { get; private set; }
    public CompletenessResult? Kompletnosc { get; private set; }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty(SupportsGet = true)] public bool Wyslana { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Idea = await _db.Ideas.Include(i => i.Karta).Include(i => i.Autor).FirstOrDefaultAsync(i => i.Id == Id);
        if (Idea?.Karta is null) return NotFound();
        Kompletnosc = _checker.Check(Idea.Karta);
        return Page();
    }

    public static string NazwaStatusu(StatusFiszki s) => Moje.IndexModel.NazwaStatusu(s);
    public static string KlasaStatusu(StatusFiszki s) => Moje.IndexModel.KlasaStatusu(s);
}
