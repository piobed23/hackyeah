using App.Infrastructure.Enums;

namespace App.Models;

public record WariantPropozycja(
    WariantWdrozenia Typ,
    string Nazwa,
    int LiczbaOdbiorcow,
    string Zespol,
    string Czas,
    decimal Koszt,
    IReadOnlyList<string> KluczoweElementy,
    IReadOnlyList<string> Zalety,
    IReadOnlyList<string> Ryzyka
);

public record AnalizaDopasowania(
    IReadOnlyList<string> Dostepne,
    IReadOnlyList<string> Brakuje
);
