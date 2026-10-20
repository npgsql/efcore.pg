using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

/// <summary>
///     Same as <see cref="DateTimeTranslationsNpgsqlTest" />, but the <see cref="DateTime" /> property is mapped to a PostgreSQL
///     <c>timestamp without time zone</c>, which corresponds to a <see cref="DateTime" /> with <see cref="DateTime.Kind" />
///     <see cref="DateTimeKind.Unspecified" />.
/// </summary>
public class DateTimeTranslationsWithoutTimeZoneTest
    : DateTimeTranslationsTestBase<DateTimeTranslationsWithoutTimeZoneTest.BasicTypesQueryNpgsqlTimestampWithoutTimeZoneFixture>
{
    public DateTimeTranslationsWithoutTimeZoneTest(
        BasicTypesQueryNpgsqlTimestampWithoutTimeZoneFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Now()
    {
        await base.Now();

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE now()::timestamp <> @myDatetime
""");
    }

    public override async Task UtcNow()
    {
        // Overriding to set Kind=Utc for timestamptz. This test generally doesn't make much sense here.
        var myDatetime = DateTime.SpecifyKind(new DateTime(2015, 4, 10), DateTimeKind.Utc);

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.UtcNow != myDatetime));

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE now() <> @myDatetime
""");
    }

    public override async Task Today()
    {
        await base.Today();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" = date_trunc('day', now()::timestamp)
""");
    }

    public override async Task Date()
    {
        await base.Date();

        AssertSql(
            """
@myDatetime='1998-05-04T00:00:00.0000000'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_trunc('day', b."DateTime") = @myDatetime
""");
    }

    public override async Task AddYear()
    {
        await base.AddYear();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('year', b."DateTime" + INTERVAL '1 years')::int = 1999
""");
    }

    [ConditionalFact]
    public virtual async Task Add_subsecond_constants()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().OrderBy(b => b.Id).Select(b => new
            {
                Milliseconds = b.DateTime.AddMilliseconds(1.5),
                NegativeMilliseconds = b.DateTime.AddMilliseconds(-0.125),
                Microseconds = b.DateTime.AddMicroseconds(1500),
                NegativeMicroseconds = b.DateTime.AddMicroseconds(-1)
            }),
            assertOrder: true);

        AssertSql(
            """
SELECT b."DateTime" + 1.5 * INTERVAL '1 milliseconds' AS "Milliseconds", b."DateTime" + -0.125 * INTERVAL '1 milliseconds' AS "NegativeMilliseconds", b."DateTime" + 1500.0 * INTERVAL '1 microseconds' AS "Microseconds", b."DateTime" + -1.0 * INTERVAL '1 microseconds' AS "NegativeMicroseconds"
FROM "BasicTypesEntities" AS b
ORDER BY b."Id" NULLS FIRST
""");
    }

    [ConditionalTheory]
    [InlineData(-1.5)]
    [InlineData(0)]
    [InlineData(1.5)]
    public virtual async Task Add_subsecond_parameters(double value)
    {
        var microseconds = value * 1000;

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTime.AddMilliseconds(value) == b.DateTime.AddMicroseconds(microseconds)));

        AssertSql(
            $"""
@value='{value}'
@microseconds='{microseconds}'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" + @value * INTERVAL '1 milliseconds' = b."DateTime" + @microseconds * INTERVAL '1 microseconds'
""");
    }

    public override async Task Year()
    {
        await base.Year();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('year', b."DateTime")::int = 1998
""");
    }

    public override async Task Month()
    {
        await base.Month();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('month', b."DateTime")::int = 5
""");
    }

    public override async Task DayOfYear()
    {
        await base.DayOfYear();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('doy', b."DateTime")::int = 124
""");
    }

    public override async Task Day()
    {
        await base.Day();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('day', b."DateTime")::int = 4
""");
    }

    public override async Task Hour()
    {
        await base.Hour();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('hour', b."DateTime")::int = 15
""");
    }

    public override async Task Minute()
    {
        await base.Minute();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('minute', b."DateTime")::int = 30
""");
    }

    public override async Task Second()
    {
        await base.Second();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('second', b."DateTime")::int = 10
""");
    }

    public override async Task Millisecond()
    {
        await base.Millisecond();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(date_part('milliseconds', b."DateTime"))::int % 1000 = 123
""");
    }

    [ConditionalFact]
    public virtual async Task Microsecond()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTime.Microsecond == 496));

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('microseconds', b."DateTime")::int % 1000 = 496
""");
    }

    // Nanosecond translation throws because PostgreSQL only supports microsecond precision.
    [ConditionalFact]
    public virtual Task Nanosecond()
        => Assert.ThrowsAsync<NotSupportedException>(
            () => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTime.Nanosecond == 0)));

    [ConditionalFact]
    public virtual async Task Microsecond_nullable()
    {
        await AssertQueryScalar(
            async: true,
            ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Id < 3).Select(b => (int?)b.DateTime!.Value.Microsecond),
            ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Id < 3)
                .Select(b => b.DateTime.HasValue ? b.DateTime.Value.Microsecond : (int?)null));

        AssertSql(
            """
