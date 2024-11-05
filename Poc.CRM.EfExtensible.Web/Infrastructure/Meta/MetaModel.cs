using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Poc.CRM.EfExtensible.Web.Infrastructure.Meta;

/// <summary>
/// Stores the in-memory metamodel.
/// </summary>
/// <param name="additionalFields"></param>
/// <param name="version"></param>
public class MetaModel(IEnumerable<AdditionalField> additionalFields, int version)
{
    /// <summary>
    /// Version of the metamodel. Changing the version invalidates the Model cache of the DbContext this model is used with.   
    /// </summary>
    public int Version { get; set; } = version;

    public List<AdditionalField> Fields { get; set; } = [..additionalFields];

    /// <summary>
    /// Returns all fields for a given entity type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public List<AdditionalField> ForEntity<T>()
    {
        return Fields.Where(f => f.EntityName == typeof(T).FullName).ToList();
    }

    /// <summary>
    /// Applies metamodel change to the given model builder using standard EF core configuration like Entity(), Property(), etc.
    /// </summary>
    /// <param name="modelBuilder"></param>
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

    /// <summary>
    /// Adds a field to a given EntityBuilder using standar EF core configuration like HasMaxLength, IsRequired, etc.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="field"></param>
    private void AddFieldToEntity(EntityTypeBuilder builder, AdditionalField field)
    {
        var propertyBuilder = builder
            .Property(field.PropertyType, field.PropertyName)
            .IsRequired(field.IsRequired);

        if (field.MaxLength.HasValue)
            propertyBuilder.HasMaxLength(field.MaxLength.Value);
    }
}

public class AdditionalField
{
    public string EntityName { get; set; }
    public string PropertyName { get; set; }
    public Type PropertyType { get; set; }
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
}