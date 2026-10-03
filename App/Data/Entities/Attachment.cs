using System.ComponentModel.DataAnnotations;

namespace App.Data.Entities;

public class Attachment
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string WlascicielTyp { get; set; } = "";

    public int WlascicielId { get; set; }

    [Required, MaxLength(260)]
    public string NazwaPliku { get; set; } = "";

    public long Rozmiar { get; set; }

    [MaxLength(100)]
    public string MimeType { get; set; } = "application/octet-stream";

    public DateTime UtworzonoUtc { get; set; } = DateTime.UtcNow;
}
