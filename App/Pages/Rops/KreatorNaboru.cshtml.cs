using App.Data;
using App.Data.Entities;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Pages.Rops;

public class KreatorNaboruModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public KreatorNaboruModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    [BindProperty] public Call Nabor { get; set; } = new()
    {
        DataOtwarcia = DateTime.UtcNow.Date,
        DataZamkniecia = DateTime.UtcNow.Date.AddMonths(2),
        BudzetMaks = 50000m,
        ProcentWkladuWlasnego = 10m
    };

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var rops = _auth.GetUser();
        if (rops is null) return Forbid();

        Nabor.OrganizatorId = rops.Id;
        Nabor.Aktywny = true;
        Nabor.KryteriaMd ??= "";
        Nabor.Opis ??= "";
        if (string.IsNullOrWhiteSpace(Nabor.DozwoloneRodzaje))
            Nabor.DozwoloneRodzaje = "Innowacja,DobraPraktyka,Mikroskala";

        if (!ModelState.IsValid) return Page();
        if (Nabor.DataZamkniecia <= Nabor.DataOtwarcia)
        {
            ModelState.AddModelError("Nabor.DataZamkniecia", "Data zamknięcia musi być po dacie otwarcia.");
            return Page();
        }

        _db.Calls.Add(Nabor);
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = rops.Id,
            Akcja = "NABOR_UTWORZONY",
            EncjaTyp = nameof(Call),
            EncjaId = 0,
            MetaJson = $"{{\"nazwa\":\"{Nabor.Nazwa}\"}}"
        });
        await _db.SaveChangesAsync();

        return RedirectToPage("/Nabory/Szczegoly", new { id = Nabor.Id });
    }
}
