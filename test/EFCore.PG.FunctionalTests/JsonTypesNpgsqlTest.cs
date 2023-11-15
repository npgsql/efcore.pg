#nullable enable

using System.Collections;

using System.Globalization;
using System.Numerics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class JsonTypesNpgsqlTest : JsonTypesRelationalTestBase
{
    public override void Can_read_write_ulong_enum_JSON_values(EnumU64 value, string json)
    {
        // Relational databases don't support unsigned numeric types, so ulong is value-converted to long
        if (value == EnumU64.Max)
        {
            json = """{"Prop":-1}""";
        }

        base.Can_read_write_ulong_enum_JSON_values(value, json);
    }

    public override void Can_read_write_nullable_ulong_enum_JSON_values(object? value, string json)
    {
        // Relational databases don't support unsigned numeric types, so ulong is value-converted to long
        if (Equals(value, ulong.MaxValue))
        {
            json = """{"Prop":-1}""";
        }

        base.Can_read_write_nullable_ulong_enum_JSON_values(value, json);
    }

    public override void Can_read_write_collection_of_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value<EnumU64CollectionType, List<EnumU64>>(
            nameof(EnumU64CollectionType.EnumU64),
            new List<EnumU64>
            {
                EnumU64.Min,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            },
            // Relational databases don't support unsigned numeric types, so ulong is value-converted to long
            """{"Prop":[0,-1,0,1,8]}""",
            mappedCollection: true);

    public override void Can_read_write_collection_of_nullable_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnumU64CollectionType, List<EnumU64?>>(
            nameof(NullableEnumU64CollectionType.EnumU64),
            new List<EnumU64?>
            {
                EnumU64.Min,
                null,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64?)8
            },
            // Relational databases don't support unsigned numeric types, so ulong is value-converted to long
            """{"Prop":[0,null,-1,0,1,8]}""",
            mappedCollection: true);

    #region TimeSpan

    public override void Can_read_write_TimeSpan_JSON_values(string value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_TimeSpan_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.477580", """{"Prop":"-10675199 02:48:05.47758"}""")]
    [InlineData("10675199.02:48:05.477580", """{"Prop":"10675199 02:48:05.47758"}""")]
    [InlineData("00:00:00", """{"Prop":"00:00:00"}""")]
    [InlineData("12:23:23.801885", """{"Prop":"12:23:23.801885"}""")]
    public virtual void Can_read_write_TimeSpan_JSON_values_npgsql(string value, string json)
        => Can_read_and_write_JSON_value<TimeSpanType, TimeSpan>(
            nameof(TimeSpanType.TimeSpan),
            TimeSpan.Parse(value, CultureInfo.InvariantCulture),
            json);

    public override void Can_read_write_nullable_TimeSpan_JSON_values(string? value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_TimeSpan_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.47758", """{"Prop":"-10675199 02:48:05.47758"}""")]
    [InlineData("10675199.02:48:05.47758", """{"Prop":"10675199 02:48:05.47758"}""")]
    [InlineData("00:00:00", """{"Prop":"00:00:00"}""")]
    [InlineData("12:23:23.801885", """{"Prop":"12:23:23.801885"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_TimeSpan_JSON_values_npgsql(string? value, string json)
        => Can_read_and_write_JSON_value<NullableTimeSpanType, TimeSpan?>(
            nameof(NullableTimeSpanType.TimeSpan),
            value == null ? default(TimeSpan?) : TimeSpan.Parse(value), json);

    [ConditionalFact]
    public override void Can_read_write_collection_of_TimeSpan_JSON_values()
    {
        Can_read_and_write_JSON_value<TimeSpanCollectionType, List<TimeSpan>>(
            nameof(TimeSpanCollectionType.TimeSpan),
            new List<TimeSpan>
            {
                // We cannot roundtrip MinValue and MaxValue since these contain sub-microsecond components, which PG does not support.
                // TimeSpan.MinValue,
                new(1, 2, 3, 4),
                new(0, 2, 3, 4, 5, 678),
                // TimeSpan.MaxValue
            },
            """{"Prop":["1 02:03:04","02:03:04.005678"]}""",
            mappedCollection: true);
    }

    [ConditionalFact]
    public override void Can_read_write_collection_of_nullable_TimeSpan_JSON_values()
        => Can_read_and_write_JSON_value<NullableTimeSpanCollectionType, List<TimeSpan?>>(
            nameof(NullableTimeSpanCollectionType.TimeSpan),
            new List<TimeSpan?>
            {
                // We cannot roundtrip MinValue and MaxValue since these contain sub-microsecond components, which PG does not support.
                // TimeSpan.MinValue,
                new(1, 2, 3, 4),
                new(0, 2, 3, 4, 5, 678),
                // TimeSpan.MaxValue
                null
            },
            """{"Prop":["1 02:03:04","02:03:04.005678",null]}""",
            mappedCollection: true);

    #endregion TimeSpan

    #region DateOnly

    public override void Can_read_write_DateOnly_JSON_values(string value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_TimeSpan_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("1/1/0001", """{"Prop":"-infinity"}""")]
    [InlineData("12/31/9999", """{"Prop":"infinity"}""")]
    [InlineData("5/29/2023", """{"Prop":"2023-05-29"}""")]
    public virtual void Can_read_write_DateOnly_JSON_values_npgsql(string value, string json)
        => Can_read_and_write_JSON_value<DateOnlyType, DateOnly>(
            nameof(DateOnlyType.DateOnly),
            DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    public override void Can_read_write_nullable_DateOnly_JSON_values(string? value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_DateOnly_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("1/1/0001", """{"Prop":"-infinity"}""")]
    [InlineData("12/31/9999", """{"Prop":"infinity"}""")]
    [InlineData("5/29/2023", """{"Prop":"2023-05-29"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateOnly_JSON_values_npgsql(string? value, string json)
        => Can_read_and_write_JSON_value<NullableDateOnlyType, DateOnly?>(
            nameof(NullableDateOnlyType.DateOnly),
            value == null ? default(DateOnly?) : DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalFact]
    public virtual void Can_read_write_DateOnly_JSON_values_infinity()
    {
        Can_read_and_write_JSON_value<DateOnlyType, DateOnly>(nameof(DateOnlyType.DateOnly), DateOnly.MinValue, """{"Prop":"-infinity"}""");
        Can_read_and_write_JSON_value<DateOnlyType, DateOnly>(nameof(DateOnlyType.DateOnly), DateOnly.MaxValue, """{"Prop":"infinity"}""");
    }

    [ConditionalFact]
    public override void Can_read_write_collection_of_DateOnly_JSON_values()
        => Can_read_and_write_JSON_value<DateOnlyCollectionType, List<DateOnly>>(
            nameof(DateOnlyCollectionType.DateOnly),
            new List<DateOnly>
            {
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue
            },
            """{"Prop":["-infinity","2023-05-29","infinity"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_nullable_DateOnly_JSON_values()
        => Can_read_and_write_JSON_value<NullableDateOnlyCollectionType, List<DateOnly?>>(
            nameof(NullableDateOnlyCollectionType.DateOnly),
            new List<DateOnly?>
            {
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue,
                null
            },
            """{"Prop":["-infinity","2023-05-29","infinity",null]}""",
            mappedCollection: true);

    #endregion DateOnly

    #region DateTime

    public override void Can_read_write_DateTime_JSON_values(string value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_TimeSpan_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("2023-05-29T10:52:47.206435Z", """{"Prop":"2023-05-29T10:52:47.206435Z"}""")]
    public virtual void Can_read_write_DateTime_JSON_values_npgsql(string value, string json)
        => Can_read_and_write_JSON_value<DateTimeType, DateTime>(
            nameof(DateTimeType.DateTime),
            DateTime.Parse(value, CultureInfo.InvariantCulture).ToUniversalTime(), json);

    [ConditionalFact]
    public virtual void Can_read_write_DateTime_JSON_values_npgsql_infinity()
        => Can_read_and_write_JSON_value<DateTimeType, DateTime>(
            nameof(DateTimeType.DateTime),
            DateTime.MaxValue, """{"Prop":"infinity"}""");

    [ConditionalFact]
    public virtual void Can_read_write_DateTime_JSON_values_npgsql_negative_infinity()
        => Can_read_and_write_JSON_value<DateTimeType, DateTime>(
            nameof(DateTimeType.DateTime),
            DateTime.MinValue, """{"Prop":"-infinity"}""");

    public override void Can_read_write_nullable_DateTime_JSON_values(string? value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_TimeSpan_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", """{"Prop":"-infinity"}""")]
    [InlineData("9999-12-31T23:59:59.9999999", """{"Prop":"infinity"}""")]
    [InlineData("2023-05-29T10:52:47.206435", """{"Prop":"2023-05-29T10:52:47.206435Z"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateTime_JSON_values_npgsql(string? value, string json)
        => Can_read_and_write_JSON_value<NullableDateTimeType, DateTime?>(
            nameof(NullableDateTimeType.DateTime),
            value == null
                ? default(DateTime?)
                : DateTime.SpecifyKind(DateTime.Parse(value, CultureInfo.InvariantCulture), DateTimeKind.Utc), json);

    [ConditionalFact]
    public override void Can_read_write_collection_of_DateTime_JSON_values()
        => Can_read_and_write_JSON_value<DateTimeCollectionType, List<DateTime>>(
            nameof(DateTimeCollectionType.DateTime),
            new List<DateTime>
            {
                DateTime.MinValue,
                new(2023, 5, 29, 10, 52, 47, DateTimeKind.Utc),
                DateTime.MaxValue
            },
            """{"Prop":["-infinity","2023-05-29T10:52:47Z","infinity"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_nullable_DateTime_JSON_values()
        => Can_read_and_write_JSON_value<NullableDateTimeCollectionType, List<DateTime?>>(
            nameof(NullableDateTimeCollectionType.DateTime),
            new List<DateTime?>
            {
                DateTime.MinValue,
                null,
                new(2023, 5, 29, 10, 52, 47, DateTimeKind.Utc),
                DateTime.MaxValue
            },
            """{"Prop":["-infinity",null,"2023-05-29T10:52:47Z","infinity"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public virtual void Can_read_write_DateTime_timestamptz_JSON_values_infinity()
    {
        Can_read_and_write_JSON_value<DateTimeType, DateTime>(nameof(DateTimeType.DateTime), DateTime.MinValue, """{"Prop":"-infinity"}""");
        Can_read_and_write_JSON_value<DateTimeType, DateTime>(nameof(DateTimeType.DateTime), DateTime.MaxValue, """{"Prop":"infinity"}""");
    }

    [ConditionalFact]
    public virtual void Can_read_write_DateTime_timestamp_JSON_values_infinity()
    {
        Can_read_and_write_JSON_property_value<DateTimeType, DateTime>(
            b => b.HasColumnType("timestamp without time zone"),
            nameof(DateTimeType.DateTime),
            DateTime.MinValue,
            """{"Prop":"-infinity"}""");

        Can_read_and_write_JSON_property_value<DateTimeType, DateTime>(
            b => b.HasColumnType("timestamp without time zone"),
            nameof(DateTimeType.DateTime),
            DateTime.MaxValue,
            """{"Prop":"infinity"}""");
    }

    #endregion DateTime

    #region DateTimeOffset

    public override void Can_read_write_DateTimeOffset_JSON_values(string value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_DateTimeOffset_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.000000-01:00", """{"Prop":"0001-01-01T00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.999999+02:00", """{"Prop":"9999-12-31T23:59:59.999999\u002B02:00"}""")]
    [InlineData("0001-01-01T00:00:00.000000-03:00", """{"Prop":"0001-01-01T00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.567285+04:00", """{"Prop":"2023-05-29T11:11:15.567285\u002B04:00"}""")]
    public virtual void Can_read_write_DateTimeOffset_JSON_values_npgsql(string value, string json)
        => Can_read_and_write_JSON_value<DateTimeOffsetType, DateTimeOffset>(
            nameof(DateTimeOffsetType.DateTimeOffset),
            DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    public override void Can_read_write_nullable_DateTimeOffset_JSON_values(string? value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_DateTimeOffset_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.000000-01:00", """{"Prop":"0001-01-01T00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.999999+02:00", """{"Prop":"9999-12-31T23:59:59.999999\u002B02:00"}""")]
    [InlineData("0001-01-01T00:00:00.000000-03:00", """{"Prop":"0001-01-01T00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.567285+04:00", """{"Prop":"2023-05-29T11:11:15.567285\u002B04:00"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateTimeOffset_JSON_values_npgsql(string? value, string json)
        => Can_read_and_write_JSON_value<NullableDateTimeOffsetType, DateTimeOffset?>(
            nameof(NullableDateTimeOffsetType.DateTimeOffset),
            value == null ? default(DateTimeOffset?) : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalFact]
    public override void Can_read_write_collection_of_DateTimeOffset_JSON_values()
        => Can_read_and_write_JSON_value<DateTimeOffsetCollectionType, List<DateTimeOffset>>(
            nameof(DateTimeOffsetCollectionType.DateTimeOffset),
            new List<DateTimeOffset>
            {
                DateTimeOffset.MinValue,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(-2, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(0, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(2, 0, 0)),
                DateTimeOffset.MaxValue
            },
            """{"Prop":["-infinity","2023-05-29T10:52:47-02:00","2023-05-29T10:52:47\u002B00:00","2023-05-29T10:52:47\u002B02:00","infinity"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values()
        => Can_read_and_write_JSON_value<NullableDateTimeOffsetCollectionType, List<DateTimeOffset?>>(
            nameof(NullableDateTimeOffsetCollectionType.DateTimeOffset),
            new List<DateTimeOffset?>
            {
                DateTimeOffset.MinValue,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(-2, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(0, 0, 0)),
                null,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(2, 0, 0)),
                DateTimeOffset.MaxValue
            },
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
    public virtual void Can_read_write_pg_enum_JSON_values(Mood value, string json)
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
    public virtual void Can_read_write_array_JSON_values(int[] value, string json)
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
    public virtual void Cannot_read_write_multidimensional_array_JSON_values()
        // EF currently throws NRE when the type mapping has no JsonValueReaderWriter (this has been improved for 9.0)
        => Assert.Throws<NullReferenceException>(
            () => Can_read_and_write_JSON_value<MultidimensionalArrayType, int[,]>(
                nameof(MultidimensionalArrayType.MultidimensionalArray),
                new[,] { { 1, 2 }, { 3, 4 } },
                ""));

    protected class MultidimensionalArrayType
    {
        public int[,] MultidimensionalArray { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual void Can_read_write_BigInteger_JSON_values()
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
    public virtual void Can_read_write_BitArray_JSON_values(bool[] value, string json)
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
    public virtual void Can_read_write_LogSequenceNumber_JSON_values(ulong value, string json)
        => Can_read_and_write_JSON_value<LogSequenceNumberType, NpgsqlLogSequenceNumber>(
            nameof(LogSequenceNumberType.LogSequenceNumber),
            new NpgsqlLogSequenceNumber(value),
            json);

    protected class LogSequenceNumberType
    {
        public NpgsqlLogSequenceNumber LogSequenceNumber { get; set; }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => base.OnConfiguring(optionsBuilder.UseNpgsql(b => b.UseNetTopologySuite()));

    static JsonTypesNpgsqlTest()
    {
#pragma warning disable CS0618 // NpgsqlConnection.GlobalTypeMapper is obsolete
        // Note that the enum doesn't actually need to be created in the database, since Can_read_and_write_JSON_value doesn't access
        // the database. We just need the mapping to be picked up by EFCore.PG from the ADO.NET layer.
        NpgsqlConnection.GlobalTypeMapper.MapEnum<Mood>("test.mapped_enum");
#pragma warning restore CS0618
    }
}
