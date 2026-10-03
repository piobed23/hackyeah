using App.Data;
using App.Data.Entities;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Middleman;

public class MojeModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public MojeModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public List<ImplementationCard> Wdrozenia { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = _auth.GetUser();
        if (user is null) return Forbid();

        Wdrozenia = await _db.ImplementationCards
            .Include(c => c.Idea).ThenInclude(i => i.Karta)
            .Where(c => c.InstytucjaUserId == user.Id)
            .OrderByDescending(c => c.UtworzonoUtc)
            .ToListAsync();
        return Page();
    }
}
