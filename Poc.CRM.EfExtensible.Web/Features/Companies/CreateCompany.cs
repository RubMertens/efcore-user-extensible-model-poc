using Poc.CRM.EfExtensible.Web.Application;
using Poc.CRM.EfExtensible.Web.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Features.Companies;

public interface ICreateCompany : ICommand
{
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
            Name = command.Name
        };
        context.Companies.Add(dto);
        if (command.AdditionalFields != null)
        {
            foreach (var field in command.AdditionalFields)
            {
                context.Entry(dto).Property(field.Key).CurrentValue = field.Value;
            }
        }

        await context.SaveChangesAsync();

        return Result<Guid>.Succeed(dto.Id);
    }
}