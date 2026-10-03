# ROPS Innowacje — Platforma Innowacji Społecznych

Projekt zbudowany na HackYeah 2026 dla **Regionalnego Ośrodka Polityki Społecznej w Krakowie**.
Platforma prowadzi mieszkańca, autora, pracownika ROPS i testera przez cały cykl życia innowacji
społecznej: od pomysłu, przez konsultacje, nabór i wniosek, aż po przetestowaną innowację.

Formularz aplikacyjny odwzorowuje **załącznik nr 3 do Ogłoszenia** naboru "Inkubator Włączenia
Społecznego 2.0" (FERS 5.1 2021-2027).

## Stack

- ASP.NET Core 9 / Razor Pages
- EF Core 9 + SQLite (plik `rops.db`, tworzony automatycznie przy pierwszym starcie)
- FluentValidation
- Bootstrap 5 + jQuery (zasoby lokalne w `App/wwwroot/lib/`)
- Mock asystenta AI (`IAiAssistant` → `MockAiAssistant`) — gotowe do podmiany na model .NET
  (ML.NET, ONNX Runtime, Microsoft.Extensions.AI) bez zmian w stronach

## Uruchomienie

```powershell
dotnet build App/App.csproj
dotnet run --project App/App.csproj
```

Przeglądarka: http://localhost:5099 (albo port z `launchSettings.json`).

Baza SQLite tworzy się automatycznie (`EnsureCreated` + idempotentny seeder).
Reset bazy: usuń plik `App/rops.db` i uruchom ponownie.

## Przełącznik ról

W prawym górnym rogu navbara znajduje się dropdown "Rola demonstracyjna" z 5 rolami:
Autor / PracownikROPS / Mieszkaniec / Ekspert / Admin. Zmiana roli przełącza sesję i otwiera
widoki właściwe dla roli. Autoryzacja działa przez standardowe `[Authorize(Roles="…")]` —
`DemoRoleMiddleware` buduje `ClaimsPrincipal` z sesji.

## Golden path demo (5 minut)

1. **Autor opisuje pomysł** — `Dodaj pomysł` w navbarze. Wybierz rodzaj (Innowacja /
   Dobra praktyka / Mikroskala). Kreator 7-krokowy z panelem AI po prawej. Na kroku 2
   pojawia się przycisk "Zobacz podobne" — system porównuje opis z Biblioteką Innowacji
   Społecznych (3 sprawdzone innowacje w seed) i problemami zgłoszonymi przez mieszkańców.
   Krok 7: podgląd fiszki + kompletność + "Wyślij do weryfikacji ROPS".

2. **ROPS weryfikuje fiszkę** — przełącz rolę na `PracownikROPS`. Idź na
   `Weryfikacja fiszek`. Wybierz decyzję (Akceptuj / Popraw / Odrzuć / Do eksperta /
   Zaproponuj połączenie) + uzasadnienie. Akceptacja publikuje fiszkę i zmienia status
   innowacji na "Poszukuje finansowania".

3. **Mieszkaniec konsultuje** — przełącz na `Mieszkaniec`. Z navbara "Innowacje do
   testowania" lub bezpośrednio publiczna karta pomysłu. Mieszkaniec może poprzeć,
   skomentować, zgłosić ofertę organizacji (lokal / zasoby / wolontariusze / partnerstwo).

4. **Autor składa wniosek** — przełącz na `Autor`. `Nabory` → otwórz nabór "Małopolska
   Innowacyjna 2026" → `Przygotuj wniosek z tego pomysłu`. Generator kopiuje treść fiszki
   do wniosku zgodnego z zał. 3 Ogłoszenia FERS 5.1 (pola 1-12: dane pomysłodawcy, opis,
   innowacyjność, diagnoza, odbiorcy, zmiana, wizja, plan działania w 3 okresach,
   wnioskowana kwota, zespół, oświadczenia). Uzupełnij braki na kolejnych zakładkach
   (Opis / Budżet / Plan / Kompletność). Procent kompletności w `Kompletność`.
   `Podgląd i złożenie` → akceptuj oświadczenia → `Złóż wniosek`.

5. **ROPS ocenia wniosek** — przełącz na `PracownikROPS`. `Ocena wniosków` → zmień status
   przez `KontrolaFormalna` → `OcenaMerytoryczna` → `Wybrany`. Innowacja dostaje status
   "Finansowanie przyznane".

6. **Autor rozpoczyna testowanie** — przełącz na `Autor`. `Moje pomysły` → kliknij `Panel`
   przy swojej innowacji → `Rozpocznij testowanie`. Wypełnij: co będzie testowane, kogo
   szukamy, liczba testerów, tryb (zdalny/stacjonarny), termin, instrukcja. Innowacja
   przechodzi w stan `Poszukuje testerów`.

7. **Mieszkaniec zgłasza się do testu** — przełącz na `Mieszkaniec`. Na publicznej karcie
   innowacji w alercie "Autor szuka testerów" rozwiń `Chcę przetestować`. Wypełnij imię,
   powód, kontakt, udogodnienia, zgoda — wyślij.

