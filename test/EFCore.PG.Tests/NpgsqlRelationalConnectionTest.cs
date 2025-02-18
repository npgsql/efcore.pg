using System.Data.Common;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities.FakeProvider;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

#nullable enable

public class NpgsqlRelationalConnectionTest
{
    [Fact]
    public void Creates_NpgsqlConnection()
    {
        using var connection = CreateConnection();

        Assert.IsType<NpgsqlConnection>(connection.DbConnection);
    }

    [Fact]
    public void Uses_DbDataSource_from_DbContextOptions()
    {
        using var dataSource = NpgsqlDataSource.Create("Host=FakeHost");

        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddNpgsqlDataSource("Host=FakeHost")
            // ReSharper disable once AccessToDisposedClosure
            .AddDbContext<FakeDbContext>(o => o.UseNpgsql(dataSource));

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        using var scope1 = serviceProvider.CreateScope();
        var context1 = scope1.ServiceProvider.GetRequiredService<FakeDbContext>();
        var relationalConnection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Same(dataSource, relationalConnection1.DbDataSource);

        var connection1 = context1.GetService<FakeDbContext>().Database.GetDbConnection();
        Assert.Equal("Host=FakeHost", connection1.ConnectionString);

        using var scope2 = serviceProvider.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<FakeDbContext>();
        var relationalConnection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Same(dataSource, relationalConnection2.DbDataSource);

        var connection2 = context2.GetService<FakeDbContext>().Database.GetDbConnection();
        Assert.Equal("Host=FakeHost", connection2.ConnectionString);
    }

    [Fact]
    public void Uses_DbDataSource_from_application_service_provider()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddNpgsqlDataSource("Host=FakeHost")
            .AddDbContext<FakeDbContext>(o => o.UseNpgsql());

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();

        using var scope1 = serviceProvider.CreateScope();
        var context1 = scope1.ServiceProvider.GetRequiredService<FakeDbContext>();
        var relationalConnection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Same(dataSource, relationalConnection1.DbDataSource);

        var connection1 = context1.GetService<FakeDbContext>().Database.GetDbConnection();
        Assert.Equal("Host=FakeHost", connection1.ConnectionString);

        using var scope2 = serviceProvider.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<FakeDbContext>();
        var relationalConnection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Same(dataSource, relationalConnection2.DbDataSource);

