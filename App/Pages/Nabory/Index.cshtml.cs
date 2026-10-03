using App.Data;
using App.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Nabory;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Call> Nabory { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Nabory = await _db.Calls
            .Where(c => c.Aktywny)
            .OrderBy(c => c.DataZamkniecia)
            .ToListAsync();
    }
}
