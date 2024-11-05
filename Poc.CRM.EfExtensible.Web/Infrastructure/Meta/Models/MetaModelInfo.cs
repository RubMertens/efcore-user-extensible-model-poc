using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Poc.CRM.EfExtensible.Web.Infrastructure.Meta.Models;

public class MetaModelInfo
{
    public long Id { get; set; }
    public int Version { get; set; }
}

public class MetaModelInfoEntityTypeConfiguration : IEntityTypeConfiguration<MetaModelInfo>
{
    public void Configure(EntityTypeBuilder<MetaModelInfo> builder)
    {
        builder.HasData([
            new MetaModelInfo()
            {
                Id = 1,
                Version = 1
            }
        ]);
    }
}