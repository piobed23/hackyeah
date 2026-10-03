using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Rops;

public class OcenaModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public OcenaModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public List<Application> DoOceny { get; private set; } = new();
    public List<Application> Rozpatrzone { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var statusyKolejki = new[] { StatusWniosku.Zlozony, StatusWniosku.KontrolaFormalna, StatusWniosku.OcenaMerytoryczna };
        DoOceny = await _db.Applications
            .Include(a => a.Call).Include(a => a.Autor).Include(a => a.Idea).ThenInclude(i => i.Karta)
            .Where(a => statusyKolejki.Contains(a.Status))
            .OrderBy(a => a.ZlozonoUtc)
            .ToListAsync();

        Rozpatrzone = await _db.Applications
            .Include(a => a.Call).Include(a => a.Autor)
            .Where(a => a.Status == StatusWniosku.Wybrany || a.Status == StatusWniosku.Niewybrany)
            .OrderByDescending(a => a.ZlozonoUtc)
            .Take(10)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostZmienAsync(int id, StatusWniosku nowyStatus, string? komentarz)
    {
        var w = await _db.Applications.Include(a => a.Idea).FirstOrDefaultAsync(a => a.Id == id);
        if (w is null) return NotFound();

        var rops = _auth.GetUser();
        if (rops is null) return Forbid();

        var stary = w.Status;
        w.Status = nowyStatus;
        if (!string.IsNullOrWhiteSpace(komentarz)) w.WynikOceny = komentarz;

        if (nowyStatus == StatusWniosku.Wybrany)
        {
            w.Idea.EtapInnowacji = EtapInnowacji.FinansowaniePrzyznane;
        }

        _db.ReviewDecisions.Add(new ReviewDecision
        {
            ApplicationId = w.Id,
            RecenzentId = rops.Id,
            Typ = nowyStatus == StatusWniosku.Wybrany ? TypDecyzjiROPS.Akceptuj
                : nowyStatus == StatusWniosku.Niewybrany ? TypDecyzjiROPS.Odrzuc
                : TypDecyzjiROPS.DoPoprawy,
            Komentarz = komentarz ?? ""
        });

        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = rops.Id,
            Akcja = $"WNIOSEK_{nowyStatus.ToString().ToUpper()}",
            EncjaTyp = nameof(Application),
            EncjaId = w.Id,
            MetaJson = $"{{\"z\":\"{stary}\",\"na\":\"{nowyStatus}\"}}"
        });

        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
