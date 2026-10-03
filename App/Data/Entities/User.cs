using System.ComponentModel.DataAnnotations;
using App.Infrastructure.Enums;

namespace App.Data.Entities;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Imie { get; set; } = "";

    [Required, MaxLength(80)]
    public string Nazwisko { get; set; } = "";

    [Required, MaxLength(160), EmailAddress]
    public string Email { get; set; } = "";

    public RolaUzytkownika Rola { get; set; }

    [MaxLength(200)]
    public string? Organizacja { get; set; }
}
