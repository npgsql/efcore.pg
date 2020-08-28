using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage
{
    public class NpgsqlTypeMappingSourceTest
    {
        [Theory]
        [InlineData("integer", typeof(int), null, false)]
        [InlineData("integer[]", typeof(int[]), null, false)]
        [InlineData("int", typeof(int), null, false, "int")]
        [InlineData("int[]", typeof(int[]), null, false, "int[]")]
        [InlineData("text", typeof(string), null, false)]
        [InlineData("TEXT", typeof(string), null, false, "TEXT")]
        [InlineData("timestamp with time zone", typeof(DateTime), null, false)]
        [InlineData("dummy", typeof(DummyType), null, false)]
        [InlineData("int4range", typeof(NpgsqlRange<int>), null, false)]
        [InlineData("floatrange", typeof(NpgsqlRange<float>), null, false)]
        [InlineData("dummyrange", typeof(NpgsqlRange<DummyType>), null, false)]
        [InlineData("geometry", typeof(Geometry), null, false)]
        [InlineData("geometry(Polygon)", typeof(Polygon), null, false)]
        [InlineData("geography(Point, 4326)", typeof(Point), null, false)]
        [InlineData("geometry(pointz, 4326)", typeof(Point), null, false)]
        [InlineData("geography(LineStringZM)", typeof(LineString), null, false)]
        [InlineData("geometry(POLYGONM)", typeof(Polygon), null, false)]
        public void By_StoreType(string typeName, Type type, int? size, bool fixedLength, string expectedType = null)
        {
            var mapping = Source.FindMapping(typeName);

            Assert.Same(type, mapping.ClrType);
            Assert.Equal(size, mapping.Size);
            Assert.False(mapping.IsUnicode);
            Assert.Equal(fixedLength, mapping.IsFixedLength);
            Assert.Equal(expectedType ?? typeName, mapping.StoreType);
        }

        [Fact]
        public void Varchar32()
        {
            var mapping = Source.FindMapping("varchar(32)");
            Assert.Same(typeof(string), mapping.ClrType);
            Assert.Equal("varchar(32)", mapping.StoreType);
            Assert.Equal(32, mapping.Size);
        }

        [Fact]
        public void Varchar32_Array()
        {
            var mapping = Source.FindMapping("varchar(32)[]");

            var arrayMapping = Assert.IsType<NpgsqlArrayArrayTypeMapping>(mapping);
            Assert.Same(typeof(string[]), arrayMapping.ClrType);
            Assert.Equal("varchar(32)[]", arrayMapping.StoreType);
            Assert.Null(arrayMapping.Size);

            var elementMapping = arrayMapping.ElementMapping;
            Assert.Same(typeof(string), elementMapping.ClrType);
            Assert.Equal("varchar(32)", elementMapping.StoreType);
            Assert.Equal(32, elementMapping.Size);
        }

        [Fact]
        public void Timestamp_without_time_zone_5()
        {
            var mapping = Source.FindMapping("timestamp(5) without time zone");
            Assert.Same(typeof(DateTime), mapping.ClrType);
            Assert.Equal("timestamp(5) without time zone", mapping.StoreType);
            // Precision/Scale not actually exposed on RelationalTypeMapping...
        }

        [Fact]
        public void Timestamp_without_time_zone_Array_5()
        {
            var arrayMapping = Assert.IsType<NpgsqlArrayArrayTypeMapping>(Source.FindMapping("timestamp(5) without time zone[]"));
            Assert.Same(typeof(DateTime[]), arrayMapping.ClrType);
            Assert.Equal("timestamp(5) without time zone[]", arrayMapping.StoreType);

            var elementMapping = arrayMapping.ElementMapping;
            Assert.Same(typeof(DateTime), elementMapping.ClrType);
            Assert.Equal("timestamp(5) without time zone", elementMapping.StoreType);
        }

        [Theory]
        [InlineData(typeof(int), "integer")]
        [InlineData(typeof(int[]), "integer[]")]
        [InlineData(typeof(byte[]), "bytea")]
        [InlineData(typeof(DummyType), "dummy")]
        [InlineData(typeof(NpgsqlRange<int>), "int4range")]
        [InlineData(typeof(NpgsqlRange<float>), "floatrange")]
        [InlineData(typeof(NpgsqlRange<DummyType>), "dummyrange")]
        [InlineData(typeof(Geometry), "geometry")]
        [InlineData(typeof(Point), "geometry")]
        public void By_ClrType(Type clrType, string expectedStoreType)
            => Assert.Equal(expectedStoreType, ((RelationalTypeMapping)Source.FindMapping(clrType)).StoreType);

        [Theory]
        [InlineData("integer", typeof(int))]
        [InlineData("integer[]", typeof(int[]))]
        [InlineData("smallint[]", typeof(byte[]))]
        [InlineData("dummy", typeof(DummyType))]
        [InlineData("int4range", typeof(NpgsqlRange<int>))]
        [InlineData("floatrange", typeof(NpgsqlRange<float>))]
        [InlineData("dummyrange", typeof(NpgsqlRange<DummyType>))]
        [InlineData("geometry", typeof(Geometry))]
        [InlineData("geometry(Point, 4326)", typeof(Geometry))]
        public void By_StoreType_with_ClrType(string storeType, Type clrType)
            => Assert.Equal(storeType, Source.FindMapping(clrType, storeType).StoreType);

        [Theory]
        [InlineData("integer", typeof(UnknownType))]
        //[InlineData("integer[]", typeof(UnknownType))] TODO Implement
        [InlineData("dummy", typeof(UnknownType))]
        [InlineData("int4range", typeof(UnknownType))]
        [InlineData("floatrange", typeof(UnknownType))]
        [InlineData("dummyrange", typeof(UnknownType))]
        [InlineData("geometry", typeof(UnknownType))]
        public void By_StoreType_with_wrong_ClrType(string storeType, Type wrongClrType)
            => Assert.Null(Source.FindMapping(wrongClrType, storeType));

        // Happens when using domain/aliases: we don't know about the domain but continue with the mapping based on the ClrType
        [Fact]
        public void Unknown_StoreType_with_known_ClrType()
            => Assert.Equal("some_domain", Source.FindMapping(typeof(int), "some_domain").StoreType);

        #region Support

        public NpgsqlTypeMappingSourceTest()
        {
            var builder = new DbContextOptionsBuilder();
            new NpgsqlDbContextOptionsBuilder(builder).MapRange<float>("floatrange");
            new NpgsqlDbContextOptionsBuilder(builder).MapRange<DummyType>("dummyrange", subtypeName: "dummy");
            var options = new NpgsqlOptions();
            options.Initialize(builder.Options);

            Source = new NpgsqlTypeMappingSource(
                new TypeMappingSourceDependencies(
                    new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                    Array.Empty<ITypeMappingSourcePlugin>()),
                new RelationalTypeMappingSourceDependencies(
                    new IRelationalTypeMappingSourcePlugin[] {
                        new NpgsqlNetTopologySuiteTypeMappingSourcePlugin(new NpgsqlNetTopologySuiteOptions()),
                        new DummyTypeMappingSourcePlugin()
                    }),
                new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                options);
        }

        NpgsqlTypeMappingSource Source { get; }

        class DummyTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
        {
            public RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
                => mappingInfo.StoreTypeName != null
                   ? mappingInfo.StoreTypeName == "dummy" && (mappingInfo.ClrType == null || mappingInfo.ClrType == typeof(DummyType))
                       ? _dummyMapping
                       : null
                   : mappingInfo.ClrType == typeof(DummyType)
                       ? _dummyMapping
                       : null;

            DummyMapping _dummyMapping = new DummyMapping();

            class DummyMapping : RelationalTypeMapping
            {
                // TODO: The DbType is a hack, we currently require of range subtype mapping that they other expose an NpgsqlDbType
                // or a DbType (from which NpgsqlDbType is computed), since RangeTypeMapping sends an NpgsqlDbType.
                // This means we currently don't support ranges over types without NpgsqlDbType, which are accessible via
                // NpgsqlParameter.DataTypeName
                public DummyMapping() : base("dummy", typeof(DummyType), System.Data.DbType.Guid) {}

                DummyMapping(RelationalTypeMappingParameters parameters) : base(parameters) {}

                protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
                    => new DummyMapping(parameters);
            }
        }

        class DummyType {}

        class UnknownType {}

        #endregion Support
    }
}
