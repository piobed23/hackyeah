using System.ComponentModel.DataAnnotations;
using App.Infrastructure.Enums;

namespace App.Data.Entities;

public class Call
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Nazwa { get; set; } = "";

    public int OrganizatorId { get; set; }
    public User Organizator { get; set; } = null!;

    [MaxLength(4000)]
    public string Opis { get; set; } = "";

    public string KryteriaMd { get; set; } = "";

    public DateTime DataOtwarcia { get; set; }
    public DateTime DataZamkniecia { get; set; }

    public decimal BudzetMaks { get; set; }

    public decimal ProcentWkladuWlasnego { get; set; }

    [MaxLength(500)]
    public string DozwoloneRodzaje { get; set; } = "Innowacja,DobraPraktyka,Mikroskala";

    public bool Aktywny { get; set; } = true;

    public List<Application> Wnioski { get; set; } = new();
}
