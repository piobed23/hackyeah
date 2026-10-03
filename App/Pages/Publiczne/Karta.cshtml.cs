using App.Data;
using App.Data.Entities;
using App.Infrastructure.Auth;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Publiczne;

[AllowAnonymous]
public class KartaModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;

    public KartaModel(AppDbContext db, IAuthContext auth)
    {
        _db = db;
        _auth = auth;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public Idea? Idea { get; private set; }
    public IdeaCard? Karta => Idea?.Karta;
    public List<Comment> Komentarze { get; private set; } = new();
    public List<OrganizationOffer> Oferty { get; private set; } = new();
    public int LiczbaPoparcia { get; private set; }
    public bool JuzPoparl { get; private set; }
    public TestSession? AktywnaSesjaTestowa { get; private set; }

    [BindProperty] public string NowyKomentarz { get; set; } = "";
    [BindProperty] public string TypOferty { get; set; } = "";
    [BindProperty] public string OpisOferty { get; set; } = "";

    [BindProperty] public string ImieLubProfil { get; set; } = "";
    [BindProperty] public string? PowodZgloszenia { get; set; }
    [BindProperty] public string PreferowanyKontakt { get; set; } = "";
    [BindProperty] public string? PotrzebneUdogodnienia { get; set; }
    [BindProperty] public bool ZgodaNaPrzekazanieDanych { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await WczytajAsync();
        if (Idea is null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostPoparcieAsync()
    {
        var user = _auth.GetUser();
        if (user is null) return Unauthorized();
        if (!Uprawnienia.MozeGlosowacIKomentowac(_auth.GetRola())) return Forbid();

        var ist = await _db.Endorsements.FirstOrDefaultAsync(e => e.IdeaId == Id && e.UzytkownikId == user.Id);
        if (ist is null)
            _db.Endorsements.Add(new Endorsement { IdeaId = Id, UzytkownikId = user.Id });
        else
            _db.Endorsements.Remove(ist);

        await _db.SaveChangesAsync();
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostKomentarzAsync()
    {
        var user = _auth.GetUser();
        if (user is null) return Unauthorized();
        if (!Uprawnienia.MozeGlosowacIKomentowac(_auth.GetRola())) return Forbid();

        if (!string.IsNullOrWhiteSpace(NowyKomentarz))
        {
            _db.Comments.Add(new Comment
            {
                IdeaId = Id,
                AutorId = user.Id,
                Tresc = NowyKomentarz.Trim()
            });
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostZglosDoTestowAsync()
    {
        var user = _auth.GetUser();
        if (user is null) return Unauthorized();
        if (!Uprawnienia.MozeGlosowacIKomentowac(_auth.GetRola())) return Forbid();

        var sesja = await _db.TestSessions
            .FirstOrDefaultAsync(t => t.IdeaId == Id && (t.Status == StatusSesjiTestowej.PoszukujeTesterow || t.Status == StatusSesjiTestowej.WTrakcie));
        if (sesja is null) return NotFound();

        if (string.IsNullOrWhiteSpace(ImieLubProfil))
        {
            ModelState.AddModelError(nameof(ImieLubProfil), "Podaj imię lub profil.");
            await WczytajAsync();
            return Page();
        }

        _db.TesterApplications.Add(new TesterApplication
        {
            TestSessionId = sesja.Id,
            UzytkownikId = user.Id,
            ImieLubProfil = ImieLubProfil.Trim(),
            Powod = PowodZgloszenia?.Trim(),
            PreferowanyKontakt = PreferowanyKontakt?.Trim() ?? "",
            PotrzebneUdogodnienia = PotrzebneUdogodnienia?.Trim(),
            ZgodaNaPrzekazanieDanych = ZgodaNaPrzekazanieDanych,
            Status = StatusTestera.Nowe
        });
        await _db.SaveChangesAsync();
        return RedirectToPage(new { id = Id, zgloszono = true });
    }

    [BindProperty(SupportsGet = true)] public bool Zgloszono { get; set; }

    public async Task<IActionResult> OnPostOfertaAsync()
    {
        var user = _auth.GetUser();
        if (user is null) return Unauthorized();
        if (!Uprawnienia.MozeSkladacOferte(_auth.GetRola())) return Forbid();

        if (!string.IsNullOrWhiteSpace(TypOferty) && !string.IsNullOrWhiteSpace(OpisOferty))
        {
            _db.OrganizationOffers.Add(new OrganizationOffer
            {
                IdeaId = Id,
                OrganizacjaUserId = user.Id,
                TypOferty = TypOferty.Trim(),
                Opis = OpisOferty.Trim()
            });
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id = Id });
    }

    private async Task WczytajAsync()
    {
        Idea = await _db.Ideas
            .Include(i => i.Autor)
            .Include(i => i.Karta)
            .Include(i => i.SesjeTestowe)
            .FirstOrDefaultAsync(i => i.Id == Id);
        if (Idea is null) return;

        Komentarze = await _db.Comments
            .Include(c => c.Autor)
            .Where(c => c.IdeaId == Id && c.Widoczny)
            .OrderByDescending(c => c.UtworzonoUtc)
            .ToListAsync();

        Oferty = await _db.OrganizationOffers
            .Include(o => o.OrganizacjaUser)
            .Where(o => o.IdeaId == Id)
            .OrderByDescending(o => o.KiedyUtc)
            .ToListAsync();

        LiczbaPoparcia = await _db.Endorsements.CountAsync(e => e.IdeaId == Id);

        var ja = _auth.GetUser();
        if (ja is not null)
            JuzPoparl = await _db.Endorsements.AnyAsync(e => e.IdeaId == Id && e.UzytkownikId == ja.Id);

        AktywnaSesjaTestowa = Idea.SesjeTestowe
            .FirstOrDefault(t => t.Status == StatusSesjiTestowej.PoszukujeTesterow || t.Status == StatusSesjiTestowej.WTrakcie);
    }
}
