using System.Text;
using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Models;

namespace App.Services.Middleman;

public class MockMiddlemanService : IMiddlemanService
{
    public Task<AnalizaDopasowania> AnalizujAsync(Idea innowacja, ImplementationCard dane)
    {
        var dostepne = new List<string>();
        var brakuje = new List<string>();

        if (!string.IsNullOrWhiteSpace(dane.DostepnyPersonel))
            dostepne.Add($"Personel: {dane.DostepnyPersonel}");
        else
            brakuje.Add("Brak informacji o dostępnym personelu");

        if (!string.IsNullOrWhiteSpace(dane.Zasoby))
            dostepne.Add($"Zasoby: {dane.Zasoby}");
        else
            brakuje.Add("Brak informacji o posiadanych zasobach (miejsce, sprzęt)");

        if (dane.Budzet >= 10_000)
            dostepne.Add($"Budżet pokrywa podstawowe koszty: {dane.Budzet:C}");
        else if (dane.Budzet > 0)
            brakuje.Add($"Budżet {dane.Budzet:C} może być niewystarczający — rozważ wariant minimalny");
        else
            brakuje.Add("Nie podano budżetu");

        if (dane.LiczbaOdbiorcow > 0)
            dostepne.Add($"Zidentyfikowana grupa odbiorców: {dane.LiczbaOdbiorcow} osób");
        else
            brakuje.Add("Nie określono skali docelowej (liczby odbiorców)");

        var tagi = (innowacja.Karta?.Tagi ?? "").ToLowerInvariant();
        if (tagi.Contains("senior") && !(dane.DostepnyPersonel?.ToLowerInvariant().Contains("wolontar") ?? false))
            brakuje.Add("Rozwiązania dla seniorów często wymagają wolontariuszy — brak w personelu");
        if (tagi.Contains("zdrowie") && !(dane.Zasoby?.ToLowerInvariant().Contains("medyczn") ?? false))
            brakuje.Add("Rozwiązanie zdrowotne — rozważ partnera medycznego");
        if (!string.IsNullOrWhiteSpace(dane.PotrzebyDostepnosci))
            dostepne.Add($"Zidentyfikowane potrzeby dostępności: {dane.PotrzebyDostepnosci}");

        return Task.FromResult(new AnalizaDopasowania(dostepne, brakuje));
    }

    public Task<IReadOnlyList<WariantPropozycja>> GenerujWariantyAsync(Idea innowacja, ImplementationCard dane)
    {
        var baza = dane.LiczbaOdbiorcow > 0 ? dane.LiczbaOdbiorcow : 40;
        var budzet = dane.Budzet > 0 ? dane.Budzet : 20_000m;
        var tagi = (innowacja.Karta?.Tagi ?? "").ToLowerInvariant();

        var warianty = new List<WariantPropozycja>
        {
            new(
                WariantWdrozenia.Minimalny,
                "Pilotaż z obecnymi zasobami",
                Math.Max(5, baza / 2),
                dane.DostepnyPersonel ?? "obecny zespół, bez dodatkowych rekrutacji",
                "2 miesiące",
                Math.Min(budzet, budzet * 0.5m),
                new[]
                {
                    "Najmniejsza możliwa skala — pilotaż na obecnej grupie",
                    "Brak dodatkowych partnerstw",
                    "Prosty harmonogram, jedna faza",
                    "Zachowanie kluczowego mechanizmu innowacji"
                },
                new[]
                {
                    "Szybki start, niski koszt wejścia",
                    "Mało ryzykowny — ograniczona grupa odbiorców",
                    "Łatwy do rozliczenia"
                },
                new[]
                {
                    "Niska liczba danych do oceny skuteczności",
                    "Trudno uogólnić wyniki"
                }
            ),

            new(
                WariantWdrozenia.Rekomendowany,
                "Pełna adaptacja innowacji",
                baza,
                $"{dane.DostepnyPersonel ?? "obecny zespół"} + 1-2 nowe osoby lub partnerstwo",
                "3-4 miesiące",
                budzet,
                new[]
                {
                    "Zachowanie wszystkich kluczowych elementów oryginalnej innowacji",
                    "Realistyczna skala dla organizacji wielkości wnioskodawcy",
                    "Szkolenie zespołu w ramach etapu przygotowawczego",
                    "Partnerstwo z lokalną organizacją branżową",
                    "Dokumentowanie wyników do raportu"
                },
                new[]
                {
                    "Zgodność z oryginalną innowacją = replikowalny efekt",
                    "Wystarczająca liczba odbiorców do walidacji",
                    "Realny plan testowania"
                },
                new[]
                {
                    "Wymaga znalezienia 1-2 partnerów",
                    "Ryzyko opóźnień przy rekrutacji nowych wolontariuszy"
                }
            ),

            new(
                WariantWdrozenia.Rozszerzony,
                "Skalowanie dla regionu",
                baza * 3,
                $"{dane.DostepnyPersonel ?? "obecny zespół"} + koordynator na część etatu + 10-20 wolontariuszy",
                "6 miesięcy",
                budzet * 2.5m,
                new[]
                {
                    "Zwiększona skala — kilka lokalizacji",
                    "Koordynator projektu zatrudniony na część etatu",
                    "Rozszerzone badanie rezultatów (badania ankietowe, wywiady)",
                    "Partnerstwa międzyinstytucjonalne",
                    "Opracowanie podręcznika replikacji dla innych organizacji"
                },
                new[]
                {
                    "Realny wpływ społeczny w skali regionu",
                    "Potencjał dalszego skalowania",
                    "Dokumentacja do upowszechnienia"
                },
                new[]
                {
                    "Wyższy budżet — konieczność finansowania zewnętrznego",
                    "Większa złożoność zarządzania projektem",
                    "Dłuższy czas przygotowania"
                }
            )
        };

        return Task.FromResult<IReadOnlyList<WariantPropozycja>>(warianty);
    }

