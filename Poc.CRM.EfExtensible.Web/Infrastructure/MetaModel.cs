using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Poc.CRM.EfExtensible.Web.Features.Companies;

namespace Poc.CRM.EfExtensible.Web.Infrastructure;

public class MetaModel
{
    public int Version { get; set; }
    public List<AdditionalField> Fields { get; set; } = new();
    public List<AdditionalField> ForEntity<T>()
    {
        return Fields.Where(f => f.EntityName == typeof(T).FullName).ToList();
    }
    public void ApplyChanges(ModelBuilder modelBuilder)
    {
        var fieldsByEntity = Fields.GroupBy(f => f.EntityName);

        foreach (var fieldsGroup in fieldsByEntity)
        {
            modelBuilder.Entity(fieldsGroup.Key, builder =>
            {
                foreach (var field in fieldsGroup)
                {
                    AddFieldToEntity(builder, field);
                }
            });
        }
    }

    private void AddFieldToEntity(EntityTypeBuilder builder, AdditionalField field)
    {
        var propertyBuilder = builder
            .Property(field.PropertyType, field.PropertyName)
            .IsRequired(field.IsRequired);

        if (field.MaxLength.HasValue)
            propertyBuilder.HasMaxLength(field.MaxLength.Value);
    }
}

public interface IMetamodelAccessor
{
    public MetaModel MetaModel { get; }
}

public class AdditionalField
{
    public string EntityName { get; set; }
    public string PropertyName { get; set; }
    public Type PropertyType { get; set; }
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
}