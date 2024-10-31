using System.Security.Principal;
using Microsoft.EntityFrameworkCore;

namespace Poc.CRM.EfExtensible.Web.Infrastructure;

public static class Dependencies
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CrmDbContext>(o =>
            o.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            );
        return services;
    }
}