8. **Autor zarządza testerami** — przełącz na `Autor`. Panel innowacji → `Zarządzaj
   testerami`. W tabeli zgłoszeń kliknij `Akceptuj` → podaj instrukcję (termin, miejsce,
   link). Po teście kliknij `Test wykonany — poproś o opinię` — otrzymasz link do
   formularza opinii.

9. **Tester wypełnia opinię** — otwórz link `/Testerzy/Opinia?zgloszenie=<id>` (np. w
   trybie incognito, bo jest `[AllowAnonymous]`). 6 pytań + ocena 1-5 → wyślij.

10. **Autor kończy testowanie** — wyniki widoczne w tabeli + sekcja "Podsumowanie wyników"
    (średnia ocena, bariery dostępności, sugestie poprawek). Przycisk `Zakończ testowanie
    — przekaż do ROPS`. Status sesji → `Zakończone`, etap innowacji → `TestyZakonczone`.

11. **ROPS zatwierdza "Sprawdzoną innowację"** — przełącz na `PracownikROPS`. `Testerzy`
    (navbar) → `Przegląd sesji testowych` → `Zatwierdź jako sprawdzoną innowację`.
    Innowacja wraca do publicznej biblioteki jako `Sprawdzona innowacja` i może posłużyć
    jako inspiracja dla kolejnych autorów (porównanie w kreatorze pokaże ją jako
    "Sprawdzoną innowację").

## Dostępność — WCAG 2.1 AA

- `<html lang="pl">`, skip-link "Przejdź do treści", semantyczne znaczniki `<header>` /
  `<nav>` / `<main>` / `<footer>`
- Każdy formularz ma `<label for>`, grupy radio w `<fieldset>+<legend>`
- Walidacja: `aria-describedby`, `aria-invalid`, sumaryczne `role="alert"
  aria-live="assertive"` dla błędów serwera, `aria-live="polite"` dla autozapisu
  i sugestii AI
- Pasek kroków kreatora: `<ol>` z `aria-current="step"` dla aktualnego
- Pasek postępu kompletności: `role="progressbar"` z `aria-valuenow/min/max`
- Nawigacja klawiaturą: Alt+→/← między krokami, focus trap w modalach, dokumentacja
  w stopce
- `prefers-reduced-motion` wyłącza animacje
- `@media print` ukrywa nav/footer/formularze → PDF-przyjazny wydruk wniosku

## Struktura

```
App/
├─ Data/
│  ├─ Entities/           (17 encji + 3 testowe)
│  ├─ AppDbContext.cs
│  └─ Seed/DemoSeeder.cs  (6 użytkowników, 3 sprawdzone innowacje, 1 nabór, 3 problemy)
├─ Infrastructure/
│  ├─ Enums/              (10 enumów statusów)
│  ├─ Auth/DemoRoleMiddleware.cs
│  └─ Validation/
├─ Models/                (DTO: AiSuggestion, CompletenessResult, SimilarityResult,
│                           Oswiadczenia)
├─ Services/
│  ├─ Ai/{IAiAssistant, MockAiAssistant}         ← podmień tutaj dla real AI
│  ├─ Similarity/InMemorySimilaritySearch
│  ├─ Completeness/{IdeaCardChecker, ApplicationChecker}
│  └─ Context/{IAuthContext, SessionAuthContext}
└─ Pages/
   ├─ Index                        (strona startowa)
   ├─ Moje/                        (lista pomysłów i wniosków autora)
   ├─ Kreator/                     (Rodzaj + 7-krokowy formularz + Podobne + Podgląd)
   ├─ Publiczne/                   (Karta publiczna + Lista do testowania)
   ├─ Nabory/                      (Index + Szczegoly)
   ├─ Wnioski/                     (Edycja + Budzet + Harmonogram + Kompletnosc +
   │                                 Podglad + Status + Generuj)
   ├─ Autor/                       (Panel innowacji + Testerzy)
   ├─ Testerzy/                    (Opinia — anonymous)
   ├─ Rops/                        (Weryfikacja + KreatorNaboru + Ocena +
   │                                 PodgladTesterow)
   └─ Shared/                      (Layout + partials)
```

## Co jest celowo uproszczone

- **Autoryzacja**: tylko przełącznik demo — brak Identity, haseł, OAuth
- **AI**: deterministyczny mock — zwraca realistyczne sugestie bez LLM; interfejs
  `IAiAssistant` gotowy do podmiany
- **Similarity search**: tokeny + Jaccard tagów + match rodzaju; w produkcji → embeddingi
- **Załączniki**: tylko metadane w `Attachment` — brak storage plików
- **Powiadomienia**: brak e-maili / SMS — statusy widoczne tylko w aplikacji
- **Konfigurowalne ankiety testerów**: jeden formularz dla wszystkich sesji
- **Testy jednostkowe**: smoke test manualny (ten dokument) wystarcza na hackathon

## Instrukcja dla agentów AI

`AGENTS.md` w głównym katalogu — konwencje kodu, strukturę, WCAG, granice decyzji.

## Licencja

HackYeah 2026 · ROPS Małopolska
