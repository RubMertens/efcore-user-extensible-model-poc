using System.Runtime.CompilerServices;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Meta;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Infrastructure;

public class CrmDbContext(DbContextOptions<CrmDbContext> options, MetaModel metaModel)
    : DbContext(options), IMetamodelAccessor
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<Contact> Contacts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly,
            t => t.IsAssignableTo(typeof(ICrmEntityTypeConfiguration)));

        // Apply the metamodel changes
        MetaModel.ApplyChanges(modelBuilder);
    }

    public MetaModel MetaModel { get; init; } = metaModel;
}

/// <summary>
/// Marker interface for entity type configurations.
/// Used to apply only configuration relevant for this DbContext. Otherwise, all configurations in the assembly would be applied, also the ones from the MetaDbContext.
/// </summary>
public interface ICrmEntityTypeConfiguration;