using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Publiczne;

[AllowAnonymous]
public class DoTestowaniaModel : PageModel
{
    private readonly AppDbContext _db;
    public DoTestowaniaModel(AppDbContext db) => _db = db;

    public List<TestSession> Sesje { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Sesje = await _db.TestSessions
            .Include(t => t.Idea).ThenInclude(i => i.Karta)
            .Where(t => t.Status == StatusSesjiTestowej.PoszukujeTesterow)
            .OrderByDescending(t => t.UtworzonoUtc)
            .ToListAsync();
    }
}
