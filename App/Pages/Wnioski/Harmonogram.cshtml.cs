using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Wnioski;

public class HarmonogramModel : PageModel
{
    private readonly AppDbContext _db;
    public HarmonogramModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public Application Wniosek { get; private set; } = null!;

    [BindProperty] public ScheduleItem NowaPozycja { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostDodajAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        if (Wniosek.Status != StatusWniosku.Szkic) return BadRequest();

        _db.ScheduleItems.Add(new ScheduleItem
        {
            ApplicationId = Id,
            Okres = NowaPozycja.Okres,
            NazwaZadania = NowaPozycja.NazwaZadania,
            Od = NowaPozycja.Od,
            Do = NowaPozycja.Do,
            Rezultat = NowaPozycja.Rezultat,
            KosztDzialania = NowaPozycja.KosztDzialania,
            Lp = Wniosek.Harmonogram.Count + 1
        });
        await _db.SaveChangesAsync();
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostUsunAsync(int pozycjaId)
    {
        if (!await WczytajAsync()) return NotFound();
        if (Wniosek.Status != StatusWniosku.Szkic) return BadRequest();

        var p = Wniosek.Harmonogram.FirstOrDefault(b => b.Id == pozycjaId);
        if (p is not null)
        {
            _db.ScheduleItems.Remove(p);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id = Id });
    }

    private async Task<bool> WczytajAsync()
    {
        var w = await _db.Applications
            .Include(a => a.Call)
            .Include(a => a.Harmonogram.OrderBy(h => h.Lp))
            .FirstOrDefaultAsync(a => a.Id == Id);
        if (w is null) return false;
        Wniosek = w;
        return true;
    }
}
