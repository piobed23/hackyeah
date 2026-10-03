using System.ComponentModel.DataAnnotations;
using App.Infrastructure.Enums;

namespace App.Data.Entities;

public class TestSession
{
    public int Id { get; set; }

    public int IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public StatusSesjiTestowej Status { get; set; } = StatusSesjiTestowej.NieRozpoczeto;

    [Required, MaxLength(2000)]
    public string CoBedzieTestowane { get; set; } = "";

    [MaxLength(1000)]
    public string KogoSzukamy { get; set; } = "";

    public int LiczbaTesterow { get; set; } = 5;

    public bool Zdalny { get; set; } = true;

    public DateTime? Termin { get; set; }

    [MaxLength(200)]
    public string? CzasTrwania { get; set; }

    [MaxLength(2000)]
    public string Instrukcja { get; set; } = "";

    [MaxLength(1000)]
    public string? Wymagania { get; set; }

    public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ZamknietoUtc { get; set; }

    public List<TesterApplication> Zgloszenia { get; set; } = new();
}

public class TesterApplication
{
    public int Id { get; set; }

    public int TestSessionId { get; set; }
    public TestSession TestSession { get; set; } = null!;

    public int UzytkownikId { get; set; }
    public User Uzytkownik { get; set; } = null!;

    public StatusTestera Status { get; set; } = StatusTestera.Nowe;

    [Required, MaxLength(120)]
    public string ImieLubProfil { get; set; } = "";

    [MaxLength(1000)]
    public string? Powod { get; set; }

    [MaxLength(200)]
    public string PreferowanyKontakt { get; set; } = "";

    [MaxLength(500)]
    public string? PotrzebneUdogodnienia { get; set; }

    public bool ZgodaNaPrzekazanieDanych { get; set; }

    [MaxLength(2000)]
    public string? WiadomoscInstrukcyjna { get; set; }

    public DateTime KiedyUtc { get; set; } = DateTime.UtcNow;

    public TesterFeedback? Opinia { get; set; }
}

public class TesterFeedback
{
    public int Id { get; set; }

    public int TesterApplicationId { get; set; }
    public TesterApplication TesterApplication { get; set; } = null!;

    public bool OdpowiadaNaProblem { get; set; }

    public int Latwosc { get; set; }

    [MaxLength(2000)]
    public string CoDzialaloDobrze { get; set; } = "";

    [MaxLength(2000)]
    public string DoPoprawy { get; set; } = "";

    public bool SkorzystalbysPonownie { get; set; }

    [MaxLength(1000)]
    public string? BariereDostepnosci { get; set; }

    public int OcenaOgolna { get; set; }

    public DateTime KiedyUtc { get; set; } = DateTime.UtcNow;
}
