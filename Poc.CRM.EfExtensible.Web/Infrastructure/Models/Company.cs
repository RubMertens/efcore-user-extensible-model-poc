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

    public enum CompanyKind
    {
        Unknown,
        Customer,
        Supplier,
        Vendor,
    }
}

public class CompanyEntityTypeConfiguration: IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
    }
}