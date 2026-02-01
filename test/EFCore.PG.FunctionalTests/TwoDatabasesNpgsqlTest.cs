namespace Microsoft.EntityFrameworkCore;

public class TwoDatabasesNpgsqlTest(NpgsqlFixture fixture) : TwoDatabasesTestBase(fixture), IClassFixture<NpgsqlFixture>
{
    protected new NpgsqlFixture Fixture
        => (NpgsqlFixture)base.Fixture;

    protected override DbContextOptionsBuilder CreateTestOptions(
        DbContextOptionsBuilder optionsBuilder,
        bool withConnectionString = false,
        bool withNullConnectionString = false)
        => withConnectionString
            ? withNullConnectionString
                ? optionsBuilder.UseNpgsql((string?)null)
                : optionsBuilder.UseNpgsql(DummyConnectionString)
            : optionsBuilder.UseNpgsql();

    protected override TwoDatabasesWithDataContext CreateBackingContext(string databaseName)
        => new(Fixture.CreateOptions(NpgsqlTestStore.Create(databaseName)));

    protected override string DummyConnectionString { get; } = "Host=localhost;Database=DoesNotExist";
}
