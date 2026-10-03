using App.Data.Entities;
using App.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Seed;

public static class DemoSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync()) return;

        var rops = new User { Imie = "Katarzyna", Nazwisko = "Nowak", Email = "rops@demo", Rola = RolaUzytkownika.PracownikROPS, Organizacja = "ROPS Małopolska" };
        var autor1 = new User { Imie = "Anna", Nazwisko = "Kowalska", Email = "autor1@demo", Rola = RolaUzytkownika.Autor, Organizacja = "Fundacja Senior+" };
        var autor2 = new User { Imie = "Marek", Nazwisko = "Wiśniewski", Email = "autor2@demo", Rola = RolaUzytkownika.Autor };
        var mieszkaniec = new User { Imie = "Paweł", Nazwisko = "Zieliński", Email = "mieszkaniec@demo", Rola = RolaUzytkownika.Mieszkaniec };
        var ekspert = new User { Imie = "Dr Joanna", Nazwisko = "Lewandowska", Email = "ekspert@demo", Rola = RolaUzytkownika.Ekspert, Organizacja = "UJ, Instytut Socjologii" };
        var admin = new User { Imie = "Tomasz", Nazwisko = "Admin", Email = "admin@demo", Rola = RolaUzytkownika.Admin };
        var instytucja = new User
        {
            Imie = "Barbara",
            Nazwisko = "Dyrektor",
            Email = "cus@demo",
            Rola = RolaUzytkownika.Instytucja,
            Organizacja = "CUS Gmina Demo",
            TypInstytucji = "Centrum Usług Społecznych",
            Lokalizacja = "Gmina Demo, powiat myślenicki",
            ZasobyDomyslne = "1 koordynator, 5 wolontariuszy, 1 sala konferencyjna, budżet operacyjny",
            Zweryfikowana = true
        };

        db.Users.AddRange(rops, autor1, autor2, mieszkaniec, ekspert, admin, instytucja);
        await db.SaveChangesAsync();

        var nabor = new Call
        {
            Nazwa = "Małopolska Innowacyjna 2026",
            OrganizatorId = rops.Id,
            Opis = "Nabór na innowacje społeczne wspierające społeczności lokalne Małopolski. Priorytet dla rozwiązań dla seniorów, młodzieży NEET i osób z niepełnosprawnościami.",
            KryteriaMd = "- Innowacyjność rozwiązania\n- Potencjał skalowania\n- Jakość diagnozy problemu\n- Realność budżetu",
            DataOtwarcia = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            DataZamkniecia = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            BudzetMaks = 50_000m,
            ProcentWkladuWlasnego = 10m,
            Aktywny = true
        };
        db.Calls.Add(nabor);

        var teleopieka = new Idea
        {
            AutorId = autor1.Id,
            Rodzaj = RodzajPomyslu.Innowacja,
            EtapInnowacji = EtapInnowacji.SprawdzonaInnowacja,
            OpublikowanoUtc = DateTime.UtcNow.AddMonths(-6),
            Karta = new IdeaCard
            {
                Status = StatusFiszki.Opublikowany,
                Tytul = "Teleopieka dla seniorów",
                Streszczenie = "Opaska SOS + dyżur telefoniczny 24/7 dla seniorów samotnie mieszkających.",
                Problem = "Seniorzy mieszkający samotnie są narażeni na brak pomocy w sytuacji nagłego zdarzenia zdrowotnego.",
                GrupaDocelowa = "Osoby 65+ mieszkające samotnie na obszarach wiejskich.",
                Rozwiazanie = "Prosta opaska z przyciskiem SOS połączona z centrum dyżurnym. Operator kontaktuje się z rodziną, sąsiadem lub pogotowiem.",
                Rezultaty = "W pilotażu: 180 seniorów, 42 interwencje, 100% skuteczność kontaktu < 90 sekund.",
                Tagi = "seniorzy,zdrowie,bezpieczeństwo,wieś"
            }
        };

        var wymiana = new Idea
        {
            AutorId = autor2.Id,
            Rodzaj = RodzajPomyslu.DobraPraktyka,
            EtapInnowacji = EtapInnowacji.SprawdzonaInnowacja,
            OpublikowanoUtc = DateTime.UtcNow.AddMonths(-10),
            Karta = new IdeaCard
            {
                Status = StatusFiszki.Opublikowany,
                Tytul = "Punkt wymiany ubrań",
                Streszczenie = "Stały punkt bezpłatnej wymiany odzieży używanej w centrum miasteczka.",
                Problem = "Rodziny w trudnej sytuacji nie mają dostępu do odzieży sezonowej. Ubrania z szafy często nadają się do dalszego użytku.",
                GrupaDocelowa = "Rodziny o niskich dochodach, osoby w kryzysie.",
                Rozwiazanie = "Lokal udostępniony przez gminę, wolontariusze sortują i wydają ubrania bez formalności.",
                Rezultaty = "W Myślenicach: 2800 osób rocznie, 5 ton ubrań zamiast do odpadów.",
                Tagi = "ubóstwo,ekologia,społeczność,wolontariat"
            }
        };

        var mentoring = new Idea
        {
            AutorId = autor1.Id,
            Rodzaj = RodzajPomyslu.Innowacja,
            EtapInnowacji = EtapInnowacji.SprawdzonaInnowacja,
            OpublikowanoUtc = DateTime.UtcNow.AddMonths(-3),
            Karta = new IdeaCard
            {
                Status = StatusFiszki.Opublikowany,
                Tytul = "Mentoring młodych NEET",
                Streszczenie = "Program 1-na-1 łączący młodzież NEET z mentorami zawodowymi.",
                Problem = "Młodzi ludzie nie w zatrudnieniu i poza edukacją tracą motywację i kontakt ze światem pracy.",
                GrupaDocelowa = "Osoby 18-29 lat w statusie NEET.",
                Rozwiazanie = "Mentorzy wolontariusze z firm regionu, 1h/tydzień przez 6 miesięcy. Platforma dopasowuje branżami.",
                Rezultaty = "Pilotaż: 60 par, 42% uczestników podjęło pracę lub naukę w ciągu roku.",
                Tagi = "młodzież,NEET,praca,mentoring"
            }
        };

        db.Ideas.AddRange(teleopieka, wymiana, mentoring);

        db.ProblemReports.Add(new ProblemReport
        {
            MieszkaniecId = mieszkaniec.Id,
            Tytul = "Samotność starszych mieszkańców po wyprowadzce dzieci",
            Opis = "W mojej miejscowości jest wielu seniorów mieszkających samotnie. Brak kontaktów społecznych.",
            Lokalizacja = "Powiat limanowski",
            Tagi = "seniorzy,samotność,wieś"
        });
        db.ProblemReports.Add(new ProblemReport
        {
            MieszkaniecId = mieszkaniec.Id,
            Tytul = "Młodzi bez pracy i bez planu",
            Opis = "Znam kilku 20-latków, którzy po technikum nie podjęli ani pracy, ani studiów. Nikt ich nie wspiera.",
            Tagi = "młodzież,NEET,praca"
        });
        db.ProblemReports.Add(new ProblemReport
        {
            MieszkaniecId = mieszkaniec.Id,
            Tytul = "Rodziny nie mają ubrań zimowych dla dzieci",
            Opis = "W przedszkolu dzieci przychodzą zimą w nieodpowiednich kurtkach. Rodzice nie mają pieniędzy.",
            Tagi = "ubóstwo,dzieci"
        });

        db.LibraryItems.AddRange(
            new LibraryItem
            {
                Tytul = "Mapa Wyzwań Społecznych Małopolski 2026",
                Typ = TypMaterialu.Raport,
                Status = StatusMaterialu.Opublikowany,
                Obszary = "seniorzy,ubóstwo,zdrowie psychiczne,wykluczenie cyfrowe",
                KrotkiOpis = "Zbiorczy raport ROPS Kraków z diagnozą kluczowych wyzwań społecznych regionu.",
                OpisPelny = "Dokument stanowi podstawę do planowania działań w ramach FERS 5.1 i innych instrumentów. Zawiera dane statystyczne, analizę trendów oraz rekomendacje.",
                AutorNazwa = "Regionalny Ośrodek Polityki Społecznej w Krakowie",
                Zrodlo = "ROPS Kraków",
                Link = "https://rops.krakow.pl/",
                Tagi = "mapa,diagnoza,strategia,małopolska",
                ZasiegGeograficzny = "Województwo Małopolskie",
                PotwierdzeniePraw = true,
                AutorPubliczny = true,
                ZglaszajacyUserId = rops.Id,
                OpublikowanoUtc = DateTime.UtcNow.AddMonths(-2)
            },
            new LibraryItem
            {
                Tytul = "Canvas Innowacji Społecznej",
                Typ = TypMaterialu.CanvasSzablon,
                Status = StatusMaterialu.Opublikowany,
                Obszary = "metodologia",
                KrotkiOpis = "Szablon do uporządkowania pomysłu na innowację — jednostronicowy canvas.",
                OpisPelny = "Pomaga ustrukturyzować: problem, odbiorców, propozycję wartości, zasoby, kanały dotarcia, mierniki.",
                AutorNazwa = "INNOAGH",
                Zrodlo = "Krakowskie Centrum Innowacyjnych Technologii INNOAGH",
                Link = "https://innoagh.pl/",
                Tagi = "canvas,metodologia,prototypowanie",
                PotwierdzeniePraw = true,
                AutorPubliczny = true,
                ZglaszajacyUserId = rops.Id,
                OpublikowanoUtc = DateTime.UtcNow.AddMonths(-1)
            },
            new LibraryItem
            {
                Tytul = "Jak zbudować wolontariat senioralny — poradnik",
                Typ = TypMaterialu.Poradnik,
                Status = StatusMaterialu.OczekujeNaWeryfikacje,
                Obszary = "seniorzy,wolontariat",
                KrotkiOpis = "Krok po kroku: rekrutacja wolontariuszy, szkolenie, pierwszy miesiąc działania.",
                AutorNazwa = "Fundacja Senior+",
                Tagi = "wolontariat,seniorzy,rekrutacja",
                PotwierdzeniePraw = true,
                AutorPubliczny = true,
                ZglaszajacyUserId = autor1.Id
            }
        );

        await db.SaveChangesAsync();
    }
}
