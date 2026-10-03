using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Rops;

public class WeryfikacjaModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public WeryfikacjaModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public List<Idea> DoWeryfikacji { get; private set; } = new();
    public List<Idea> Rozpatrzone { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var kolejka = new[] { StatusFiszki.Przekazany, StatusFiszki.KonsultacjaEkspercka, StatusFiszki.PropozycjaPolaczenia };
        DoWeryfikacji = await _db.Ideas
            .Include(i => i.Karta).Include(i => i.Autor)
            .Where(i => i.Karta != null && kolejka.Contains(i.Karta.Status))
            .OrderBy(i => i.Karta!.AktualizowanoUtc)
            .ToListAsync();

        Rozpatrzone = await _db.Ideas
            .Include(i => i.Karta).Include(i => i.Autor)
            .Where(i => i.Karta != null && (i.Karta.Status == StatusFiszki.Zaakceptowany || i.Karta.Status == StatusFiszki.Opublikowany || i.Karta.Status == StatusFiszki.Odrzucony))
            .OrderByDescending(i => i.Karta!.AktualizowanoUtc)
            .Take(10)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDecyzjaAsync(int ideaId, TypDecyzjiROPS typ, string komentarz)
    {
        var idea = await _db.Ideas.Include(i => i.Karta).FirstOrDefaultAsync(i => i.Id == ideaId);
        if (idea?.Karta is null) return NotFound();

        var recenzent = _auth.GetUser();
        if (recenzent is null) return Forbid();

        var decyzja = new ReviewDecision
        {
            IdeaCardId = idea.Karta.Id,
            RecenzentId = recenzent.Id,
            Typ = typ,
            Komentarz = komentarz ?? ""
        };
        _db.ReviewDecisions.Add(decyzja);

        var staryStatus = idea.Karta.Status;
        var nowyStatus = typ switch
        {
            TypDecyzjiROPS.Akceptuj => StatusFiszki.Zaakceptowany,
            TypDecyzjiROPS.DoPoprawy => StatusFiszki.WymagaUzupelnienia,
            TypDecyzjiROPS.Odrzuc => StatusFiszki.Odrzucony,
            TypDecyzjiROPS.DoEksperta => StatusFiszki.KonsultacjaEkspercka,
            TypDecyzjiROPS.Polacz => StatusFiszki.PropozycjaPolaczenia,
            _ => idea.Karta.Status
        };

        idea.Karta.Status = nowyStatus;
        idea.Karta.AktualizowanoUtc = DateTime.UtcNow;

        if (typ == TypDecyzjiROPS.Akceptuj)
        {
            idea.Karta.Status = StatusFiszki.Opublikowany;
            idea.OpublikowanoUtc = DateTime.UtcNow;
            idea.EtapInnowacji = EtapInnowacji.PoszukujeFinansowania;
        }

        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = recenzent.Id,
            Akcja = $"FISZKA_{typ.ToString().ToUpper()}",
            EncjaTyp = nameof(IdeaCard),
            EncjaId = idea.Karta.Id,
            MetaJson = $"{{\"z\":\"{staryStatus}\",\"na\":\"{idea.Karta.Status}\"}}"
        });

        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
