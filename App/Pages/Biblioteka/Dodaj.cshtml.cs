using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Pages.Biblioteka;

public class DodajModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public DodajModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    [BindProperty] public LibraryItem Material { get; set; } = new() { AutorPubliczny = true };

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = _auth.GetUser();
        if (user is null) return Forbid();

        if (!Material.PotwierdzeniePraw)
        {
            ModelState.AddModelError("Material.PotwierdzeniePraw", "Musisz potwierdzić, że masz prawo udostępnić ten materiał.");
            return Page();
        }
        if (!ModelState.IsValid) return Page();

        Material.ZglaszajacyUserId = user.Id;
        Material.Status = StatusMaterialu.OczekujeNaWeryfikacje;
        _db.LibraryItems.Add(Material);
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = user.Id,
            Akcja = "BIBLIOTEKA_MATERIAL_ZGLOSZONY",
            EncjaTyp = nameof(LibraryItem),
            EncjaId = 0,
            MetaJson = $"{{\"tytul\":\"{Material.Tytul.Replace("\"", "'")}\"}}"
        });
        await _db.SaveChangesAsync();
        TempData["Komunikat"] = "Dziękujemy! Materiał trafił do weryfikacji ROPS.";
        return RedirectToPage("/Biblioteka/Index");
    }
}
