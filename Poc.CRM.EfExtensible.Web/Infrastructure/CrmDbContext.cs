using Microsoft.EntityFrameworkCore;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Infrastructure;

public class CrmDbContext(DbContextOptions<CrmDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<Contact> Contacts { get; set; }
}