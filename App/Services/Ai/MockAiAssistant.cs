using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Models;

namespace App.Services.Ai;

public class MockAiAssistant : IAiAssistant
{
    private const string Preambula = "Propozycja asystenta (demo)";

    private static readonly Dictionary<string, string[]> KlauzuleRyzyk = new()
    {
        ["seniorzy"] = new[]
        {
            "Trudności z dotarciem do osób bardzo samotnych, bez sieci wsparcia",
            "Opór przed technologią — potrzebny instruktaż 1-na-1",
            "Konieczność ciągłego serwisu urządzeń (baterie, ładowarki)"
        },
        ["zdrowie"] = new[]
        {
            "Odpowiedzialność prawna za udzielanie informacji medycznych",
            "Ochrona danych wrażliwych (RODO, art. 9)",
            "Konieczność współpracy z personelem medycznym"
        },
        ["młodzież"] = new[]
        {
            "Rotacja uczestników, przerywanie udziału w trakcie",
            "Konieczność atrakcyjnej komunikacji przez kanały własne grupy",
            "Zgody opiekunów prawnych dla niepełnoletnich"
        },
        ["edukacja"] = new[]
        {
            "Dopasowanie do podstawy programowej / wymagań szkoły",
            "Dostępność dla uczniów ze specjalnymi potrzebami",
            "Zaangażowanie nauczycieli jako partnerów"
        },
        ["ubóstwo"] = new[]
        {
            "Stygmatyzacja uczestników — warto zaprojektować dyskretny dostęp",
            "Nieprzewidywalność potrzeb — konieczny bufor zasobów",
            "Współpraca z OPS i parafiami kluczowa dla dotarcia"
        },
        ["ekologia"] = new[]
        {
            "Trwałość rozwiązania po zakończeniu finansowania",
            "Dokumentowanie realnego wpływu — potrzebne wskaźniki",
            "Współpraca z gminą dla logistyki"
        }
    };

    private static readonly string[] RyzykaOgolne =
    {
        "Niska frekwencja na początku — zarezerwuj czas na promocję",
        "Potrzeba partnera dysponującego przestrzenią",
        "Zaplanuj mechanizm informacji zwrotnej od uczestników",
        "Zabezpiecz dane osobowe uczestników zgodnie z RODO"
    };

    public Task<AiSuggestion> SuggestClarificationAsync(string pole, string tresc)
    {
        tresc ??= "";
        var punkty = new List<string>();

        if (tresc.Length < 40)
            punkty.Add($"Pole '{pole}' wygląda na zbyt krótkie — rozwiń o 2-3 zdania.");
        if (!tresc.Contains(' '))
            punkty.Add("Zapisz pełnym zdaniem, nie hasłowo — czytelnicy spoza projektu nie zrozumieją kontekstu.");

        switch (pole.ToLowerInvariant())
        {
            case "problem":
                punkty.Add("Opisz, kogo problem dotyczy liczbowo (ile osób, w jakim regionie).");
                punkty.Add("Podaj źródło informacji o problemie — raport, obserwacja, rozmowy, dane GUS.");
                punkty.Add("Rozdziel symptomy od przyczyn — ułatwi to zaprojektowanie rozwiązania.");
                break;
            case "rozwiazanie":
            case "rozwiązanie":
                punkty.Add("Opisz pierwszy kontakt odbiorcy z rozwiązaniem — jak się dowie i jak zacznie.");
                punkty.Add("Wyjaśnij, co odróżnia Twoje rozwiązanie od istniejących prób.");
                punkty.Add("Wskaż minimalną wersję — co można uruchomić w 30 dni, a co jest docelowe.");
                break;
            case "grupadocelowa":
            case "grupa docelowa":
                punkty.Add("Oszacuj liczbę odbiorców pilotażu i docelowo.");
                punkty.Add("Dopisz segment demograficzny (wiek, miejsce zamieszkania, sytuacja życiowa).");
                punkty.Add("Zidentyfikuj bariery dostępu — język, mobilność, technologia, finanse.");
                break;
            case "rezultaty":
                punkty.Add("Dla każdego celu dopisz wskaźnik mierzalny (liczba, procent, czas).");
                punkty.Add("Rozdziel rezultaty twarde (frekwencja) od miękkich (zmiana postaw).");
                punkty.Add("Opisz, jak zmierzysz rezultaty i kto za to odpowiada.");
                break;
            case "zasoby":
                punkty.Add("Rozbij zasoby na ludzkie, rzeczowe i finansowe.");
                punkty.Add("Zaznacz, co już masz (partnerzy, lokal), a czego szukasz.");
                break;
            case "ryzyka":
                punkty.Add("Dla każdego ryzyka opisz działanie zapobiegawcze i plan B.");
                punkty.Add("Pamiętaj o ryzykach technicznych, prawnych i wizerunkowych.");
                break;
        }

        if (punkty.Count == 0)
            punkty.Add($"Zawartość pola '{pole}' wygląda sensownie — rozważ dodanie konkretnego przykładu.");

        return Task.FromResult(new AiSuggestion(Preambula, punkty, pole));
    }

    public Task<AiSuggestion> IdentifyRisksAsync(IdeaCard karta)
    {
        var tagi = (karta.Tagi ?? "").ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var punkty = new List<string>();

        foreach (var tag in tagi)
        {
            foreach (var (klucz, ryzyka) in KlauzuleRyzyk)
            {
                if (tag.Contains(klucz) || klucz.Contains(tag))
                {
                    punkty.AddRange(ryzyka);
                    break;
                }
            }
        }

        if (punkty.Count < 3)
        {
            var seed = Math.Abs((karta.Tytul + karta.Problem).GetHashCode());
            var rnd = new Random(seed);
            foreach (var r in RyzykaOgolne.OrderBy(_ => rnd.Next()).Take(3 - punkty.Count))
                punkty.Add(r);
        }

        return Task.FromResult(new AiSuggestion(Preambula, punkty.Take(4).ToList(), "Ryzyka"));
    }

