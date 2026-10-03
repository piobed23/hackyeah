using App.Models;

namespace App.Services.Completeness;

public interface ICompletenessChecker<T>
{
    CompletenessResult Check(T obj);
}
