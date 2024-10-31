using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poc.CRM.EfExtensible.Web.Features;
using Poc.CRM.EfExtensible.Web.Infrastructure;
using Respawn;
using Testcontainers.MsSql;

namespace Poc.CRM.EfExtensible.Tests;

public class TestFixture
{
#pragma warning disable NUnit1032
    private MsSqlContainer _container;
#pragma warning restore NUnit1032
    private Respawner _respawner;

    protected IServiceProvider Provider;
    private ServiceProvider _rootProvider;

    protected void InAnotherScope()
    {
        Provider = _rootProvider.CreateScope().ServiceProvider;
    }

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new MsSqlBuilder()
            .WithReuse(true)
            .Build();
    }

    [SetUp]
    public async Task Setup()
    {
        await _container.StartAsync();
        var services = new ServiceCollection();
        services.AddDomain();
        services.AddSingleton<MetaModel>();
        services
            .AddDbContext<CrmDbContext>(o =>
                o
                    .UseSqlServer(_container.GetConnectionString())
                    .ReplaceService<IModelCacheKeyFactory, MetamodelAwareCacheKeyFactory>()
            );
        
        _rootProvider = services.BuildServiceProvider();

        using var scope = _rootProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        await context.Database.EnsureCreatedAsync();
        _respawner = await Respawner.CreateAsync(_container.GetConnectionString());

        Provider = _rootProvider.CreateScope().ServiceProvider;
    }

    [TearDown]
    public async Task TearDown()
    {
        await _rootProvider.DisposeAsync();
        await _respawner.ResetAsync(_container.GetConnectionString());
        await _container.StopAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
    }
}