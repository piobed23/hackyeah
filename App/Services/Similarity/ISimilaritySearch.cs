using App.Data.Entities;
using App.Models;

namespace App.Services.Similarity;

public interface ISimilaritySearch
{
    Task<IReadOnlyList<SimilarityResult>> ZnajdzPodobneAsync(IdeaCard karta, int topN = 5);
}
