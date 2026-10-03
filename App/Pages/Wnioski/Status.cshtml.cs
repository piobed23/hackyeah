using App.Data;
using App.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Wnioski;

public class StatusModel : PageModel
{
    private readonly AppDbContext _db;
    public StatusModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty(SupportsGet = true)] public bool Zlozony { get; set; }

    public Application Wniosek { get; private set; } = null!;
    public List<AuditLogEntry> Historia { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var w = await _db.Applications
            .Include(a => a.Call)
            .Include(a => a.Idea).ThenInclude(i => i.Karta)
            .FirstOrDefaultAsync(a => a.Id == Id);
        if (w is null) return NotFound();
        Wniosek = w;

        Historia = await _db.AuditLog
            .Include(x => x.Uzytkownik)
            .Where(x => x.EncjaTyp == nameof(Application) && x.EncjaId == Id)
            .OrderByDescending(x => x.KiedyUtc)
            .ToListAsync();
        return Page();
    }
}
