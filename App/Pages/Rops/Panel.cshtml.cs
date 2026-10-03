using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Rops;

public class PanelModel : PageModel
{
    private readonly AppDbContext _db;
    public PanelModel(AppDbContext db) => _db = db;

    public int ProblemyNowe { get; private set; }
    public int ProblemyLuki { get; private set; }
    public int FiszkiDoWeryfikacji { get; private set; }
    public int WnioskiDoOceny { get; private set; }
    public int Nabory { get; private set; }
    public int SesjeTestowe { get; private set; }
    public int SesjeDoZatwierdzenia { get; private set; }
    public int BibliotekaKolejka { get; private set; }
    public int PropozycjeZmianBiblioteka { get; private set; }
    public int BibliotekaOpublikowana { get; private set; }
    public int InnowacjeSprawdzone { get; private set; }

    public List<AuditLogEntry> Historia { get; private set; } = new();

    public List<(string Tag, int Liczba)> TopTagi { get; private set; } = new();

    public async Task OnGetAsync()
    {
        ProblemyNowe = await _db.ProblemReports.CountAsync(p => p.Status == StatusProblemu.Zgloszony);
        ProblemyLuki = await _db.ProblemReports.CountAsync(p => p.Status == StatusProblemu.Luka);
        FiszkiDoWeryfikacji = await _db.IdeaCards.CountAsync(c =>
            c.Status == StatusFiszki.Przekazany || c.Status == StatusFiszki.KonsultacjaEkspercka || c.Status == StatusFiszki.PropozycjaPolaczenia);
        WnioskiDoOceny = await _db.Applications.CountAsync(a =>
            a.Status == StatusWniosku.Zlozony || a.Status == StatusWniosku.KontrolaFormalna || a.Status == StatusWniosku.OcenaMerytoryczna);
        Nabory = await _db.Calls.CountAsync(c => c.Aktywny);
        SesjeTestowe = await _db.TestSessions.CountAsync(t => t.Status != StatusSesjiTestowej.Zatwierdzone && t.Status != StatusSesjiTestowej.NieRozpoczeto);
        SesjeDoZatwierdzenia = await _db.TestSessions.CountAsync(t => t.Status == StatusSesjiTestowej.Zakonczone);
        BibliotekaKolejka = await _db.LibraryItems.CountAsync(l =>
            l.Status == StatusMaterialu.OczekujeNaWeryfikacje || l.Status == StatusMaterialu.WymagaPoprawy);
        PropozycjeZmianBiblioteka = await _db.LibraryItemChangeProposals.CountAsync(p => p.Status == StatusPropozycjiZmiany.Oczekujaca);
        BibliotekaOpublikowana = await _db.LibraryItems.CountAsync(l => l.Status == StatusMaterialu.Opublikowany);
        InnowacjeSprawdzone = await _db.Ideas.CountAsync(i => i.EtapInnowacji == EtapInnowacji.SprawdzonaInnowacja);

        Historia = await _db.AuditLog
            .Include(a => a.Uzytkownik)
            .OrderByDescending(a => a.KiedyUtc)
            .Take(30)
            .ToListAsync();

        var tagi = await _db.ProblemReports.Select(p => p.Tagi).ToListAsync();
        TopTagi = tagi
            .SelectMany(t => (t ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .GroupBy(t => t.ToLowerInvariant())
            .Select(g => (Tag: g.Key, Liczba: g.Count()))
            .OrderByDescending(g => g.Liczba)
            .Take(8)
            .ToList();
    }
}
