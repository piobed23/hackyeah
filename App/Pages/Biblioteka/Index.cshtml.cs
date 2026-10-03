using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Biblioteka;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public TypMaterialu? FiltrTyp { get; set; }
    [BindProperty(SupportsGet = true)] public string? FiltrObszar { get; set; }
    [BindProperty(SupportsGet = true)] public string? Szukaj { get; set; }

    public List<LibraryItem> Materialy { get; private set; } = new();
    public List<string> DostepneObszary { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var q = _db.LibraryItems.Where(l => l.Status == StatusMaterialu.Opublikowany);
        if (FiltrTyp is not null) q = q.Where(l => l.Typ == FiltrTyp);
        if (!string.IsNullOrWhiteSpace(FiltrObszar))
            q = q.Where(l => l.Obszary.Contains(FiltrObszar));
        if (!string.IsNullOrWhiteSpace(Szukaj))
            q = q.Where(l => l.Tytul.Contains(Szukaj) || l.KrotkiOpis.Contains(Szukaj) || l.Tagi.Contains(Szukaj));

        Materialy = await q.OrderByDescending(l => l.OpublikowanoUtc).ToListAsync();

        DostepneObszary = (await _db.LibraryItems
            .Where(l => l.Status == StatusMaterialu.Opublikowany)
            .Select(l => l.Obszary).ToListAsync())
            .SelectMany(o => o.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct()
            .OrderBy(o => o)
            .ToList();
    }

    public static string NazwaTypu(TypMaterialu t) => t switch
    {
        TypMaterialu.Innowacja => "Innowacja społeczna",
        TypMaterialu.DobraPraktyka => "Dobra praktyka",
        TypMaterialu.Raport => "Raport",
        TypMaterialu.BadanieAnaliza => "Badanie/analiza",
        TypMaterialu.Poradnik => "Poradnik",
        TypMaterialu.MaterialEdukacyjny => "Materiał edukacyjny",
        TypMaterialu.Film => "Film",
        TypMaterialu.CanvasSzablon => "Canvas/szablon",
        TypMaterialu.WynikTestu => "Wynik testu",
        TypMaterialu.MaterialRops => "Materiał ROPS",
        TypMaterialu.LinkDoZrodla => "Link do źródła",
        _ => t.ToString()
    };
}
