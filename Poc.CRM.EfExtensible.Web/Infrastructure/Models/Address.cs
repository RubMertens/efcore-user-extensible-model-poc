namespace Poc.CRM.EfExtensible.Web.Infrastructure.Models;

public class Address
{
    public long Id { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string[] AddressLines { get; set; }
}