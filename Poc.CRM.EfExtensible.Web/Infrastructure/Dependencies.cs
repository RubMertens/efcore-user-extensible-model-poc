using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Infrastructure;

public static class Dependencies
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<MetaModel>();
        services.AddDbContext<CrmDbContext>(o =>
            o.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .ReplaceService<IModelCacheKeyFactory, MetamodelAwareCacheKeyFactory>()
        );
        return services;
    }
}