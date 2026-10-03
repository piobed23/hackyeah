using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Rops;

public class BibliotekaModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public BibliotekaModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public List<LibraryItem> Kolejka { get; private set; } = new();
    public List<LibraryItem> Opublikowane { get; private set; } = new();
    public List<LibraryItem> Archiwum { get; private set; } = new();
    public List<LibraryItemChangeProposal> PropozycjeZmian { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Kolejka = await _db.LibraryItems
            .Include(l => l.ZglaszajacyUser)
            .Where(l => l.Status == StatusMaterialu.OczekujeNaWeryfikacje || l.Status == StatusMaterialu.WymagaPoprawy)
            .OrderBy(l => l.AktualizowanoUtc).ToListAsync();

        Opublikowane = await _db.LibraryItems
            .Where(l => l.Status == StatusMaterialu.Opublikowany)
            .OrderByDescending(l => l.OpublikowanoUtc).Take(10).ToListAsync();

        Archiwum = await _db.LibraryItems
            .Where(l => l.Status == StatusMaterialu.Zarchiwizowany || l.Status == StatusMaterialu.Odrzucony || l.Status == StatusMaterialu.Duplikat)
            .OrderByDescending(l => l.AktualizowanoUtc).Take(10).ToListAsync();

        PropozycjeZmian = await _db.LibraryItemChangeProposals
            .Include(p => p.LibraryItem)
            .Include(p => p.ProponujacyUser)
            .Where(p => p.Status == StatusPropozycjiZmiany.Oczekujaca)
            .OrderBy(p => p.UtworzonoUtc).ToListAsync();
    }

    public async Task<IActionResult> OnPostDecyzjaAsync(int id, string akcja, string? komentarz)
    {
        var m = await _db.LibraryItems.FirstOrDefaultAsync(l => l.Id == id);
        if (m is null) return NotFound();
        var rops = _auth.GetUser();

        var stary = m.Status;
        switch (akcja)
        {
            case "Publikuj":
                m.Status = StatusMaterialu.Opublikowany;
                m.OpublikowanoUtc = DateTime.UtcNow;
                break;
            case "DoPoprawy": m.Status = StatusMaterialu.WymagaPoprawy; break;
            case "Odrzuc": m.Status = StatusMaterialu.Odrzucony; break;
            case "Archiwizuj": m.Status = StatusMaterialu.Zarchiwizowany; break;
        }
        m.AktualizowanoUtc = DateTime.UtcNow;

        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = rops?.Id,
            Akcja = $"BIBLIOTEKA_{akcja.ToUpper()}",
            EncjaTyp = nameof(LibraryItem),
            EncjaId = id,
            MetaJson = $"{{\"z\":\"{stary}\",\"na\":\"{m.Status}\"}}"
        });
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAkceptujPropozycjeAsync(int propozycjaId)
    {
        var p = await _db.LibraryItemChangeProposals
            .Include(x => x.LibraryItem).FirstOrDefaultAsync(x => x.Id == propozycjaId);
        if (p is null) return NotFound();
        var rops = _auth.GetUser();

        if (!string.IsNullOrWhiteSpace(p.NowyTytul)) p.LibraryItem.Tytul = p.NowyTytul;
        if (!string.IsNullOrWhiteSpace(p.NowyKrotkiOpis)) p.LibraryItem.KrotkiOpis = p.NowyKrotkiOpis;
        if (!string.IsNullOrWhiteSpace(p.NowyOpisPelny)) p.LibraryItem.OpisPelny = p.NowyOpisPelny;
        if (!string.IsNullOrWhiteSpace(p.NoweTagi)) p.LibraryItem.Tagi = p.NoweTagi;
        if (!string.IsNullOrWhiteSpace(p.NowyLink)) p.LibraryItem.Link = p.NowyLink;
        p.LibraryItem.AktualizowanoUtc = DateTime.UtcNow;

        p.Status = StatusPropozycjiZmiany.Zaakceptowana;
        p.RozpatrzonoUtc = DateTime.UtcNow;

        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = rops?.Id,
            Akcja = "BIBLIOTEKA_PROPOZYCJA_ZAAKCEPTOWANA",
            EncjaTyp = nameof(LibraryItemChangeProposal),
            EncjaId = propozycjaId
        });
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostOdrzucPropozycjeAsync(int propozycjaId)
    {
        var p = await _db.LibraryItemChangeProposals.FirstOrDefaultAsync(x => x.Id == propozycjaId);
        if (p is null) return NotFound();
        p.Status = StatusPropozycjiZmiany.Odrzucona;
        p.RozpatrzonoUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
