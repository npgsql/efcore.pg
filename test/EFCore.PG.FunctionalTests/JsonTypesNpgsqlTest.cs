using System.Collections;
using System.Globalization;
using System.Numerics;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore;

public class JsonTypesNpgsqlTest(NonSharedFixture fixture) : JsonTypesRelationalTestBase(fixture)
{
    #region Nested collections (unsupported)

    // The following tests are disabled because they use nested collections, which are not supported by EFCore.PG (arrays of arrays aren't
    // supported).

    public override Task Can_read_write_array_of_array_of_array_of_int_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_array_of_array_of_array_of_int_JSON_values());

    public override Task Can_read_write_array_of_list_of_array_of_IPAddress_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_array_of_list_of_array_of_IPAddress_JSON_values());

    public override Task Can_read_write_array_of_list_of_array_of_string_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_array_of_list_of_array_of_string_JSON_values());

    public override Task Can_read_write_array_of_list_of_binary_JSON_values(string expected)
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_array_of_list_of_binary_JSON_values(expected));

    public override Task Can_read_write_array_of_list_of_GUID_JSON_values(string expected)
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_array_of_list_of_GUID_JSON_values(expected));

    public override Task Can_read_write_array_of_list_of_int_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_array_of_list_of_int_JSON_values());

    public override Task Can_read_write_array_of_list_of_IPAddress_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_array_of_list_of_IPAddress_JSON_values());

    public override Task Can_read_write_array_of_list_of_string_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_array_of_list_of_string_JSON_values());

    public override Task Can_read_write_array_of_list_of_ulong_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_array_of_list_of_ulong_JSON_values());

    public override Task Can_read_write_list_of_array_of_GUID_JSON_values(string expected)
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_GUID_JSON_values(expected));

    public override Task Can_read_write_list_of_array_of_int_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_int_JSON_values());

    public override Task Can_read_write_list_of_array_of_IPAddress_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_IPAddress_JSON_values());

    public override Task Can_read_write_list_of_array_of_list_of_array_of_binary_JSON_values(string expected)
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_list_of_array_of_binary_JSON_values(expected));

    public override Task Can_read_write_list_of_array_of_list_of_IPAddress_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_list_of_IPAddress_JSON_values());

    public override Task Can_read_write_list_of_array_of_list_of_string_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_list_of_string_JSON_values());

    public override Task Can_read_write_list_of_array_of_list_of_ulong_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_list_of_ulong_JSON_values());

    public override Task Can_read_write_list_of_array_of_nullable_GUID_JSON_values(string expected)
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_nullable_GUID_JSON_values(expected));

    public override Task Can_read_write_list_of_array_of_nullable_int_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_nullable_int_JSON_values());

    public override Task Can_read_write_list_of_array_of_nullable_ulong_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_nullable_ulong_JSON_values());

    public override Task Can_read_write_list_of_array_of_string_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_string_JSON_values());

    public override Task Can_read_write_list_of_array_of_ulong_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_ulong_JSON_values());

    public override Task Can_read_write_list_of_list_of_list_of_int_JSON_values()
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_list_of_list_of_int_JSON_values());

    #endregion Nested collections (unsupported)

    // IEnumerable property
    public override Task Can_read_write_list_of_array_of_binary_JSON_values(string expected)
        => Assert.ThrowsAsync<EqualException>(() => base.Can_read_write_list_of_array_of_binary_JSON_values(expected));

    // public override Task Can_read_write_ulong_enum_JSON_values(EnumU64 value, string json)
    // {
    //     // Relational databases don't support unsigned numeric types, so ulong is value-converted to long
    //     if (value == EnumU64.Max)
    //     {
    //         json = """{"Prop":-1}""";
    //     }
    //
    //     return base.Can_read_write_ulong_enum_JSON_values(value, json);
    // }
    //
    // public override void Can_read_write_nullable_ulong_enum_JSON_values(object? value, string json)
    // {
    //     // Relational databases don't support unsigned numeric types, so ulong is value-converted to long
    //     if (Equals(value, ulong.MaxValue))
    //     {
    //         json = """{"Prop":-1}""";
    //     }
    //
    //     base.Can_read_write_nullable_ulong_enum_JSON_values(value, json);
    // }
    //
    // public override void Can_read_write_collection_of_ulong_enum_JSON_values()
    //     => Can_read_and_write_JSON_value<EnumU64CollectionType, List<EnumU64>>(
    //         nameof(EnumU64CollectionType.EnumU64),
    //         [
    //             EnumU64.Min,
    //             EnumU64.Max,
    //             EnumU64.Default,
    //             EnumU64.One,
    //             (EnumU64)8
    //         ],
    //         // Relational databases don't support unsigned numeric types, so ulong is value-converted to long
    //         """{"Prop":[0,-1,0,1,8]}""",
    //         mappedCollection: true);
    //
    // public override void Can_read_write_collection_of_nullable_ulong_enum_JSON_values()
    //     => Can_read_and_write_JSON_value<NullableEnumU64CollectionType, List<EnumU64?>>(
    //         nameof(NullableEnumU64CollectionType.EnumU64),
    //         [
    //             EnumU64.Min,
    //             null,
    //             EnumU64.Max,
    //             EnumU64.Default,
    //             EnumU64.One,
    //             (EnumU64?)8
    //         ],
    //         // Relational databases don't support unsigned numeric types, so ulong is value-converted to long
    //         """{"Prop":[0,null,-1,0,1,8]}""",
    //         mappedCollection: true);

    #region TimeSpan

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_TimeSpan_JSON_values_npgsql instead.
    // TODO: Implement Can_read_write_TimeSpan_JSON_values_npgsql
    public override Task Can_read_write_TimeSpan_JSON_values(string value, string json)
        => Task.CompletedTask;

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_nullable_TimeSpan_JSON_values_npgsql instead.
    // TODO: Implement Can_read_write_nullable_TimeSpan_JSON_values_npgsql
    public override Task Can_read_write_nullable_TimeSpan_JSON_values(string? value, string json)
        => Task.CompletedTask;

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_nullable_TimeSpan_JSON_values_npgsql instead.
    // TODO: Implement Can_read_write_collection_of_TimeSpan_JSON_values_npgsql
    public override Task Can_read_write_collection_of_TimeSpan_JSON_values()
        => Task.CompletedTask;

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_nullable_TimeSpan_JSON_values_npgsql instead.
    // TODO: Implement Can_read_write_collection_of_nullable_TimeSpan_JSON_values_npgsql
    public override Task Can_read_write_collection_of_nullable_TimeSpan_JSON_values()
    => Task.CompletedTask;

    #endregion TimeSpan

    #region DateOnly

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_DateOnly_JSON_values_npgsql instead.
    public override Task Can_read_write_DateOnly_JSON_values(string value, string json)
        => Task.CompletedTask;

    [ConditionalTheory]
    [InlineData("1/1/0001", """{"Prop":"-infinity"}""")]
    [InlineData("12/31/9999", """{"Prop":"infinity"}""")]
    [InlineData("5/29/2023", """{"Prop":"2023-05-29"}""")]
    public virtual Task Can_read_write_DateOnly_JSON_values_npgsql(string value, string json)
        => Can_read_and_write_JSON_value<DateOnlyType, DateOnly>(
            nameof(DateOnlyType.DateOnly),
            DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_nullable_DateOnly_JSON_values_npgsql instead.
    // TODO: Implement Can_read_write_nullable_DateOnly_JSON_values_npgsql
    public override Task Can_read_write_nullable_DateOnly_JSON_values(string? value, string json)
        => Task.CompletedTask;

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_TimeSpan_JSON_values_npgsql instead.
    public override Task Can_read_write_collection_of_DateOnly_JSON_values()
        => Task.CompletedTask;

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_DateOnly_JSON_values_npgsql()
        => Can_read_and_write_JSON_value<NpgsqlDateOnlyCollectionType, List<DateOnly>>(
            nameof(DateOnlyCollectionType.DateOnly),
            [
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue
            ],
            """{"Prop":["-infinity","2023-05-29","infinity"]}""",
            mappedCollection: true);

    protected class NpgsqlDateOnlyCollectionType
    {
        public List<DateOnly> DateOnly { get; set; } = null!;
    }

    [ConditionalFact]
    public override Task Can_read_write_collection_of_nullable_DateOnly_JSON_values()
        => Can_read_and_write_JSON_value<NullableDateOnlyCollectionType, List<DateOnly?>>(
            nameof(NullableDateOnlyCollectionType.DateOnly),
            [
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue,
                null
            ],
            """{"Prop":["-infinity","2023-05-29","infinity",null]}""",
            mappedCollection: true);

    #endregion DateOnly

    #region DateTime

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_DateTime_JSON_values_npgsql instead.
    public override Task Can_read_write_DateTime_JSON_values(string value, string json)
        => Task.CompletedTask;

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", """{"Prop":"-infinity"}""")]
    [InlineData("9999-12-31T23:59:59.9999999", """{"Prop":"infinity"}""")]
    [InlineData("2023-05-29T10:52:47.2064350", """{"Prop":"2023-05-29T10:52:47.206435"}""")]
    public virtual Task Can_read_write_DateTime_JSON_values_npgsql(string value, string json)
        => Can_read_and_write_JSON_value<DateTimeType, DateTime>(
            modelBuilder => modelBuilder
                .Entity<DateTimeType>()
                .HasNoKey()
                .Property(nameof(DateTimeType.DateTime))
                .HasColumnType("timestamp without time zone"),
            configureConventions: null,
            nameof(DateTimeType.DateTime),
            DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_nullable_DateTime_JSON_values_npgsql instead.
    // TODO: Implement Can_read_write_nullable_DateTime_JSON_values_npgsql
    public override Task Can_read_write_nullable_DateTime_JSON_values(string? value, string json)
        => Task.CompletedTask;

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_TimeSpan_JSON_values_npgsql instead.
    public override Task Can_read_write_collection_of_DateTime_JSON_values(string expected)
        => Task.CompletedTask;

    [ConditionalTheory]
    [InlineData("""{"Prop":["-infinity","2023-05-29T10:52:47","infinity"]}""")]
    public virtual Task Can_read_write_collection_of_DateTime_JSON_values_npgsql(string expected)
        => Can_read_and_write_JSON_value<NpgsqlDateTimeCollectionType, List<DateTime>>(
            modelBuilder => modelBuilder
                .Entity<NpgsqlDateTimeCollectionType>()
                .HasNoKey()
                .PrimitiveCollection(nameof(DateTimeCollectionType.DateTime))
                .ElementType()
                .HasStoreType("timestamp without time zone"),
            configureConventions: null,
            nameof(DateTimeCollectionType.DateTime),
            [
                DateTime.MinValue,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            ],
            expected,
            mappedCollection: true);

    protected class NpgsqlDateTimeCollectionType
    {
        public List<DateTime> DateTime { get; set; } = null!;
    }

    public override Task Can_read_write_collection_of_nullable_DateTime_JSON_values(string expected)
        => Can_read_and_write_JSON_value<NullableDateTimeCollectionType, List<DateTime?>>(
            modelBuilder => modelBuilder
                .Entity<NullableDateTimeCollectionType>()
                .HasNoKey()
                .PrimitiveCollection(nameof(NullableDateTimeCollectionType.DateTime))
                .ElementType()
                .HasStoreType("timestamp without time zone"),
            configureConventions: null,
            nameof(NullableDateTimeCollectionType.DateTime),
            [
                DateTime.MinValue,
                null,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            ],
            """{"Prop":["-infinity",null,"2023-05-29T10:52:47","infinity"]}""",
            mappedCollection: true);

    #endregion DateTime

    #region DateTimeOffset

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_DateTimeOffset_JSON_values_npgsql instead.
    public override Task Can_read_write_DateTimeOffset_JSON_values(string value, string json)
        => Task.CompletedTask;

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", """{"Prop":"0001-01-01T00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999990+02:00", """{"Prop":"9999-12-31T23:59:59.999999\u002B02:00"}""")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", """{"Prop":"0001-01-01T00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.5672850+04:00", """{"Prop":"2023-05-29T11:11:15.567285\u002B04:00"}""")]
    public virtual Task Can_read_write_DateTimeOffset_JSON_values_npgsql(string value, string json)
        => Can_read_and_write_JSON_value<DateTimeOffsetType, DateTimeOffset>(
            nameof(DateTimeOffsetType.DateTimeOffset),
            DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_nullable_DateTimeOffset_JSON_values_npgsql instead.
    // TODO: Implement Can_read_write_nullable_DateTimeOffset_JSON_values_npgsql
    public override Task Can_read_write_nullable_DateTimeOffset_JSON_values(string? value, string json)
        => Task.CompletedTask;

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_collection_of_DateTimeOffset_JSON_values_npgsql instead.
    public override Task Can_read_write_collection_of_DateTimeOffset_JSON_values(string expected)
        => Task.CompletedTask;

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_DateTimeOffset_JSON_values_npgsql()
        => Can_read_and_write_JSON_value<DateTimeOffsetCollectionType, List<DateTimeOffset>>(
            nameof(DateTimeOffsetCollectionType.DateTimeOffset),
            [
                DateTimeOffset.MinValue,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(-2, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(0, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(2, 0, 0)),
                DateTimeOffset.MaxValue
            ],
            """{"Prop":["-infinity","2023-05-29T10:52:47-02:00","2023-05-29T10:52:47\u002B00:00","2023-05-29T10:52:47\u002B02:00","infinity"]}""",
            mappedCollection: true);

    // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
    // to override. See Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values_npgsql instead.
    public override Task Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values(string expected)
        => Task.CompletedTask;

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values_npgsql()
        => Can_read_and_write_JSON_value<NullableDateTimeOffsetCollectionType, List<DateTimeOffset?>>(
            nameof(NullableDateTimeOffsetCollectionType.DateTimeOffset),
            [
                DateTimeOffset.MinValue,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(-2, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(0, 0, 0)),
                null,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(2, 0, 0)),
                DateTimeOffset.MaxValue
            ],
            """{"Prop":["-infinity","2023-05-29T10:52:47-02:00","2023-05-29T10:52:47\u002B00:00",null,"2023-05-29T10:52:47\u002B02:00","infinity"]}""",
            mappedCollection: true);

    #endregion DateTimeOffset

    [ConditionalTheory]
    [InlineData("12:34:56.123456+05:00", """{"Prop":"12:34:56.123456\u002B5"}""")]
    public virtual void Can_read_write_timetz_JSON_values(string value, string json)
        => Can_read_and_write_JSON_property_value<DateTimeOffsetType, DateTimeOffset>(
            b => b.HasColumnType("timetz"),
            nameof(DateTimeOffsetType.DateTimeOffset),
            DateTimeOffset.Parse(value),
            json);

    [ConditionalTheory]
    [InlineData(Mood.Happy, """{"Prop":"Happy"}""")]
    [InlineData(Mood.Sad, """{"Prop":"Sad"}""")]
    public virtual Task Can_read_write_pg_enum_JSON_values(Mood value, string json)
        => Can_read_and_write_JSON_value<EnumType, Mood>(
            nameof(EnumType.Mood),
            value,
            json);

    protected class EnumType
    {
        public Mood Mood { get; set; }
    }

    public enum Mood
    {
        Happy,
        Sad
    }

    [ConditionalTheory]
    [InlineData(new[] { 1, 2, 3 }, """{"Prop":[1,2,3]}""")]
    [InlineData(new int[0], """{"Prop":[]}""")]
    public virtual Task Can_read_write_array_JSON_values(int[] value, string json)
        => Can_read_and_write_JSON_value<ArrayType, int[]>(
            nameof(ArrayType.Array),
            value,
            json,
            mappedCollection: true);

    protected class ArrayType
    {
        public int[] Array { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Cannot_read_write_multidimensional_array_JSON_values()
        // EF currently throws NRE when the type mapping has no JsonValueReaderWriter (this has been improved for 9.0)
        => Assert.ThrowsAsync<NullReferenceException>(
            () => Can_read_and_write_JSON_value<MultidimensionalArrayType, int[,]>(
                nameof(MultidimensionalArrayType.MultidimensionalArray),
                new[,] { { 1, 2 }, { 3, 4 } },
                ""));

    protected class MultidimensionalArrayType
    {
        public int[,] MultidimensionalArray { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_BigInteger_JSON_values()
        => Can_read_and_write_JSON_value<BigIntegerType, BigInteger>(
            nameof(BigIntegerType.BigInteger),
            new BigInteger(ulong.MaxValue),
            """{"Prop":"18446744073709551615"}""");

    protected class BigIntegerType
    {
        public BigInteger BigInteger { get; set; }
    }

    [ConditionalTheory]
    [InlineData(new[] { true, false, true }, """{"Prop":"101"}""")]
    [InlineData(new[] { true, false, true, true, false, true, false, true, false }, """{"Prop":"101101010"}""")]
    [InlineData(new bool[0], """{"Prop":""}""")]
    public virtual Task Can_read_write_BitArray_JSON_values(bool[] value, string json)
        => Can_read_and_write_JSON_value<BitArrayType, BitArray>(
            nameof(BitArrayType.BitArray),
            new BitArray(value),
            json);

    protected class BitArrayType
    {
        public BitArray BitArray { get; set; } = null!;
    }

    [ConditionalTheory]
    [InlineData(1000, """{"Prop":"0/3E8"}""")]
    [InlineData(0, """{"Prop":"0/0"}""")]
    public virtual Task Can_read_write_LogSequenceNumber_JSON_values(ulong value, string json)
        => Can_read_and_write_JSON_value<LogSequenceNumberType, NpgsqlLogSequenceNumber>(
            nameof(LogSequenceNumberType.LogSequenceNumber),
            new NpgsqlLogSequenceNumber(value),
            json);

    [ConditionalFact]
    public override async Task Can_read_write_point()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<PointType, Point>(
            nameof(PointType.Point),
            factory.CreatePoint(new Coordinate(2, 4)),
            """{"Prop":"SRID=4326;POINT (2 4)"}""");
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_point_without_SRID()
        => await Can_read_and_write_JSON_value<PointType, Point>(
            nameof(PointType.Point),
            new Point(2, 4),
            """{"Prop":"POINT (2 4)"}""");

    [ConditionalFact]
    public override async Task Can_read_write_point_with_M()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<PointMType, Point>(
            nameof(PointMType.PointM),
            factory.CreatePoint(new CoordinateM(2, 4, 6)),
            """{"Prop":"SRID=4326;POINT (2 4)"}""");
    }

    public override async Task Can_read_write_point_with_Z()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<PointZType, Point>(
            nameof(PointZType.PointZ),
            factory.CreatePoint(new CoordinateZ(2, 4, 6)),
            """{"Prop":"SRID=4326;POINT Z(2 4 6)"}""");
    }

    public override async Task Can_read_write_point_with_Z_and_M()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<PointZMType, Point>(
            nameof(PointZMType.PointZM),
            factory.CreatePoint(new CoordinateZM(1, 2, 3, 4)),
            """{"Prop":"SRID=4326;POINT Z(1 2 3)"}""");
    }

    [ConditionalFact]
    public override async Task Can_read_write_line_string()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<LineStringType, LineString>(
            nameof(LineStringType.LineString),
            factory.CreateLineString([new Coordinate(0, 0), new Coordinate(1, 0)]),
            """{"Prop":"SRID=4326;LINESTRING (0 0, 1 0)"}""");
    }

    public override async Task Can_read_write_multi_line_string()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<MultiLineStringType, MultiLineString>(
            nameof(MultiLineStringType.MultiLineString),
            factory.CreateMultiLineString(
            [
                factory.CreateLineString(
                    [new Coordinate(0, 0), new Coordinate(0, 1)]),
                factory.CreateLineString(
                    [new Coordinate(1, 0), new Coordinate(1, 1)])
            ]),
            """{"Prop":"SRID=4326;MULTILINESTRING ((0 0, 0 1), (1 0, 1 1))"}""");
    }

    public override async Task Can_read_write_polygon()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<PolygonType, Polygon>(
            nameof(PolygonType.Polygon),
            factory.CreatePolygon([new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0)]),
            """{"Prop":"SRID=4326;POLYGON ((0 0, 1 0, 0 1, 0 0))"}""");
    }

    public override async Task Can_read_write_polygon_typed_as_geometry()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<GeometryType, Geometry>(
            nameof(GeometryType.Geometry),
            factory.CreatePolygon([new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0)]),
            """{"Prop":"SRID=4326;POLYGON ((0 0, 1 0, 0 1, 0 0))"}""");
    }

    protected class LogSequenceNumberType
    {
        public NpgsqlLogSequenceNumber LogSequenceNumber { get; set; }
    }

    protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkNpgsqlNetTopologySuite();

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        // Note that the enum doesn't actually need to be created in the database, since Can_read_and_write_JSON_value doesn't access
        // the database. We just need the mapping to be picked up by EFCore.PG from the ADO.NET layer.
        new NpgsqlDbContextOptionsBuilder(builder)
            .MapEnum<Mood>("mapped_enum", "test")
            .UseNetTopologySuite();
        return builder;
    }
}
