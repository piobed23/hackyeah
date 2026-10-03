using App.Data;
using App.Data.Entities;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Problemy;

public class MojeModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public MojeModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    public List<ProblemReport> Problemy { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = _auth.GetUser();
        if (user is null) return Forbid();

        Problemy = await _db.ProblemReports
            .Where(p => p.MieszkaniecId == user.Id)
            .OrderByDescending(p => p.UtworzonoUtc)
            .ToListAsync();
        return Page();
    }
}