    public Task<string> GenerujKarteWdrozeniaMdAsync(Idea innowacja, ImplementationCard dane, WariantPropozycja w)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Karta wdrożenia — {innowacja.Karta?.Tytul}");
        sb.AppendLine();
        sb.AppendLine($"**Instytucja wdrażająca:** {dane.NazwaInstytucji} ({dane.TypInstytucji})");
        if (!string.IsNullOrWhiteSpace(dane.Lokalizacja)) sb.AppendLine($"**Lokalizacja:** {dane.Lokalizacja}");
        sb.AppendLine($"**Wybrany wariant:** {w.Nazwa}");
        sb.AppendLine();

        sb.AppendLine("## Problem i grupa odbiorców");
        sb.AppendLine(innowacja.Karta?.Problem ?? "—");
        sb.AppendLine();
        sb.AppendLine($"**Grupa docelowa:** {dane.OpisPotrzeby ?? innowacja.Karta?.GrupaDocelowa}");
        sb.AppendLine($"**Skala:** {w.LiczbaOdbiorcow} odbiorców");
        sb.AppendLine();

        sb.AppendLine("## Dostosowany sposób działania");
        foreach (var el in w.KluczoweElementy) sb.AppendLine($"- {el}");
        sb.AppendLine();

        sb.AppendLine("## Role i zasoby");
        sb.AppendLine($"**Zespół:** {w.Zespol}");
        if (!string.IsNullOrWhiteSpace(dane.Zasoby)) sb.AppendLine($"**Posiadane zasoby:** {dane.Zasoby}");
        sb.AppendLine();

        sb.AppendLine("## Harmonogram");
        sb.AppendLine($"**Czas realizacji:** {w.Czas}");
        if (dane.PlanowanyStart is not null) sb.AppendLine($"**Planowany start:** {dane.PlanowanyStart:yyyy-MM-dd}");
        sb.AppendLine();

        sb.AppendLine("## Budżet");
        sb.AppendLine($"**Szacowany koszt wariantu:** {w.Koszt:C}");
        sb.AppendLine($"**Deklarowany budżet instytucji:** {dane.Budzet:C}");
        if (w.Koszt > dane.Budzet)
            sb.AppendLine($"⚠ Różnica {(w.Koszt - dane.Budzet):C} — rozważ nabór lub partnerstwo finansowe.");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(dane.PotrzebyDostepnosci))
        {
            sb.AppendLine("## Dostępność");
            sb.AppendLine(dane.PotrzebyDostepnosci);
            sb.AppendLine();
        }

        sb.AppendLine("## Ryzyka i rekomendacje");
        foreach (var r in w.Ryzyka) sb.AppendLine($"- {r}");
        sb.AppendLine();

        sb.AppendLine("## Mierniki");
        sb.AppendLine($"- Liczba odbiorców objętych usługą: {w.LiczbaOdbiorcow}");
        sb.AppendLine("- Satysfakcja odbiorców (ankieta po usłudze)");
        sb.AppendLine("- Udział wolontariuszy / partnerów");
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine("_Plan przygotowany przy wsparciu AI i zatwierdzony przez instytucję. Nie stanowi formalnego zatwierdzenia przez ROPS._");

        return Task.FromResult(sb.ToString());
    }

    public Task<IReadOnlyList<string>> WykryjBrakujacychPartnerowAsync(Idea innowacja, ImplementationCard dane)
    {
        var brakuje = new List<string>();
        var tagi = (innowacja.Karta?.Tagi ?? "").ToLowerInvariant();
        var personel = (dane.DostepnyPersonel ?? "").ToLowerInvariant();
        var zasoby = (dane.Zasoby ?? "").ToLowerInvariant();

        if (tagi.Contains("senior") && !personel.Contains("wolontar"))
            brakuje.Add("Organizacja dostarczająca wolontariuszy do pracy z seniorami");
        if (tagi.Contains("zdrowie") && !personel.Contains("lekarz") && !personel.Contains("psycholog"))
            brakuje.Add("Partner medyczny lub psycholog (konsultacje merytoryczne)");
        if (tagi.Contains("młodzie") && !personel.Contains("pedagog"))
            brakuje.Add("Pedagog lub organizacja pracująca z młodzieżą");
        if (tagi.Contains("ubóst") || tagi.Contains("społeczn"))
            brakuje.Add("Współpraca z OPS lub lokalną parafią (dotarcie do grupy)");
        if (tagi.Contains("edukac") && !zasoby.Contains("szkoł"))
            brakuje.Add("Partnerstwo ze szkołą lub placówką edukacyjną");
        if (dane.LiczbaOdbiorcow > 50 && !zasoby.Contains("lokal") && !zasoby.Contains("sala"))
            brakuje.Add("Miejsce do spotkań / testów dla większej grupy");
        if (dane.Budzet < 20_000)
            brakuje.Add("Partner współfinansujący lub nabór zewnętrzny");

        if (brakuje.Count == 0)
            brakuje.Add("Nie wykryto krytycznych braków partnerstw dla wybranego wariantu");

        return Task.FromResult<IReadOnlyList<string>>(brakuje);
    }
}
