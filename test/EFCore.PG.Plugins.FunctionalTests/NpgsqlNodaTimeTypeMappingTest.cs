using System;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;
using NodaTime.TimeZones;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlNodaTimeTypeMappingTest
    {
        [Fact]
        public void GenerateSqlLiteral_returns_instant_literal()
        {
            var mapping = GetMapping(typeof(Instant));
            Assert.Equal("timestamp", mapping.StoreType);

            var instant = (new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660)).InUtc().ToInstant();
            Assert.Equal("TIMESTAMP '2018-04-20T10:31:33.666666Z'", mapping.GenerateSqlLiteral(instant));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_local_date_time_literal()
        {
            var mapping = GetMapping(typeof(LocalDateTime));
            Assert.Equal("timestamp", mapping.StoreType);

            var localDateTime = new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660);
            Assert.Equal("TIMESTAMP '2018-04-20T10:31:33.666666'", mapping.GenerateSqlLiteral(localDateTime));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_timestamptz_instant_literal()
        {
            var mapping = GetMapping(typeof(Instant), "timestamp with time zone");
            Assert.Equal(typeof(Instant), mapping.ClrType);
            Assert.Equal("timestamp with time zone", mapping.StoreType);

            var instant = (new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660)).InUtc().ToInstant();
            Assert.Equal("TIMESTAMPTZ '2018-04-20T10:31:33.666666Z'", mapping.GenerateSqlLiteral(instant));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_zoned_date_time_literal()
        {
            var mapping = GetMapping(typeof(ZonedDateTime));
            Assert.Equal("timestamp with time zone", mapping.StoreType);

            var zonedDateTime = (new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660))
                .InZone(DateTimeZone.ForOffset(Offset.FromHours(2)), Resolvers.LenientResolver);
            Assert.Equal("TIMESTAMPTZ '2018-04-20T10:31:33.666666+02'", mapping.GenerateSqlLiteral(zonedDateTime));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_offset_date_time_literal()
        {
            var mapping = GetMapping(typeof(OffsetDateTime));
            Assert.Equal("timestamp with time zone", mapping.StoreType);

            var offsetDateTime = new OffsetDateTime(
                new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660),
                Offset.FromHours(2));
            Assert.Equal("TIMESTAMPTZ '2018-04-20T10:31:33.666666+02'", mapping.GenerateSqlLiteral(offsetDateTime));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_local_date_literal()
        {
            var mapping = GetMapping(typeof(LocalDate));

            Assert.Equal("DATE '2018-04-20'", mapping.GenerateSqlLiteral(new LocalDate(2018, 4, 20)));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_local_time_literal()
        {
            var mapping = GetMapping(typeof(LocalTime));

            Assert.Equal("TIME '10:31:33'", mapping.GenerateSqlLiteral(new LocalTime(10, 31, 33)));
            Assert.Equal("TIME '10:31:33.666'", mapping.GenerateSqlLiteral(new LocalTime(10, 31, 33, 666)));
            Assert.Equal("TIME '10:31:33.666666'", mapping.GenerateSqlLiteral(new LocalTime(10, 31, 33, 666) + Period.FromTicks(6660)));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_offset_time_literal()
        {
            var mapping = GetMapping(typeof(OffsetTime));

            Assert.Equal("TIMETZ '10:31:33+02'", mapping.GenerateSqlLiteral(
                new OffsetTime(new LocalTime(10, 31, 33), Offset.FromHours(2))));
            Assert.Equal("TIMETZ '10:31:33-02:30'", mapping.GenerateSqlLiteral(
                new OffsetTime(new LocalTime(10, 31, 33), Offset.FromHoursAndMinutes(-2, -30))));
            Assert.Equal("TIMETZ '10:31:33.666666Z'", mapping.GenerateSqlLiteral(
                new OffsetTime(new LocalTime(10, 31, 33, 666) + Period.FromTicks(6660), Offset.Zero)));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_period_literal()
        {
            var mapping = GetMapping(typeof(Period));

            var hms = Period.FromHours(4) + Period.FromMinutes(3) + Period.FromSeconds(2);
            Assert.Equal("INTERVAL 'PT4H3M2S'", mapping.GenerateSqlLiteral(hms));

            var withMilliseconds = hms + Period.FromMilliseconds(1);
            Assert.Equal("INTERVAL 'PT4H3M2.001S'", mapping.GenerateSqlLiteral(withMilliseconds));

            var withMicroseconds = hms + Period.FromTicks(6660);
            Assert.Equal("INTERVAL 'PT4H3M2.000666S'", mapping.GenerateSqlLiteral(withMicroseconds));

            var withYearMonthDay = hms + Period.FromYears(2018) + Period.FromMonths(4) + Period.FromDays(20);
            Assert.Equal("INTERVAL 'P2018Y4M20DT4H3M2S'", mapping.GenerateSqlLiteral(withYearMonthDay));
        }

        #region Support

        static readonly IRelationalTypeMappingSourcePlugin Mapper =
            new NpgsqlNodaTimeTypeMappingSourcePlugin(new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()));

        static RelationalTypeMapping GetMapping(Type clrType)
            => Mapper.FindMapping(new RelationalTypeMappingInfo(clrType));

        static RelationalTypeMapping GetMapping(Type clrType, string storeType)
            => Mapper.FindMapping(new RelationalTypeMappingInfo(clrType, storeType, false, null, null, null, null, null, null));

        #endregion Support
    }
}
