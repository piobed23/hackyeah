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

public class KartaModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IMiddlemanService _mm;

    public KartaModel(AppDbContext db, IMiddlemanService mm)
    {
        _db = db;
        _mm = mm;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public ImplementationCard Karta { get; private set; } = null!;
    public Idea Innowacja { get; private set; } = null!;
    public WariantPropozycja? Wariant { get; private set; }
    public IReadOnlyList<string> BrakujacyPartnerzy { get; private set; } = Array.Empty<string>();
    public List<Call> PasujaceNabory { get; private set; } = new();

    [BindProperty] public string? EdytowanaKarta { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        if (Karta.WybranyWariant is null) return RedirectToPage("/Middleman/Warianty", new { id = Id });

        Wariant = OdczytajWariant();
        if (Wariant is null) return RedirectToPage("/Middleman/Warianty", new { id = Id });

        if (string.IsNullOrWhiteSpace(Karta.KartaMd))
        {
            Karta.KartaMd = await _mm.GenerujKarteWdrozeniaMdAsync(Innowacja, Karta, Wariant);
            Karta.BrakujacyPartnerzy = string.Join("\n", await _mm.WykryjBrakujacychPartnerowAsync(Innowacja, Karta));
            await _db.SaveChangesAsync();
        }

        BrakujacyPartnerzy = (Karta.BrakujacyPartnerzy ?? "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        EdytowanaKarta = Karta.KartaMd;

        PasujaceNabory = await _db.Calls
            .Where(c => c.Aktywny && c.DataZamkniecia > DateTime.UtcNow)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostZapiszAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        Karta.KartaMd = EdytowanaKarta;
        await _db.SaveChangesAsync();
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostZatwierdzAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        Karta.Status = StatusWdrozenia.Zatwierdzony;
        Karta.ZatwierdzonoUtc = DateTime.UtcNow;
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = Karta.InstytucjaUserId,
            Akcja = "MIDDLEMAN_ZATWIERDZONE",
            EncjaTyp = nameof(ImplementationCard),
            EncjaId = Karta.Id
        });
        await _db.SaveChangesAsync();
        return RedirectToPage(new { id = Id });
    }

    [BindProperty] public string? PytanieDoAutora { get; set; }

    public async Task<IActionResult> OnPostPytajAutoraAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        if (string.IsNullOrWhiteSpace(PytanieDoAutora))
        {
            TempData["Komunikat"] = "Opisz pytanie dla autora.";
            return RedirectToPage(new { id = Id });
        }
        Karta.PytanieDoAutora = PytanieDoAutora;
        Karta.PytanieWyslaneUtc = DateTime.UtcNow;
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = Karta.InstytucjaUserId,
            Akcja = "MIDDLEMAN_PYTANIE_DO_AUTORA",
            EncjaTyp = nameof(ImplementationCard),
            EncjaId = Karta.Id
        });
        await _db.SaveChangesAsync();
        TempData["Komunikat"] = "Pytanie przekazane autorowi innowacji.";
        return RedirectToPage(new { id = Id });
    }

    private WariantPropozycja? OdczytajWariant()
    {
        if (string.IsNullOrWhiteSpace(Karta.WariantyJson) || Karta.WybranyWariant is null) return null;
        try
        {
            var warianty = JsonSerializer.Deserialize<List<WariantPropozycja>>(Karta.WariantyJson);
            return warianty?.FirstOrDefault(w => w.Typ == Karta.WybranyWariant);
        }
        catch
        {
            return null;
        }
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
