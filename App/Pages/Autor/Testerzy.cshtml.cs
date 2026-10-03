using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Autor;

public class TesterzyModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public TesterzyModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    [BindProperty(SupportsGet = true)] public int IdeaId { get; set; }

    public Idea Idea { get; private set; } = null!;
    public TestSession? Sesja { get; private set; }

    public int Zgloszenia => Sesja?.Zgloszenia.Count ?? 0;
    public int Przyjeci => Sesja?.Zgloszenia.Count(z => z.Status != StatusTestera.Nowe && z.Status != StatusTestera.Odrzucony) ?? 0;
    public int Zakonczone => Sesja?.Zgloszenia.Count(z => z.Status == StatusTestera.OpiniaPrzeslana) ?? 0;
    public double SredniaOcena => (Sesja?.Zgloszenia.Where(z => z.Opinia is not null).Select(z => (double)z.Opinia!.OcenaOgolna).DefaultIfEmpty(0).Average()) ?? 0;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAkceptujAsync(int zgloszenieId, string wiadomosc)
    {
        if (!await WczytajAsync()) return NotFound();
        var z = Sesja?.Zgloszenia.FirstOrDefault(x => x.Id == zgloszenieId);
        if (z is null) return NotFound();

        z.Status = StatusTestera.Przyjety;
        z.WiadomoscInstrukcyjna = wiadomosc;
        if (Sesja!.Status == StatusSesjiTestowej.PoszukujeTesterow)
            Sesja.Status = StatusSesjiTestowej.WTrakcie;

        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = _auth.GetUser()?.Id,
            Akcja = "TESTER_PRZYJETY",
            EncjaTyp = nameof(TesterApplication),
            EncjaId = z.Id
        });

        await _db.SaveChangesAsync();
        return RedirectToPage(new { ideaId = IdeaId });
    }

    public async Task<IActionResult> OnPostOdrzucAsync(int zgloszenieId)
    {
        if (!await WczytajAsync()) return NotFound();
        var z = Sesja?.Zgloszenia.FirstOrDefault(x => x.Id == zgloszenieId);
        if (z is null) return NotFound();
        z.Status = StatusTestera.Odrzucony;
        await _db.SaveChangesAsync();
        return RedirectToPage(new { ideaId = IdeaId });
    }

    public async Task<IActionResult> OnPostPoprosOpinieAsync(int zgloszenieId)
    {
        if (!await WczytajAsync()) return NotFound();
        var z = Sesja?.Zgloszenia.FirstOrDefault(x => x.Id == zgloszenieId);
        if (z is null || z.Status != StatusTestera.Przyjety) return NotFound();
        z.Status = StatusTestera.TestWykonany;
        await _db.SaveChangesAsync();
        return RedirectToPage(new { ideaId = IdeaId });
    }

    public async Task<IActionResult> OnPostZakonczAsync(string akcja)
    {
        if (!await WczytajAsync()) return NotFound();
        if (Sesja is null) return NotFound();

        if (akcja == "WPoprawie")
        {
            Sesja.Status = StatusSesjiTestowej.WPoprawie;
            Idea.EtapInnowacji = EtapInnowacji.WPoprawie;
        }
        else if (akcja == "KolejniTesterzy")
        {
            Sesja.Status = StatusSesjiTestowej.PoszukujeTesterow;
            Idea.EtapInnowacji = EtapInnowacji.PoszukujeTesterow;
        }
        else if (akcja == "Zakoncz")
        {
            Sesja.Status = StatusSesjiTestowej.Zakonczone;
            Sesja.ZamknietoUtc = DateTime.UtcNow;
            Idea.EtapInnowacji = EtapInnowacji.TestyZakonczone;
        }

        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = _auth.GetUser()?.Id,
            Akcja = $"TEST_{akcja?.ToUpper()}",
            EncjaTyp = nameof(TestSession),
            EncjaId = Sesja.Id
        });

        await _db.SaveChangesAsync();
        return RedirectToPage(new { ideaId = IdeaId });
    }

    private async Task<bool> WczytajAsync()
    {
        var idea = await _db.Ideas
            .Include(i => i.Karta)
            .Include(i => i.SesjeTestowe).ThenInclude(t => t.Zgloszenia).ThenInclude(z => z.Opinia)
            .Include(i => i.SesjeTestowe).ThenInclude(t => t.Zgloszenia).ThenInclude(z => z.Uzytkownik)
            .FirstOrDefaultAsync(i => i.Id == IdeaId);
        if (idea is null) return false;
        Idea = idea;
        Sesja = idea.SesjeTestowe
            .OrderByDescending(t => t.UtworzonoUtc)
            .FirstOrDefault();
        return true;
    }
}
