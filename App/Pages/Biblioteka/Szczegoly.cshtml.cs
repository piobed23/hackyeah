using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Biblioteka;

[AllowAnonymous]
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

    public LibraryItem Material { get; private set; } = null!;

    [BindProperty] public LibraryItemChangeProposal Propozycja { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var m = await _db.LibraryItems
            .Include(l => l.ZglaszajacyUser)
            .FirstOrDefaultAsync(l => l.Id == Id);
        if (m is null) return NotFound();
        Material = m;
        return Page();
    }

    public async Task<IActionResult> OnPostPropozycjaAsync()
    {
        var user = _auth.GetUser();
        if (user is null) return Forbid();
        var m = await _db.LibraryItems.FirstOrDefaultAsync(l => l.Id == Id);
        if (m is null) return NotFound();

        if (string.IsNullOrWhiteSpace(Propozycja.Uzasadnienie))
        {
            ModelState.AddModelError("Propozycja.Uzasadnienie", "Opisz, co i dlaczego chcesz zmienić.");
            Material = m;
            return Page();
        }

        Propozycja.LibraryItemId = Id;
        Propozycja.ProponujacyUserId = user.Id;
        _db.LibraryItemChangeProposals.Add(Propozycja);
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = user.Id,
            Akcja = "BIBLIOTEKA_PROPOZYCJA_ZMIANY",
            EncjaTyp = nameof(LibraryItem),
            EncjaId = Id
        });
        await _db.SaveChangesAsync();
        TempData["Komunikat"] = "Dziękujemy, Twoja propozycja zmiany trafiła do ROPS do rozpatrzenia.";
        return RedirectToPage(new { id = Id });
    }
}
