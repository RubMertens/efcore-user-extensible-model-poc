using System.Reflection;
using Poc.CRM.EfExtensible.Web.Application;
using Poc.CRM.EfExtensible.Web.Features.Companies;

namespace Poc.CRM.EfExtensible.Web.Features;

public static class Dependencies
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.RegisterHandlerInCurrentAssembly();
        return services;
    }
    
    private static IServiceCollection RegisterHandlerInCurrentAssembly(this IServiceCollection services)
    {
        return services.RegisterHandlersInAssembly(Assembly.GetCallingAssembly());
    }


    private static IServiceCollection RegisterHandlersInAssembly(this IServiceCollection services, Assembly assembly)
    {
        var commandInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.GetInterfaces().Any(i => i == typeof(ICommand)));

        foreach (var commandInterface in commandInterfaces)
        {
            var matchingHandlerType = assembly.GetTypes()
                .SingleOrDefault(t => t.GetInterfaces().Any(i => i == commandInterface));
            if (matchingHandlerType != null)
            {
                services.AddScoped(commandInterface, matchingHandlerType);
                services.AddScoped(matchingHandlerType);
            }
        }

        return services;
    }
}