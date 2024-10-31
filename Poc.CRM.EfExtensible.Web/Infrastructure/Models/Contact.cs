namespace Poc.CRM.EfExtensible.Web.Infrastructure.Models;

public class Contact
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }

    public List<Company> Companies { get; set; }
}