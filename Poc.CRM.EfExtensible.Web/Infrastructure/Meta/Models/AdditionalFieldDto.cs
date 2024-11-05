using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Poc.CRM.EfExtensible.Web.Infrastructure.Meta.Models;

public class AdditionalFieldDto
{
    public string EntityName { get; set; }
    public string PropertyName { get; set; }
    public string PropertyFullyQualifiedType { get; set; }
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
}

public class AdditionalFieldDtoEntityTypeConfiguration : IEntityTypeConfiguration<AdditionalFieldDto>
{
    public void Configure(EntityTypeBuilder<AdditionalFieldDto> builder)
    {
        builder.HasKey(af => new { af.EntityName, af.PropertyName });
    }
}
