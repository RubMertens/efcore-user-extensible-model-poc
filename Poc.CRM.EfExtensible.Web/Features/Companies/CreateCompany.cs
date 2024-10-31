using Poc.CRM.EfExtensible.Web.Application;
using Poc.CRM.EfExtensible.Web.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Features.Companies;

public interface ICreateCompany : ICommand
{
    public record Command(string Name);

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
        await context.SaveChangesAsync();

        return Result<Guid>.Succeed(dto.Id);
    }
}