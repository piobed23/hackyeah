using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Autor;

public class InnowacjaModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public InnowacjaModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public Idea Idea { get; private set; } = null!;
    public TestSession? AktywnaSesja { get; private set; }
    public int LiczbaZgloszen { get; private set; }
    public int LiczbaOpinii { get; private set; }
    public List<ImplementationCard> PytaniaOdInstytucji { get; private set; } = new();

    [BindProperty] public TestSession NowaSesja { get; set; } = new()
    {
        LiczbaTesterow = 5,
        Zdalny = true,
        Termin = DateTime.UtcNow.Date.AddDays(14)
    };

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        return Page();
    }

    [BindProperty] public int KartaWdrozeniaId { get; set; }
    [BindProperty] public string? Odpowiedz { get; set; }

    public async Task<IActionResult> OnPostOdpowiedzAsync()
    {
        var k = await _db.ImplementationCards.FirstOrDefaultAsync(x => x.Id == KartaWdrozeniaId);
        if (k is null) return NotFound();
        if (string.IsNullOrWhiteSpace(Odpowiedz)) return RedirectToPage(new { id = Id });
        k.OdpowiedzAutora = Odpowiedz;
        k.OdpowiedzUtc = DateTime.UtcNow;
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = _auth.GetUser()?.Id,
            Akcja = "MIDDLEMAN_ODPOWIEDZ_AUTORA",
            EncjaTyp = nameof(ImplementationCard),
            EncjaId = k.Id
        });
        await _db.SaveChangesAsync();
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostRozpocznijAsync()
    {
        if (!await WczytajAsync()) return NotFound();

        var sesja = new TestSession
        {
            IdeaId = Idea.Id,
            Status = StatusSesjiTestowej.PoszukujeTesterow,
            CoBedzieTestowane = NowaSesja.CoBedzieTestowane,
            KogoSzukamy = NowaSesja.KogoSzukamy,
            LiczbaTesterow = NowaSesja.LiczbaTesterow,
            Zdalny = NowaSesja.Zdalny,
            Termin = NowaSesja.Termin,
            CzasTrwania = NowaSesja.CzasTrwania,
            Instrukcja = NowaSesja.Instrukcja,
            Wymagania = NowaSesja.Wymagania
        };
        _db.TestSessions.Add(sesja);

        Idea.EtapInnowacji = EtapInnowacji.PoszukujeTesterow;
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = _auth.GetUser()?.Id,
            Akcja = "TEST_ROZPOCZETY",
            EncjaTyp = nameof(TestSession),
            EncjaId = 0,
            MetaJson = $"{{\"ideaId\":{Idea.Id}}}"
        });
        await _db.SaveChangesAsync();

        return RedirectToPage("/Autor/Testerzy", new { ideaId = Idea.Id });
    }

    private async Task<bool> WczytajAsync()
    {
        var idea = await _db.Ideas
            .Include(i => i.Karta)
            .Include(i => i.SesjeTestowe).ThenInclude(t => t.Zgloszenia).ThenInclude(z => z.Opinia)
            .FirstOrDefaultAsync(i => i.Id == Id);
        if (idea is null) return false;
        Idea = idea;
        AktywnaSesja = idea.SesjeTestowe.FirstOrDefault(t =>
            t.Status == StatusSesjiTestowej.PoszukujeTesterow
            || t.Status == StatusSesjiTestowej.WTrakcie
            || t.Status == StatusSesjiTestowej.WPoprawie);
        LiczbaZgloszen = AktywnaSesja?.Zgloszenia.Count ?? 0;
        LiczbaOpinii = AktywnaSesja?.Zgloszenia.Count(z => z.Opinia is not null) ?? 0;

        PytaniaOdInstytucji = await _db.ImplementationCards
            .Include(k => k.InstytucjaUser)
            .Where(k => k.IdeaId == Id && k.PytanieDoAutora != null)
            .OrderByDescending(k => k.PytanieWyslaneUtc)
            .ToListAsync();

        return true;
    }
}
