using Microsoft.EntityFrameworkCore;

namespace Poc.CRM.EfExtensible.Web.Infrastructure.Meta;

public static class MetaDependencies
{
    public static IServiceCollection AddMetaInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MetaDbContext>(o =>
            o.UseSqlServer(connectionString)
        );
        services.AddSingleton<MetaModelFactory>();
        services.AddSingleton<MetaModel>((p) =>
            p.GetRequiredService<MetaModelFactory>().Create().ConfigureAwait(false).GetAwaiter().GetResult()
        );
        
        return services;
    }
}