SELECT date_part('microseconds', n."DateTime")::int % 1000
FROM "NullableBasicTypesEntities" AS n
WHERE n."Id" < 3
""");
    }

    [ConditionalFact]
    public virtual async Task Millisecond_nullable()
    {
        await AssertQueryScalar(
            async: true,
            ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Id < 3).Select(b => (int?)b.DateTime!.Value.Millisecond),
            ss => ss.Set<NullableBasicTypesEntity>().Where(b => b.Id < 3)
                .Select(b => b.DateTime.HasValue ? b.DateTime.Value.Millisecond : (int?)null));

        AssertSql(
            """
SELECT floor(date_part('milliseconds', n."DateTime"))::int % 1000
FROM "NullableBasicTypesEntities" AS n
WHERE n."Id" < 3
""");
    }

    public override async Task TimeOfDay()
    {
        await base.TimeOfDay();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime"::time = TIME '00:00:00'
""");
    }

    public override async Task subtract_and_TotalDays()
    {
        await base.subtract_and_TotalDays();

        AssertSql(
            """
@date='1997-01-01T00:00:00.0000000'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE date_part('epoch', b."DateTime" - @date) / 86400.0 > 365.0
""");
    }

    public override async Task Parse_with_constant()
    {
        await base.Parse_with_constant();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" = TIMESTAMP '1998-05-04T15:30:10'
""");
    }

    public override async Task Parse_with_parameter()
    {
        await base.Parse_with_parameter();

        AssertSql(
            """
@Parse='1998-05-04T15:30:10.0000000'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" = @Parse
""");
    }

    public override async Task New_with_constant()
    {
        await base.New_with_constant();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" = TIMESTAMP '1998-05-04T15:30:10'
""");
    }

    public override async Task New_with_parameters()
    {
        await base.New_with_parameters();

        AssertSql(
            """
@p='1998-05-04T15:30:10.0000000'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" = @p
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class BasicTypesQueryNpgsqlTimestampWithoutTimeZoneFixture : BasicTypesQueryNpgsqlFixture
    {
        private BasicTypesData? _expectedData;

        protected override string StoreName
            => "BasicTypesTimestampWithoutTimeZoneTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<BasicTypesEntity>().Property(b => b.DateTime).HasColumnType("timestamp without time zone");
            modelBuilder.Entity<NullableBasicTypesEntity>().Property(b => b.DateTime).HasColumnType("timestamp without time zone");
        }

        protected override Task SeedAsync(BasicTypesContext context)
        {
            _expectedData ??= LoadAndTweakData();
            context.AddRange(_expectedData.BasicTypesEntities);
            context.AddRange(_expectedData.NullableBasicTypesEntities);
            return context.SaveChangesAsync();
        }

        public override ISetSource GetExpectedData()
            => _expectedData ??= LoadAndTweakData();

        private BasicTypesData LoadAndTweakData()
        {
            var data = (BasicTypesData)base.GetExpectedData();

            foreach (var item in data.BasicTypesEntities)
            {
                // Change Kind fo all DateTimes from Utc to Unspecified, as we're mapping to 'timestamp without time zone'
                item.DateTime = DateTime.SpecifyKind(item.DateTime, DateTimeKind.Unspecified);
            }

            // Do the same for the nullable counterparts
            foreach (var item in data.NullableBasicTypesEntities)
            {
                if (item.DateTime.HasValue)
                {
                    item.DateTime = DateTime.SpecifyKind(item.DateTime.Value, DateTimeKind.Unspecified);
                }
            }

            return data;
        }
    }
}
