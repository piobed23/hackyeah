using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Models;
using App.Services.Completeness;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Wnioski;

public class PodgladModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICompletenessChecker<Application> _checker;
    private readonly IAuthContext _auth;

    public PodgladModel(AppDbContext db, ICompletenessChecker<Application> checker, IAuthContext auth)
    {
        _db = db;
        _checker = checker;
        _auth = auth;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public bool OswiadczenieZgoda { get; set; }

    public Application Wniosek { get; private set; } = null!;
    public CompletenessResult Wynik { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostZlozAsync()
    {
        if (!await WczytajAsync()) return NotFound();

        if (!Wynik.CanSubmit)
        {
            ModelState.AddModelError(string.Empty, "Wniosek nie jest kompletny — uzupełnij braki.");
            return Page();
        }
        if (!OswiadczenieZgoda)
        {
            ModelState.AddModelError(nameof(OswiadczenieZgoda), "Musisz zatwierdzić oświadczenia przed złożeniem.");
            return Page();
        }
        if (Wniosek.Status != StatusWniosku.Szkic)
        {
            ModelState.AddModelError(string.Empty, "Wniosek już został złożony.");
            return Page();
        }

        Wniosek.Status = StatusWniosku.Zlozony;
        Wniosek.ZlozonoUtc = DateTime.UtcNow;
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = _auth.GetUser()?.Id,
            Akcja = "WNIOSEK_ZLOZONY",
            EncjaTyp = nameof(Application),
            EncjaId = Wniosek.Id
        });
        await _db.SaveChangesAsync();
        return RedirectToPage("/Wnioski/Status", new { id = Wniosek.Id, zlozony = true });
    }

    private async Task<bool> WczytajAsync()
    {
        var w = await _db.Applications
            .Include(a => a.Call)
            .Include(a => a.Idea).ThenInclude(i => i.Karta)
            .Include(a => a.Budzet)
            .Include(a => a.Harmonogram)
            .FirstOrDefaultAsync(a => a.Id == Id);
        if (w is null) return false;
        Wniosek = w;
        Wynik = _checker.Check(w);
        return true;
    }
}
