using App.Infrastructure.Enums;

namespace App.Models;

public record OswiadczenieItem(string Klucz, string Tresc, bool Wymagane = true);

public static class Oswiadczenia
{
    public static readonly IReadOnlyList<OswiadczenieItem> OsobaFizyczna = new[]
    {
        new OswiadczenieItem("miejsce_zam", "Posiadam miejsce zamieszkania na terenie Polski."),
        new OswiadczenieItem("zdolnosc", "Posiadam pełną zdolność do czynności prawnych."),
        new OswiadczenieItem("niekaralnosc", "Nie byłem/am skazany/a prawomocnym wyrokiem sądu za umyślne przestępstwo ścigane z oskarżenia publicznego lub umyślne przestępstwo skarbowe."),
        new OswiadczenieItem("art_207", "Nie jestem wykluczony/a z możliwości otrzymania środków europejskich na podstawie art. 207 ust. 4 ustawy o finansach publicznych."),
        new OswiadczenieItem("sankcje", "Nie podlegam wykluczeniu wynikającemu z sankcji w związku z agresją Federacji Rosyjskiej na Ukrainę."),
        new OswiadczenieItem("zobowiazania", "Nie zalegam z uiszczaniem podatków, opłat lub składek na ubezpieczenia społeczne lub zdrowotne."),
        new OswiadczenieItem("deklaracja", "Dobrowolnie deklaruję uczestnictwo w projekcie „Inkubator Włączenia Społecznego 2.0”."),
        new OswiadczenieItem("procedury", "Zapoznałem/am się z Procedurami realizacji projektu i akceptuję warunki w nich zawarte."),
        new OswiadczenieItem("prawda", "Dane zawarte w niniejszym formularzu są zgodne z prawdą."),
        new OswiadczenieItem("niezaleznosc", "Nie jestem zatrudniony/a w ROPS Kraków ani w INNOAGH, nie łączą mnie z pracownikami tych instytucji więzy rodzinne."),
        new OswiadczenieItem("niepowielanie", "Nie aplikuję równolegle o wsparcie na ten sam pomysł w innym projekcie FERS 5.1."),
        new OswiadczenieItem("niewdrozone", "Składana innowacja nie powiela standardowych form wsparcia ani innowacji już wdrożonych w PO KL, POWER, FERS i RPO."),
        new OswiadczenieItem("bez_oplat", "Nie będę pobierał/a wpłat i opłat od osób biorących udział w testowaniu."),
        new OswiadczenieItem("max_2", "W ramach naboru składam nie więcej niż 2 aplikacje."),
        new OswiadczenieItem("brak_wdrozenia", "Innowacja nie ma charakteru wdrożeniowego."),
        new OswiadczenieItem("udostepnienie", "Jestem świadomy/a, że formularz zostanie udostępniony Komisji Oceny i Radzie Innowacji Społecznych."),
        new OswiadczenieItem("rownosciowe", "Deklaruję stosowanie zasad równościowych, dostępności WCAG, równości kobiet i mężczyzn oraz DNSH."),
        new OswiadczenieItem("rodo_moje", "Potwierdzam wypełnienie wobec mnie obowiązku informacyjnego RODO."),
        new OswiadczenieItem("rodo_innych", "Wypełniłem/am obowiązki informacyjne RODO wobec osób fizycznych, których dane pozyskałem/am."),
    };

    public static readonly IReadOnlyList<OswiadczenieItem> Podmiot = new[]
    {
        new OswiadczenieItem("siedziba_pl", "Reprezentowany podmiot posiada siedzibę lub oddział na terenie Polski."),
        new OswiadczenieItem("niekaralnosc_rep", "Urzędujący członek organu zarządzającego/nadzorczego nie został skazany prawomocnym wyrokiem sądu."),
        new OswiadczenieItem("art_207_p", "Podmiot nie został wykluczony z możliwości otrzymania środków europejskich (art. 207 ust. 4)."),
        new OswiadczenieItem("sankcje_p", "Podmiot nie podlega wykluczeniu wynikającemu z sankcji w związku z agresją Federacji Rosyjskiej."),
        new OswiadczenieItem("zobowiazania_p", "Podmiot nie zalega z uiszczaniem podatków, opłat ani składek."),
        new OswiadczenieItem("niezaleznosc_p", "Wspólnicy ani członkowie organów nie są zatrudnieni w ROPS/INNOAGH."),
        new OswiadczenieItem("bezstronnosc", "Nie łączą mnie z personelem ROPS/INNOAGH więzy pokrewieństwa budzące wątpliwości co do bezstronności."),
        new OswiadczenieItem("nie_woj_mal", "Podmiot nie jest jednostką organizacyjną Województwa Małopolskiego."),
        new OswiadczenieItem("nie_agh", "Podmiot nie jest powiązany kapitałowo z AGH."),
        new OswiadczenieItem("deklaracja_p", "W imieniu podmiotu dobrowolnie deklaruję uczestnictwo w projekcie."),
        new OswiadczenieItem("procedury_p", "Zapoznałem/am się z Procedurami realizacji projektu."),
        new OswiadczenieItem("prawda_p", "Dane w formularzu są zgodne z prawdą."),
        new OswiadczenieItem("niepowielanie_p", "Podmiot nie aplikuje równolegle o wsparcie na ten sam pomysł."),
        new OswiadczenieItem("niewdrozone_p", "Składana innowacja nie powiela innowacji już wdrożonych."),
        new OswiadczenieItem("bez_oplat_p", "Podmiot nie będzie pobierał opłat od testerów."),
        new OswiadczenieItem("max_2_p", "Podmiot składa nie więcej niż 2 aplikacje."),
        new OswiadczenieItem("brak_wdrozenia_p", "Innowacja nie ma charakteru wdrożeniowego."),
        new OswiadczenieItem("udostepnienie_p", "Formularz zostanie udostępniony Komisji Oceny i Radzie Innowacji."),
        new OswiadczenieItem("rownosciowe_p", "Deklaruję stosowanie zasad równościowych i DNSH."),
        new OswiadczenieItem("rodo_moje_p", "Potwierdzam wypełnienie obowiązku informacyjnego RODO."),
        new OswiadczenieItem("rodo_innych_p", "Wypełniłem/am obowiązki informacyjne RODO wobec osób fizycznych."),
    };

    public static IReadOnlyList<OswiadczenieItem> DlaTypu(TypWnioskodawcy typ) => typ switch
    {
        TypWnioskodawcy.OsobaFizyczna => OsobaFizyczna,
        TypWnioskodawcy.Podmiot or TypWnioskodawcy.GrupaNieformalna => Podmiot,
        _ => OsobaFizyczna
    };
}
