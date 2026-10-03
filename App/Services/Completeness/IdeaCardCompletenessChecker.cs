using App.Data.Entities;
using App.Models;

namespace App.Services.Completeness;

public class IdeaCardCompletenessChecker : ICompletenessChecker<IdeaCard>
{
    private static readonly (string Pole, string Etykieta, int Waga, string Anchor)[] Pola =
    {
        ("Tytul",         "Tytuł",             5,  "krok1"),
        ("Streszczenie",  "Streszczenie",      5,  "krok1"),
        ("Problem",       "Opis problemu",     15, "krok2"),
        ("GrupaDocelowa", "Grupa docelowa",    10, "krok2"),
        ("Rozwiazanie",   "Rozwiązanie",       20, "krok3"),
        ("Rezultaty",     "Rezultaty",         15, "krok4"),
        ("Zasoby",        "Potrzebne zasoby",  10, "krok5"),
        ("Ryzyka",        "Ryzyka",            10, "krok6"),
        ("Tagi",          "Tagi",              5,  "krok6"),
        ("PodsumowanieAI","Podsumowanie",      5,  "krok7")
    };

    public CompletenessResult Check(IdeaCard k)
    {
        var braki = new List<MissingField>();
        int zdobyte = 0;

        foreach (var (pole, etykieta, waga, anchor) in Pola)
        {
            var wartosc = pole switch
            {
                "Tytul" => k.Tytul,
                "Streszczenie" => k.Streszczenie,
                "Problem" => k.Problem,
                "GrupaDocelowa" => k.GrupaDocelowa,
                "Rozwiazanie" => k.Rozwiazanie,
                "Rezultaty" => k.Rezultaty,
                "Zasoby" => k.Zasoby,
                "Ryzyka" => k.Ryzyka,
                "Tagi" => k.Tagi,
                "PodsumowanieAI" => k.PodsumowanieAI,
                _ => null
            };

            var minDlugosc = pole switch
            {
                "Tytul" => 5,
                "Tagi" => 3,
                "Streszczenie" => 20,
                _ => 30
            };

            if (!string.IsNullOrWhiteSpace(wartosc) && wartosc.Trim().Length >= minDlugosc)
            {
                zdobyte += waga;
            }
            else
            {
                var opis = string.IsNullOrWhiteSpace(wartosc)
                    ? "Pole puste — uzupełnij."
                    : $"Zbyt krótkie — min. {minDlugosc} znaków.";
                braki.Add(new MissingField(anchor, etykieta, opis));
            }
        }

        return new CompletenessResult(zdobyte, braki, braki.Count == 0);
    }
}
