using Microsoft.EntityFrameworkCore.TestModels.Array;

namespace Microsoft.EntityFrameworkCore.Query;

public class ArrayListQueryTest : QueryTestBase<ArrayListQueryTest.ArrayListQueryFixture>
{
    public ArrayListQueryTest(ArrayListQueryFixture fixture, ITestOutputHelper testOutputHelper)
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
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList[0] == 3));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList"[1] = 3
""");
    }

    [ConditionalFact]
    public virtual async Task Index_with_parameter()
    {
        var x = 0;
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList[x] == 3));

        AssertSql(
            """
@x='0'

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList"[@x + 1] = 3
""");
    }

    [ConditionalFact]
    public virtual async Task Nullable_index_with_constant()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntList[0] == 3));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableIntList"[1] = 3
""");
    }

    [ConditionalFact]
    public virtual async Task Nullable_value_array_index_compare_to_null()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntList[2] == null));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableIntList"[3] IS NULL
""");
    }

#pragma warning disable CS0472
    [ConditionalFact]
    public virtual async Task Non_nullable_value_array_index_compare_to_null()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList[1] == null), assertEmpty: true);

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE FALSE
""");
    }
#pragma warning restore CS0472

    [ConditionalFact]
    public virtual async Task Nullable_reference_array_index_compare_to_null()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableStringList[2] == null));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableStringList"[3] IS NULL
""");
    }

    [ConditionalFact]
    public virtual async Task Non_nullable_reference_array_index_compare_to_null()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.StringList[1] == null),  assertEmpty: true);

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE FALSE
""");
    }

    #endregion

    #region SequenceEqual

    [ConditionalFact]
    public virtual async Task SequenceEqual_with_parameter()
    {
        var arr = new[] { 3, 4 };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.SequenceEqual(arr)));

        AssertSql(
            """
@arr={ '3'
'4' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList" = @arr
""");
    }

//     [ConditionalFact]
//     public virtual async Task SequenceEqual_with_array_literal()
//     {
//         await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.SequenceEqual(new[] { 3, 4 })));

