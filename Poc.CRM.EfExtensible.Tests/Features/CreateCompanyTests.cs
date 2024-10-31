using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Poc.CRM.EfExtensible.Web.Features.Companies;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Tests.Features;

public class CreateCompanyTests:TestFixture
{

    [Test]
    public async Task ShouldCreateCompany()
    {
        var result = await Provider
            .GetRequiredService<ICreateCompany>()
            .Create(new ICreateCompany.Command("Jack & Russel Co."));
        result.Should().Succeed();
        
        InAnotherScope();

        var details = await Provider.GetRequiredService<IGetCompanyDetails>()
            .Get(result.Data);
        ;
        details.Should().SucceedWith(new IGetCompanyDetails.Model()
        {
            Name = "Jack & Russel Co.",
            Address = "",
            CompanyKind = Company.CompanyKind.Unknown,
            Contacts = []
        });
    }
}