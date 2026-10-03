using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Ekspert;

public class DoOcenyModel : PageModel
{
    private readonly AppDbContext _db;

    public DoOcenyModel(AppDbContext db) => _db = db;

    public List<IdeaCard> Fiszki { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Fiszki = await _db.IdeaCards
            .Where(c => c.Status == StatusFiszki.KonsultacjaEkspercka)
            .OrderByDescending(c => c.AktualizowanoUtc)
            .ToListAsync();
    }
}
