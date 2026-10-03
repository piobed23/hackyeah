using App.Data.Entities;
using App.Models;

namespace App.Services.Similarity;

public interface ISimilaritySearch
{
    Task<IReadOnlyList<SimilarityResult>> ZnajdzPodobneAsync(IdeaCard karta, int topN = 5);
    Task<IReadOnlyList<SimilarityResult>> ZnajdzDlaProblemuAsync(ProblemReport problem, int topN = 5);
    Task<IReadOnlyList<ProblemReport>> ZnajdzPodobneProblemyAsync(ProblemReport problem, int topN = 5);
}
