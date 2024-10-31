using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Infrastructure.Config;

public class CompanyEntityTypeConfiguration: IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
    }
}