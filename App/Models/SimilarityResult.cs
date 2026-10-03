using App.Infrastructure.Enums;

namespace App.Models;

public enum KategoriaPodobienstwa
{
    SprawdzonaInnowacja,
    PodobnyPomysl,
    TenSamProblem
}

public record SimilarityResult(
    int IdeaId,
    string Tytul,
    string? Streszczenie,
    double Wynik,
    KategoriaPodobienstwa Kategoria,
    RodzajPomyslu Rodzaj,
    EtapInnowacji Etap,
    IReadOnlyList<string> WspolneTagi
);
