using Microsoft.EntityFrameworkCore;
using Poc.CRM.EfExtensible.Web.Infrastructure.Meta.Models;

namespace Poc.CRM.EfExtensible.Web.Infrastructure.Meta;

/// <summary>
/// DbContext for the meta model.
/// This needs to be a separate DbContext to avoid circular dependencies.
/// </summary>
/// <param name="options"></param>
public class MetaDbContext(DbContextOptions<MetaDbContext> options) : DbContext(options)
{
    public DbSet<AdditionalFieldDto> Fields { get; set; }
    public DbSet<MetaModelInfo> Info { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AdditionalFieldDtoEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new MetaModelInfoEntityTypeConfiguration());
    }
}