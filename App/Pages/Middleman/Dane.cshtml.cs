using App.Data;
using App.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Middleman;

public class DaneModel : PageModel
{
    private readonly AppDbContext _db;
    public DaneModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public ImplementationCard Karta { get; set; } = new();

    public Idea Innowacja { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var k = await _db.ImplementationCards.Include(x => x.Idea).ThenInclude(i => i.Karta).FirstOrDefaultAsync(x => x.Id == Id);
        if (k is null) return NotFound();
        Karta = k;
        Innowacja = k.Idea;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var k = await _db.ImplementationCards.Include(x => x.Idea).ThenInclude(i => i.Karta).FirstOrDefaultAsync(x => x.Id == Id);
        if (k is null) return NotFound();

        k.NazwaInstytucji = Karta.NazwaInstytucji;
        k.TypInstytucji = Karta.TypInstytucji;
        k.Lokalizacja = Karta.Lokalizacja;
        k.LiczbaOdbiorcow = Karta.LiczbaOdbiorcow;
        k.Budzet = Karta.Budzet;
        k.DostepnyPersonel = Karta.DostepnyPersonel;
        k.Zasoby = Karta.Zasoby;
        k.PotrzebyDostepnosci = Karta.PotrzebyDostepnosci;
        k.Ograniczenia = Karta.Ograniczenia;
        k.OpisPotrzeby = Karta.OpisPotrzeby;
        k.PlanowanyStart = Karta.PlanowanyStart;

        await _db.SaveChangesAsync();
        return RedirectToPage("/Middleman/Warianty", new { id = Id });
    }
}
