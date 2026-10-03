using App.Data.Entities;
using App.Services.Ai;
using App.Services.Completeness;
using App.Services.Context;
using App.Services.Similarity;

namespace App.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthContext, SessionAuthContext>();
        services.AddSingleton<IAiAssistant, MockAiAssistant>();
        services.AddSingleton<ICompletenessChecker<IdeaCard>, IdeaCardCompletenessChecker>();
        services.AddSingleton<ICompletenessChecker<Application>, ApplicationCompletenessChecker>();
        services.AddScoped<ISimilaritySearch, InMemorySimilaritySearch>();
        return services;
    }
}
