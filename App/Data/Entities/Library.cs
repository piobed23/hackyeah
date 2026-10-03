using System.ComponentModel.DataAnnotations;
using App.Infrastructure.Enums;

namespace App.Data.Entities;

public class LibraryItem
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Tytul { get; set; } = "";

    public TypMaterialu Typ { get; set; }

    public StatusMaterialu Status { get; set; } = StatusMaterialu.Szkic;

    [MaxLength(500)]
    public string Obszary { get; set; } = "";

    [Required, MaxLength(500)]
    public string KrotkiOpis { get; set; } = "";

    [MaxLength(4000)]
    public string? OpisPelny { get; set; }

    [MaxLength(500)]
    public string? GrupaOdbiorcow { get; set; }

    [MaxLength(200)]
    public string? AutorNazwa { get; set; }

    [MaxLength(300)]
    public string? Zrodlo { get; set; }

    [MaxLength(500)]
    public string? Link { get; set; }

    [MaxLength(500)]
    public string Tagi { get; set; } = "";

    [MaxLength(200)]
    public string? ZasiegGeograficzny { get; set; }

    [MaxLength(500)]
    public string? UrlGrafiki { get; set; }

    public bool PotwierdzeniePraw { get; set; }
    public bool AutorPubliczny { get; set; }

    [MaxLength(200)]
    public string? Licencja { get; set; }

    public int ZglaszajacyUserId { get; set; }
    public User ZglaszajacyUser { get; set; } = null!;

    public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;
    public DateTime AktualizowanoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? OpublikowanoUtc { get; set; }

    public int? ZastapionyPrzezId { get; set; }
    public LibraryItem? ZastapionyPrzez { get; set; }

    public int? DuplikatOfId { get; set; }
    public LibraryItem? DuplikatOf { get; set; }

    public List<LibraryItemChangeProposal> PropozycjeZmian { get; set; } = new();
}

public class LibraryItemChangeProposal
{
    public int Id { get; set; }

    public int LibraryItemId { get; set; }
    public LibraryItem LibraryItem { get; set; } = null!;

    public int ProponujacyUserId { get; set; }
    public User ProponujacyUser { get; set; } = null!;

    public StatusPropozycjiZmiany Status { get; set; } = StatusPropozycjiZmiany.Oczekujaca;

    [MaxLength(200)]
    public string? NowyTytul { get; set; }

    [MaxLength(500)]
    public string? NowyKrotkiOpis { get; set; }

    [MaxLength(4000)]
    public string? NowyOpisPelny { get; set; }

    [MaxLength(500)]
    public string? NoweTagi { get; set; }

    [MaxLength(500)]
    public string? NowyLink { get; set; }

    [Required, MaxLength(1000)]
    public string Uzasadnienie { get; set; } = "";

    public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RozpatrzonoUtc { get; set; }

    [MaxLength(1000)]
    public string? OdpowiedzRops { get; set; }
}
