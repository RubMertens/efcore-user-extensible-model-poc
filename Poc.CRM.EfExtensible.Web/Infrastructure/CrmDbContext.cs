using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Infrastructure;

public class CrmDbContext(DbContextOptions<CrmDbContext> options, MetaModel metaModel)
    : DbContext(options), IMetamodelAccessor
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<Contact> Contacts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
        MetaModel.ApplyChanges(modelBuilder);
    }

    public MetaModel MetaModel { get; init; } = metaModel;
}

public class MetamodelAwareCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        return context is IMetamodelAccessor metamodelAccessor
            ? new MetamodelCacheKey(context, designTime, metamodelAccessor.MetaModel.Version)
            : new ModelCacheKey(context, designTime);
    }
}

public sealed class MetamodelCacheKey(DbContext context, bool designTime, int metamodelVersion)
    : ModelCacheKey(context, designTime)
{
    private readonly int _metamodelVerion = metamodelVersion;

    protected override bool Equals(ModelCacheKey other)
    {
        return other is MetamodelCacheKey otherCacheKey
               && base.Equals(other)
               && otherCacheKey._metamodelVerion == _metamodelVerion;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), _metamodelVerion);
    }
}