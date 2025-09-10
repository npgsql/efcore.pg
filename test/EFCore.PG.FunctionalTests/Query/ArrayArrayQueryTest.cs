using Microsoft.EntityFrameworkCore.TestModels.Array;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

// ReSharper disable ConvertToConstant.Local

namespace Microsoft.EntityFrameworkCore.Query;

public class ArrayArrayQueryTest : QueryTestBase<ArrayArrayQueryTest.ArrayArrayQueryFixture>
{
    public ArrayArrayQueryTest(ArrayArrayQueryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Roundtrip

    [ConditionalFact]
    public virtual void Roundtrip()
    {
        using var ctx = CreateContext();
        var x = ctx.SomeEntities.Single(e => e.Id == 1);

        Assert.Equal([3, 4], x.IntArray);
        Assert.Equal([3, 4], x.IntList);
        Assert.Equal([3, 4, null], x.NullableIntArray);
        Assert.Equal(
        [
            3,
            4,
            null
        ], x.NullableIntList);
    }

    #endregion

    #region Indexers

    [ConditionalFact]
    public virtual async Task Index_with_constant()
    {        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray[0] == 3));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntArray"[1] = 3
""");
    }

    [ConditionalFact]
    public virtual async Task Index_with_parameter()
    {
        // ReSharper disable once ConvertToConstant.Local
        var x = 0;
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray[x] == 3));

        AssertSql(
            """
@x='0'

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntArray"[@x + 1] = 3
""");
    }

    [ConditionalFact]
    public virtual async Task Nullable_index_with_constant()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntArray[0] == 3));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableIntArray"[1] = 3
""");
    }

    [ConditionalFact]
    public virtual async Task Nullable_value_array_index_compare_to_null()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntArray[2] == null));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableIntArray"[3] IS NULL
""");
    }

    [ConditionalFact]
    public virtual async Task Non_nullable_value_array_index_compare_to_null()
    {
#pragma warning disable CS0472
        await AssertQuery(
            ss => ss.Set<ArrayEntity>().Where(e => e.IntArray[1] == null),
            assertEmpty: true);
#pragma warning restore CS0472

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE FALSE
""");
    }

    [ConditionalFact]
    public virtual async Task Nullable_reference_array_index_compare_to_null()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>()
                .Where(e => e.NullableStringArray[2] == null));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableStringArray"[3] IS NULL
""");
    }

    [ConditionalFact]
    public virtual async Task Non_nullable_reference_array_index_compare_to_null()
    {
#pragma warning disable CS0472
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.StringArray[1] == null),
            assertEmpty: true);
#pragma warning restore CS0472

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE FALSE
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Index_bytea_with_constant(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => e.Bytea[0] == 3));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE get_byte(s."Bytea", 0) = 3
""");
    }

    #endregion

    #region SequenceEqual

    [ConditionalFact]
    public virtual async Task SequenceEqual_with_parameter()
    {
        var arr = new[] { 3, 4 };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.SequenceEqual(arr)));

        AssertSql(
            """
@arr={ '3'
'4' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntArray" = @arr
""");
    }

//     [ConditionalFact]
//     public virtual async Task SequenceEqual_with_array_literal()
//     {
//         await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.SequenceEqual(new[] { 3, 4 })));

//         AssertSql(
//             """
// SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
// FROM "SomeEntities" AS s
// WHERE s."IntArray" = ARRAY[3,4]::integer[]
// """);
//     }

//     [ConditionalFact]
//     public virtual async Task SequenceEqual_over_nullable_with_parameter()
//     {
//         var arr = new int?[] { 3, 4, null };
//         await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntArray.SequenceEqual(arr)));
//         AssertSql(
//             """
// @arr={ '3', '4', NULL } (DbType = Object)

// SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
// FROM "SomeEntities" AS s
// WHERE s."NullableIntArray" = @arr
// """);
//     }

    #endregion SequenceEqual

    #region Containment

    [ConditionalFact]
    public virtual async Task Array_column_Any_equality_operator()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.StringArray.Any(p => p == "3")));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE '3' = ANY (s."StringArray")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Any_Equals()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.StringArray.Any(p => "3".Equals(p))));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE '3' = ANY (s."StringArray")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_literal_item()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Contains(3)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE 3 = ANY (s."IntArray")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_parameter_item()
    {
        var p = 3;
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Contains(p)));

        AssertSql(
            """
@p='3'

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE @p = ANY (s."IntArray")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_column_item()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Contains(e.Id + 2)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."Id" + 2 = ANY (s."IntArray")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_null_constant()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableStringArray.Contains(null)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE array_position(s."NullableStringArray", NULL) IS NOT NULL
""");
    }

    [ConditionalFact]
    public virtual void Array_column_Contains_null_parameter_does_not_work()
    {
        using var ctx = CreateContext();

        string? p = null;

        // We incorrectly miss arrays containing non-constant nulls, because detecting those
        // would prevent index use.
        Assert.Equal(
            0,
            ctx.SomeEntities.Count(e => e.StringArray.Contains(p)));

        AssertSql(
            """
SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE NULL = ANY (s."StringArray") OR (NULL IS NULL AND array_position(s."StringArray", NULL) IS NOT NULL)
""");
    }

    [ConditionalFact]
    public virtual async Task Nullable_array_column_Contains_literal_item()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntArray.Contains(3)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE 3 = ANY (s."NullableIntArray")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_constant_Contains_column()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => new[] { "foo", "xxx" }.Contains(e.NullableText)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" IN ('foo', 'xxx')
""");
    }

    [ConditionalFact]
    public virtual async Task Array_param_Contains_nullable_column()
    {
        var array = new[] { "foo", "xxx" };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.NullableText)));

        AssertSql(
            """
