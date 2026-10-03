using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Rops;

public class PodgladTesterowModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public PodgladTesterowModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public List<TestSession> Sesje { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Sesje = await _db.TestSessions
            .Include(t => t.Idea).ThenInclude(i => i.Karta)
            .Include(t => t.Idea).ThenInclude(i => i.Autor)
            .Include(t => t.Zgloszenia).ThenInclude(z => z.Opinia)
            .OrderByDescending(t => t.UtworzonoUtc)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostZatwierdzAsync(int sesjaId)
    {
        var s = await _db.TestSessions.Include(x => x.Idea).FirstOrDefaultAsync(x => x.Id == sesjaId);
        if (s is null) return NotFound();
        if (s.Status != StatusSesjiTestowej.Zakonczone)
        {
            TempData["Blad"] = "Można zatwierdzić tylko sesje w statusie 'Zakończone'.";
            return RedirectToPage();
        }

        var rops = _auth.GetUser();
        s.Status = StatusSesjiTestowej.Zatwierdzone;
        s.Idea.EtapInnowacji = EtapInnowacji.SprawdzonaInnowacja;

        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = rops?.Id,
            Akcja = "INNOWACJA_SPRAWDZONA",
            EncjaTyp = nameof(TestSession),
            EncjaId = s.Id,
            MetaJson = $"{{\"ideaId\":{s.IdeaId}}}"
        });

        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
