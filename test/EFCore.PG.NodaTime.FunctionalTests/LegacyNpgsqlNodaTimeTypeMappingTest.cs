using Microsoft.EntityFrameworkCore.Storage.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

#if DEBUG

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    [Collection("LegacyNodaTimeTest")]
    public class LegacyNpgsqlNodaTimeTypeMappingTest
        : IClassFixture<LegacyNpgsqlNodaTimeTypeMappingTest.LegacyNpgsqlNodaTimeTypeMappingFixture>
    {
        [Fact]
        public void Timestamp_maps_to_Instant_by_default()
            => Assert.Same(typeof(Instant), GetMapping("timestamp without time zone").ClrType);

        [Fact]
        public void Timestamptz_maps_to_Instant_by_default()
            => Assert.Same(typeof(Instant), GetMapping("timestamp with time zone").ClrType);

        [Fact]
        public void LocalDateTime_does_not_map_to_timestamptz()
            => Assert.Null(GetMapping(typeof(LocalDateTime), "timestamp with time zone"));

        [Fact]
        public void GenerateSqlLiteral_returns_instant_literal()
        {
            var mapping = GetMapping(typeof(Instant));
            Assert.Equal("timestamp without time zone", mapping.StoreType);

            var instant = (new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660)).InUtc().ToInstant();
            Assert.Equal("TIMESTAMP '2018-04-20T10:31:33.666666Z'", mapping.GenerateSqlLiteral(instant));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_instant_infinity_literal()
        {
            var mapping = GetMapping(typeof(Instant));
            Assert.Equal(typeof(Instant), mapping.ClrType);
            Assert.Equal("timestamp without time zone", mapping.StoreType);

            Assert.Equal("TIMESTAMP '-infinity'", mapping.GenerateSqlLiteral(Instant.MinValue));
            Assert.Equal("TIMESTAMP 'infinity'", mapping.GenerateSqlLiteral(Instant.MaxValue));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_instant_range_in_legacy_mode()
        {
            var mapping = (NpgsqlRangeTypeMapping)GetMapping(typeof(NpgsqlRange<Instant>));
            Assert.Equal("tsrange", mapping.StoreType);
            Assert.Equal("timestamp without time zone", mapping.SubtypeMapping.StoreType);

            var value = new NpgsqlRange<Instant>(
                new LocalDateTime(2020, 1, 1, 12, 0, 0).InUtc().ToInstant(),
                new LocalDateTime(2020, 1, 2, 12, 0, 0).InUtc().ToInstant());
            Assert.Equal(@"'[""2020-01-01T12:00:00Z"",""2020-01-02T12:00:00Z""]'::tsrange", mapping.GenerateSqlLiteral(value));
        }

        #region Support

        private static readonly NpgsqlTypeMappingSource Mapper = new(
            new TypeMappingSourceDependencies(
                new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                new JsonValueReaderWriterSource(new JsonValueReaderWriterSourceDependencies()),
                Array.Empty<ITypeMappingSourcePlugin>()),
            new RelationalTypeMappingSourceDependencies(
                new IRelationalTypeMappingSourcePlugin[]
                {
                    new NpgsqlNodaTimeTypeMappingSourcePlugin(
                        new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()))
                }),
            new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
            new NpgsqlSingletonOptions()
        );

        private static RelationalTypeMapping GetMapping(string storeType)
            => Mapper.FindMapping(storeType);

        private static RelationalTypeMapping GetMapping(Type clrType)
            => Mapper.FindMapping(clrType);

        private static RelationalTypeMapping GetMapping(Type clrType, string storeType)
            => Mapper.FindMapping(clrType, storeType);

        private class LegacyNpgsqlNodaTimeTypeMappingFixture : IDisposable
        {
            public LegacyNpgsqlNodaTimeTypeMappingFixture()
            {
                NpgsqlNodaTimeTypeMappingSourcePlugin.LegacyTimestampBehavior = true;
            }

            public void Dispose()
                => NpgsqlNodaTimeTypeMappingSourcePlugin.LegacyTimestampBehavior = false;
        }

        #endregion Support
    }

    [CollectionDefinition("LegacyNodaTimeTest", DisableParallelization = true)]
    public class EventSourceTestCollection
    {
    }
}

#endif