//         AssertSql(
//             """
// SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
// FROM "SomeEntities" AS s
// WHERE s."IntList" = ARRAY[3,4]::integer[]
// """);
//     }

    [ConditionalFact]
    public virtual async Task SequenceEqual_over_nullable_with_parameter()
    {
        var arr = new int?[] { 3, 4, null };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntList.SequenceEqual(arr)));

        AssertSql(
            """
@arr={ '3'
'4'
NULL } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableIntList" = @arr
""");
    }

    #endregion SequenceEqual

    #region Containment

    [ConditionalFact]
    public virtual async Task Array_column_Any_equality_operator()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.StringList.Any(p => p == "3")));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE '3' = ANY (s."StringList")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Any_Equals()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.StringList.Any(p => "3".Equals(p))));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE '3' = ANY (s."StringList")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_literal_item()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.Contains(3)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE 3 = ANY (s."IntList")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_parameter_item()
    {
        var p = 3;

        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.Contains(p)));

        AssertSql(
            """
@p='3'

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE @p = ANY (s."IntList")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_column_item()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.Contains(e.Id + 2)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."Id" + 2 = ANY (s."IntList")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_null_constant()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableStringList.Contains(null)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE array_position(s."NullableStringList", NULL) IS NOT NULL
""");
    }

    public void Array_column_Contains_null_parameter_does_not_work()
    {
        using var ctx = CreateContext();

        string? p = null;

        // We incorrectly miss arrays containing non-constant nulls, because detecting those
        // would prevent index use.
        Assert.Equal(
            0,
            ctx.SomeEntities.Count(e => e.StringList.Contains(p!)));

        AssertSql(
            """
SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE NULL = ANY (s."StringList") OR (NULL IS NULL AND array_position(s."StringList", NULL) IS NOT NULL)
""");
    }

    [ConditionalFact]
    public virtual async Task Nullable_array_column_Contains_literal_item()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntList.Contains(3)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE 3 = ANY (s."NullableIntList")
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
        var array = new List<string> { "foo", "xxx" };

        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.NullableText!)));

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
        var array = new List<int> { 1 };

        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.Id)));

        AssertSql(
            """
@array={ '1' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."Id" = ANY (@array)
""");
    }

    public void Array_param_with_null_Contains_non_nullable_not_found()
    {
        using var ctx = CreateContext();

        var array = new List<string?>
        {
            "unknown1",
            "unknown2",
            null
        };

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

    public void Array_param_with_null_Contains_non_nullable_not_found_negated()
    {
        using var ctx = CreateContext();

        var array = new List<string?>
        {
            "unknown1",
            "unknown2",
            null
        };

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

    public void Array_param_with_null_Contains_nullable_not_found()
    {
        using var ctx = CreateContext();

        var array = new List<string?>
        {
            "unknown1",
            "unknown2",
            null
        };

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

    public void Array_param_with_null_Contains_nullable_not_found_negated()
    {
        using var ctx = CreateContext();

        var array = new List<string?>
        {
            "unknown1",
            "unknown2",
            null
        };

        Assert.Equal(2, ctx.SomeEntities.Count(e => !array.Contains(e.NullableText!)));

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
        var values = new List<string> { "1", "999" };

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
        var values = new List<byte> { 20 };

        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => values.Contains(e.Byte)));

        AssertSql(
            """
@values={ '20' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."Byte" = ANY (@values)
""");
    }

    [ConditionalFact]
    public virtual async Task Array_param_Contains_value_converted_column_enum_to_int()
    {
        var array = new List<SomeEnum> { SomeEnum.Two, SomeEnum.Three };

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
        var array = new List<SomeEnum> { SomeEnum.Two, SomeEnum.Three };

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
        var array = new List<SomeEnum?> { SomeEnum.Two, SomeEnum.Three };

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
        var array = new List<SomeEnum?> { SomeEnum.Two, SomeEnum.Three };

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

        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedListOfEnum.Contains(item)));

        AssertSql(
            """
@item='Eight' (Nullable = false)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE @item = ANY (s."ValueConvertedListOfEnum")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_column_Contains_value_converted_constant()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedListOfEnum.Contains(SomeEnum.Eight)));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE 'Eight' = ANY (s."ValueConvertedListOfEnum")
""");
    }

    [ConditionalFact]
    public virtual async Task Array_param_Contains_value_converted_array_column()
    {
        var p = new List<SomeEnum> { SomeEnum.Eight, SomeEnum.Nine };

        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedListOfEnum.All(x => p.Contains(x))));

        AssertSql(
            """
@p={ 'Eight'
'Nine' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."ValueConvertedListOfEnum" <@ @p
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

    [ConditionalFact]
    public virtual async Task Array_column_Contains_in_scalar_subquery()
    {
        await AssertQuery(ss => ss.Set<ArrayContainerEntity>().Where(c => c.ArrayEntities.OrderBy(e => e.Id).First().NullableIntList.Contains(3)));

        AssertSql(
            """
SELECT s."Id"
FROM "SomeEntityContainers" AS s
WHERE 3 = ANY ((
    SELECT s0."NullableIntList"
    FROM "SomeEntities" AS s0
    WHERE s."Id" = s0."ArrayContainerEntityId"
    ORDER BY s0."Id" NULLS FIRST
    LIMIT 1)::integer[])
""");
    }

    #endregion Containment

    #region Length/Count

    [ConditionalFact]
    public virtual async Task Array_Length()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.Count == 2));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."IntList") = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Nullable_array_Length()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntList.Count == 3));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."NullableIntList") = 3
""");
    }

    [ConditionalFact]
    public virtual async Task Array_Length_on_EF_Property()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => EF.Property<List<int>>(e, nameof(ArrayEntity.IntList)).Count == 2));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."IntList") = 2
""");
    }

    #endregion Length/Count

    #region Any/All

    [ConditionalFact]
    public virtual async Task Any_no_predicate()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.Any()));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."IntList") > 0
""");
    }

    [ConditionalFact]
    public virtual async Task Any_like()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.Like(e.NullableText, p))),
            ss => ss.Set<ArrayEntity>()
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
        await AssertQuery(ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.ILike(e.NullableText!, p))),
            ss => ss.Set<ArrayEntity>()
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

        var patternsActual = new List<string>
        {
            "a%",
            "b%",
            "c%"
        };
        var patternsExpected = new List<string>
        {
            "a",
            "b",
            "c"
        };

        await AssertQuery(ss => ss.Set<ArrayEntity>()
                .Where(e => patternsActual.Any(p => EF.Functions.Like(e.NullableText, p))),
            ss => ss.Set<ArrayEntity>()
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
        await AssertQuery(ss => ss.Set<ArrayEntity>()
                .Where(e => new List<string> { "b%", "ba%" }.All(p => EF.Functions.Like(e.NullableText, p))),
            ss => ss.Set<ArrayEntity>()
                .Where(e => new List<string> { "b", "ba" }.All(p => e.NullableText!.StartsWith(p, StringComparison.Ordinal))));

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
        await AssertQuery(ss => ss.Set<ArrayEntity>()
                .Where(e => new List<string> { "B%", "ba%" }.All(p => EF.Functions.ILike(e.NullableText!, p))),
            ss => ss.Set<ArrayEntity>()
                .Where(e => new List<string> { "B", "ba" }.All(p => e.NullableText!.StartsWith(p, StringComparison.OrdinalIgnoreCase))));

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
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => new[] { 2, 3 }.Any(p => e.IntList.Contains(p))));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE ARRAY[2,3]::integer[] && s."IntList"
""");
    }

    [ConditionalFact]
    public virtual async Task Any_Contains_between_column_and_List()
    {
        var ints = new List<int> { 2, 3 };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.Any(i => ints.Contains(i))));

        AssertSql(
            """
