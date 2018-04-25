using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
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

            var instant = new LocalDateTime(2018, 4, 20, 10, 31, 33, 666).InUtc().ToInstant();
            Assert.Equal("TIMESTAMP '2018-04-20T10:31:33.666Z'", mapping.GenerateSqlLiteral(instant));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_local_date_time_literal()
        {
            var mapping = GetMapping(typeof(LocalDateTime));

            var localDateTime = new LocalDateTime(2018, 4, 20, 10, 31, 33, 666);
            Assert.Equal("TIMESTAMP '2018-04-20T10:31:33.666'", mapping.GenerateSqlLiteral(localDateTime));
        }

        [Fact(Skip = "Need to implement")]
        public void GenerateSqlLiteral_returns_zoned_date_time_literal()
        {
            var mapping = GetMapping(typeof(ZonedDateTime));

            throw new NotImplementedException();
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
        }

        [Fact(Skip = "Need to implement")]
        public void GenerateSqlLiteral_returns_offset_date_time_literal()
        {
            var mapping = GetMapping(typeof(LocalTime));

            throw new NotImplementedException();
        }

        [Fact]
        public void GenerateSqlLiteral_returns_period_literal()
        {
            var mapping = GetMapping(typeof(Period));

            var hms = Period.FromHours(4) + Period.FromMinutes(3) + Period.FromSeconds(2);
            Assert.Equal("INTERVAL 'PT4H3M2S'", mapping.GenerateSqlLiteral(hms));

            var withMilliseconds = hms + Period.FromMilliseconds(1);
            Assert.Equal("INTERVAL 'PT4H3M2.001S'", mapping.GenerateSqlLiteral(withMilliseconds));

            var withYearMonthDay = hms + Period.FromYears(2018) + Period.FromMonths(4) + Period.FromDays(20);
            Assert.Equal("INTERVAL 'P2018Y4M20DT4H3M2S'", mapping.GenerateSqlLiteral(withYearMonthDay));
        }

        #region Support

        static NpgsqlNodaTimeTypeMappingTest()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            var npgsqlBuilder = new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNodaTime();
            var options = new NpgsqlOptions();
            options.Initialize(optionsBuilder.Options);

            Mapper = new NpgsqlTypeMappingSource(
                new TypeMappingSourceDependencies(
                    new ValueConverterSelector(new ValueConverterSelectorDependencies())
                ),
                new RelationalTypeMappingSourceDependencies(),
                options
            );
        }

        static readonly NpgsqlTypeMappingSource Mapper;

        static RelationalTypeMapping GetMapping(string storeType)
            => Mapper.FindMapping(storeType);

        public static RelationalTypeMapping GetMapping(Type clrType)
            => (RelationalTypeMapping)Mapper.FindMapping(clrType);

        #endregion Support
    }
}
