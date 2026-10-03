using System.Text.Json;
using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Models;
using App.Services.Middleman;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Middleman;

public class WariantyModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IMiddlemanService _mm;

    public WariantyModel(AppDbContext db, IMiddlemanService mm)
    {
        _db = db;
        _mm = mm;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public ImplementationCard Karta { get; private set; } = null!;
    public Idea Innowacja { get; private set; } = null!;

    public AnalizaDopasowania Analiza { get; private set; } = null!;
    public IReadOnlyList<WariantPropozycja> Warianty { get; private set; } = Array.Empty<WariantPropozycja>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        Analiza = await _mm.AnalizujAsync(Innowacja, Karta);
        Warianty = await _mm.GenerujWariantyAsync(Innowacja, Karta);
        Karta.WariantyJson = JsonSerializer.Serialize(Warianty);
        await _db.SaveChangesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostWybierzAsync(WariantWdrozenia wariant)
    {
        if (!await WczytajAsync()) return NotFound();
        Karta.WybranyWariant = wariant;
        await _db.SaveChangesAsync();
        return RedirectToPage("/Middleman/Karta", new { id = Id });
    }

    private async Task<bool> WczytajAsync()
    {
        var k = await _db.ImplementationCards.Include(x => x.Idea).ThenInclude(i => i.Karta).FirstOrDefaultAsync(x => x.Id == Id);
        if (k is null) return false;
        Karta = k;
        Innowacja = k.Idea;
        return true;
    }
}
