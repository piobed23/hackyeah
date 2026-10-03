using System.ComponentModel.DataAnnotations;
using App.Infrastructure.Enums;

namespace App.Data.Entities;

public class Application
{
    public int Id { get; set; }

    public int IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public int CallId { get; set; }
    public Call Call { get; set; } = null!;

    public int AutorId { get; set; }
    public User Autor { get; set; } = null!;

    public StatusWniosku Status { get; set; } = StatusWniosku.Szkic;

    // 1. Tytuł innowacji
    [Required, MaxLength(200)]
    public string TytulProjektu { get; set; } = "";

    // 2. Dane pomysłodawcy
    public TypWnioskodawcy TypWnioskodawcy { get; set; } = TypWnioskodawcy.OsobaFizyczna;

    // Osoba fizyczna + Reprezentant podmiotu/grupy
    [MaxLength(80)] public string? Imie { get; set; }
    [MaxLength(80)] public string? Nazwisko { get; set; }
    [MaxLength(200)] public string? AdresKorespondencyjny { get; set; }
    [MaxLength(10)] public string? KodPocztowy { get; set; }
    [MaxLength(80)] public string? Miejscowosc { get; set; }
    [MaxLength(30)] public string? Telefon { get; set; }
    [MaxLength(160)] public string? Email { get; set; }

    // Podmiot
    [MaxLength(200)] public string? NazwaPodmiotu { get; set; }
    [MaxLength(30)] public string? KRS { get; set; }
    [MaxLength(30)] public string? REGON { get; set; }
    [MaxLength(30)] public string? NIP { get; set; }
    [MaxLength(200)] public string? OsobaReprezentujaca { get; set; }
    [MaxLength(100)] public string? FunkcjaReprezentujacego { get; set; }
    [MaxLength(200)] public string? OsobaKontaktowa { get; set; }
    [MaxLength(30)] public string? TelefonKontaktowy { get; set; }
    [MaxLength(160)] public string? EmailKontaktowy { get; set; }

    // Grupa nieformalna — partnerzy jako JSON (lista obiektów {Nazwa, Dane})
    [MaxLength(4000)]
    public string? PartnerzyJson { get; set; }

    // 3. Opis innowacji — na czym polega, jaki charakter, włączenie społeczne, deinstytucjonalizacja
    [MaxLength(4000)]
    public string? OpisInnowacji { get; set; }

    // 4. Innowacyjność rozwiązania
    [MaxLength(4000)]
    public string? Innowacyjnosc { get; set; }

    // 5. Diagnoza problemu
    [MaxLength(4000)]
    public string? DiagnozaProblemu { get; set; }

    [MaxLength(200)]
    public string? MapaWyzwanTemat { get; set; }

    // 6. Opis odbiorców innowacji
    [MaxLength(4000)]
    public string? OpisOdbiorcow { get; set; }

    // 7. Zmiana jaką wprowadza innowacja
    [MaxLength(4000)]
    public string? ZmianaJakaWprowadza { get; set; }

    // 8. Wizja przyszłości innowacji
    [MaxLength(4000)]
    public string? WizjaPrzyszlosci { get; set; }

    // 9. Plan działania i koszty — przez BudgetItem + ScheduleItem z pole Okres
    // 10. Wnioskowana kwota grantu
    public decimal WnioskowanaKwotaGrantu { get; set; }

    // 11. Zespół projektowy i doświadczenie
    [MaxLength(4000)]
    public string? ZespolDoswiadczenie { get; set; }

    // 12. Oświadczenia — JSON ze słownikiem { kluczOswiadczenia: bool }
    [MaxLength(4000)]
    public string? OswiadczeniaJson { get; set; }

    // Streszczenie (nie w formularzu wprost — do automatycznego podglądu / Moje)
    [MaxLength(1000)]
    public string? Streszczenie { get; set; }

    public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ZlozonoUtc { get; set; }

    [MaxLength(4000)]
    public string? WynikOceny { get; set; }

    public List<BudgetItem> Budzet { get; set; } = new();
    public List<ScheduleItem> Harmonogram { get; set; } = new();
}

public class BudgetItem
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;

    public OkresPlanu Okres { get; set; } = OkresPlanu.Przygotowawczy;

    [Required, MaxLength(100)]
    public string Kategoria { get; set; } = "";

    [MaxLength(500)]
    public string Opis { get; set; } = "";

    public int Jednostki { get; set; } = 1;

    public decimal KosztJednostkowy { get; set; }

    public decimal KwotaBrutto { get; set; }

    public decimal WkladWlasny { get; set; }

    [MaxLength(1000)]
    public string Uzasadnienie { get; set; } = "";

    public int Lp { get; set; }
}

public class ScheduleItem
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;

    public OkresPlanu Okres { get; set; } = OkresPlanu.Przygotowawczy;

    [Required, MaxLength(200)]
    public string NazwaZadania { get; set; } = "";

    public DateTime Od { get; set; }
    public DateTime Do { get; set; }

    [MaxLength(500)]
    public string Rezultat { get; set; } = "";

    public decimal KosztDzialania { get; set; }

    public int Lp { get; set; }
}
