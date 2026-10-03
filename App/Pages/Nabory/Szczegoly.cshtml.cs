using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Nabory;

public class SzczegolyModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public SzczegolyModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public Call? Nabor { get; private set; }
    public List<Idea> MojeFiszki { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Nabor = await _db.Calls.FirstOrDefaultAsync(c => c.Id == Id);
        if (Nabor is null)
        {
            TempData["Komunikat"] = $"Nabór nr {Id} nie istnieje lub został usunięty.";
            return RedirectToPage("/Nabory/Index");
        }

        var user = _auth.GetUser();
        if (user is not null)
        {
            MojeFiszki = await _db.Ideas
                .Include(i => i.Karta)
                .Where(i => i.AutorId == user.Id
                    && i.Karta != null
                    && (i.Karta.Status == StatusFiszki.Opublikowany
                        || i.Karta.Status == StatusFiszki.Zaakceptowany
                        || i.Karta.Status == StatusFiszki.Szkic))
                .ToListAsync();
        }

        return Page();
    }
}
