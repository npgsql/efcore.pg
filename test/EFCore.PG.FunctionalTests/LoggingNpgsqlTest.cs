using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class LoggingNpgsqlTest : LoggingRelationalTestBase<NpgsqlDbContextOptionsBuilder, NpgsqlOptionsExtension>
{
    [Fact]
    public void Logs_context_initialization_admin_database()
        => Assert.Equal(
            ExpectedMessage($"AdminDatabase=foo {DefaultOptions}"),
            ActualMessage(s => CreateOptionsBuilder(s, b => ((NpgsqlDbContextOptionsBuilder)b).UseAdminDatabase("foo"))));

    [Fact]
    public void Logs_context_initialization_postgres_version()
        => Assert.Equal(
            ExpectedMessage($"PostgresVersion=10.7 {DefaultOptions}"),
            ActualMessage(s => CreateOptionsBuilder(s, b => ((NpgsqlDbContextOptionsBuilder)b).SetPostgresVersion(Version.Parse("10.7")))));

    [Fact]
    public void Logs_context_initialization_provide_client_certificates_callback()
        => Assert.Equal(
            ExpectedMessage($"ProvideClientCertificatesCallback {DefaultOptions}"),
            ActualMessage(
                s => CreateOptionsBuilder(
                    s, b => ((NpgsqlDbContextOptionsBuilder)b).ProvideClientCertificatesCallback(_ => { }))));

    [Fact]
    public void Logs_context_initialization_provide_password_callback()
        => Assert.Equal(
            ExpectedMessage($"ProvidePasswordCallback {DefaultOptions}"),
            ActualMessage(
                s => CreateOptionsBuilder(
                    s, b => ((NpgsqlDbContextOptionsBuilder)b).ProvidePasswordCallback((_, _, _, _) => "password"))));

    [Fact]
    public void Logs_context_initialization_remote_certificate_validation_callback()
        => Assert.Equal(
            ExpectedMessage($"RemoteCertificateValidationCallback {DefaultOptions}"),
            ActualMessage(
                s => CreateOptionsBuilder(
                    s,
                    b => ((NpgsqlDbContextOptionsBuilder)b).RemoteCertificateValidationCallback((_, _, _, _) => true))));

    [Fact]
    public void Logs_context_initialization_reverse_null_ordering()
        => Assert.Equal(
            ExpectedMessage($"ReverseNullOrdering {DefaultOptions}"),
            ActualMessage(s => CreateOptionsBuilder(s, b => ((NpgsqlDbContextOptionsBuilder)b).ReverseNullOrdering())));

    [Fact]
    public void Logs_context_initialization_user_range_definitions()
        => Assert.Equal(
            ExpectedMessage($"UserRangeDefinitions=[{typeof(int)}=>int4range] " + DefaultOptions),
            ActualMessage(s => CreateOptionsBuilder(s, b => ((NpgsqlDbContextOptionsBuilder)b).MapRange<int>("int4range"))));

    protected override DbContextOptionsBuilder CreateOptionsBuilder(
        IServiceCollection services,
        Action<RelationalDbContextOptionsBuilder<NpgsqlDbContextOptionsBuilder, NpgsqlOptionsExtension>> relationalAction)
        => new DbContextOptionsBuilder()
            .UseInternalServiceProvider(services.AddEntityFrameworkNpgsql().BuildServiceProvider())
            .UseNpgsql("Data Source=LoggingNpgsqlTest.db", relationalAction);

    protected override TestLogger CreateTestLogger()
        => new TestLogger<NpgsqlLoggingDefinitions>();

    protected override string ProviderName
        => "Npgsql.EntityFrameworkCore.PostgreSQL";

    protected override string ProviderVersion
        => typeof(NpgsqlOptionsExtension).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
}