@array={ 'foo'
'xxx' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" = ANY (@array) OR (s."NullableText" IS NULL AND array_position(@array, NULL) IS NOT NULL)
""");
    }

    [ConditionalFact]
    public virtual async Task Array_param_Contains_non_nullable_column()
    {
        var array = new[] { 1 };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.Id)));

        AssertSql(
            """
@array={ '1' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."Id" = ANY (@array)
""");
    }

    [ConditionalFact]
    public virtual void Array_param_with_null_Contains_non_nullable_not_found()
    {
        using var ctx = CreateContext();

        var array = new[] { "unknown1", "unknown2", null };

        Assert.Equal(0, ctx.SomeEntities.Count(e => array.Contains(e.NonNullableText)));

        AssertSql(
            """
@array={ 'unknown1'
'unknown2'
NULL } (DbType = Object)

SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE s."NonNullableText" = ANY (@array)
""");
    }

    [ConditionalFact]
    public virtual void Array_param_with_null_Contains_non_nullable_not_found_negated()
    {
        using var ctx = CreateContext();

        var array = new[] { "unknown1", "unknown2", null };

        Assert.Equal(2, ctx.SomeEntities.Count(e => !array.Contains(e.NonNullableText)));

        AssertSql(
            """
@array={ 'unknown1'
'unknown2'
NULL } (DbType = Object)

SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE NOT (s."NonNullableText" = ANY (@array) AND s."NonNullableText" = ANY (@array) IS NOT NULL)
""");
    }

    [ConditionalFact]
    public virtual void Array_param_with_null_Contains_nullable_not_found()
    {
        using var ctx = CreateContext();

        var array = new[] { "unknown1", "unknown2", null };

        Assert.Equal(0, ctx.SomeEntities.Count(e => array.Contains(e.NullableText)));

        AssertSql(
            """
@array={ 'unknown1'
'unknown2'
NULL } (DbType = Object)

SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE s."NullableText" = ANY (@array) OR (s."NullableText" IS NULL AND array_position(@array, NULL) IS NOT NULL)
""");
    }

    [ConditionalFact]
    public virtual void Array_param_with_null_Contains_nullable_not_found_negated()
    {
        using var ctx = CreateContext();

        var array = new[] { "unknown1", "unknown2", null };

        Assert.Equal(2, ctx.SomeEntities.Count(e => !array.Contains(e.NullableText)));

        AssertSql(
            """
@array={ 'unknown1'
'unknown2'
NULL } (DbType = Object)

SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE NOT (s."NullableText" = ANY (@array) AND s."NullableText" = ANY (@array) IS NOT NULL) AND (s."NullableText" IS NOT NULL OR array_position(@array, NULL) IS NULL)
""");
    }

    [ConditionalFact]
    public virtual async Task Array_param_Contains_column_with_ToString()
    {
        var values = new[] { "1", "999" };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => values.Contains(e.Id.ToString())));

        AssertSql(
            """
@values={ '1'
'999' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."Id"::text = ANY (@values)
""");
    }

    [ConditionalFact]
    public virtual async Task Byte_array_parameter_contains_column()
    {
        var values = new byte[] { 20 };

        await AssertQuery(
            ss => ss.Set<ArrayEntity>().Where(e => values.Contains(e.Byte)));

        // Note: EF Core prints the parameter as a bytea, but it's actually a smallint[] (otherwise ANY would fail)
        AssertSql(
            """
@values='0x14' (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."Byte" = ANY (@values)
""");
    }

    [ConditionalFact]
    public virtual async Task Array_param_Contains_value_converted_column_enum_to_int()
    {
        var array = new[] { SomeEnum.Two, SomeEnum.Three };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.EnumConvertedToInt)));

        AssertSql(
            """
@array={ '-2'
'-3' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."EnumConvertedToInt" = ANY (@array)
""");
    }

    [ConditionalFact]
    public virtual async Task Array_param_Contains_value_converted_column_enum_to_string()
    {
        var array = new[] { SomeEnum.Two, SomeEnum.Three };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.EnumConvertedToString)));

        AssertSql(
            """
@array={ 'Two'
'Three' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."EnumConvertedToString" = ANY (@array)
""");
    }

    [ConditionalFact]
    public virtual async Task Array_param_Contains_value_converted_column_nullable_enum_to_string()
    {
        var array = new SomeEnum?[] { SomeEnum.Two, SomeEnum.Three };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.NullableEnumConvertedToString)));

        AssertSql(
            """
@array={ 'Two'
'Three' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableEnumConvertedToString" = ANY (@array) OR (s."NullableEnumConvertedToString" IS NULL AND array_position(@array, NULL) IS NOT NULL)
""");
    }

    [ConditionalFact]
    public virtual async Task Array_param_Contains_value_converted_column_nullable_enum_to_string_with_non_nullable_lambda()
    {
        var array = new SomeEnum?[] { SomeEnum.Two, SomeEnum.Three };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.NullableEnumConvertedToStringWithNonNullableLambda)));

        AssertSql(
            """
@array={ 'Two'
'Three' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableEnumConvertedToStringWithNonNullableLambda" = ANY (@array) OR (s."NullableEnumConvertedToStringWithNonNullableLambda" IS NULL AND array_position(@array, NULL) IS NOT NULL)
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_value_converted_param()
    {
        var item = SomeEnum.Eight;
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedArrayOfEnum.Contains(item)));

        AssertSql(
            """
@item='Eight' (Nullable = false)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE @item = ANY (s."ValueConvertedArrayOfEnum")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_value_converted_constant()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedArrayOfEnum.Contains(SomeEnum.Eight)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE 'Eight' = ANY (s."ValueConvertedArrayOfEnum")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_param_Contains_value_converted_array_column()
    {
        var p = new[] { SomeEnum.Eight, SomeEnum.Nine };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedArrayOfEnum.All(x => p.Contains(x))));

        AssertSql(
            """
@p={ 'Eight'
'Nine' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."ValueConvertedArrayOfEnum" <@ @p
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_in_scalar_subquery()
    {
        await AssertQuery(
            ss => ss.Set<ArrayContainerEntity>().Where(c => c.ArrayEntities.OrderBy(e => e.Id).First().NullableIntArray.Contains(3)));

        AssertSql(
            """
SELECT s."Id"
FROM "SomeEntityContainers" AS s
WHERE 3 = ANY ((
    SELECT s0."NullableIntArray"
    FROM "SomeEntities" AS s0
    WHERE s."Id" = s0."ArrayContainerEntityId"
    ORDER BY s0."Id" NULLS FIRST
    LIMIT 1)::integer[])
""");
    }

    [ConditionalFact]
    public virtual async Task IList_column_contains_constant()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(a => a.IList.Contains(10)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE 10 = ANY (s."IList")
""");
    }

    #endregion Containment

    #region Length/Count

    [ConditionalFact]
    public virtual async Task Array_Length()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Length == 2));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."IntArray") = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Nullable_array_Length()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntArray.Length == 3));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."NullableIntArray") = 3
""");
    }

    [ConditionalFact]
    public virtual async Task Array_Length_on_EF_Property()
    {
        await AssertQuery(
            ss => ss.Set<ArrayEntity>().Where(e => EF.Property<int[]>(e, nameof(ArrayEntity.IntArray)).Length == 2));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."IntArray") = 2
""");
    }

    #endregion Length/Count

    #region Any/All

    [ConditionalFact]
    public virtual async Task Any_no_predicate()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Any()));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."IntArray") > 0
""");
    }

    [ConditionalFact]
    public virtual async Task Any_like()
    {
        await AssertQuery(
            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.Like(e.NullableText, p))),            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "a", "b", "c" }.Any(p => e.NullableText!.StartsWith(p, StringComparison.Ordinal))));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" LIKE ANY (ARRAY['a%','b%','c%']::text[])
""");
    }

    [ConditionalFact]
    public virtual async Task Any_ilike()
    {
        await AssertQuery(
            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.ILike(e.NullableText!, p))),            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "a", "b", "c" }.Any(p => e.NullableText!.StartsWith(p, StringComparison.OrdinalIgnoreCase))));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" ILIKE ANY (ARRAY['a%','b%','c%']::text[])
""");
    }

    [ConditionalFact]
    public virtual async Task Any_like_anonymous()
    {
        await using var ctx = CreateContext();

        var patternsActual = new[] { "a%", "b%", "c%" };
        var patternsExpected = new[] { "a", "b", "c" };

        await AssertQuery(
            ss => ss.Set<ArrayEntity>()
                .Where(e => patternsActual.Any(p => EF.Functions.Like(e.NullableText, p))),            ss => ss.Set<ArrayEntity>()
                .Where(e => patternsExpected.Any(p => e.NullableText!.StartsWith(p, StringComparison.Ordinal))));

        AssertSql(
            """
@patternsActual={ 'a%'
'b%'
'c%' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" LIKE ANY (@patternsActual)
""");
    }

    [ConditionalFact]
    public virtual async Task All_like()
    {
        await AssertQuery(
            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "b%", "ba%" }.All(p => EF.Functions.Like(e.NullableText, p))),            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "b", "ba" }.All(p => e.NullableText!.StartsWith(p, StringComparison.Ordinal))));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" LIKE ALL (ARRAY['b%','ba%']::text[])
