using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Testerzy;

[AllowAnonymous]
public class OpiniaModel : PageModel
{
    private readonly AppDbContext _db;
    public OpiniaModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Zgloszenie { get; set; }

    public TesterApplication? Aplikacja { get; private set; }

    [BindProperty] public TesterFeedback Opinia { get; set; } = new() { OcenaOgolna = 4, Latwosc = 4 };

    public bool Wyslana { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Aplikacja = await _db.TesterApplications
            .Include(a => a.TestSession).ThenInclude(t => t.Idea).ThenInclude(i => i.Karta)
            .Include(a => a.Opinia)
            .FirstOrDefaultAsync(a => a.Id == Zgloszenie);
        if (Aplikacja is null) return NotFound();
        if (Aplikacja.Opinia is not null) Wyslana = true;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var a = await _db.TesterApplications.Include(x => x.Opinia).FirstOrDefaultAsync(x => x.Id == Zgloszenie);
        if (a is null) return NotFound();
        if (a.Opinia is not null)
        {
            Aplikacja = a;
            Wyslana = true;
            return Page();
        }

        Opinia.TesterApplicationId = a.Id;
        if (Opinia.OcenaOgolna < 1 || Opinia.OcenaOgolna > 5)
        {
            ModelState.AddModelError("Opinia.OcenaOgolna", "Ocena musi być w zakresie 1-5.");
            Aplikacja = await _db.TesterApplications
                .Include(x => x.TestSession).ThenInclude(t => t.Idea).ThenInclude(i => i.Karta)
                .FirstOrDefaultAsync(x => x.Id == Zgloszenie);
            return Page();
        }

        _db.TesterFeedbacks.Add(Opinia);
        a.Status = StatusTestera.OpiniaPrzeslana;

        _db.AuditLog.Add(new AuditLogEntry
        {
            Akcja = "OPINIA_PRZESLANA",
            EncjaTyp = nameof(TesterFeedback),
            EncjaId = 0,
            MetaJson = $"{{\"zgloszenieId\":{a.Id}}}"
        });

        await _db.SaveChangesAsync();
        return RedirectToPage(new { zgloszenie = Zgloszenie, wyslana = true });
    }
}
