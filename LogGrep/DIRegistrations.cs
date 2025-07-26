using Microsoft.Extensions.DependencyInjection;

namespace LogGrep;

public static class DIRegistrations
{
    public static IServiceCollection AddLogGrep(this IServiceCollection services)
    {
        //services.AddSingleton<Program>();
        return services;
    }
}
