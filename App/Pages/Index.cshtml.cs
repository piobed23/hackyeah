using App.Data;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public IndexModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public int LiczbaInnowacji { get; private set; }
    public int LiczbaOpublikowanych { get; private set; }
    public int LiczbaAktywnychNaborow { get; private set; }
    public int LiczbaProblemow { get; private set; }

    public async Task OnGetAsync()
    {
        LiczbaInnowacji = await _db.Ideas.CountAsync();
        LiczbaOpublikowanych = await _db.IdeaCards.CountAsync(c => c.Status == StatusFiszki.Opublikowany);
        LiczbaAktywnychNaborow = await _db.Calls.CountAsync(c => c.Aktywny);
        LiczbaProblemow = await _db.ProblemReports.CountAsync();
    }

    public async Task<IActionResult> OnPostZmienRoleAsync(RolaUzytkownika rola)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Rola == rola);
        if (user is not null)
        {
            _auth.SetUser(user.Id, rola);
        }
        return RedirectToPage();
    }
}