        var connection2 = context2.GetService<FakeDbContext>().Database.GetDbConnection();
        Assert.Equal("Host=FakeHost", connection2.ConnectionString);
    }

    [Fact] // #3060
    public void DbDataSource_from_application_service_provider_does_not_used_if_connection_string_is_specified()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddNpgsqlDataSource("Host=FakeHost1")
            .AddDbContext<FakeDbContext>(o => o.UseNpgsql("Host=FakeHost2"));

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        using var scope1 = serviceProvider.CreateScope();
        var context1 = scope1.ServiceProvider.GetRequiredService<FakeDbContext>();
        var relationalConnection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Null(relationalConnection1.DbDataSource);

        var connection1 = context1.GetService<FakeDbContext>().Database.GetDbConnection();
        Assert.Equal("Host=FakeHost2", connection1.ConnectionString);
    }

    [Fact]
    public void Data_source_config_with_same_connection_string()
    {
        // The connection string is the same, so the same data source gets resolved.
        // This works well as long as ConfigureDataSource() has the same lambda.
        var context1 = new ConfigurableContext(
            "Host=FakeHost1", no => no.ConfigureDataSource(dsb => dsb.ConnectionStringBuilder.ApplicationName = "App1"));
        var connection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1;Application Name=App1", connection1.ConnectionString);
        Assert.NotNull(connection1.DbDataSource);

        var context2 = new ConfigurableContext(
            "Host=FakeHost1", no => no.ConfigureDataSource(dsb => dsb.ConnectionStringBuilder.ApplicationName = "App1"));
        var connection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1;Application Name=App1", connection1.ConnectionString);
        Assert.Same(connection1.DbDataSource, connection2.DbDataSource);
    }

    [Fact]
    public void Data_source_config_with_different_connection_strings()
    {
        // When different connection strings are used, different data sources are created internally.
        var context1 = new ConfigurableContext(
            "Host=FakeHost1", no => no.ConfigureDataSource(dsb => dsb.ConnectionStringBuilder.ApplicationName = "App1"));
        var connection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1;Application Name=App1", connection1.ConnectionString);
        Assert.NotNull(connection1.DbDataSource);

        var context2 = new ConfigurableContext(
            "Host=FakeHost2", no => no.ConfigureDataSource(dsb => dsb.ConnectionStringBuilder.ApplicationName = "App2"));
        var connection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost2;Application Name=App2", connection2.ConnectionString);
        Assert.NotSame(connection1.DbDataSource, connection2.DbDataSource);
    }

    [Fact]
    public void Data_source_config_with_same_connection_string_and_different_lambda()
    {
        // Bad case: same connection string but with a different data source config lambda.
        // Same data source gets reused, and so the differing data source config gets ignored.
        var context1 = new ConfigurableContext(
            "Host=FakeHost1", no => no.ConfigureDataSource(dsb => dsb.ConnectionStringBuilder.ApplicationName = "App1"));
        var connection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1;Application Name=App1", connection1.ConnectionString);
        Assert.NotNull(connection1.DbDataSource);

        var context2 = new ConfigurableContext(
            "Host=FakeHost1", no => no.ConfigureDataSource(dsb => dsb.ConnectionStringBuilder.ApplicationName = "App2"));
        var connection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        // Note the incorrect Application Name below, because the 1st data source was resolved based on the connection string only
        Assert.Equal("Host=FakeHost1;Application Name=App1", connection2.ConnectionString);
        Assert.Same(connection1.DbDataSource, connection2.DbDataSource);
    }

    [Fact]
    public void Plugin_config_with_same_connection_string()
    {
        // The connection string and plugin config are the same, so the same data source gets resolved.
        var context1 = new ConfigurableContext("Host=FakeHost1", no => no.UseNetTopologySuite());
        var connection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection1.ConnectionString);
        Assert.NotNull(connection1.DbDataSource);

        var context2 = new ConfigurableContext("Host=FakeHost1", no => no.UseNetTopologySuite());
        var connection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection1.ConnectionString);
        Assert.Same(connection1.DbDataSource, connection2.DbDataSource);
    }

    [Fact]
    public void Plugin_config_with_different_connection_strings()
    {
        // When different connection strings are used, different data sources are created internally.
        var context1 = new ConfigurableContext("Host=FakeHost1", no => no.UseNetTopologySuite());
        var connection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection1.ConnectionString);
        Assert.NotNull(connection1.DbDataSource);

        var context2 = new ConfigurableContext("Host=FakeHost2", no => no.UseNetTopologySuite());
        var connection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost2", connection2.ConnectionString);
        Assert.NotSame(connection1.DbDataSource, connection2.DbDataSource);
    }

    [Fact]
    public void Plugin_config_with_different_connection_strings_and_different_plugins()
    {
        // Since the plugin configuration is a singleton option, a different service provider gets resolved and we have different data
        // sources.
        var context1 = new ConfigurableContext("Host=FakeHost1", no => no.UseNetTopologySuite());
        var connection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection1.ConnectionString);
        Assert.NotNull(connection1.DbDataSource);

        var context2 = new ConfigurableContext("Host=FakeHost1", no => no.UseNodaTime());
        var connection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection2.ConnectionString);
        Assert.NotSame(connection1.DbDataSource, connection2.DbDataSource);
    }

    [Fact]
    public void Enum_config_with_same_connection_string()
    {
        // The connection string and plugin config are the same, so the same data source gets resolved.
        var context1 = new ConfigurableContext("Host=FakeHost1", no => no.MapEnum<Mood>("mood"));
        var connection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection1.ConnectionString);
        Assert.NotNull(connection1.DbDataSource);

        var context2 = new ConfigurableContext("Host=FakeHost1", no => no.MapEnum<Mood>("mood"));
        var connection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection1.ConnectionString);
        Assert.Same(connection1.DbDataSource, connection2.DbDataSource);
    }

    [Fact]
    public void Enum_config_with_different_connection_strings()
    {
        // When different connection strings are used, different data sources are created internally.
        var context1 = new ConfigurableContext("Host=FakeHost1", no => no.MapEnum<Mood>("mood"));
        var connection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection1.ConnectionString);
        Assert.NotNull(connection1.DbDataSource);

        var context2 = new ConfigurableContext("Host=FakeHost2", no => no.MapEnum<Mood>("mood"));
        var connection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost2", connection2.ConnectionString);
        Assert.NotSame(connection1.DbDataSource, connection2.DbDataSource);
    }

    [Fact]
    public void Enum_config_with_different_connection_strings_and_different_enums()
    {
        // Since the enum configuration is a singleton option, a different service provider gets resolved, and we have different data
        // sources.
        var context1 = new ConfigurableContext("Host=FakeHost1", no => no.MapEnum<Mood>("mood"));
        var connection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection1.ConnectionString);
        Assert.NotNull(connection1.DbDataSource);

        var context2 = new ConfigurableContext("Host=FakeHost1", _ => { /* no enums */});
        var connection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection2.ConnectionString);
        Assert.NotSame(connection1.DbDataSource, connection2.DbDataSource);
    }

    [Fact]
    public void Data_source_and_data_source_config_are_incompatible()
    {
        using var dataSource = NpgsqlDataSource.Create("Host=FakeHost");

        var optionsBuilder = new DbContextOptionsBuilder<FakeDbContext>();
        optionsBuilder.UseNpgsql(dataSource, no => no.ConfigureDataSource(dsb => dsb.ConnectionStringBuilder.ApplicationName = "foo"));

        var context1 = new FakeDbContext(optionsBuilder.Options);
        var exception = Assert.Throws<NotSupportedException>(() => context1.GetService<IRelationalConnection>());
        Assert.Equal(NpgsqlStrings.DataSourceAndConfigNotSupported, exception.Message);
    }

    [Fact]
    public void Multiple_connection_strings_without_data_source_features()
    {
        var context1 = new ConfigurableContext("Host=FakeHost1");
        var connection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection1.ConnectionString);
        Assert.Null(connection1.DbDataSource);

        var context2 = new ConfigurableContext("Host=FakeHost1");
        var connection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost1", connection2.ConnectionString);
        Assert.Null(connection2.DbDataSource);

        var context3 = new ConfigurableContext("Host=FakeHost2");
        var connection3 = (NpgsqlRelationalConnection)context3.GetService<IRelationalConnection>();
        Assert.Equal("Host=FakeHost2", connection3.ConnectionString);
        Assert.Null(connection3.DbDataSource);
    }

    [Fact]
    public void Uses_correct_DbDataSource_from_application_service_provider_with_cached_DbContextOptions_extension()
    {
        var serviceCollection1 = new ServiceCollection();

        serviceCollection1
            .AddNpgsqlDataSource("Host=FakeHost1")
            .AddDbContext<FakeDbContext>(o => o.UseNpgsql());

        using (var serviceProvider1 = serviceCollection1.BuildServiceProvider())
        {
            var dataSource1 = serviceProvider1.GetRequiredService<NpgsqlDataSource>();

            Assert.Equal("Host=FakeHost1", dataSource1.ConnectionString);

            var context1 = serviceProvider1.GetRequiredService<FakeDbContext>();
            var relationalConnection1 = (NpgsqlRelationalConnection)context1.GetService<IRelationalConnection>()!;

            Assert.Same(dataSource1, relationalConnection1.DbDataSource);
        }

        var serviceCollection2 = new ServiceCollection();

        serviceCollection2
            .AddNpgsqlDataSource("Host=FakeHost2")
            .AddDbContext<FakeDbContext>(o => o.UseNpgsql());

        using (var serviceProvider2 = serviceCollection2.BuildServiceProvider())
        {
            var dataSource2 = serviceProvider2.GetRequiredService<NpgsqlDataSource>();

            Assert.Equal("Host=FakeHost2", dataSource2.ConnectionString);

            var context2 = serviceProvider2.GetRequiredService<FakeDbContext>();
            var relationalConnection2 = (NpgsqlRelationalConnection)context2.GetService<IRelationalConnection>()!;

            Assert.Same(dataSource2, relationalConnection2.DbDataSource);
        }
    }

    [Fact]
    public void Can_create_master_connection_with_connection_string()
    {
        using var connection = CreateConnection();
        using var master = connection.CreateAdminConnection();

        Assert.Equal(
            @"Host=localhost;Database=postgres;Username=some_user;Password=some_password;Pooling=False;Multiplexing=False",
            master.ConnectionString);
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

        Assert.Equal(
            @"Host=localhost;Database=template0;Username=some_user;Password=some_password;Pooling=False;Multiplexing=False",
            master.ConnectionString);
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
    public async Task CloneWith_with_connection_and_connection_string()
    {
        var services = NpgsqlTestHelpers.Instance.CreateContextServices(
            new DbContextOptionsBuilder()
                .UseNpgsql("Host=localhost;Database=DummyDatabase")
                .Options);

        var relationalConnection = (NpgsqlRelationalConnection)services.GetRequiredService<IRelationalConnection>();

        var clone = await relationalConnection.CloneWith("Host=localhost;Database=DummyDatabase;Application Name=foo", async: true);

        Assert.Equal("Host=localhost;Database=DummyDatabase;Application Name=foo", clone.ConnectionString);
    }

    public static NpgsqlRelationalConnection CreateConnection(DbContextOptions? options = null, DbDataSource? dataSource = null)
    {
        options ??= new DbContextOptionsBuilder()
            .UseNpgsql(@"Host=localhost;Database=NpgsqlConnectionTest;Username=some_user;Password=some_password")
            .Options;

        foreach (var extension in options.Extensions)
        {
            extension.Validate(options);
        }

        var dbContextOptions = CreateOptions();

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
                    dbContextOptions),
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
                        new ExceptionDetector()))),
            new NpgsqlDataSourceManager([]),
            dbContextOptions);
    }

    private const string ConnectionString = "Fake Connection String";

    private static IDbContextOptions CreateOptions(RelationalOptionsExtension? optionsExtension = null)
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
        public FakeDbContext()
        {
        }

        public FakeDbContext(DbContextOptions<FakeDbContext> options)
            : base(options)
        {
        }
    }

    private class ConfigurableContext(string connectionString, Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(connectionString, npgsqlOptionsAction);
    }

    private enum Mood
    {
        // ReSharper disable once UnusedMember.Local
        Happy,
        // ReSharper disable once UnusedMember.Local
        Sad
    }
}
