using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Wnioski;

public class BudzetModel : PageModel
{
    private readonly AppDbContext _db;
    public BudzetModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public Application Wniosek { get; private set; } = null!;

    [BindProperty] public BudgetItem NowaPozycja { get; set; } = new();

    public decimal SumaBrutto => Wniosek?.Budzet.Sum(b => b.KwotaBrutto) ?? 0m;
    public decimal SumaWkladu => Wniosek?.Budzet.Sum(b => b.WkladWlasny) ?? 0m;
    public decimal ProcentWkladu => SumaBrutto == 0 ? 0 : SumaWkladu / SumaBrutto * 100m;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostDodajAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        if (Wniosek.Status != StatusWniosku.Szkic) return BadRequest();

        var kwota = NowaPozycja.Jednostki * NowaPozycja.KosztJednostkowy;
        _db.BudgetItems.Add(new BudgetItem
        {
            ApplicationId = Id,
            Okres = NowaPozycja.Okres,
            Kategoria = NowaPozycja.Kategoria,
            Opis = NowaPozycja.Opis,
            Jednostki = NowaPozycja.Jednostki,
            KosztJednostkowy = NowaPozycja.KosztJednostkowy,
            KwotaBrutto = kwota,
            WkladWlasny = NowaPozycja.WkladWlasny,
            Uzasadnienie = NowaPozycja.Uzasadnienie,
            Lp = Wniosek.Budzet.Count + 1
        });
        await _db.SaveChangesAsync();
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostUsunAsync(int pozycjaId)
    {
        if (!await WczytajAsync()) return NotFound();
        if (Wniosek.Status != StatusWniosku.Szkic) return BadRequest();

        var p = Wniosek.Budzet.FirstOrDefault(b => b.Id == pozycjaId);
        if (p is not null)
        {
            _db.BudgetItems.Remove(p);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id = Id });
    }

    private async Task<bool> WczytajAsync()
    {
        var w = await _db.Applications
            .Include(a => a.Call)
            .Include(a => a.Budzet.OrderBy(b => b.Lp))
            .FirstOrDefaultAsync(a => a.Id == Id);
        if (w is null) return false;
        Wniosek = w;
        return true;
    }
}
