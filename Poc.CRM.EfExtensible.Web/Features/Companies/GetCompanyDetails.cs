using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Poc.CRM.EfExtensible.Web.Application;
using Poc.CRM.EfExtensible.Web.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Features.Companies;

/// <summary>
/// Retrieve the details for a company. This includes the name, address, contacts, and any additional fields.
/// Can return a NotFound error if the company does not exist.
/// </summary>
public interface IGetCompanyDetails : ICommand
{
    /// <summary>
    /// Returned if the company does not exist.
    /// </summary>
    public record NotFound() : DomainError("Company not found");

    public record Model
    {
        public string Name { get; init; }
        public string Address { get; init; }
        public Company.CompanyKind CompanyKind { get; init; }
        public string[] Contacts { get; init; }
        public Dictionary<string, object>? AdditionalFields { get; set; }
    }

    public Task<Result<Model>> Get(Guid id);
}

public class GetCompanyDetailsHandler(CrmDbContext context) : IGetCompanyDetails
{
    public async Task<Result<IGetCompanyDetails.Model>> Get(Guid id)
    {
        var company = await context.Companies
            .Include(c => c.Contacts)
            .Include(company => company.Address)
            .Include(c => c.Meta)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
        {
            return Result<IGetCompanyDetails.Model>.Fail(new IGetCompanyDetails.NotFound());
        }

        var definedAdditionalFieldsForCompany = context.MetaModel.ForEntity<CompanyMetaModel>();

        var additionalFields = new Dictionary<string, object>();
        foreach (var field in definedAdditionalFieldsForCompany)
        {
            // Get the value of the field from the entity and add it to the returned query model.
            var value = context.Entry(company.Meta).Property(field.PropertyName).CurrentValue;
            if (value != null)
                additionalFields[field.PropertyName] = value;
        }

        return Result<IGetCompanyDetails.Model>.Succeed(new IGetCompanyDetails.Model
        {
            Name = company.Name,
            Address = AddressToString(company.Address),
            CompanyKind = company.Kind,
            Contacts = company.Contacts.Select(c => $"{c.FirstName} {c.LastName}").ToArray(),
            AdditionalFields = additionalFields.Count > 0 ? additionalFields : null
        });
    }

    private string AddressToString(Address? address)
    {
        if (address == null)
            return "";

        return $"{address.City}, {address.PostCode}, {string.Join(", ", address.AddressLines)}";
    }
}