    public Task<AiSuggestion> SummarizeNeedsAsync(IdeaCard karta)
    {
        var grupa = string.IsNullOrWhiteSpace(karta.GrupaDocelowa) ? "wskazana grupa odbiorców" : karta.GrupaDocelowa.Trim();
        var problem = string.IsNullOrWhiteSpace(karta.Problem) ? "opisany problem" : karta.Problem.Trim();
        var skrot = problem.Length > 120 ? problem[..120] + "…" : problem;

        var punkty = new List<string>
        {
            $"Grupa odbiorców: {grupa}.",
            $"Istota potrzeby: {skrot}",
            "Zaproponowany projekt odpowiada na realną potrzebę społeczną zdiagnozowaną w regionie."
        };

        return Task.FromResult(new AiSuggestion(Preambula, punkty, "Streszczenie"));
    }

    public Task<AiSuggestion> ProposeStructureAsync(RodzajPomyslu rodzaj, string problem)
    {
        var punkty = rodzaj switch
        {
            RodzajPomyslu.Innowacja => new List<string>
            {
                "1. Diagnoza problemu — co już wiemy, co trzeba sprawdzić",
                "2. Pierwsza wersja rozwiązania (prototyp) — do przetestowania z 10-20 osobami",
                "3. Pilotaż na większej grupie z pomiarem rezultatów",
                "4. Skalowanie: materiały dla innych gmin, otwarte zasoby"
            },
            RodzajPomyslu.DobraPraktyka => new List<string>
            {
                "1. Opis wdrożonego rozwiązania (gdzie działa, od kiedy, dla kogo)",
                "2. Wyniki — konkretne liczby i cytaty uczestników",
                "3. Kluczowe lekcje — co zadziałało, czego nie powtarzać",
                "4. Pakiet do adaptacji przez inne instytucje"
            },
            RodzajPomyslu.Mikroskala => new List<string>
            {
                "1. Krótki test rozwiązania z pojedynczą grupą (do 20 osób)",
                "2. 2-tygodniowy pomiar z protokołem obserwacji",
                "3. Decyzja: skaluj / popraw / zakończ",
                "4. Raport do publikacji w bibliotece"
            },
            _ => new List<string> { "Opisz etapy projektu po kolei." }
        };

        return Task.FromResult(new AiSuggestion(Preambula, punkty, "Sposób działania"));
    }

    public Task<IReadOnlyList<string>> SuggestTagsAsync(string tresc)
    {
        tresc = (tresc ?? "").ToLowerInvariant();
        var tagi = new List<string>();
        var slownik = new Dictionary<string, string>
        {
            ["senior"] = "seniorzy", ["starsz"] = "seniorzy", ["65+"] = "seniorzy",
            ["mlodzie"] = "młodzież", ["młodzie"] = "młodzież", ["neet"] = "NEET",
            ["dzieci"] = "dzieci", ["przedszkol"] = "dzieci",
            ["ubóst"] = "ubóstwo", ["bieda"] = "ubóstwo",
            ["niepełnosp"] = "niepełnosprawność", ["dostępn"] = "dostępność",
            ["zdrowi"] = "zdrowie", ["lekarz"] = "zdrowie",
            ["wie"] = "wieś", ["gmin"] = "wieś", ["sołect"] = "wieś",
            ["edukac"] = "edukacja", ["szkoł"] = "edukacja",
            ["ekolog"] = "ekologia", ["klimat"] = "ekologia",
            ["przemoc"] = "bezpieczeństwo", ["uzale"] = "uzależnienia",
            ["kultur"] = "kultura", ["sport"] = "sport",
            ["praca"] = "praca", ["zatrud"] = "praca"
        };

        foreach (var (fragment, tag) in slownik)
        {
            if (tresc.Contains(fragment) && !tagi.Contains(tag))
                tagi.Add(tag);
            if (tagi.Count >= 6) break;
        }

        if (tagi.Count == 0)
            tagi.AddRange(new[] { "społeczność", "lokalnie" });

        return Task.FromResult<IReadOnlyList<string>>(tagi);
    }

    public Task<AiSuggestion> DraftApplicationSectionAsync(IdeaCard karta, string sekcja)
    {
        var punkty = sekcja.ToLowerInvariant() switch
        {
            "uzasadnienie" => new List<string>
            {
                $"Potrzeba: {karta.Problem}",
                $"Grupa odbiorców: {karta.GrupaDocelowa}",
                "Argument liczbowy — wstaw dane statystyczne z regionu.",
                "Dotychczasowe próby i ich ograniczenia."
            },
            "cele" => new List<string>
            {
                "Cel główny — opis mierzalnego rezultatu końcowego.",
                "Cel 1 — frekwencja uczestników (ile osób).",
                "Cel 2 — rezultat jakościowy (zmiana w życiu uczestnika).",
                "Cel 3 — trwałość (co zostaje po projekcie)."
            },
            _ => new List<string>
            {
                "Opisz etap po etapie.",
                "Dodaj mierniki dla każdego etapu."
            }
        };

        return Task.FromResult(new AiSuggestion(Preambula, punkty, sekcja));
    }
}
