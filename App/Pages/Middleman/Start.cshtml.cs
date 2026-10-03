using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Middleman;

public class StartModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public StartModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public IActionResult OnGet() => RedirectToPage("/Publiczne/DoTestowania");

    public async Task<IActionResult> OnPostAsync(int ideaId)
    {
        var user = _auth.GetUser();
        if (user is null) return Forbid();

        var idea = await _db.Ideas.Include(i => i.Karta).FirstOrDefaultAsync(i => i.Id == ideaId);
        if (idea?.Karta is null) return NotFound();
        if (idea.EtapInnowacji != EtapInnowacji.SprawdzonaInnowacja)
        {
            TempData["Blad"] = "Middleman działa tylko dla sprawdzonych innowacji z Biblioteki.";
            return RedirectToPage("/Publiczne/Karta", new { id = ideaId });
        }

        var karta = new ImplementationCard
        {
            IdeaId = idea.Id,
            InstytucjaUserId = user.Id,
            Status = StatusWdrozenia.Szkic,
            NazwaInstytucji = user.Organizacja ?? $"{user.Imie} {user.Nazwisko}",
            TypInstytucji = user.TypInstytucji ?? "",
            Lokalizacja = user.Lokalizacja,
            Zasoby = user.ZasobyDomyslne,
            Budzet = 20_000m,
            LiczbaOdbiorcow = 30
        };
        _db.ImplementationCards.Add(karta);
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = user.Id,
            Akcja = "MIDDLEMAN_START",
            EncjaTyp = nameof(ImplementationCard),
            EncjaId = 0,
            MetaJson = $"{{\"ideaId\":{ideaId}}}"
        });
        await _db.SaveChangesAsync();

        return RedirectToPage("/Middleman/Dane", new { id = karta.Id });
    }
}
