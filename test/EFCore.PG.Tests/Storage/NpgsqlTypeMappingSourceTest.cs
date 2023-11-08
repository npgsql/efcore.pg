using Microsoft.EntityFrameworkCore.Storage.Json;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage;

public class NpgsqlTypeMappingSourceTest
{
    [Theory]
    [InlineData("integer", typeof(int), null, null, null, false)]
    [InlineData("integer[]", typeof(List<int>), null, null, null, false)]
    [InlineData("int", typeof(int), null, null, null, false)]
    [InlineData("int[]", typeof(List<int>), null, null, null, false)]
    [InlineData("numeric", typeof(decimal), null, null, null, false)]
    [InlineData("numeric(10,2)", typeof(decimal), null, 10, 2, false)]
    [InlineData("text", typeof(string), null, null, null, false)]
    [InlineData("TEXT", typeof(string), null, null, null, false)]
    [InlineData("character(8)", typeof(string), 8, null, null, true)]
    [InlineData("char(8)", typeof(string), 8, null, null, true)]
    [InlineData("character(1)", typeof(char), 1, null, null, true)]
    [InlineData("char(1)", typeof(char), 1, null, null, true)]
    [InlineData("character", typeof(char), null, null, null, true)]
    [InlineData("character varying(8)", typeof(string), 8, null, null, false)]
    [InlineData("varchar(8)", typeof(string), 8, null, null, false)]
    [InlineData("varchar", typeof(string), null, null, null, false)]
    [InlineData("timestamp with time zone", typeof(DateTime), null, null, null, false)]
    [InlineData("dummy", typeof(DummyType), null, null, null, false)]
    [InlineData("int4range", typeof(NpgsqlRange<int>), null, null, null, false)]
    [InlineData("floatrange", typeof(NpgsqlRange<float>), null, null, null, false)]
    [InlineData("dummyrange", typeof(NpgsqlRange<DummyType>), null, null, null, false)]
    [InlineData("int4multirange", typeof(List<NpgsqlRange<int>>), null, null, null, false)]
    [InlineData("geometry", typeof(Geometry), null, null, null, false)]
    [InlineData("geometry(Polygon)", typeof(Polygon), null, null, null, false)]
    [InlineData("geography(Point, 4326)", typeof(Point), null, null, null, false)]
    [InlineData("geometry(pointz, 4326)", typeof(Point), null, null, null, false)]
    [InlineData("geography(LineStringZM)", typeof(LineString), null, null, null, false)]
    [InlineData("geometry(POLYGONM)", typeof(Polygon), null, null, null, false)]
    [InlineData("xid", typeof(uint), null, null, null, false)]
    [InlineData("xid8", typeof(ulong), null, null, null, false)]
    public void By_StoreType(string typeName, Type type, int? size, int? precision, int? scale, bool fixedLength)
    {
        var mapping = CreateTypeMappingSource().FindMapping(typeName);

        Assert.NotNull(mapping);
        Assert.Same(type, mapping.ClrType);
        Assert.Equal(size, mapping.Size);
        Assert.Equal(precision, mapping.Precision);
        Assert.Equal(scale, mapping.Scale);
        Assert.False(mapping.IsUnicode);
        Assert.Equal(fixedLength, mapping.IsFixedLength);
        Assert.Equal(typeName, mapping.StoreType);
    }

    [Fact]
    public void Varchar32()
    {
        var mapping = CreateTypeMappingSource().FindMapping("varchar(32)");
        Assert.Same(typeof(string), mapping.ClrType);
        Assert.Equal("varchar(32)", mapping.StoreType);
        Assert.Equal(32, mapping.Size);
    }

    [Fact]
    public void Varchar32_Array()
    {
        var mapping = CreateTypeMappingSource().FindMapping("varchar(32)[]");

        var arrayMapping = Assert.IsAssignableFrom<NpgsqlArrayTypeMapping>(mapping);
        Assert.Same(typeof(List<string>), arrayMapping.ClrType);
        Assert.Equal("varchar(32)[]", arrayMapping.StoreType);
        Assert.Null(arrayMapping.Size);

        var elementMapping = arrayMapping.ElementTypeMapping;
        Assert.Same(typeof(string), elementMapping.ClrType);
        Assert.Equal("varchar(32)", elementMapping.StoreType);
        Assert.Equal(32, elementMapping.Size);
    }

