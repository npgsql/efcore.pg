using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Xunit;

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
        public void GenerateSqlLiteral_returns_instant_literal_in_legacy_mode()
        {
            var mapping = GetMapping(typeof(Instant));
            Assert.Equal("timestamp", mapping.StoreType);

            var instant = (new LocalDateTime(2018, 4, 20, 10, 31, 33, 666) + Period.FromTicks(6660)).InUtc().ToInstant();
            Assert.Equal("TIMESTAMP '2018-04-20T10:31:33.666666Z'", mapping.GenerateSqlLiteral(instant));
        }

        #region Support

        private static readonly NpgsqlTypeMappingSource Mapper = new(
            new TypeMappingSourceDependencies(
                new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                Array.Empty<ITypeMappingSourcePlugin>()),
            new RelationalTypeMappingSourceDependencies(
                new IRelationalTypeMappingSourcePlugin[] {
                    new NpgsqlNodaTimeTypeMappingSourcePlugin(new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()))
                }),
            new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
            new NpgsqlOptions()
        );

        private static RelationalTypeMapping GetMapping(string storeType) => Mapper.FindMapping(storeType);

        private static RelationalTypeMapping GetMapping(Type clrType) => (RelationalTypeMapping)Mapper.FindMapping(clrType);

        private static RelationalTypeMapping GetMapping(Type clrType, string storeType)
            => Mapper.FindMapping(clrType, storeType);

        class LegacyNpgsqlNodaTimeTypeMappingFixture : IDisposable
        {
            public LegacyNpgsqlNodaTimeTypeMappingFixture()
                => NpgsqlNodaTimeTypeMappingSourcePlugin.LegacyTimestampBehavior = true;

            public void Dispose()
                => NpgsqlNodaTimeTypeMappingSourcePlugin.LegacyTimestampBehavior = false;
        }

        #endregion Support
    }

    [CollectionDefinition("LegacyNodaTimeTest", DisableParallelization = true)]
    public class EventSourceTestCollection {}
}

#endif
