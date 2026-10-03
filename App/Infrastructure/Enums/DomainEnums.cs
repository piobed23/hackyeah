namespace App.Infrastructure.Enums;

public enum RolaUzytkownika
{
    Autor,
    PracownikROPS,
    Mieszkaniec,
    Ekspert,
    Admin
}

public enum RodzajPomyslu
{
    Innowacja,
    DobraPraktyka,
    Mikroskala
}

public enum StatusFiszki
{
    Szkic,
    Przekazany,
    WymagaUzupelnienia,
    Zaakceptowany,
    Opublikowany,
    Archiwalny,
    KonsultacjaEkspercka,
    Odrzucony,
    PropozycjaPolaczenia
}

public enum StatusWniosku
{
    Szkic,
    Gotowy,
    Zlozony,
    KontrolaFormalna,
    OcenaMerytoryczna,
    Wybrany,
    Niewybrany
}

public enum EtapInnowacji
{
    Pomysl,
    PoszukujeFinansowania,
    FinansowaniePrzyznane,
    WPrzygotowaniu,
    WRealizacji,
    PoszukujeTesterow,
    WTestach,
    WPoprawie,
    TestyZakonczone,
    SprawdzonaInnowacja
}

public enum TypDecyzjiROPS
{
    Akceptuj,
    DoPoprawy,
    Polacz,
    Odrzuc,
    DoEksperta
}

public enum StatusSesjiTestowej
{
    NieRozpoczeto,
    PoszukujeTesterow,
    WTrakcie,
    WPoprawie,
    Zakonczone,
    Zatwierdzone
}

public enum StatusTestera
{
    Nowe,
    Przyjety,
    Odrzucony,
    TestWykonany,
    OpiniaPrzeslana
}

public enum TypWnioskodawcy
{
    OsobaFizyczna,
    Podmiot,
    GrupaNieformalna
}

public enum OkresPlanu
{
    Przygotowawczy,
    TestFaza1,
    TestFaza2
}
