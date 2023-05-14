using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace PixiEditor.Platform;

public static class PlatformServiceCollection
{
    public static IServiceCollection AddPlatform(this IServiceCollection services)
    {
        if(IPlatform.Current == null)
            throw new InvalidOperationException("No platform was found");

        services.AddSingleton(IPlatform.Current);

        if (IPlatform.Current.AdditionalContentProvider != null)
            services.AddSingleton(IPlatform.Current.AdditionalContentProvider);
        return services;
    }
}
