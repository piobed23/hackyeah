using System.ComponentModel.DataAnnotations;

namespace App.Data.Entities;

public class Comment
{
    public int Id { get; set; }

    public int IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public int AutorId { get; set; }
    public User Autor { get; set; } = null!;

    [Required, MaxLength(2000)]
    public string Tresc { get; set; } = "";

    public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;

    public bool Widoczny { get; set; } = true;
}

public class Endorsement
{
    public int Id { get; set; }

    public int IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public int UzytkownikId { get; set; }
    public User Uzytkownik { get; set; } = null!;

    public DateTime KiedyUtc { get; set; } = DateTime.UtcNow;
}

public class OrganizationOffer
{
    public int Id { get; set; }

    public int IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public int OrganizacjaUserId { get; set; }
    public User OrganizacjaUser { get; set; } = null!;

    [Required, MaxLength(100)]
    public string TypOferty { get; set; } = "";

    [MaxLength(1000)]
    public string Opis { get; set; } = "";

    public DateTime KiedyUtc { get; set; } = DateTime.UtcNow;
}

public class ProblemReport
{
    public int Id { get; set; }

    public int MieszkaniecId { get; set; }
    public User Mieszkaniec { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Tytul { get; set; } = "";

    [Required, MaxLength(4000)]
    public string Opis { get; set; } = "";

    [MaxLength(200)]
    public string? Lokalizacja { get; set; }

    [MaxLength(500)]
    public string Tagi { get; set; } = "";

    [MaxLength(1000)]
    public string? KogoDotyczy { get; set; }

    [MaxLength(1000)]
    public string? Skutki { get; set; }

    [MaxLength(1000)]
    public string? CoProbowano { get; set; }

    [MaxLength(1000)]
    public string? OczekiwanyEfekt { get; set; }

    public App.Infrastructure.Enums.StatusProblemu Status { get; set; } = App.Infrastructure.Enums.StatusProblemu.Zgloszony;

    public int? PowiazanaIdeaId { get; set; }
    public Idea? PowiazanaIdea { get; set; }

    public bool PowiadamiajORozwiazaniu { get; set; }

    public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;
}