""");
    }

    [ConditionalFact]
    public virtual async Task All_ilike()
    {
        await AssertQuery(
            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "B%", "ba%" }.All(p => EF.Functions.ILike(e.NullableText!, p))),            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "B", "ba" }.All(p => e.NullableText!.StartsWith(p, StringComparison.OrdinalIgnoreCase))));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" ILIKE ALL (ARRAY['B%','ba%']::text[])
""");
    }

    [ConditionalFact]
    public virtual async Task Any_Contains_on_constant_array()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => new[] { 2, 3 }.Any(p => e.IntArray.Contains(p))));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE ARRAY[2,3]::integer[] && s."IntArray"
""");
    }

    [ConditionalFact]
    public virtual async Task Any_Contains_between_column_and_List()
    {
        var ints = new List<int> { 2, 3 };

        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Any(i => ints.Contains(i))));

        AssertSql(
            """
@ints={ '2'
'3' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntArray" && @ints
""");
    }

    [ConditionalFact]
    public virtual async Task Any_Contains_between_column_and_array()
    {
        var ints = new[] { 2, 3 };

        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntArray.Any(i => ints.Contains(i))));

        AssertSql(
            """
@ints={ '2'
'3' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntArray" && @ints
""");
    }

    [ConditionalFact]
    public virtual async Task Any_Contains_between_column_and_other_type()
    {
        var list = new List<SomeEnum> { SomeEnum.Eight };

        await AssertQuery(
            ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedArrayOfEnum.Any(i => list.Contains(i))));

        AssertSql(
            """
@list={ 'Eight' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."ValueConvertedArrayOfEnum" && @list
""");
    }

    [ConditionalFact]
    public virtual async Task All_Contains()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => new[] { 5, 6 }.All(p => e.IntArray.Contains(p))));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE ARRAY[5,6]::integer[] <@ s."IntArray"
""");
    }

    #endregion Any/All

    #region Other translations

    [ConditionalFact]
    public virtual async Task Append()
        // TODO: https://github.com/dotnet/efcore/issues/30669
        => await AssertTranslationFailed(() => AssertQuery(
            ss => ss.Set<ArrayEntity>()
                .Where(e => e.IntArray.Append(5).SequenceEqual(new[] { 3, 4, 5 }))));

    //         await base.Append(async);
    //
    //         AssertSql(
    // """
    // SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
    // FROM "SomeEntities" AS s
    // WHERE array_append(s."IntArray", 5) = ARRAY[3,4,5]::integer[]
    // """);

    // [ConditionalFact]
    // public virtual async Task Concat()
    // {
    //     await AssertQuery(
    //         ss => ss.Set<ArrayEntity>()
    //             .Where(e => e.IntArray.Concat(new[] { 5, 6 }).SequenceEqual(new[] { 3, 4, 5, 6 })));

//         AssertSql(
//             """
// SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
// FROM "SomeEntities" AS s
// WHERE array_cat(s."IntArray", ARRAY[5,6]::integer[]) = ARRAY[3,4,5,6]::integer[]
// """);
//     }

    [ConditionalFact]
    public virtual async Task Array_IndexOf1()
    {
        await AssertQuery(
            ss => ss.Set<ArrayEntity>().Where(e => Array.IndexOf(e.IntArray, 6) == 1));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE COALESCE(array_position(s."IntArray", 6) - 1, -1) = 1
""");
    }

    [ConditionalFact]
    public virtual async Task Array_IndexOf2()
    {
        await AssertQuery(
            ss => ss.Set<ArrayEntity>().Where(e => Array.IndexOf(e.IntArray, 6, 1) == 1));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE COALESCE(array_position(s."IntArray", 6, 2) - 1, -1) = 1
""");
    }

    // Note: see NorthwindFunctionsQueryNpgsqlTest.String_Join_non_aggregate for regular use without an array column/parameter
    [ConditionalFact]
    public virtual async Task String_Join_with_array_of_int_column()
    {
        await AssertQuery(
            ss => ss.Set<ArrayEntity>()
                .Where(e => string.Join(", ", e.IntArray) == "3, 4"));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE array_to_string(s."IntArray", ', ', '') = '3, 4'
""");
    }

    [ConditionalFact]
    public virtual async Task String_Join_with_array_of_string_column()
    {
        // This is not in ArrayQueryTest because string.Join uses another overload for string[] than for List<string> and thus
        // ArrayToListReplacingExpressionVisitor won't work.
        await AssertQuery(
            ss => ss.Set<ArrayEntity>()
                .Where(e => string.Join(", ", e.StringArray) == "3, 4"));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE array_to_string(s."StringArray", ', ', '') = '3, 4'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Join_disallow_non_array_type_mapped_parameter(bool async)
    {
        // This is not in ArrayQueryTest because string.Join uses another overload for string[] than for List<string> and thus
        // ArrayToListReplacingExpressionVisitor won't work.
        await AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>()
                .Where(e => string.Join(", ", e.ArrayOfStringConvertedToDelimitedString) == "3, 4")));
    }

    #endregion Other translations

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected ArrayQueryContext CreateContext()
        => Fixture.CreateContext();

    public class ArrayArrayQueryFixture : ArrayQueryFixture
    {
        protected override string StoreName
            => "ArrayQueryTest";
    }
}
