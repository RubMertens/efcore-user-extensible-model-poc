using Poc.CRM.EfExtensible.Web.Application;
using Poc.CRM.EfExtensible.Web.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Features.Companies;

/// <summary>
/// Creates a new company with any additional fields.
/// </summary>
public interface ICreateCompany : ICommand
{
    static class Errors
    {
        public static DomainError AdditionalFieldDoesNotExist(string fieldName) =>
            new($"Field {fieldName} does not exist");
    }

    public record Command
    {
        public string Name { get; init; }
        public Dictionary<string, object>? AdditionalFields { get; init; } = null;
    };

    public Task<Result<Guid>> Create(Command command);
}

class CreateCompanyHandler(CrmDbContext context) : ICreateCompany
{
    public async Task<Result<Guid>> Create(ICreateCompany.Command command)
    {
        var dto = new Company()
        {
            Name = command.Name,
            Meta = new CompanyMetaModel()
        };
        context.Companies.Add(dto);
        if (command.AdditionalFields != null)
        {
            //dynamically set the additional fields
            //if the property does not exist, it will fail
            foreach (var field in command.AdditionalFields)
            {
                var entry = context.Entry(dto.Meta);
                var property = entry.Metadata.FindProperty(field.Key);
                if (property == null)
                    return Result<Guid>.Fail(ICreateCompany.Errors.AdditionalFieldDoesNotExist(field.Key));
                entry.Property(property).CurrentValue = field.Value;
            }
        }

        await context.SaveChangesAsync();

        return Result<Guid>.Succeed(dto.Id);
    }
}