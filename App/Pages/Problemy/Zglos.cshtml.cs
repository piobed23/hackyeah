using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Pages.Problemy;

[AllowAnonymous]
public class ZglosModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public ZglosModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    [BindProperty] public ProblemReport Problem { get; set; } = new();
    [BindProperty] public bool Powiadamiaj { get; set; } = true;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = _auth.GetUser();
        if (user is null) return Forbid();

        if (string.IsNullOrWhiteSpace(Problem.Tytul) || string.IsNullOrWhiteSpace(Problem.Opis))
        {
            ModelState.AddModelError(string.Empty, "Podaj przynajmniej tytuł i opis sytuacji.");
            return Page();
        }

        Problem.MieszkaniecId = user.Id;
        Problem.Status = StatusProblemu.Zgloszony;
        Problem.PowiadamiajORozwiazaniu = Powiadamiaj;
        _db.ProblemReports.Add(Problem);
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = user.Id,
            Akcja = "PROBLEM_ZGLOSZONY",
            EncjaTyp = nameof(ProblemReport),
            EncjaId = 0,
            MetaJson = $"{{\"tytul\":\"{Problem.Tytul.Replace("\"", "'")}\"}}"
        });
        await _db.SaveChangesAsync();

        return RedirectToPage("/Problemy/Wyniki", new { id = Problem.Id });
    }
}
