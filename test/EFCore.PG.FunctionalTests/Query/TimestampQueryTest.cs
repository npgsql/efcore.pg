using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

// The EF Core specification test suite has:
//
// * Northwind Orders.OrderDate and others: DateTime mapped to 'timestamp without time zone' (so Unspecified/Local)
// * GearsOfWar Mission.Timeline: DateTimeOffset (mapped to 'timestamp with time zone')
//
// But there's no DateTime mapped to 'timestamp with time zone' (so Utc).
// This test suite checks this and various PG-specific things.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class TimestampQueryTest : QueryTestBase<TimestampQueryTest.TimestampQueryFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public TimestampQueryTest(TimestampQueryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact]
        public void DateTime_maps_to_timestamptz_by_default()
        {
            using var ctx = CreateContext();

            Assert.Equal(
                "timestamp with time zone",
                ctx.Model.GetEntityTypes().Single().GetProperty(nameof(Entity.TimestamptzDateTime)).GetColumnType());
        }

        [ConditionalFact]
        public async Task Cannot_insert_utc_datetime_into_timestamp()
        {
            using var ctx = CreateContext();

            ctx.Entities.Add(new() { TimestampDateTime = DateTime.UtcNow });
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
            Assert.IsType<InvalidCastException>(exception.InnerException);
        }

        [ConditionalFact]
        public async Task Cannot_insert_unspecified_datetime_into_timestamptz()
        {
            using var ctx = CreateContext();

            ctx.Entities.Add(new() { TimestamptzDateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified) });
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
            Assert.IsType<InvalidCastException>(exception.InnerException);
        }

        [ConditionalFact]
        public async Task Cannot_insert_local_datetime_into_timestamptz()
        {
            using var ctx = CreateContext();

            ctx.Entities.Add(new() { TimestamptzDateTime = DateTime.Now });
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
            Assert.IsType<InvalidCastException>(exception.InnerException);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Compare_timestamp_column_to_local_DateTime_literal(bool async)
        {
            using var ctx = CreateContext();

            // Note that we're in the Europe/Berlin timezone (see CreateContext)
            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e => e.TimestampDateTime == new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Local)),
                entryCount: 1);

            AssertSql(
                @"SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE e.""TimestampDateTime"" = TIMESTAMP '1998-04-12 15:26:38'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Compare_timestamp_column_to_local_DateTime_parameter(bool async)
        {
            using var ctx = CreateContext();

            var dateTime = new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Local);

            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e => e.TimestampDateTime == dateTime),
                entryCount: 1);

            // The string representation of our local DateTime is generated with the local time zone (by the EF Core test infra),
            // so we can't assert on it.
            Assert.Contains(
                @"SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE e.""TimestampDateTime"" = @__dateTime_0",
                Fixture.TestSqlLoggerFactory.SqlStatements.Single());
        }

        // Compare_timestamp_column_to_unspecified_DateTime_literal: requires translating DateTime.SpecifyKind

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Compare_timestamp_column_to_unspecified_DateTime_parameter(bool async)
        {
            using var ctx = CreateContext();

            var dateTime = new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Unspecified);

            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e => e.TimestampDateTime == dateTime),
                entryCount: 1);

            AssertSql(
                @"@__dateTime_0='1998-04-12T15:26:38.0000000' (DbType = DateTime)

SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE e.""TimestampDateTime"" = @__dateTime_0");
        }

        [ConditionalFact]
        public async Task Cannot_compare_timestamp_column_to_utc_DateTime_literal()
        {
            using var ctx = CreateContext();

            await Assert.ThrowsAsync<InvalidCastException>(
                () => ctx.Entities.Where(e => e.TimestampDateTime == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc))
                    .ToListAsync());
        }

        [ConditionalFact]
        public async Task Cannot_compare_timestamp_column_to_utc_DateTime_parameter()
        {
            using var ctx = CreateContext();

            var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc);

            await Assert.ThrowsAsync<InvalidCastException>(
                () => ctx.Entities.Where(e => e.TimestampDateTime == dateTime)
                    .ToListAsync());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Compare_timestamptz_column_to_utc_DateTime_literal(bool async)
        {
            using var ctx = CreateContext();

            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e => e.TimestamptzDateTime == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc)),
                entryCount: 1);

            AssertSql(
                @"SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE e.""TimestamptzDateTime"" = TIMESTAMPTZ '1998-04-12 13:26:38Z'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Compare_timestamptz_column_to_utc_DateTime_parameter(bool async)
        {
            using var ctx = CreateContext();

            var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc);

            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e => e.TimestamptzDateTime == dateTime),
                entryCount: 1);

            AssertSql(
                @"@__dateTime_0='1998-04-12T13:26:38.0000000Z' (DbType = DateTimeOffset)

SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE e.""TimestamptzDateTime"" = @__dateTime_0");
        }

        [ConditionalFact]
        public async Task Cannot_compare_timestamptz_column_to_local_DateTime_literal()
        {
            using var ctx = CreateContext();

            await Assert.ThrowsAsync<InvalidCastException>(
                () => ctx.Entities.Where(e => e.TimestamptzDateTime == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local))
                    .ToListAsync());
        }

        [ConditionalFact]
        public async Task Cannot_compare_timestamptz_column_to_local_DateTime_parameter()
        {
            using var ctx = CreateContext();

            var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Local);

            await Assert.ThrowsAsync<InvalidCastException>(
                () => ctx.Entities.Where(e => e.TimestamptzDateTime == dateTime)
                    .ToListAsync());
        }

        [ConditionalFact]
        public async Task Cannot_compare_timestamptz_column_to_unspecified_DateTime_literal()
        {
            using var ctx = CreateContext();

            await Assert.ThrowsAsync<InvalidCastException>(
                () => ctx.Entities.Where(e => e.TimestamptzDateTime == new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified))
                    .ToListAsync());
        }

        [ConditionalFact]
        public async Task Cannot_compare_timestamptz_column_to_unspecified_DateTime_parameter()
        {
            using var ctx = CreateContext();

            var dateTime = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified);

            await Assert.ThrowsAsync<InvalidCastException>(
                () => ctx.Entities.Where(e => e.TimestamptzDateTime == dateTime)
                    .ToListAsync());
        }

        [ConditionalFact]
        public async Task Cannot_compare_timestamptz_column_to_timestamp_column()
        {
            using var ctx = CreateContext();

            await Assert.ThrowsAsync<NotSupportedException>(
                () => ctx.Entities.Where(e => e.TimestamptzDateTime == e.TimestampDateTime)
                    .ToListAsync());
        }

        [ConditionalFact]
        public async Task Compare_timestamptz_column_to_timestamp_column_with_ToUniversalTime()
        {
            using var ctx = CreateContext();

            // We can't use AssertQuery since the local (expected) evaluation is dependent on the machine's timezone, which is out of
            // our control.
            var count = await ctx.Set<Entity>()
                .Where(e => e.TimestamptzDateTime == e.TimestampDateTime.ToUniversalTime())
                .CountAsync();

            Assert.Equal(1, count);

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Entities"" AS e
WHERE e.""TimestamptzDateTime"" = e.""TimestampDateTime""::timestamptz");
        }

        [ConditionalFact]
        public async Task Compare_timestamptz_column_to_timestamp_column_with_ToLocalTime()
        {
            using var ctx = CreateContext();

            // We can't use AssertQuery since the local (expected) evaluation is dependent on the machine's timezone, which is out of
            // our control.
            var count = await ctx.Set<Entity>()
                .Where(e => e.TimestamptzDateTime.ToLocalTime() == e.TimestampDateTime)
                .CountAsync();

            Assert.Equal(1, count);

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Entities"" AS e
WHERE e.""TimestamptzDateTime""::timestamp = e.""TimestampDateTime""");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_datetime_now(bool async)
        {
            var myDatetime = new DateTime(2015, 4, 10);

            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(c => DateTime.Now != myDatetime),
                entryCount: 2);

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00.0000000' (DbType = DateTime)

SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE now()::timestamp <> @__myDatetime_0");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_datetime_utcnow(bool async)
        {
            var myDatetime = new DateTime(2015, 4, 10, 0, 0, 0, DateTimeKind.Utc);

            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(c => DateTime.UtcNow != myDatetime),
                entryCount: 2);

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00.0000000Z' (DbType = DateTimeOffset)

SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE now() <> @__myDatetime_0");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_datetime_ctor1(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e =>
                    new DateTime(e.TimestampDateTime.Year, e.TimestampDateTime.Month, 1) == new DateTime(1998, 4, 12)));

            AssertSql(
                @"SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE make_date(date_part('year', e.""TimestampDateTime"")::INT, date_part('month', e.""TimestampDateTime"")::INT, 1) = TIMESTAMP '1998-04-12 00:00:00'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_datetime_ctor2(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e =>
                    new DateTime(e.TimestampDateTime.Year, e.TimestampDateTime.Month, 1, 0, 0, 0) == new DateTime(1998, 4, 12)));

            AssertSql(
                @"SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE make_timestamp(date_part('year', e.""TimestampDateTime"")::INT, date_part('month', e.""TimestampDateTime"")::INT, 1, 0, 0, 0::double precision) = TIMESTAMP '1998-04-12 00:00:00'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_datetime_ctor3_local(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Entity>().Where(e =>
                    new DateTime(e.TimestampDateTime.Year, e.TimestampDateTime.Month, 1, 0, 0, 0, DateTimeKind.Local) == new DateTime(1996, 9, 11)));

            AssertSql(
                @"SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE make_timestamp(date_part('year', e.""TimestampDateTime"")::INT, date_part('month', e.""TimestampDateTime"")::INT, 1, 0, 0, 0::double precision) = TIMESTAMP '1996-09-11 00:00:00'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
         public virtual async Task Where_datetime_ctor3_utc(bool async)
         {
             using var ctx = CreateContext();

             await AssertQuery(
                 async,
                 ss => ss.Set<Entity>().Where(
                     o =>
                         new DateTime(o.TimestamptzDateTime.Year, o.TimestamptzDateTime.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                         == new DateTime(1998, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
                 entryCount: 1);

             AssertSql(
                 @"SELECT e.""Id"", e.""TimestampDateTime"", e.""TimestampDateTimeOffset"", e.""TimestamptzDateTime""
FROM ""Entities"" AS e
WHERE make_timestamptz(date_part('year', e.""TimestamptzDateTime"")::INT, date_part('month', e.""TimestamptzDateTime"")::INT, 1, 0, 0, 0::double precision, 'UTC') = TIMESTAMPTZ '1998-04-01 00:00:00Z'");
         }

        #region Support

        private TimestampQueryContext CreateContext()
        {
            var ctx = Fixture.CreateContext();

            // Set the PostgreSQL TimeZone parameter to something local, to ensure that operations which take TimeZone into account don't
            // depend on the database's time zone, and also that operations which shouldn't take TimeZone into account indeed don't.
            ctx.Database.BeginTransaction();
            ctx.Database.ExecuteSqlRaw("SET TimeZone='Europe/Berlin'");
            Fixture.TestSqlLoggerFactory.Clear();

            return ctx;
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class TimestampQueryContext : PoolableDbContext
        {
            public DbSet<Entity> Entities { get; set; }

            public TimestampQueryContext(DbContextOptions options) : base(options) {}

            public static void Seed(TimestampQueryContext context)
            {
                context.Entities.AddRange(TimestampData.CreateEntities());
                context.SaveChanges();
            }
        }

        public class Entity
        {
            public int Id { get; set; }
            public DateTime TimestamptzDateTime { get; set; }
            [Column(TypeName = "timestamp without time zone")]
            public DateTime TimestampDateTime { get; set; }
            public DateTimeOffset TimestampDateTimeOffset { get; set; }
        }

        #nullable restore

        public class TimestampQueryFixture : SharedStoreFixtureBase<TimestampQueryContext>, IQueryFixtureBase
        {
            protected override string StoreName => "TimestampQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            private TimestampData _expectedData;

            protected override void Seed(TimestampQueryContext context) => TimestampQueryContext.Seed(context);

            public Func<DbContext> GetContextCreator()
                => CreateContext;

            public ISetSource GetExpectedData()
                => _expectedData ??= new TimestampData();

            public IReadOnlyDictionary<Type, object> GetEntitySorters()
                => new Dictionary<Type, Func<object, object>> { { typeof(Entity), e => ((Entity)e)?.Id } }
                    .ToDictionary(e => e.Key, e => (object)e.Value);

            public IReadOnlyDictionary<Type, object> GetEntityAsserters()
                => new Dictionary<Type, Action<object, object>>
                {
                    {
                        typeof(Entity), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (Entity)e;
                                var aa = (Entity)a;

                                Assert.Equal(ee.Id, aa.Id);
                                Assert.Equal(ee.TimestamptzDateTime, aa.TimestamptzDateTime);
                                Assert.Equal(ee.TimestampDateTime, aa.TimestampDateTime);
                                Assert.Equal(ee.TimestampDateTimeOffset, aa.TimestampDateTimeOffset);
                            }
                        }
                    }
                }.ToDictionary(e => e.Key, e => (object)e.Value);
        }

        protected class TimestampData : ISetSource
        {
            public IReadOnlyList<Entity> Entities { get; }

            public TimestampData()
                => Entities = CreateEntities();

            public IQueryable<TEntity> Set<TEntity>()
                where TEntity : class
            {
                if (typeof(TEntity) == typeof(Entity))
                {
                    return (IQueryable<TEntity>)Entities.AsQueryable();
                }

                throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
            }

            public static IReadOnlyList<Entity> CreateEntities()
            {
                var utcDateTime1 = new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc);
                var localDateTime1 = new DateTime(1998, 4, 12, 15, 26, 38, DateTimeKind.Local);
                var utcDateTime2 = new DateTime(2015, 1, 27, 8, 45, 12, 345, DateTimeKind.Utc);
                var unspecifiedDateTime2 = new DateTime(2015, 1, 27, 10, 45, 12, 345, DateTimeKind.Unspecified);

                return new List<Entity>
                {
                    new()
                    {
                        Id = 1,
                        TimestamptzDateTime = utcDateTime1,
                        TimestampDateTime = localDateTime1,
                        TimestampDateTimeOffset = new DateTimeOffset(utcDateTime1)
                    },
                    new()
                    {
                        Id = 2,
                        TimestamptzDateTime = utcDateTime2,
                        TimestampDateTime = unspecifiedDateTime2,
                        TimestampDateTimeOffset = new DateTimeOffset(utcDateTime2)
                    }
                };
            }
        }

        #endregion
    }
}
