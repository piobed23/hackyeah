namespace App.Models;

public record AiSuggestion(string Preambula, IReadOnlyList<string> Punkty, string? PoleDocelowe = null);

public record CompletenessResult(int Procent, IReadOnlyList<MissingField> Braki, bool CanSubmit);

public record MissingField(string Pole, string Etykieta, string Opis);
