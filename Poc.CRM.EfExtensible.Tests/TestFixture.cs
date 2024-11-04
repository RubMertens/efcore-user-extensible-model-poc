using Microsoft.Data.SqlClient;
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

    private string ConnectionString;

    protected void InAnotherScope()
    {
        Provider = _rootProvider.CreateScope().ServiceProvider;
    }

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new MsSqlBuilder()
            .Build();
        await _container.StartAsync();
    }

    [SetUp]
    public async Task Setup()
    {
        var connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString());
        connectionString.InitialCatalog = TestContext.CurrentContext.Test.FullName;
        ConnectionString = connectionString.ConnectionString;
        await CreateDatabase();

        var services = new ServiceCollection();
        services.AddDomain();
        services.AddSingleton<MetaModel>();
        services
            .AddDbContext<CrmDbContext>(o =>
                o
                    .UseSqlServer(ConnectionString)
                    .ReplaceService<IModelCacheKeyFactory, MetamodelAwareCacheKeyFactory>()
            );

        _rootProvider = services.BuildServiceProvider();

        using var scope = _rootProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        await context.Database.EnsureCreatedAsync();
        // _respawner = await Respawner.CreateAsync(_container.GetConnectionString());

        Provider = _rootProvider.CreateScope().ServiceProvider;
    }

    private async Task CreateDatabase()
    {
        await using var conn = new SqlConnection(_container.GetConnectionString());
        conn.Open();
        var command = conn.CreateCommand();
        command.CommandText = $"create database [{TestContext.CurrentContext.Test.FullName}];";
        await command.ExecuteNonQueryAsync();
        await conn.CloseAsync();
    }

    private async Task DeleteDatabase()
    {
        await using var conn = new SqlConnection(_container.GetConnectionString());
        conn.Open();
        
        var command = conn.CreateCommand();
        command.CommandText = $"alter database [{TestContext.CurrentContext.Test.FullName}] set single_user with rollback immediate;";
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = $"drop database [{TestContext.CurrentContext.Test.FullName}];";
        await command.ExecuteNonQueryAsync();
        
        await conn.CloseAsync();
            }

    [TearDown]
    public async Task TearDown()
    {
        // await DeleteDatabasek();
        // using (var scope = _rootProvider.CreateScope())
        // {
        //     // await scope.ServiceProvider
        //     //     .GetRequiredService<CrmDbContext>()
        //     //     .Database
        //     //     .EnsureDeletedAsync();
        // }

        await _rootProvider.DisposeAsync();

        // await _respawner.ResetAsync(_container.GetConnectionString());
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
    }
}