using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Wnioski;

public class EdycjaModel : PageModel
{
    private readonly AppDbContext _db;
    public EdycjaModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public Application Wniosek { get; set; } = new();

    public Call? Nabor { get; private set; }
    public string? Komunikat { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var w = await _db.Applications.Include(a => a.Call).FirstOrDefaultAsync(a => a.Id == Id);
        if (w is null) return NotFound();
        Wniosek = w;
        Nabor = w.Call;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var w = await _db.Applications.Include(a => a.Call).FirstOrDefaultAsync(a => a.Id == Id);
        if (w is null) return NotFound();
        if (w.Status != StatusWniosku.Szkic) return BadRequest("Nie można edytować złożonego wniosku.");

        // 1. Tytuł
        w.TytulProjektu = Wniosek.TytulProjektu;

        // 2. Dane wnioskodawcy
        w.TypWnioskodawcy = Wniosek.TypWnioskodawcy;
        w.Imie = Wniosek.Imie;
        w.Nazwisko = Wniosek.Nazwisko;
        w.AdresKorespondencyjny = Wniosek.AdresKorespondencyjny;
        w.KodPocztowy = Wniosek.KodPocztowy;
        w.Miejscowosc = Wniosek.Miejscowosc;
        w.Telefon = Wniosek.Telefon;
        w.Email = Wniosek.Email;
        w.NazwaPodmiotu = Wniosek.NazwaPodmiotu;
        w.KRS = Wniosek.KRS;
        w.REGON = Wniosek.REGON;
        w.NIP = Wniosek.NIP;
        w.OsobaReprezentujaca = Wniosek.OsobaReprezentujaca;
        w.FunkcjaReprezentujacego = Wniosek.FunkcjaReprezentujacego;
        w.OsobaKontaktowa = Wniosek.OsobaKontaktowa;
        w.TelefonKontaktowy = Wniosek.TelefonKontaktowy;
        w.EmailKontaktowy = Wniosek.EmailKontaktowy;
        w.PartnerzyJson = Wniosek.PartnerzyJson;

        // 3-8, 11
        w.OpisInnowacji = Wniosek.OpisInnowacji;
        w.Innowacyjnosc = Wniosek.Innowacyjnosc;
        w.DiagnozaProblemu = Wniosek.DiagnozaProblemu;
        w.MapaWyzwanTemat = Wniosek.MapaWyzwanTemat;
        w.OpisOdbiorcow = Wniosek.OpisOdbiorcow;
        w.ZmianaJakaWprowadza = Wniosek.ZmianaJakaWprowadza;
        w.WizjaPrzyszlosci = Wniosek.WizjaPrzyszlosci;
        w.WnioskowanaKwotaGrantu = Wniosek.WnioskowanaKwotaGrantu;
        w.ZespolDoswiadczenie = Wniosek.ZespolDoswiadczenie;
        w.Streszczenie = Wniosek.Streszczenie;

        await _db.SaveChangesAsync();
        Komunikat = $"Zapisano o {DateTime.Now:HH:mm}.";
        Wniosek = w;
        Nabor = w.Call;
        return Page();
    }
}
