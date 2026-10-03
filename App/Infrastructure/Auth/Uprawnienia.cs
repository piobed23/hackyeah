using App.Infrastructure.Enums;

namespace App.Infrastructure.Auth;

/// <summary>
/// Jedno źródło prawdy o tym, co może która rola. Używane przez menu, strony i handlery,
/// dzięki czemu to, co ukryte w interfejsie, jest też zablokowane po stronie serwera.
/// Admin widzi wszystko.
/// </summary>
public static class Uprawnienia
{
    public static bool MozeZglaszacProblem(RolaUzytkownika r) =>
        r is RolaUzytkownika.Mieszkaniec or RolaUzytkownika.Autor or RolaUzytkownika.Instytucja or RolaUzytkownika.Admin;

    public static bool MozeGlosowacIKomentowac(RolaUzytkownika r) => r != RolaUzytkownika.PracownikROPS;

    public static bool MozeSkladacOferte(RolaUzytkownika r) =>
        r is RolaUzytkownika.Instytucja or RolaUzytkownika.Autor or RolaUzytkownika.Admin;

    public static bool MozeDostosowacInnowacje(RolaUzytkownika r) =>
        r is RolaUzytkownika.Instytucja or RolaUzytkownika.Admin;

    public static bool MozeProponowacMaterial(RolaUzytkownika r) => r != RolaUzytkownika.PracownikROPS;

    public static bool MozeTworzycPomysly(RolaUzytkownika r) =>
        r is RolaUzytkownika.Autor or RolaUzytkownika.Admin;

    public static bool MozeSkladacWnioski(RolaUzytkownika r) =>
        r is RolaUzytkownika.Autor or RolaUzytkownika.Instytucja or RolaUzytkownika.Admin;

    public static bool PracujeWRops(RolaUzytkownika r) =>
        r is RolaUzytkownika.PracownikROPS or RolaUzytkownika.Admin;

    public static bool JestEkspertem(RolaUzytkownika r) =>
        r is RolaUzytkownika.Ekspert or RolaUzytkownika.Admin;

    public static string Nazwa(RolaUzytkownika r) => r switch
    {
        RolaUzytkownika.Mieszkaniec => "Mieszkaniec",
        RolaUzytkownika.Autor => "Autor innowacji",
        RolaUzytkownika.Instytucja => "Instytucja",
        RolaUzytkownika.Ekspert => "Ekspert",
        RolaUzytkownika.PracownikROPS => "Pracownik ROPS",
        RolaUzytkownika.Admin => "Administrator",
        _ => r.ToString()
    };

    /// <summary>Jedno zdanie "co możesz tu robić" pokazywane pod nawigacją.</summary>
    public static string Opis(RolaUzytkownika r) => r switch
    {
        RolaUzytkownika.Mieszkaniec =>
            "Zgłaszasz problemy, sprawdzasz dopasowane rozwiązania, popierasz pomysły i zgłaszasz się jako tester.",
        RolaUzytkownika.Autor =>
            "Tworzysz pomysły w kreatorze, składasz wnioski w naborach i prowadzisz testy swoich innowacji.",
        RolaUzytkownika.Instytucja =>
            "Dostosowujesz sprawdzone innowacje do swojej gminy lub organizacji, składasz oferty współpracy i wnioski.",
        RolaUzytkownika.Ekspert =>
            "Oceniasz merytorycznie fiszki skierowane do konsultacji eksperckiej. Resztę platformy możesz przeglądać.",
        RolaUzytkownika.PracownikROPS =>
            "Weryfikujesz fiszki i materiały, zarządzasz naborami, oceniasz wnioski i zatwierdzasz sprawdzone innowacje.",
        RolaUzytkownika.Admin =>
            "Widzisz widoki wszystkich ról. W prawdziwym wdrożeniu to konto techniczne, nie robocze.",
        _ => ""
    };
}
