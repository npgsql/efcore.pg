using System.Collections;
using System.Numerics;

#nullable enable

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
        => Assert.Throws<NullReferenceException>(() => Can_read_and_write_JSON_value<MultidimensionalArrayType, int[,]>(
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

    [ConditionalTheory]
    [InlineData("12:34:56.123456+05:00", """{"Prop":"12:34:56.123456\u002B5"}""")]
    public virtual void Can_read_write_timetz_JSON_values(string value, string json)
        => Can_read_and_write_JSON_property_value<DateTimeOffsetType, DateTimeOffset>(
            b => b.HasColumnType("timetz"),
            nameof(DateTimeOffsetType.DateTimeOffset),
            DateTimeOffset.Parse(value),
            json);

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
