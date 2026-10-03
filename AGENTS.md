# AGENTS.md — Instrukcja dla agentów AI

Krótki, obowiązujący zestaw zasad dla każdej automatycznej edycji w tym repozytorium. Czytaj w całości przed pierwszą zmianą w sesji.

## 1. Czym jest projekt

Projekt HackYeah. Szczegółowy temat i user stories są **do uzupełnienia** — jeśli nie znasz celu zadania, zapytaj użytkownika, nie zgaduj zakresu funkcjonalnego.

Co jest ustalone:
- Jedna aplikacja webowa w katalogu `App/`.
- Odbiorca końcowy: użytkownik przez przeglądarkę.
- Produkt musi być **dostępny cyfrowo zgodnie z WCAG 2.1 poziom AA** (patrz §5).

## 2. Stack i wersje

- **.NET 9** (`net9.0`), `Nullable` + `ImplicitUsings` włączone — nie wyłączaj.
- **ASP.NET Core Razor Pages** (`Microsoft.NET.Sdk.Web`). Nie wprowadzaj MVC Controllers, Blazora ani SPA bez zgody użytkownika.
- Zasoby statyczne w `App/wwwroot/` (CSS, JS, biblioteki w `lib/`). Nowe biblioteki frontendowe dodawaj przez `libman` lub ręcznie do `wwwroot/lib/` — nie wprowadzaj `npm`/`node_modules` bez uzgodnienia.
- Konfiguracja: `appsettings.json` (prod) i `appsettings.Development.json` (dev). Sekretów nie commituj — używaj `dotnet user-secrets`.

## 3. Struktura katalogów

```
hackyeah/
├─ App/
│  ├─ Pages/            # Razor Pages (.cshtml + .cshtml.cs PageModel)
│  │  └─ Shared/        # _Layout, partials, komponenty wspólne
│  ├─ wwwroot/          # pliki statyczne serwowane publicznie
│  ├─ Program.cs        # minimal hosting, konfiguracja pipeline
│  └─ App.csproj
├─ projekt.slnx
└─ README.md
```

Nowe foldery dodawaj świadomie. Rozsądne rozszerzenia: `App/Models/`, `App/Services/`, `App/Data/` — tworzyć tylko gdy pojawia się realna potrzeba (nie na zapas).

## 4. Konwencje kodu

- **Nazewnictwo**: `PascalCase` dla klas/metod/właściwości publicznych, `camelCase` dla parametrów i zmiennych lokalnych, `_camelCase` dla pól prywatnych.
- **PageModel per strona**: logika strony w `Xxx.cshtml.cs`, widok wyłącznie w `Xxx.cshtml`. Nie wrzucaj logiki do widoku poza prostym renderowaniem.
- **Walidacja wejścia**: `[BindProperty]` + atrybuty `DataAnnotations`, sprawdzaj `ModelState.IsValid`. Nigdy nie ufaj klientowi.
- **Null-safety**: nie wyłączaj `#nullable`. Używaj `?`, `required`, operatorów `??`/`?.`. Unikaj `!` (null-forgiving) poza przypadkami faktycznie udowodnionymi.
- **Async**: metody I/O oznaczaj `async Task`/`async Task<IActionResult>`, sufiks `Async`.
- **DI**: zależności wstrzykuj przez konstruktor PageModelu, nie przez `HttpContext.RequestServices`.
- **Bez komentarzy opisujących „co" robi kod** — nazwy mają mówić. Komentarz tylko gdy wyjaśnia *dlaczego* (nieoczywista decyzja, workaround).

## 5. Dostępność — WCAG 2.1 AA (wymaganie twarde)

Każda nowa strona/komponent przechodzi te checki, zanim uznasz zadanie za zrobione:

- **Semantyka HTML**: `<header>`, `<nav>`, `<main>`, `<footer>`, nagłówki `h1→h6` w kolejności, listy przez `<ul>/<ol>`, nie `<div>`-y. Jeden `<h1>` na stronę.
- **Formularze**: każde pole ma `<label for="...">` lub `aria-label`. Komunikaty błędów powiązane przez `aria-describedby`, nie tylko kolor.
- **Kontrast**: tekst normalny ≥ 4.5:1, duży tekst ≥ 3:1. Nie koduj informacji wyłącznie kolorem.
- **Klawiatura**: cała funkcjonalność osiągalna klawiszem Tab, widoczny focus (`:focus-visible`), bez „pułapek" fokusu.
- **Obrazki**: każdy `<img>` ma `alt` (pusty `alt=""` dla dekoracji).
- **Język**: `<html lang="pl">` (albo właściwy język podstrony).
- **Dynamiczne zmiany**: zmiany treści bez przeładowania ogłaszaj przez `aria-live`.
- **Zoom**: układ działa przy 200% zoomu i szerokości 320px.

Jeśli zmiana dotyka UI, w opisie zmiany wymień, jak została spełniona dostępność.

## 6. Praca z gitem

- Główna gałąź: `master`. Praca na gałęziach feature, PR do `master`.
- Commit message po polsku, imperatyw: „Dodaj formularz zgłoszeń", „Popraw kontrast przycisku". Opis *dlaczego*, nie *co*.
- Nie commituj `bin/`, `obj/`, `*.user`, sekretów. Jeśli widzisz, że są śledzone — zgłoś użytkownikowi, nie usuwaj po cichu.
- Nie rób `push --force`, `reset --hard`, nie amenduj cudzych commitów bez pytania.

## 7. Uruchamianie i weryfikacja

- Build: `dotnet build App/App.csproj`
- Dev: `dotnet run --project App/App.csproj`
- Przed oznaczeniem zadania jako gotowe: build musi przejść bez błędów i warningów nowo wprowadzonych. Zmiany UI przetestuj w przeglądarce (golden path + nawigacja klawiaturą).

## 8. Granice decyzji agenta

Rób bez pytania:
- Edycje istniejących plików w zakresie zleconego zadania.
- Dodawanie nowych Razor Pages, modeli, serwisów zgodnie z §3–§4.
- Instalacja pakietów NuGet potrzebnych do zleconego zadania (odnotuj w podsumowaniu).

Pytaj przed:
- Zmianą stacku (Blazor, MVC, SPA, baza danych, auth provider).
- Dodaniem zewnętrznego SaaS/API.
- Zmianą struktury rozwiązania (`.slnx`), gałęzi głównej, pipeline'u CI.
- Instalowaniem narzędzi globalnych lub `npm`/`node_modules`.
- Jakimkolwiek `git push`, otwarciem PR, usunięciem gałęzi.

## 9. Czego nie robić

- Nie generuj „na zapas" abstrakcji, interfejsów ani warstw repozytorium bez konkretnego wymagania.
- Nie dodawaj `try/catch` tam, gdzie nie ma czego obsłużyć — niech wyjątki propagują do middleware.
- Nie wprowadzaj feature flag/kompatybilności wstecznej dla kodu, który jeszcze nie istnieje w produkcji.
- Nie zostawiaj TODO/placeholderów w kodzie — albo zrób, albo zapytaj.
- Nie usuwaj niezrozumiałych plików/branchy — najpierw ustal, czy to praca użytkownika w toku.
