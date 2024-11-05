using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Poc.CRM.EfExtensible.Web.Infrastructure.Meta;

/// <summary>
/// Register EF service to make the DbContext aware of the metamodel version.
/// </summary>
public class MetamodelAwareCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        return context is IMetamodelAccessor metamodelAccessor
            ? new MetamodelCacheKey(context, designTime, metamodelAccessor.MetaModel.Version)
            : new ModelCacheKey(context, designTime);
    }
}

/// <summary>
/// In order to invalidat the model cache of EF core to reload the metamodel from the database we need to have a difference between the other cache and the new one.
/// This add an extra field to the cache key to store the metamodel version.
/// </summary>
/// <param name="context"></param>
/// <param name="designTime"></param>
/// <param name="metamodelVersion"></param>
public sealed class MetamodelCacheKey(DbContext context, bool designTime, int metamodelVersion)
    : ModelCacheKey(context, designTime)
{
    private readonly int _metamodelVersion = metamodelVersion;

    protected override bool Equals(ModelCacheKey other)
    {
        return other is MetamodelCacheKey otherCacheKey
               && base.Equals(other)
               && otherCacheKey._metamodelVersion == _metamodelVersion;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), _metamodelVersion);
    }
}