@ints={ '2'
'3' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList" && @ints
""");
    }

    [ConditionalFact]
    public virtual async Task Any_Contains_between_column_and_array()
    {
        var ints = new[] { 2, 3 };
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.Any(i => ints.Contains(i))));

        AssertSql(
            """
@ints={ '2'
'3' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList" && @ints
""");
    }

    [ConditionalFact]
    public virtual async Task Any_Contains_between_column_and_other_type()
    {
        var array = new[] { SomeEnum.Eight };

        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedListOfEnum.Any(i => array.Contains(i))));

        AssertSql(
            """
@array={ 'Eight' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."ValueConvertedListOfEnum" && @array
""");
    }

    [ConditionalFact]
    public virtual async Task All_Contains()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => new[] { 5, 6 }.All(p => e.IntList.Contains(p))));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE ARRAY[5,6]::integer[] <@ s."IntList"
""");
    }

    #endregion Any/All

    #region Other translations

    // TODO: https://github.com/dotnet/efcore/issues/30669
    // [ConditionalFact]
    // public virtual async Task Append()
    // {

    //         await base.Append(async);
    //
    //         AssertSql(
    // """
    // SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
    // FROM "SomeEntities" AS s
    // WHERE array_append(s."IntList", 5) = ARRAY[3,4,5]::integer[]
    // """);

//     [ConditionalFact]
//     public virtual async Task Concat()
//     {
//         await AssertQuery(ss => ss.Set<ArrayEntity>()
//                 .Where(e => e.IntList.Concat(new[] { 5, 6 }).SequenceEqual(new[] { 3, 4, 5, 6 })));

//         AssertSql(
//             """
// SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
// FROM "SomeEntities" AS s
// WHERE array_cat(s."IntList", ARRAY[5,6]::integer[]) = ARRAY[3,4,5,6]::integer[]
// """);
//     }

    [ConditionalFact]
    public virtual async Task Array_IndexOf1()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.IndexOf(6) == 1));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE COALESCE(array_position(s."IntList", 6) - 1, -1) = 1
""");
    }

    [ConditionalFact]
    public virtual async Task Array_IndexOf2()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => e.IntList.IndexOf(6, 1) == 1));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE COALESCE(array_position(s."IntList", 6, 2) - 1, -1) = 1
""");
    }

    // Note: see NorthwindFunctionsQueryNpgsqlTest.String_Join_non_aggregate for regular use without an array column/parameter
    [ConditionalFact]
    public virtual async Task String_Join_with_array_of_int_column()
    {
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => string.Join(", ", e.IntList) == "3, 4"));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE array_to_string(s."IntList", ', ', '') = '3, 4'
""");
    }

    [ConditionalFact]
    public virtual async Task String_Join_with_array_of_string_column()
    {
        // This is not in ArrayQueryTest because string.Join uses another overload for string[] than for List<string> and thus
        // ArrayToListReplacingExpressionVisitor won't work.
        await AssertQuery(ss => ss.Set<ArrayEntity>().Where(e => string.Join(", ", e.StringList) == "3, 4"));

        AssertSql(
            """
SELECT s."Id", s."ArrayContainerEntityId", s."ArrayOfStringConvertedToDelimitedString", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IList", s."IntArray", s."IntList", s."ListOfStringConvertedToDelimitedString", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArrayOfEnum", s."ValueConvertedListOfEnum", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE array_to_string(s."StringList", ', ', '') = '3, 4'
""");
    }

    [ConditionalFact]
    public virtual async Task String_Join_disallow_non_array_type_mapped_parameter()
    {
        // This is not in ArrayQueryTest because string.Join uses another overload for string[] than for List<string> and thus
        // ArrayToListReplacingExpressionVisitor won't work.
        await AssertTranslationFailed(() => AssertQuery(
            ss => ss.Set<ArrayEntity>()
                .Where(e => string.Join(", ", e.ListOfStringConvertedToDelimitedString) == "3, 4")));
    }

    #endregion Other translations

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected ArrayQueryContext CreateContext()
        => Fixture.CreateContext();

    public class ArrayListQueryFixture : ArrayQueryFixture
    {
        protected override string StoreName
            => "ArrayListTest";
    }
}
