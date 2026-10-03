using App.Data;
using App.Data.Entities;
using App.Models;
using App.Services.Completeness;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Wnioski;

public class KompletnoscModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICompletenessChecker<Application> _checker;

    public KompletnoscModel(AppDbContext db, ICompletenessChecker<Application> checker)
    {
        _db = db;
        _checker = checker;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public Application Wniosek { get; private set; } = null!;
    public CompletenessResult Wynik { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var w = await _db.Applications
            .Include(a => a.Call)
            .Include(a => a.Budzet)
            .Include(a => a.Harmonogram)
            .FirstOrDefaultAsync(a => a.Id == Id);
        if (w is null) return NotFound();
        Wniosek = w;
        Wynik = _checker.Check(w);
        return Page();
    }
}
