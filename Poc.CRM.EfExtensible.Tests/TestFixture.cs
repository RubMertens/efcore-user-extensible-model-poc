using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poc.CRM.EfExtensible.Web.Features;
using Poc.CRM.EfExtensible.Web.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Meta;
using Respawn;
using Testcontainers.MsSql;

namespace Poc.CRM.EfExtensible.Tests;

public class TestFixture
{
    private MsSqlContainer _container;

    //cannot use respawner to reset the database because the addFields changes the schema, which it does not reset
    private Respawner _respawner;

    protected IServiceProvider Provider;
    private ServiceProvider _rootProvider;

    private string ConnectionString;
    private ServiceCollection _services;

    /// <summary>
    /// Run the rest of the test in a new DI scope simulating a new request
    /// </summary>
    protected void InAnotherScope()
    {
        Provider = _rootProvider.CreateScope().ServiceProvider;
    }

    /// <summary>
    /// Run the rest of the test with a new Root DI provider simulating a full restart of the application
    /// </summary>
    protected void WithNewRootProvider()
    {
        _rootProvider.Dispose();
        _rootProvider = _services.BuildServiceProvider();
        Provider = _rootProvider.CreateScope().ServiceProvider;
    }

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        //build sql server test container and start it
        _container = new MsSqlBuilder()
            .Build();
        await _container.StartAsync();
    }

    [SetUp]
    public async Task Setup()
    {
        //the connection string of the test database automatically sets the initial catalog to master.
        //This makes EF unhappy when trying to create and drop the database. 
        var connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString());
        //Change the initial catalog to the test name
        //This allows not only for an easier time cleaning up, but also allows for parallel test runs
        connectionString.InitialCatalog = TestContext.CurrentContext.Test.FullName;
        ConnectionString = connectionString.ConnectionString;

        await CreateDatabase();

        //build the DI container like in the application
        _services = [];
        _services.AddDomain();
        _services.AddInfrastructure(ConnectionString);


        _rootProvider = _services.BuildServiceProvider();
        using var scope = _rootProvider.CreateScope();
        var metaDbContext = scope.ServiceProvider.GetRequiredService<MetaDbContext>();
        await metaDbContext.Database.EnsureCreatedAsync();

        var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        await context.Database.EnsureCreatedAsync();
        //explicitly create the tables because we cannot use EnsureCreated
        //this is because the metacontext already created some tables which is the condition for EnsureCreated to not run
        var databaseCreator = context.GetService<IRelationalDatabaseCreator>();
        await databaseCreator.CreateTablesAsync();
        
        // _respawner = await Respawner.CreateAsync(_container.GetConnectionString());

        Provider = _rootProvider.CreateScope().ServiceProvider;
    }

    /// <summary>
    /// Creates the database for the test. We cannot leverage EF to "EnsureCreated".
    /// </summary>
    private async Task CreateDatabase()
    {
        await using var conn = new SqlConnection(_container.GetConnectionString());
        conn.Open();
        var command = conn.CreateCommand();
        command.CommandText = $"create database [{TestContext.CurrentContext.Test.FullName}];";
        await command.ExecuteNonQueryAsync();
        await conn.CloseAsync();
    }

    /// <summary>
    /// Deletes the database used in the test.
    /// </summary>
    private async Task DeleteDatabase()
    {
        await using var conn = new SqlConnection(_container.GetConnectionString());
        conn.Open();

        var command = conn.CreateCommand();
        command.CommandText =
            $"alter database [{TestContext.CurrentContext.Test.FullName}] set single_user with rollback immediate;";
        await command.ExecuteNonQueryAsync();

        command.CommandText = $"drop database [{TestContext.CurrentContext.Test.FullName}];";
        await command.ExecuteNonQueryAsync();

        await conn.CloseAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        // await DeleteDatabase();
        await _rootProvider.DisposeAsync();
        // await _respawner.ResetAsync(_container.GetConnectionString());
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
    }
}