using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Models;

namespace App.Services.Completeness;

public class ApplicationCompletenessChecker : ICompletenessChecker<Application>
{
    public CompletenessResult Check(Application a)
    {
        var braki = new List<MissingField>();
        int zdobyte = 0;

        void Sprawdz(string? wartosc, int waga, string anchor, string etykieta, int min = 30)
        {
            if (!string.IsNullOrWhiteSpace(wartosc) && wartosc.Trim().Length >= min) zdobyte += waga;
            else
            {
                var opis = string.IsNullOrWhiteSpace(wartosc) ? "Pole puste — uzupełnij." : $"Zbyt krótkie — min. {min} znaków.";
                braki.Add(new MissingField(anchor, etykieta, opis));
            }
        }

        // 1. Tytuł (5)
        Sprawdz(a.TytulProjektu, 5, "wnioskodawca", "Tytuł innowacji", 5);

        // 2. Dane wnioskodawcy (10)
        bool daneOk = a.TypWnioskodawcy switch
        {
            TypWnioskodawcy.OsobaFizyczna =>
                !string.IsNullOrWhiteSpace(a.Imie) && !string.IsNullOrWhiteSpace(a.Nazwisko)
                && !string.IsNullOrWhiteSpace(a.Email) && !string.IsNullOrWhiteSpace(a.Telefon),
            TypWnioskodawcy.Podmiot =>
                !string.IsNullOrWhiteSpace(a.NazwaPodmiotu) && !string.IsNullOrWhiteSpace(a.NIP)
                && !string.IsNullOrWhiteSpace(a.OsobaReprezentujaca) && !string.IsNullOrWhiteSpace(a.EmailKontaktowy),
            TypWnioskodawcy.GrupaNieformalna =>
                !string.IsNullOrWhiteSpace(a.PartnerzyJson) && !string.IsNullOrWhiteSpace(a.Imie),
            _ => false
        };
        if (daneOk) zdobyte += 10;
        else braki.Add(new MissingField("wnioskodawca", "Dane wnioskodawcy", "Uzupełnij dane kontaktowe wymagane dla wybranego typu wnioskodawcy."));

        // 3-8. Opisy merytoryczne (po ~10)
        Sprawdz(a.OpisInnowacji, 10, "opis", "3. Opis innowacji");
        Sprawdz(a.Innowacyjnosc, 8, "opis", "4. Innowacyjność rozwiązania");
        Sprawdz(a.DiagnozaProblemu, 10, "opis", "5. Diagnoza problemu");
        Sprawdz(a.OpisOdbiorcow, 8, "opis", "6. Opis odbiorców");
        Sprawdz(a.ZmianaJakaWprowadza, 8, "opis", "7. Zmiana jaką wprowadza");
        Sprawdz(a.WizjaPrzyszlosci, 6, "opis", "8. Wizja przyszłości");

        // 9. Plan działania (10)
        var harmPrzyg = a.Harmonogram.Any(h => h.Okres == OkresPlanu.Przygotowawczy);
        var harmTest = a.Harmonogram.Any(h => h.Okres == OkresPlanu.TestFaza1 || h.Okres == OkresPlanu.TestFaza2);
        if (harmPrzyg && harmTest) zdobyte += 10;
        else
        {
            if (!harmPrzyg) braki.Add(new MissingField("harmonogram", "Plan — okres przygotowawczy", "Dodaj co najmniej jedną pozycję w okresie przygotowawczym."));
            if (!harmTest) braki.Add(new MissingField("harmonogram", "Plan — testowanie", "Dodaj pozycje w fazie I lub II testowania."));
        }
        if (a.Call is not null)
        {
            if (a.Harmonogram.Any(h => h.Od < a.Call.DataOtwarcia || h.Do > a.Call.DataZamkniecia))
                braki.Add(new MissingField("harmonogram", "Harmonogram", "Przynajmniej jedna pozycja wychodzi poza okno naboru."));
            if (a.Harmonogram.Any(h => h.Do < h.Od))
                braki.Add(new MissingField("harmonogram", "Harmonogram", "Data końcowa pozycji jest wcześniejsza niż początkowa."));
            if (a.Harmonogram.Any(h => h.Okres == OkresPlanu.Przygotowawczy && (h.Do - h.Od).TotalDays > 92))
                braki.Add(new MissingField("harmonogram", "Okres przygotowawczy", "Okres przygotowawczy nie może przekraczać 3 miesięcy."));
            var czasTestow = a.Harmonogram.Where(h => h.Okres != OkresPlanu.Przygotowawczy).ToList();
            if (czasTestow.Count > 0)
            {
                var min = czasTestow.Min(h => h.Od);
                var max = czasTestow.Max(h => h.Do);
                if ((max - min).TotalDays > 275)
                    braki.Add(new MissingField("harmonogram", "Okres testowania", "Okres testowania nie może przekraczać 9 miesięcy."));
            }
        }

        // 10. Wnioskowana kwota (5)
        if (a.WnioskowanaKwotaGrantu > 0)
        {
            zdobyte += 5;
            if (a.Call is not null && a.WnioskowanaKwotaGrantu > a.Call.BudzetMaks)
                braki.Add(new MissingField("budzet", "Wnioskowana kwota", $"Kwota {a.WnioskowanaKwotaGrantu:C} przekracza limit naboru {a.Call.BudzetMaks:C}."));
        }
        else
        {
            braki.Add(new MissingField("budzet", "10. Wnioskowana kwota grantu", "Podaj całkowitą wnioskowaną kwotę."));
        }

        // Budżet szczegółowy (10)
        if (a.Budzet.Count > 0) zdobyte += 10;
        else braki.Add(new MissingField("budzet", "Budżet szczegółowy", "Dodaj pozycje budżetowe uzasadniające wnioskowaną kwotę."));

        // 11. Zespół (10)
        Sprawdz(a.ZespolDoswiadczenie, 10, "opis", "11. Zespół projektowy i doświadczenie");

        if (zdobyte > 100) zdobyte = 100;
        return new CompletenessResult(zdobyte, braki, braki.Count == 0);
    }
}
