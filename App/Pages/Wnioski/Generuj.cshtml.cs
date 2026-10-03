using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Wnioski;

public class GenerujModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public GenerujModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public async Task<IActionResult> OnPostAsync(int ideaId, int callId)
    {
        var user = _auth.GetUser();
        if (user is null) return Forbid();

        var idea = await _db.Ideas.Include(i => i.Karta).FirstOrDefaultAsync(i => i.Id == ideaId);
        var nabor = await _db.Calls.FirstOrDefaultAsync(c => c.Id == callId);
        if (idea?.Karta is null || nabor is null) return NotFound();

        var k = idea.Karta;
        var typ = string.IsNullOrWhiteSpace(user.Organizacja) ? TypWnioskodawcy.OsobaFizyczna : TypWnioskodawcy.Podmiot;

        var wniosek = new Application
        {
            IdeaId = idea.Id,
            CallId = nabor.Id,
            AutorId = user.Id,
            Status = StatusWniosku.Szkic,
            TytulProjektu = k.Tytul,
            TypWnioskodawcy = typ,
            Imie = user.Imie,
            Nazwisko = user.Nazwisko,
            Email = user.Email,
            NazwaPodmiotu = typ == TypWnioskodawcy.Podmiot ? user.Organizacja : null,
            OsobaReprezentujaca = typ == TypWnioskodawcy.Podmiot ? $"{user.Imie} {user.Nazwisko}" : null,
            Streszczenie = k.PodsumowanieAI ?? k.Streszczenie,
            OpisInnowacji = string.Join(Environment.NewLine + Environment.NewLine,
                new[] { k.Rozwiazanie, k.SposobDzialania }.Where(s => !string.IsNullOrWhiteSpace(s))),
            DiagnozaProblemu = k.Problem,
            OpisOdbiorcow = k.GrupaDocelowa,
            ZmianaJakaWprowadza = k.Rezultaty,
            ZespolDoswiadczenie = k.PosiadaneZasoby,
            UtworzonoUtc = DateTime.UtcNow
        };
        _db.Applications.Add(wniosek);
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = user.Id,
            Akcja = "WNIOSEK_UTWORZONY",
            EncjaTyp = nameof(Application),
            EncjaId = 0,
            MetaJson = $"{{\"ideaId\":{ideaId},\"callId\":{callId}}}"
        });
        await _db.SaveChangesAsync();

        return RedirectToPage("/Wnioski/Edycja", new { id = wniosek.Id });
    }

    public IActionResult OnGet() => RedirectToPage("/Nabory/Index");
}
