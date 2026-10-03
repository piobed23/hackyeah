using System.ComponentModel.DataAnnotations;
using App.Infrastructure.Enums;

namespace App.Data.Entities;

public class ImplementationCard
{
    public int Id { get; set; }

    public int IdeaId { get; set; }
    public Idea Idea { get; set; } = null!;

    public int InstytucjaUserId { get; set; }
    public User InstytucjaUser { get; set; } = null!;

    public StatusWdrozenia Status { get; set; } = StatusWdrozenia.Szkic;

    [Required, MaxLength(200)]
    public string NazwaInstytucji { get; set; } = "";

    [MaxLength(100)]
    public string TypInstytucji { get; set; } = "";

    [MaxLength(200)]
    public string? Lokalizacja { get; set; }

    public int LiczbaOdbiorcow { get; set; }

    public decimal Budzet { get; set; }

    [MaxLength(1000)]
    public string? DostepnyPersonel { get; set; }

    [MaxLength(1000)]
    public string? Zasoby { get; set; }

    [MaxLength(1000)]
    public string? PotrzebyDostepnosci { get; set; }

    [MaxLength(1000)]
    public string? Ograniczenia { get; set; }

    [MaxLength(2000)]
    public string? OpisPotrzeby { get; set; }

    public DateTime? PlanowanyStart { get; set; }

    public WariantWdrozenia? WybranyWariant { get; set; }

    public string? WariantyJson { get; set; }

    public string? KartaMd { get; set; }

    public string? BrakujacyPartnerzy { get; set; }

    // Prośba o konsultację autora innowacji
    [MaxLength(2000)]
    public string? PytanieDoAutora { get; set; }

    public DateTime? PytanieWyslaneUtc { get; set; }

    [MaxLength(4000)]
    public string? OdpowiedzAutora { get; set; }

    public DateTime? OdpowiedzUtc { get; set; }

    public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ZatwierdzonoUtc { get; set; }
}
