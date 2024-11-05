using Microsoft.EntityFrameworkCore;

namespace Poc.CRM.EfExtensible.Web.Infrastructure.Meta;

/// <summary>
/// Create MetaModel from the database.
/// </summary>
/// <param name="context"></param>
public class MetaModelFactory(MetaDbContext context)
{
    public async Task<MetaModel> Create()
    {
        var info = await context.Info.SingleOrDefaultAsync();
        var fields = await context.Fields.ToListAsync();

        var metaModel = new MetaModel(
            fields.Select(f => new AdditionalField
            {
                EntityName = f.EntityName,
                PropertyName = f.PropertyName,
                PropertyType = Type.GetType(f.PropertyFullyQualifiedType) ??
                               throw new InvalidOperationException($"Type {f.PropertyFullyQualifiedType} not found"),
                IsRequired = f.IsRequired,
                MaxLength = f.MaxLength
            }),
            info.Version
        );
        return metaModel;
    }
}