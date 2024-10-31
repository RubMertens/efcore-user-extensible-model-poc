using Microsoft.Extensions.DependencyInjection;
using Poc.CRM.EfExtensible.Web.Features.Companies;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Tests.Features;

public class AddAddressToCompanyTests : TestFixture
{
    [Test]
    public async Task WhenCompanyNotExists_FailWithNotFound()
    {
        var result = await Provider
            .GetRequiredService<IAddAddressToCompany>()
            .Add(new IAddAddressToCompany.Command()
            {
                CompanyId = Guid.NewGuid(),
                City = "London",
                PostCode = "SW1A 1AA",
                AddressLines = ["10 Downing Street"]
            });
        result.Should().FailWith(IAddAddressToCompany.Errors.NotFound());
    }

    [Test]
    public async Task GivenCompanyExists_WhenAddAdressToCompany_Succeeds()
    {
        var createCompanyResult = await Provider
            .GetRequiredService<ICreateCompany>()
            .Create(new ICreateCompany.Command("Jack & Russel Co."));
        createCompanyResult.Should().Succeed();

        InAnotherScope();

        var result = await Provider
            .GetRequiredService<IAddAddressToCompany>()
            .Add(new IAddAddressToCompany.Command()
            {
                CompanyId = createCompanyResult.Data,
                City = "London",
                PostCode = "SW1A 1AA",
                AddressLines = ["10 Downing Street"]
            });
        result.Should().Succeed();

        InAnotherScope();

        var details = await Provider.GetRequiredService<IGetCompanyDetails>()
            .Get(createCompanyResult.Data);

        details.Should().SucceedWith(new IGetCompanyDetails.Model()
        {
            Name = "Jack & Russel Co.",
            Address = "London, SW1A 1AA, 10 Downing Street",
            CompanyKind = Company.CompanyKind.Unknown,
            Contacts = []
        });
    }
}