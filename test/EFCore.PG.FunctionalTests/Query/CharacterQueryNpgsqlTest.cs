using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class CharacterQueryNpgsqlTest : IClassFixture<CharacterQueryNpgsqlTest.CharacterQueryNpgsqlFixture>
{
    private CharacterQueryNpgsqlFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public CharacterQueryNpgsqlTest(CharacterQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Tests

    [Fact]
    public void Find_in_database()
    {
        Fixture.ClearEntities();

        // important: add here so they aren't locally available below.
        using (var ctx = CreateContext())
        {
            ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "12345678" });
            ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "123456  " });
            ctx.SaveChanges();
        }

        using (var ctx = CreateContext())
        {
            const string update = "update";

            var m1 = ctx.CharacterTestEntities.Find("12345678");
            Assert.NotNull(m1);
            m1.Character6 = update;
            ctx.SaveChanges();

            var m2 = ctx.CharacterTestEntities.Find("123456  ");
            Assert.NotNull(m2);
            m2.Character6 = update;
            ctx.SaveChanges();

            var item0 = ctx.CharacterTestEntities.Find("12345678").Character6;
            Assert.Equal(update, item0);

            var item1 = ctx.CharacterTestEntities.Find("123456  ").Character6;
            Assert.Equal(update, item1);
        }
    }

    [Fact]
    public void Find_locally_available()
    {
        Fixture.ClearEntities();

        // important: add here so they are locally available below.
        using var ctx = CreateContext();
        ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "12345678" });
        ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "123456  " });
        ctx.SaveChanges();

        const string update = "update";

        var m1 = ctx.CharacterTestEntities.Find("12345678");
        m1.Character6 = update;
        ctx.SaveChanges();

        var m2 = ctx.CharacterTestEntities.Find("123456  ");
        m2.Character6 = update;
        ctx.SaveChanges();

        var item0 = ctx.CharacterTestEntities.Find("12345678").Character6;
        Assert.Equal(update, item0);

        var item1 = ctx.CharacterTestEntities.Find("123456  ").Character6;
        Assert.Equal(update, item1);
    }

    /// <summary>
    ///     Test something like: select '123456  '::char(8) = '123456'::char(8);
    /// </summary>
    [Fact]
    public void Test_change_tracking()
    {
        Fixture.ClearEntities();

        using var ctx = CreateContext();
        const string update = "update";

        ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "12345678" });
        ctx.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "123456  " });
        ctx.SaveChanges();

        var m1 = ctx.CharacterTestEntities.Find("12345678");
        m1.Character6 = update;
        ctx.SaveChanges();

        var m2 = ctx.CharacterTestEntities.Find("123456  ");
        m2.Character6 = update;
        ctx.SaveChanges();
    }

    /// <summary>
    ///     Test that comparisons are treated correctly.
    /// </summary>
    [Fact]
    public void Test_change_tracking_key_sizes()
    {
        Fixture.ClearEntities();

        using (var ctx = CreateContext())
        {
            var entity = new CharacterTestEntity { Character8 = "123456  ", Character6 = "12345 " };
            ctx.CharacterTestEntities.Add(entity);
            ctx.SaveChanges();

            // In memory, the properties are unchanged.
            Assert.Equal("12345 ", entity.Character6);

            // Trailing whitespace is ignored when querying.
            var fromLocal = ctx.CharacterTestEntities.Single(x => x.Character6 == "12345");

            // And since we queried the same context, we received the same object.
            Assert.Equal(entity, fromLocal);

            // Which means that the property actually still has trailing whitespace...
            Assert.Equal("12345 ", fromLocal.Character6);

            // No changes are detected/saved when trailing whitespace is added.
            entity.Character6 += "     ";
            Assert.Equal(0, ctx.SaveChanges());
        }

        using (var ctx = CreateContext())
        {
            // The query still ignores the trailing whitespace,
            // but the materialized object won't have any trailing whitespace.
            var fromDb = ctx.CharacterTestEntities.Single(x => x.Character6 == "12345    ");

            // BUG: Why isn't the local cache clean? This shouldn't have the trailing whitespace.
            Assert.Equal("12345 ", fromDb.Character6);
        }
    }

    #endregion

    #region Fixture

    // ReSharper disable once ClassNeverInstantiated.Global
    /// <summary>
    ///     Represents a fixture suitable for testing character data.
    /// </summary>
    public class CharacterQueryNpgsqlFixture : SharedStoreFixtureBase<CharacterContext>
    {
        protected override string StoreName
            => "CharacterQueryNpgsqlTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        /// <summary>
        ///     Clears the entities in the context.
        /// </summary>
        public void ClearEntities()
        {
            using var ctx = CreateContext();
            var entities = ctx.CharacterTestEntities.ToArray();
            foreach (var e in entities)
            {
                ctx.CharacterTestEntities.Remove(e);
            }

            ctx.SaveChanges();
        }
    }

    public class CharacterTestEntity
    {
        public string Character8 { get; set; }
        public string Character6 { get; set; }
    }

    public class CharacterContext : PoolableDbContext
    {
        public DbSet<CharacterTestEntity> CharacterTestEntities { get; set; }

        /// <summary>
        ///     Initializes a <see cref="CharacterContext" />.
        /// </summary>
        /// <param name="options">
        ///     The options to be used for configuration.
        /// </param>
        public CharacterContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder builder)
            => builder.Entity<CharacterTestEntity>(
                entity =>
                {
                    entity.HasKey(e => e.Character8);
                    entity.Property(e => e.Character8).HasColumnType("character(8)");
                    entity.Property(e => e.Character6).HasColumnType("character(6)");
                });
    }

    #endregion

    #region Helpers

    protected CharacterContext CreateContext()
        => Fixture.CreateContext();

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    ///     Asserts that the SQL fragment appears in the logs.
    /// </summary>
    /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
    public void AssertContainsSql(string sql)
        => Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);

    #endregion
}
