using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Moje;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public IndexModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public List<Idea> MojePomysly { get; private set; } = new();
    public List<Application> MojeWnioski { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var user = _auth.GetUser();
        if (user is null) return;

        MojePomysly = await _db.Ideas
            .Include(i => i.Karta)
            .Where(i => i.AutorId == user.Id)
            .OrderByDescending(i => i.UtworzonoUtc)
            .ToListAsync();

        MojeWnioski = await _db.Applications
            .Include(a => a.Call)
            .Include(a => a.Idea).ThenInclude(i => i.Karta)
            .Where(a => a.AutorId == user.Id)
            .OrderByDescending(a => a.UtworzonoUtc)
            .ToListAsync();
    }

    public static string NazwaStatusu(StatusFiszki s) => s switch
    {
        StatusFiszki.Szkic => "Szkic",
        StatusFiszki.Przekazany => "Przekazany do weryfikacji",
        StatusFiszki.WymagaUzupelnienia => "Wymaga uzupełnienia",
        StatusFiszki.Zaakceptowany => "Zaakceptowany",
        StatusFiszki.Opublikowany => "Opublikowany",
        StatusFiszki.Archiwalny => "Archiwalny",
        StatusFiszki.KonsultacjaEkspercka => "Konsultacja ekspercka",
        StatusFiszki.Odrzucony => "Odrzucony",
        StatusFiszki.PropozycjaPolaczenia => "Propozycja połączenia",
        _ => s.ToString()
    };

    public static string KlasaStatusu(StatusFiszki s) => s switch
    {
        StatusFiszki.Opublikowany or StatusFiszki.Zaakceptowany => "bg-success",
        StatusFiszki.WymagaUzupelnienia or StatusFiszki.KonsultacjaEkspercka => "bg-warning text-dark",
        StatusFiszki.Odrzucony => "bg-danger",
        StatusFiszki.Archiwalny => "bg-secondary",
        _ => "bg-info text-dark"
    };
}
