namespace Microsoft.EntityFrameworkCore.Query;

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

            var item0 = ctx.CharacterTestEntities.Find("12345678")!.Character6;
            Assert.Equal(update, item0);

            var item1 = ctx.CharacterTestEntities.Find("123456  ")!.Character6;
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

        var m1 = ctx.CharacterTestEntities.Find("12345678")!;
        m1.Character6 = update;
        ctx.SaveChanges();

        var m2 = ctx.CharacterTestEntities.Find("123456  ")!;
        m2.Character6 = update;
        ctx.SaveChanges();

        var item0 = ctx.CharacterTestEntities.Find("12345678")!.Character6;
        Assert.Equal(update, item0);

        var item1 = ctx.CharacterTestEntities.Find("123456  ")!.Character6;
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

        var m1 = ctx.CharacterTestEntities.Find("12345678")!;
        m1.Character6 = update;
        ctx.SaveChanges();

        var m2 = ctx.CharacterTestEntities.Find("123456  ")!;
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

    [Theory]
    [InlineData('e', "Seattle")]
    [InlineData(' ', "Seattle")]
    [InlineData(' ', " Seattle ")]
    public void String_functions_with_char_column(char character, string value)
    {
        Fixture.ClearEntities();

        using var context = CreateContext();
        context.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "12345678", Character = character });
        context.SaveChanges();
        Fixture.TestSqlLoggerFactory.Clear();

        var result = context.CharacterTestEntities.Select(e => new
        {
            StartsWith = value.StartsWith(e.Character),
            EndsWith = value.EndsWith(e.Character),
            Contains = value.Contains(e.Character),
            IndexOf = value.IndexOf(e.Character),
            ReplaceOld = value.Replace(e.Character, 'x'),
            ReplaceNew = value.Replace('e', e.Character)
        }).Single();

        Assert.Equal(
            new
            {
                StartsWith = value.StartsWith(character),
                EndsWith = value.EndsWith(character),
                Contains = value.Contains(character),
                IndexOf = value.IndexOf(character),
                ReplaceOld = value.Replace(character, 'x'),
                ReplaceNew = value.Replace('e', character)
            },
            result);

        Fixture.TestSqlLoggerFactory.AssertBaseline(
            [
                $$"""
@value='{{value}}'

SELECT left(@value, length(rpad(c."Character", 1))) = rpad(c."Character", 1) AS "StartsWith", right(@value, length(rpad(c."Character", 1))) = rpad(c."Character", 1) AS "EndsWith", strpos(@value, rpad(c."Character", 1)) > 0 AS "Contains", strpos(@value, rpad(c."Character", 1)) - 1 AS "IndexOf", replace(@value, rpad(c."Character", 1), 'x') AS "ReplaceOld", replace(@value, 'e', rpad(c."Character", 1)) AS "ReplaceNew"
FROM "CharacterTestEntities" AS c
LIMIT 2
"""
            ]);
    }

    [Theory]
    [InlineData('e')]
    [InlineData(' ')]
    public void Text_conversions_with_char(char character)
    {
        Fixture.ClearEntities();

        using var context = CreateContext();
        context.CharacterTestEntities.Add(new CharacterTestEntity { Character8 = "12345678", Character = character });
        context.SaveChanges();
        Fixture.TestSqlLoggerFactory.Clear();

        var result = context.CharacterTestEntities.Select(e => new
        {
            Text = e.Character.ToString(),
            PadLeft = e.Character8!.PadLeft(10, e.Character),
            PadRight = e.Character8!.PadRight(10, e.Character),
            PadRightParameter = e.Character8!.PadRight(10, character),
            Join = string.Join(e.Character, new[] { e.Character8, e.Character8 }),
            JoinParameter = string.Join(character, new[] { e.Character8, e.Character8 })
        }).Single();

        Assert.Equal(
            new
            {
                Text = character.ToString(),
                PadLeft = "12345678".PadLeft(10, character),
                PadRight = "12345678".PadRight(10, character),
                PadRightParameter = "12345678".PadRight(10, character),
                Join = string.Join(character, "12345678", "12345678"),
                JoinParameter = string.Join(character, "12345678", "12345678")
            },
            result);

        Fixture.TestSqlLoggerFactory.AssertBaseline(
            [
                $$"""
@character='{{character}}' (DbType = String)

SELECT rpad(c."Character", 1) AS "Text", CASE
    WHEN length(c."Character8") >= 10 THEN c."Character8"
    ELSE lpad(c."Character8", 10, rpad(c."Character", 1))
END AS "PadLeft", CASE
    WHEN length(c."Character8") >= 10 THEN c."Character8"
    ELSE rpad(c."Character8", 10, rpad(c."Character", 1))
END AS "PadRight", CASE
    WHEN length(c."Character8") >= 10 THEN c."Character8"
    ELSE rpad(c."Character8", 10, @character)
END AS "PadRightParameter", concat_ws(rpad(c."Character", 1), c."Character8", c."Character8") AS "Join", concat_ws(@character, c."Character8", c."Character8") AS "JoinParameter"
FROM "CharacterTestEntities" AS c
LIMIT 2
"""
            ]);
    }

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
        public string? Character8 { get; set; }
        public string? Character6 { get; set; }
        public char Character { get; set; } = ' ';
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
