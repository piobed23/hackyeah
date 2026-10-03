using System.ComponentModel.DataAnnotations;
using App.Infrastructure.Enums;

namespace App.Data.Entities;

public class AuditLogEntry
{
    public int Id { get; set; }

    public DateTime KiedyUtc { get; set; } = DateTime.UtcNow;

    public int? UzytkownikId { get; set; }
    public User? Uzytkownik { get; set; }

    [Required, MaxLength(100)]
    public string Akcja { get; set; } = "";

    [Required, MaxLength(100)]
    public string EncjaTyp { get; set; } = "";

    public int EncjaId { get; set; }

    [MaxLength(4000)]
    public string? MetaJson { get; set; }
}

public class ReviewDecision
{
    public int Id { get; set; }

    public int? IdeaCardId { get; set; }
    public IdeaCard? IdeaCard { get; set; }

    public int? ApplicationId { get; set; }
    public Application? Application { get; set; }

    public int RecenzentId { get; set; }
    public User Recenzent { get; set; } = null!;

    public TypDecyzjiROPS Typ { get; set; }

    [MaxLength(2000)]
    public string Komentarz { get; set; } = "";

    public int? PolaczZIdeaId { get; set; }
    public Idea? PolaczZIdea { get; set; }

    public DateTime KiedyUtc { get; set; } = DateTime.UtcNow;
}
