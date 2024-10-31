using Microsoft.Extensions.DependencyInjection;
using Poc.CRM.EfExtensible.Web.Features.Companies;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Tests.Features;

public class AddFieldToCompany : TestFixture
{
    [Test]
    public async Task AddFieldToCompany_ShouldSaveField()
    {
        var result = await Provider
            .GetRequiredService<IAddFieldToCompany>()
            .AddField(new IAddFieldToCompany.Command
            {
                FieldName = "Description",
                FieldType = IAddFieldToCompany.Command.FieldTypes.Text,
                IsRequired = true,
                MaxLength = 100
            });
        result.Should().Succeed();

        InAnotherScope();

        var createResult = await Provider.GetRequiredService<ICreateCompany>()
            .Create(new ICreateCompany.Command
                {
                    Name = "Jack & Russel Co.",
                    AdditionalFields = new()
                    {
                        { "Description", "A company that sells dogs!" }
                    }
                }
            );

        createResult.Should().Succeed();

        InAnotherScope();

        var details = await Provider.GetRequiredService<IGetCompanyDetails>()
            .Get(createResult.Data);

        details.Should().SucceedWith(new IGetCompanyDetails.Model()
        {
            Name = "Jack & Russel Co.",
            Address = "",
            CompanyKind = Company.CompanyKind.Unknown,
            Contacts = [],
            AdditionalFields = new()
            {
                { "Description", "A company that sells dogs!" }
            }
        });
    }

    [Test, Combinatorial]
    public async Task AddField_Combinatorial_Succeeds(
        [
            Values(IAddFieldToCompany.Command.FieldTypes.Number,
                IAddFieldToCompany.Command.FieldTypes.Text,
                IAddFieldToCompany.Command.FieldTypes.Date)
        ]
        IAddFieldToCompany.Command.FieldTypes type,
        [Values(true, false)] bool isRequired,
        [Values(1, 10, 100)] int maxLength
    )
    {
        await Provider.GetRequiredService<IAddFieldToCompany>().AddField(
            new IAddFieldToCompany.Command
            {
                FieldName = $"{Enum.GetName(type)}_Required-{isRequired}_Maxlength-{maxLength}",
                FieldType = type,
                IsRequired = isRequired,
                MaxLength = maxLength,
            }
        );
    }

    [Test]
    public async Task ExistingData_WIthDefaultProvided_Succeeds()
    {
        var createResult = await Provider.GetRequiredService<ICreateCompany>().Create(new ICreateCompany.Command()
        {
            Name = "Jack & Russel Co."
        });
        createResult.Should().Succeed();

        InAnotherScope();

        var addFieldResult = await Provider.GetRequiredService<IAddFieldToCompany>().AddField(
            new IAddFieldToCompany.Command()
            {
                FieldName = "Description",
                FieldType = IAddFieldToCompany.Command.FieldTypes.Text,
                IsRequired = true,
                MaxLength = 100,
                DefaultValue = "Missing Description"
            });

        addFieldResult.Should().Succeed();
        InAnotherScope();

        var details = await Provider.GetRequiredService<IGetCompanyDetails>().Get(createResult.Data);

        details.Should().SucceedWith(new IGetCompanyDetails.Model()
        {
            Name = "Jack & Russel Co.",
            Address = "",
            CompanyKind = Company.CompanyKind.Unknown,
            Contacts = [],
            AdditionalFields = new()
            {
                { "Description", "Missing Description" }
            }
        });
    }
}