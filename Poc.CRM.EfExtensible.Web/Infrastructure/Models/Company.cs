using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Poc.CRM.EfExtensible.Web.Infrastructure.Models;

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Address? Address { get; set; }
    public CompanyKind Kind { get; set; }
    public List<Contact> Contacts { get; set; }

    /// <summary>
    /// In order to be able to add additional fields to the company, we have a separate entity that holds the additional fields.
    /// This prevents table locking during alter table
    /// This makes merging easier
    /// <br/>
    /// A Navigation property cannot be a shadow property however, so we need to have a separate entity for this.
    /// </summary>
    public CompanyMetaModel Meta { get; set; }

    public enum CompanyKind
    {
        Unknown,
        Customer,
        Supplier,
        Vendor,
    }
}

public class CompanyMetaModel
{
    public long Id { get; set; }
    public Company Company { get; set; }
    public Guid CompanyId { get; set; }
}

public class CompanyEntityTypeConfiguration : IEntityTypeConfiguration<Company>, ICrmEntityTypeConfiguration
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
    }
}