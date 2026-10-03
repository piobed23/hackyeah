using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Infrastructure.Validation;
using App.Models;
using App.Services.Ai;
using App.Services.Completeness;
using App.Services.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace App.Pages.Kreator;

public class KrokModel : PageModel
{
    public const int LacznieKrokow = 7;

    public static readonly string[] Etykiety =
    {
        "Identyfikacja",
        "Potrzeba",
        "Rozwiązanie",
        "Rezultaty",
        "Zasoby",
        "Ryzyka i tagi",
        "Podsumowanie"
    };

    private readonly AppDbContext _db;
    private readonly IAuthContext _auth;
    private readonly IAiAssistant _ai;
    private readonly ICompletenessChecker<IdeaCard> _checker;

    public KrokModel(AppDbContext db, IAuthContext auth, IAiAssistant ai, ICompletenessChecker<IdeaCard> checker)
    {
        _db = db;
        _auth = auth;
        _ai = ai;
        _checker = checker;
    }

    public Idea Idea { get; private set; } = null!;

    [BindProperty] public IdeaCard Karta { get; set; } = new();
    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty(SupportsGet = true)] public int Krok { get; set; } = 1;

    public AiSuggestion? Sugestia { get; private set; }
    public CompletenessResult? Kompletnosc { get; private set; }
    public string? KomunikatZapisu { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await WczytajAsync()) return NotFound();
        await PrzygotujAiAsync();
        if (Krok == 7) Kompletnosc = _checker.Check(Karta);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string akcja)
    {
        if (!await WczytajAsync(dolaczBinding: true)) return NotFound();

        var walidator = new IdeaCardValidator();
        var wynik = await walidator.ValidateAsync(Karta);
        foreach (var b in wynik.Errors)
            ModelState.AddModelError($"Karta.{b.PropertyName}", b.ErrorMessage);

        if (!ModelState.IsValid && akcja != "Wstecz")
        {
            await PrzygotujAiAsync();
            return Page();
        }

        await ZapiszAsync();
        KomunikatZapisu = $"Zapisano o {DateTime.Now:HH:mm}.";

        return akcja switch
        {
            "Dalej" when Krok < LacznieKrokow => RedirectToPage(new { id = Id, krok = Krok + 1 }),
            "Wstecz" when Krok > 1 => RedirectToPage(new { id = Id, krok = Krok - 1 }),
            "Wyslij" => await WyslijDoWeryfikacjiAsync(),
            _ => RedirectToPage(new { id = Id, krok = Krok })
        };
    }

    private async Task<bool> WczytajAsync(bool dolaczBinding = false)
    {
        var idea = await _db.Ideas.Include(i => i.Karta).FirstOrDefaultAsync(i => i.Id == Id);
        if (idea?.Karta is null) return false;
        Idea = idea;

        if (dolaczBinding)
        {
            PrzepiszPolaBiezacegoKroku(idea.Karta, Karta);
            Karta = idea.Karta;
        }
        else
        {
            Karta = idea.Karta;
        }
        return true;
    }

    private void PrzepiszPolaBiezacegoKroku(IdeaCard cel, IdeaCard z)
    {
        switch (Krok)
        {
            case 1:
                cel.Tytul = z.Tytul ?? "";
                cel.Streszczenie = z.Streszczenie;
                break;
            case 2:
                cel.Problem = z.Problem;
                cel.GrupaDocelowa = z.GrupaDocelowa;
                break;
            case 3:
                cel.Rozwiazanie = z.Rozwiazanie;
                cel.SposobDzialania = z.SposobDzialania;
                break;
            case 4:
                cel.Rezultaty = z.Rezultaty;
                cel.Obszar = z.Obszar;
                break;
            case 5:
                cel.Zasoby = z.Zasoby;
                cel.PosiadaneZasoby = z.PosiadaneZasoby;
                break;
            case 6:
                cel.Ryzyka = z.Ryzyka;
                cel.Tagi = z.Tagi ?? "";
                break;
            case 7:
                cel.PodsumowanieAI = z.PodsumowanieAI;
                break;
        }
        cel.AktualizowanoUtc = DateTime.UtcNow;
    }

    private async Task ZapiszAsync()
    {
        await _db.SaveChangesAsync();
    }

    private async Task<IActionResult> WyslijDoWeryfikacjiAsync()
    {
        var komplet = _checker.Check(Karta);
        if (!komplet.CanSubmit)
        {
            Kompletnosc = komplet;
            ModelState.AddModelError(string.Empty, "Fiszka nie jest kompletna — uzupełnij brakujące pola przed wysłaniem.");
            await PrzygotujAiAsync();
            return Page();
        }

        Karta.Status = StatusFiszki.Przekazany;
        Idea.HistoriaEtapow.Add(new IdeaStageHistory
        {
            IdeaId = Idea.Id,
            Etap = Idea.EtapInnowacji,
            Notatka = "Fiszka wysłana do weryfikacji przez ROPS."
        });
        _db.AuditLog.Add(new AuditLogEntry
        {
            UzytkownikId = _auth.GetUser()?.Id,
            Akcja = "FISZKA_PRZEKAZANA",
            EncjaTyp = nameof(IdeaCard),
            EncjaId = Karta.Id
        });
        await _db.SaveChangesAsync();
        return RedirectToPage("/Kreator/Podglad", new { id = Idea.Id, wyslana = true });
    }

    private async Task PrzygotujAiAsync()
    {
        Sugestia = Krok switch
        {
            1 => await _ai.SuggestClarificationAsync("tytuł", Karta.Tytul),
            2 => await _ai.SuggestClarificationAsync("problem", Karta.Problem ?? ""),
            3 => await _ai.ProposeStructureAsync(Idea.Rodzaj, Karta.Problem ?? ""),
            4 => await _ai.SuggestClarificationAsync("rezultaty", Karta.Rezultaty ?? ""),
            5 => await _ai.SuggestClarificationAsync("zasoby", Karta.Zasoby ?? ""),
            6 => await _ai.IdentifyRisksAsync(Karta),
            7 => await _ai.SummarizeNeedsAsync(Karta),
            _ => null
        };
    }
}
