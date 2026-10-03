using System.ComponentModel.DataAnnotations;
using App.Infrastructure.Enums;

namespace App.Data.Entities;

public class Idea
{
    public int Id { get; set; }

    public int AutorId { get; set; }
    public User Autor { get; set; } = null!;

    public RodzajPomyslu Rodzaj { get; set; }

    public EtapInnowacji EtapInnowacji { get; set; } = EtapInnowacji.Pomysl;

    public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;

    public DateTime? OpublikowanoUtc { get; set; }

    public IdeaCard? Karta { get; set; }

    public List<IdeaStageHistory> HistoriaEtapow { get; set; } = new();
    public List<Application> Wnioski { get; set; } = new();
    public List<Comment> Komentarze { get; set; } = new();
    public List<Endorsement> Poparcia { get; set; } = new();
    public List<OrganizationOffer> OfertyOrganizacji { get; set; } = new();
    public List<ProblemReport> ZgloszeniaProblemow { get; set; } = new();
    public List<TestSession> SesjeTestowe { get; set; } = new();
}

public class IdeaCard
{
    public int Id { get; set; }

    public int IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public StatusFiszki Status { get; set; } = StatusFiszki.Szkic;

    [Required, MaxLength(200)]
    public string Tytul { get; set; } = "";

    [MaxLength(400)]
    public string? Streszczenie { get; set; }

    [MaxLength(4000)]
    public string? Problem { get; set; }

    [MaxLength(1000)]
    public string? GrupaDocelowa { get; set; }

    [MaxLength(4000)]
    public string? Rozwiazanie { get; set; }

    [MaxLength(2000)]
    public string? SposobDzialania { get; set; }

    [MaxLength(2000)]
    public string? Rezultaty { get; set; }

    [MaxLength(1000)]
    public string? Zasoby { get; set; }

    [MaxLength(1000)]
    public string? PosiadaneZasoby { get; set; }

    [MaxLength(2000)]
    public string? Ryzyka { get; set; }

    [MaxLength(500)]
    public string? Obszar { get; set; }

    [MaxLength(500)]
    public string Tagi { get; set; } = "";

    public string? WersjaRoboczaJson { get; set; }

    [MaxLength(2000)]
    public string? PodsumowanieAI { get; set; }

    public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;
    public DateTime AktualizowanoUtc { get; set; } = DateTime.UtcNow;
}

public class IdeaStageHistory
{
    public int Id { get; set; }

    public int IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public EtapInnowacji Etap { get; set; }

    public DateTime KiedyUtc { get; set; } = DateTime.UtcNow;

    public int? KtoId { get; set; }
    public User? Kto { get; set; }

    [MaxLength(1000)]
    public string? Notatka { get; set; }
}
