using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities.FakeProvider;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NpgsqlRelationalConnectionTest
{
    [Fact]
    public void Creates_Npgsql_Server_connection_string()
    {
        using var connection = CreateConnection();

        Assert.IsType<NpgsqlConnection>(connection.DbConnection);
    }

    [Fact]
    public void Uses_DbDataSource_from_DbContextOptions()
    {
        using var dataSource = NpgsqlDataSource.Create("Host=FakeHost");

        var options = new DbContextOptionsBuilder()
            .UseNpgsql(dataSource)
            .Options;

        using var connection = CreateConnection(options);

        Assert.Equal("Host=FakeHost", connection.ConnectionString);
    }

    [Fact]
    public void Uses_DbDataSource_from_application_service_provider()
    {
        using var dataSource = NpgsqlDataSource.Create("Host=FakeHost");

        var appServiceProvider = new ServiceCollection()
            .AddNpgsqlDataSource("Host=FakeHost")
            .BuildServiceProvider();

        var options = new DbContextOptionsBuilder()
            .UseApplicationServiceProvider(appServiceProvider)
            .UseNpgsql()
            .Options;

        var npgsqlSingletonOptions = new NpgsqlSingletonOptions();
        npgsqlSingletonOptions.Initialize(options);

        using var connection = CreateConnection(options);

        Assert.Equal("Host=FakeHost", connection.ConnectionString);
    }

    [Fact]
    public void Can_create_master_connection_with_connection_string()
    {
        using var connection = CreateConnection();
        using var master = connection.CreateAdminConnection();

        Assert.Equal(@"Host=localhost;Database=postgres;Username=some_user;Password=some_password;Pooling=False;Multiplexing=False", master.ConnectionString);
    }

    [Fact]
    public void Can_create_master_connection_with_connection_string_and_alternate_admin_db()
    {
        var options = new DbContextOptionsBuilder()
            .UseNpgsql(
                @"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password",
                b => b.UseAdminDatabase("template0"))
            .Options;

        using var connection = CreateConnection(options);
        using var master = connection.CreateAdminConnection();

        Assert.Equal(@"Host=localhost;Database=template0;Username=some_user;Password=some_password;Pooling=False;Multiplexing=False", master.ConnectionString);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("False")]
    [InlineData("FALSE")]
    public void CurrentAmbientTransaction_returns_null_with_enlist_set_to_false(string falseValue)
    {
        var options = new DbContextOptionsBuilder()
            .UseNpgsql(
                @"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password;Enlist=" + falseValue)
            .Options;

        Transaction.Current = new CommittableTransaction();

        using var connection = CreateConnection(options);
        Assert.Null(connection.CurrentAmbientTransaction);

        Transaction.Current = null;
    }

    [Theory]
    [InlineData(";Enlist=true")]
    [InlineData("")] // Enlist is true by default
    public void CurrentAmbientTransaction_returns_transaction_with_enlist_enabled(string enlist)
    {
        var options = new DbContextOptionsBuilder()
            .UseNpgsql(
                @"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password" + enlist)
            .Options;

        var transaction = new CommittableTransaction();
        Transaction.Current = transaction;

        using var connection = CreateConnection(options);
        Assert.Equal(transaction, connection.CurrentAmbientTransaction);

        Transaction.Current = null;
    }

    [ConditionalFact]
    public void CloneWith_with_connection_and_connection_string()
    {
        var services = NpgsqlTestHelpers.Instance.CreateContextServices(
            new DbContextOptionsBuilder()
                .UseNpgsql("Host=localhost;Database=DummyDatabase")
                .Options);

        var relationalConnection = (NpgsqlRelationalConnection)services.GetRequiredService<IRelationalConnection>();

        var clone = relationalConnection.CloneWith("Host=localhost;Database=DummyDatabase;Application Name=foo");

        Assert.Equal("Host=localhost;Database=DummyDatabase;Application Name=foo", clone.ConnectionString);
    }

    public static NpgsqlRelationalConnection CreateConnection(DbContextOptions options = null)
    {
        options ??= new DbContextOptionsBuilder()
            .UseNpgsql(@"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password")
            .Options;

        foreach (var extension in options.Extensions)
        {
            extension.Validate(options);
        }

        return new NpgsqlRelationalConnection(
            new RelationalConnectionDependencies(
                options,
                new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                    new LoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener"),
                    new NpgsqlLoggingDefinitions(),
                    new NullDbContextLogger()),
                new RelationalConnectionDiagnosticsLogger(
                    new LoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener"),
                    new NpgsqlLoggingDefinitions(),
                    new NullDbContextLogger(),
                    CreateOptions()),
                new NamedConnectionStringResolver(options),
                new RelationalTransactionFactory(
                    new RelationalTransactionFactoryDependencies(
                        new RelationalSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies()))),
                new CurrentDbContext(new FakeDbContext()),
                new RelationalCommandBuilderFactory(
                    new RelationalCommandBuilderDependencies(
                        new NpgsqlTypeMappingSource(
                            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
                            new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                            new NpgsqlSingletonOptions()),
                        new ExceptionDetector()))));
    }

    private const string ConnectionString = "Fake Connection String";

    private static IDbContextOptions CreateOptions(
        RelationalOptionsExtension optionsExtension = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(
                optionsExtension
                ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

        return optionsBuilder.Options;
    }

    private class FakeDbContext : DbContext
    {
    }
}
