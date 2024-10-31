using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Poc.CRM.EfExtensible.Web.Application;
using Poc.CRM.EfExtensible.Web.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Features.Companies;

public interface IAddAddressToCompany : ICommand
{
    public static class Errors
    {
        public static DomainError NotFound() => new("Company not found");    
    }

    record Command
    {
        public Guid CompanyId { get; init; }
        public string City { get; init; }
        public string PostCode { get; init; }
        public string[] AddressLines { get; init; }
    };

    Task<Result<Guid>> Add(Command command);
}

class AddAddressToCompanyHandler(CrmDbContext context) : IAddAddressToCompany
{
    public async Task<Result<Guid>> Add(IAddAddressToCompany.Command command)
    {
        var company = await context.Companies.FirstOrDefaultAsync(c => c.Id == command.CompanyId);

        if (company == null)
            return Result<Guid>.Fail(new IGetCompanyDetails.NotFound());

        company.Address = new Address()
        {
            City = command.City,
            PostCode = command.PostCode,
            AddressLines = command.AddressLines
        };

        await context.SaveChangesAsync();
        return Result<Guid>.Succeed(company.Id);
    }
}