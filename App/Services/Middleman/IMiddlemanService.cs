using App.Data.Entities;
using App.Models;

namespace App.Services.Middleman;

public interface IMiddlemanService
{
    Task<AnalizaDopasowania> AnalizujAsync(Idea innowacja, ImplementationCard dane);
    Task<IReadOnlyList<WariantPropozycja>> GenerujWariantyAsync(Idea innowacja, ImplementationCard dane);
    Task<string> GenerujKarteWdrozeniaMdAsync(Idea innowacja, ImplementationCard dane, WariantPropozycja wariant);
    Task<IReadOnlyList<string>> WykryjBrakujacychPartnerowAsync(Idea innowacja, ImplementationCard dane);
}