    [Fact]
    public void Timestamp_without_time_zone_5()
    {
        var mapping = CreateTypeMappingSource().FindMapping("timestamp(5) without time zone");
        Assert.Same(typeof(DateTime), mapping.ClrType);
        Assert.Equal("timestamp(5) without time zone", mapping.StoreType);
        // Precision/Scale not actually exposed on RelationalTypeMapping...
    }

    [Fact]
    public void Timestamp_without_time_zone_Array_5()
    {
        var arrayMapping = Assert.IsAssignableFrom<NpgsqlArrayTypeMapping>(CreateTypeMappingSource().FindMapping("timestamp(5) without time zone[]"));
        Assert.Same(typeof(List<DateTime>), arrayMapping.ClrType);
        Assert.Equal("timestamp(5) without time zone[]", arrayMapping.StoreType);

        var elementMapping = arrayMapping.ElementTypeMapping;
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
    [InlineData(typeof(NpgsqlRange<int>[]), "int4multirange")]
    [InlineData(typeof(List<NpgsqlRange<int>>), "int4multirange")]
    [InlineData(typeof(Geometry), "geometry")]
    [InlineData(typeof(Point), "geometry")]
    public void By_ClrType(Type clrType, string expectedStoreType)
    {
        var mapping = CreateTypeMappingSource().FindMapping(clrType);
        Assert.Equal(expectedStoreType, mapping.StoreType);
        Assert.Same(clrType, mapping.ClrType);
    }

    [Theory]
    [InlineData(typeof(decimal), "numeric(5)")]
    [InlineData(typeof(DateTime), "timestamp(5) with time zone")]
    [InlineData(typeof(TimeSpan), "interval(5)")]
    [InlineData(typeof(int), "integer")]
    public void By_ClrType_and_precision(Type clrType, string expectedStoreType)
    {
        var mapping = CreateTypeMappingSource().FindMapping(clrType, null, precision: 5);
        Assert.Equal(expectedStoreType, mapping.StoreType);
        Assert.Same(clrType, mapping.ClrType);
    }

    [Theory]
    [InlineData(typeof(decimal[]), "numeric(5)[]")]
    [InlineData(typeof(DateTime[]), "timestamp(5) with time zone[]")]
    [InlineData(typeof(TimeSpan[]), "interval(5)[]")]
    [InlineData(typeof(int[]), "integer[]")]
    public void By_ClrType_and_element_precision(Type clrType, string expectedStoreType)
    {
        var model = CreateEmptyModel();
        var arrayMapping = CreateTypeMappingSource().FindMapping(clrType, model,
                CreateTypeMappingSource().FindMapping(clrType.GetElementType()!, null, precision: 5)!);

        Assert.Equal(expectedStoreType, arrayMapping.StoreType);
        Assert.Same(clrType, arrayMapping.ClrType);
        Assert.Null(arrayMapping.Precision);

        var elementMapping = Assert.IsAssignableFrom<RelationalTypeMapping>(arrayMapping.ElementTypeMapping);
        Assert.Equal(5, elementMapping.Precision);
        Assert.Equal(expectedStoreType[..^2], elementMapping.StoreType);
        Assert.Same(clrType.GetElementType(), elementMapping.ClrType);
    }

    [Theory]
    [InlineData("integer", typeof(int))]
    [InlineData("numeric", typeof(float))]
    [InlineData("numeric", typeof(double))]
    [InlineData("integer[]", typeof(int[]))]
    [InlineData("integer[]", typeof(List<int>))]
    [InlineData("smallint[]", typeof(byte[]))]
    [InlineData("dummy", typeof(DummyType))]
    [InlineData("int4range", typeof(NpgsqlRange<int>))]
    [InlineData("floatrange", typeof(NpgsqlRange<float>))]
    [InlineData("dummyrange", typeof(NpgsqlRange<DummyType>))]
    [InlineData("geometry", typeof(Geometry))]
    [InlineData("geometry(Point, 4326)", typeof(Geometry))]
    [InlineData("xid", typeof(uint))]
    [InlineData("xid8", typeof(ulong))]
    public void By_StoreType_with_ClrType(string storeType, Type clrType)
    {
        var mapping = CreateTypeMappingSource().FindMapping(clrType, storeType);
        Assert.Equal(storeType, mapping.StoreType);
        Assert.Same(clrType, mapping.ClrType);
    }

    [Theory]
    [InlineData("integer", typeof(UnknownType))]
    //[InlineData("integer[]", typeof(UnknownType))] TODO Implement
    [InlineData("dummy", typeof(UnknownType))]
    [InlineData("int4range", typeof(UnknownType))]
    [InlineData("floatrange", typeof(UnknownType))]
    [InlineData("dummyrange", typeof(UnknownType))]
    [InlineData("geometry", typeof(UnknownType))]
    public void By_StoreType_with_wrong_ClrType(string storeType, Type wrongClrType)
        => Assert.Null(CreateTypeMappingSource().FindMapping(wrongClrType, storeType));

    // Happens when using domain/aliases: we don't know about the domain but continue with the mapping based on the ClrType
    [Fact]
    public void Unknown_StoreType_with_known_ClrType()
        => Assert.Equal("some_domain", CreateTypeMappingSource().FindMapping(typeof(int), "some_domain").StoreType);

    [Fact]
    public void Varchar_mapping_sets_NpgsqlDbType()
    {
        var mapping = CreateTypeMappingSource().FindMapping("character varying");
        var parameter = (NpgsqlParameter)mapping.CreateParameter(new NpgsqlCommand(), "p", "foo");
        Assert.Equal(NpgsqlDbType.Varchar, parameter.NpgsqlDbType);
    }

    [Fact]
    public void Single_char_mapping_sets_NpgsqlDbType()
    {
        var mapping = CreateTypeMappingSource().FindMapping(typeof(char));
        var parameter = (NpgsqlParameter)mapping.CreateParameter(new NpgsqlCommand(), "p", "foo");
        Assert.Equal(NpgsqlDbType.Char, parameter.NpgsqlDbType);
    }

    [Fact]
    public void String_as_single_char_mapping_sets_NpgsqlDbType()
    {
        var mapping = CreateTypeMappingSource().FindMapping(typeof(string), "char(1)");
        var parameter = (NpgsqlParameter)mapping.CreateParameter(new NpgsqlCommand(), "p", "foo");
        Assert.Equal(NpgsqlDbType.Char, parameter.NpgsqlDbType);
    }

    [Fact]
    public void Array_over_type_mapping_with_value_converter_by_clr_type_array()
        => Array_over_type_mapping_with_value_converter(CreateTypeMappingSource().FindMapping(typeof(LTree[])), typeof(LTree[]));

    [Fact]
    public void Array_over_type_mapping_with_value_converter_by_clr_type_list()
        => Array_over_type_mapping_with_value_converter(CreateTypeMappingSource().FindMapping(typeof(List<LTree>)), typeof(List<LTree>));

    [Fact]
    public void Array_over_type_mapping_with_value_converter_by_store_type()
        => Array_over_type_mapping_with_value_converter(CreateTypeMappingSource().FindMapping("ltree[]"), typeof(List<LTree>));

    private void Array_over_type_mapping_with_value_converter(CoreTypeMapping mapping, Type expectedType)
    {
        var arrayMapping = (NpgsqlArrayTypeMapping)mapping;
        Assert.Equal("ltree[]", arrayMapping.StoreType);
        Assert.Same(expectedType, arrayMapping.ClrType);

        var elementMapping = arrayMapping.ElementTypeMapping;
        Assert.NotNull(elementMapping);
        Assert.Equal("ltree", elementMapping.StoreType);
        Assert.Same(typeof(LTree), elementMapping.ClrType);

        var arrayConverter = arrayMapping.Converter;
        Assert.NotNull(arrayConverter);
        Assert.Same(expectedType, arrayConverter.ModelClrType);
        Assert.Same(typeof(string[]), arrayConverter.ProviderClrType);

        Assert.Collection((ICollection<string>)arrayConverter.ConvertToProvider(
                expectedType.IsArray
                    ? new LTree[] { new("foo"), new("bar") }
                    : new List<LTree> { new("foo"), new("bar") }),
            s => Assert.Equal("foo", s),
            s => Assert.Equal("bar", s));
    }

    [Fact]
    public void Multirange_by_clr_type_across_pg_versions()
    {
        var mapping14 = CreateTypeMappingSource(postgresVersion: new(14, 0)).FindMapping(typeof(NpgsqlRange<int>[]))!;
        var mapping13 = CreateTypeMappingSource(postgresVersion: new(13, 0)).FindMapping(typeof(NpgsqlRange<int>[]))!;
        var mappingDefault = CreateTypeMappingSource().FindMapping(typeof(NpgsqlRange<int>[]))!;

        Assert.Equal("int4multirange", mapping14.StoreType);
        Assert.Equal("int4range[]", mapping13.StoreType);

        // See #2351 - we didn't put multiranges behind a version opt-in in 6.0, although the default PG version is still 12; this causes
        // anyone with arrays of ranges to fail if they upgrade to 6.0 with pre-14 PG.
        // Changing this in a patch would break people already using 6.0 with PG14, so multiranges are on by default unless users explicitly
        // specify < 14.
        // Once 14 is made the default version, this stuff can be removed.
        Assert.Equal("int4multirange", mappingDefault.StoreType);
    }

    [Fact]
    public void Multirange_by_store_type_across_pg_versions()
    {
        var mapping14 = CreateTypeMappingSource(postgresVersion: new(14, 0)).FindMapping("int4multirange")!;
        var mapping13 = CreateTypeMappingSource(postgresVersion: new(13, 0)).FindMapping("int4multirange");
        var mappingDefault = CreateTypeMappingSource().FindMapping("int4multirange")!;

        Assert.Same(typeof(List<NpgsqlRange<int>>), mapping14.ClrType);
        Assert.Null(mapping13);

        // See #2351 - we didn't put multiranges behind a version opt-in in 6.0, although the default PG version is still 12; this causes
        // anyone with arrays of ranges to fail if they upgrade to 6.0 with pre-14 PG.
        // Changing this in a patch would break people already using 6.0 with PG14, so multiranges are on by default unless users explicitly
        // specify < 14.
        // Once 14 is made the default version, this stuff can be removed.
        Assert.Same(typeof(List<NpgsqlRange<int>>), mappingDefault.ClrType);
    }

    #region Support

    private NpgsqlTypeMappingSource CreateTypeMappingSource(Version postgresVersion = null)
    {
        var builder = new DbContextOptionsBuilder();
        var npgsqlBuilder = new NpgsqlDbContextOptionsBuilder(builder);

        npgsqlBuilder.MapRange<float>("floatrange");
        npgsqlBuilder.MapRange<DummyType>("dummyrange", subtypeName: "dummy");
        npgsqlBuilder.SetPostgresVersion(postgresVersion);

        var options = new NpgsqlSingletonOptions();
        options.Initialize(builder.Options);

        return new NpgsqlTypeMappingSource(
            new TypeMappingSourceDependencies(
                new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                new JsonValueReaderWriterSource(new JsonValueReaderWriterSourceDependencies()),
                Array.Empty<ITypeMappingSourcePlugin>()),
            new RelationalTypeMappingSourceDependencies(
                new IRelationalTypeMappingSourcePlugin[] {
                    new NpgsqlNetTopologySuiteTypeMappingSourcePlugin(new NpgsqlNetTopologySuiteOptions()),
                    new DummyTypeMappingSourcePlugin()
                }),
            new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
            options);
    }

    private class DummyTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
    {
        public RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => mappingInfo.StoreTypeName is not null
                ? mappingInfo.StoreTypeName == "dummy" && (mappingInfo.ClrType is null || mappingInfo.ClrType == typeof(DummyType))
                    ? _dummyMapping
                    : null
                : mappingInfo.ClrType == typeof(DummyType)
                    ? _dummyMapping
                    : null;

        private readonly DummyMapping _dummyMapping = new();

        private class DummyMapping : RelationalTypeMapping
        {
            // TODO: The DbType is a hack, we currently require of range subtype mapping that they other expose an NpgsqlDbType
            // or a DbType (from which NpgsqlDbType is computed), since RangeTypeMapping sends an NpgsqlDbType.
            // This means we currently don't support ranges over types without NpgsqlDbType, which are accessible via
            // NpgsqlParameter.DataTypeName
            public DummyMapping() : base("dummy", typeof(DummyType), System.Data.DbType.Guid) {}

            private DummyMapping(RelationalTypeMappingParameters parameters) : base(parameters) {}

            protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
                => new DummyMapping(parameters);
        }
    }

    private class DummyType {}

    private class UnknownType {}

    protected IModel CreateEmptyModel()
        => CreateModelBuilder().Model.FinalizeModel();

    protected ModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configureConventions = null)
        => NpgsqlTestHelpers.Instance.CreateConventionBuilder(configureConventions: configureConventions);

    #endregion Support
}
