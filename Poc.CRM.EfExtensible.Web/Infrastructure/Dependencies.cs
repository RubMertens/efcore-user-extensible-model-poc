using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Meta;

namespace Poc.CRM.EfExtensible.Web.Infrastructure;

public static class Dependencies
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddMetaInfrastructure(connectionString);
        services.AddDbContext<CrmDbContext>(o =>
            o.UseSqlServer(connectionString)
                .ReplaceService<IModelCacheKeyFactory, MetamodelAwareCacheKeyFactory>()
        );
        return services;
    }
}