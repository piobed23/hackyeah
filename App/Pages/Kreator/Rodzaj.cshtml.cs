using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Pages.Kreator;

public class RodzajModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public RodzajModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(RodzajPomyslu rodzaj)
    {
        var user = _auth.GetUser();
        if (user is null) return Forbid();

        var idea = new Idea
        {
            AutorId = user.Id,
            Rodzaj = rodzaj,
            EtapInnowacji = EtapInnowacji.Pomysl,
            UtworzonoUtc = DateTime.UtcNow,
            Karta = new IdeaCard { Status = StatusFiszki.Szkic }
        };
        _db.Ideas.Add(idea);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Kreator/Krok", new { id = idea.Id, krok = 1 });
    }
}
