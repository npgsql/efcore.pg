using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class SystemColumnTest : IClassFixture<SystemColumnTest.SystemColumnFixture>
{
    private SystemColumnFixture Fixture { get; }

    public SystemColumnTest(SystemColumnFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public void Xmin()
    {
        using var context = CreateContext();

        var e = new SomeEntity { Name = "Bart" };
        context.Entities.Add(e);
        context.SaveChanges();
        var firstVersion = e.Version;

        e.Name = "Lisa";
        context.SaveChanges();
        var secondVersion = e.Version;

        Assert.NotEqual(firstVersion, secondVersion);
    }

    public class SystemColumnContext : PoolableDbContext
    {
        public SystemColumnContext(DbContextOptions options)
            : base(options)
        {
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<SomeEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
            => builder.Entity<SomeEntity>().Property(e => e.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public class SomeEntity
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public int Id { get; set; }
        public string Name { get; set; }

        public uint Version { get; set; }
        // ReSharper restore UnusedMember.Global
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    private SystemColumnContext CreateContext()
        => Fixture.CreateContext();

    public class SystemColumnFixture : SharedStoreFixtureBase<SystemColumnContext>
    {
        protected override string StoreName
            => "SystemColumnTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
