using App.Data.Entities;
using App.Infrastructure.Enums;
using App.Models;

namespace App.Services.Ai;

public interface IAiAssistant
{
    Task<AiSuggestion> SuggestClarificationAsync(string pole, string tresc);
    Task<AiSuggestion> IdentifyRisksAsync(IdeaCard karta);
    Task<AiSuggestion> SummarizeNeedsAsync(IdeaCard karta);
    Task<AiSuggestion> ProposeStructureAsync(RodzajPomyslu rodzaj, string problem);
    Task<IReadOnlyList<string>> SuggestTagsAsync(string tresc);
    Task<AiSuggestion> DraftApplicationSectionAsync(IdeaCard karta, string sekcja);
}
