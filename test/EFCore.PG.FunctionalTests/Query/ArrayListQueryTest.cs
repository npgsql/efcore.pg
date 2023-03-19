using Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Array;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class ArrayListQueryTest : ArrayQueryTest<ArrayListQueryTest.ArrayListQueryFixture>
{
    public ArrayListQueryTest(ArrayListQueryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Indexers

    public override async Task Index_with_constant(bool async)
    {
        await base.Index_with_constant(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList"[1] = 3
""");
    }

    public override async Task Index_with_parameter(bool async)
    {
        await base.Index_with_parameter(async);

        AssertSql(
"""
@__x_0='0'

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList"[@__x_0 + 1] = 3
""");
    }

    public override async Task Nullable_index_with_constant(bool async)
    {
        await base.Nullable_index_with_constant(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableIntList"[1] = 3
""");
    }

    public override async Task Nullable_value_array_index_compare_to_null(bool async)
    {
        await base.Nullable_value_array_index_compare_to_null(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableIntList"[3] IS NULL
""");
    }

    public override async Task Non_nullable_value_array_index_compare_to_null(bool async)
    {
        await base.Non_nullable_value_array_index_compare_to_null(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE FALSE
""");
    }

    public override async Task Nullable_reference_array_index_compare_to_null(bool async)
    {
        await base.Nullable_reference_array_index_compare_to_null(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableStringList"[3] IS NULL
""");
    }

    public override async Task Non_nullable_reference_array_index_compare_to_null(bool async)
    {
        await base.Non_nullable_reference_array_index_compare_to_null(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE FALSE
""");
    }

    #endregion

    #region SequenceEqual

    public override async Task SequenceEqual_with_parameter(bool async)
    {
        await base.SequenceEqual_with_parameter(async);

        AssertSql(
"""
@__arr_0={ '3', '4' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList" = @__arr_0
""");
    }

    public override async Task SequenceEqual_with_array_literal(bool async)
    {
        await base.SequenceEqual_with_array_literal(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList" = ARRAY[3,4]::integer[]
""");
    }

    public override async Task SequenceEqual_over_nullable_with_parameter(bool async)
    {
        await base.SequenceEqual_over_nullable_with_parameter(async);

        AssertSql(
"""
@__arr_0={ '3', '4', NULL } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableIntList" = @__arr_0
""");
    }

    #endregion SequenceEqual

    #region Containment

    public override async Task Array_column_Any_equality_operator(bool async)
    {
        await base.Array_column_Any_equality_operator(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."StringList" @> ARRAY['3']::text[]
""");
    }

    public override async Task Array_column_Any_Equals(bool async)
    {
        await base.Array_column_Any_Equals(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."StringList" @> ARRAY['3']::text[]
""");
    }

    public override async Task Array_column_Contains_literal_item(bool async)
    {
        await base.Array_column_Contains_literal_item(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList" @> ARRAY[3]::integer[]
""");
    }

    public override async Task Array_column_Contains_parameter_item(bool async)
    {
        await base.Array_column_Contains_parameter_item(async);

        AssertSql(
"""
@__p_0='3'

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList" @> ARRAY[@__p_0]::integer[]
""");
    }

    public override async Task Array_column_Contains_column_item(bool async)
    {
        await base.Array_column_Contains_column_item(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList" @> ARRAY[s."Id" + 2]::integer[]
""");
    }

    public override async Task Array_column_Contains_null_constant(bool async)
    {
        await base.Array_column_Contains_null_constant(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE array_position(s."NullableStringList", NULL) IS NOT NULL
""");
    }

    public override void Array_column_Contains_null_parameter_does_not_work()
    {
        using var ctx = CreateContext();

        string p = null;

        // We incorrectly miss arrays containing non-constant nulls, because detecting those
        // would prevent index use.
        Assert.Equal(
            0,
            ctx.SomeEntities.Count(e => e.StringList.Contains(p)));

        AssertSql(
"""
SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE s."StringList" @> ARRAY[NULL]::text[]
""");
    }

    public override async Task Nullable_array_column_Contains_literal_item(bool async)
    {
        await base.Nullable_array_column_Contains_literal_item(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableIntList" @> ARRAY[3]::integer[]
""");
    }

    public override async Task Array_constant_Contains_column(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => new[] { "foo", "xxx" }.Contains(e.NullableText)),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" IN ('foo', 'xxx')
""");
    }

    public override async Task Array_param_Contains_nullable_column(bool async)
    {
        var array = new List<string> { "foo", "xxx" };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.NullableText)),
            entryCount: 1);

        AssertSql(
"""
@__array_0={ 'foo', 'xxx' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" = ANY (@__array_0) OR (s."NullableText" IS NULL AND array_position(@__array_0, NULL) IS NOT NULL)
""");
    }

    public override async Task Array_param_Contains_non_nullable_column(bool async)
    {
        var array = new List<int> { 1 };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.Id)),
            entryCount: 1);

        AssertSql(
"""
@__array_0={ '1' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."Id" = ANY (@__array_0)
""");
    }

    public override void Array_param_with_null_Contains_non_nullable_not_found()
    {
        using var ctx = CreateContext();

        var array = new List<string>
        {
            "unknown1",
            "unknown2",
            null
        };

        Assert.Equal(0, ctx.SomeEntities.Count(e => array.Contains(e.NonNullableText)));

        AssertSql(
"""
@__array_0={ 'unknown1', 'unknown2', NULL } (DbType = Object)

SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE s."NonNullableText" = ANY (@__array_0)
""");
    }

    public override void Array_param_with_null_Contains_non_nullable_not_found_negated()
    {
        using var ctx = CreateContext();

        var array = new List<string>
        {
            "unknown1",
            "unknown2",
            null
        };

        Assert.Equal(2, ctx.SomeEntities.Count(e => !array.Contains(e.NonNullableText)));

        AssertSql(
"""
@__array_0={ 'unknown1', 'unknown2', NULL } (DbType = Object)

SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE NOT (s."NonNullableText" = ANY (@__array_0) AND s."NonNullableText" = ANY (@__array_0) IS NOT NULL)
""");
    }

    public override void Array_param_with_null_Contains_nullable_not_found()
    {
        using var ctx = CreateContext();

        var array = new List<string>
        {
            "unknown1",
            "unknown2",
            null
        };

        Assert.Equal(0, ctx.SomeEntities.Count(e => array.Contains(e.NullableText)));

        AssertSql(
"""
@__array_0={ 'unknown1', 'unknown2', NULL } (DbType = Object)

SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE s."NullableText" = ANY (@__array_0) OR (s."NullableText" IS NULL AND array_position(@__array_0, NULL) IS NOT NULL)
""");
    }

    public override void Array_param_with_null_Contains_nullable_not_found_negated()
    {
        using var ctx = CreateContext();

        var array = new List<string>
        {
            "unknown1",
            "unknown2",
            null
        };

        Assert.Equal(2, ctx.SomeEntities.Count(e => !array.Contains(e.NullableText)));

        AssertSql(
"""
@__array_0={ 'unknown1', 'unknown2', NULL } (DbType = Object)

SELECT count(*)::int
FROM "SomeEntities" AS s
WHERE NOT (s."NullableText" = ANY (@__array_0) AND s."NullableText" = ANY (@__array_0) IS NOT NULL) AND (s."NullableText" IS NOT NULL OR array_position(@__array_0, NULL) IS NULL)
""");
    }

    public override async Task Array_param_Contains_column_with_ToString(bool async)
    {
        var values = new List<string> { "1", "999" };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => values.Contains(e.Id.ToString())),
            entryCount: 1);

        AssertSql(
"""
@__values_0={ '1', '999' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."Id"::text = ANY (@__values_0)
""");
    }

    public override async Task Byte_array_parameter_contains_column(bool async)
    {
        var values = new List<byte> { 20 };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => values.Contains(e.Byte)),
            entryCount: 1);

        AssertSql(
"""
@__values_0={ '20' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."Byte" = ANY (@__values_0)
""");
    }

    public override async Task Array_param_Contains_value_converted_column_enum_to_int(bool async)
    {
        var array = new List<SomeEnum> { SomeEnum.Two, SomeEnum.Three };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.EnumConvertedToInt)),
            entryCount: 1);

        AssertSql(
"""
@__array_0={ '-2', '-3' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."EnumConvertedToInt" = ANY (@__array_0)
""");
    }

    public override async Task Array_param_Contains_value_converted_column_enum_to_string(bool async)
    {
        var array = new List<SomeEnum> { SomeEnum.Two, SomeEnum.Three };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.EnumConvertedToString)),
            entryCount: 1);

        AssertSql(
"""
@__array_0={ 'Two', 'Three' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."EnumConvertedToString" = ANY (@__array_0)
""");
    }

    public override async Task Array_param_Contains_value_converted_column_nullable_enum_to_string(bool async)
    {
        var array = new List<SomeEnum?> { SomeEnum.Two, SomeEnum.Three };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.NullableEnumConvertedToString)),
            entryCount: 1);

        AssertSql(
"""
@__array_0={ 'Two', 'Three' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableEnumConvertedToString" = ANY (@__array_0) OR (s."NullableEnumConvertedToString" IS NULL AND array_position(@__array_0, NULL) IS NOT NULL)
""");
    }

    public override async Task Array_param_Contains_value_converted_column_nullable_enum_to_string_with_non_nullable_lambda(bool async)
    {
        var array = new List<SomeEnum?> { SomeEnum.Two, SomeEnum.Three };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => array.Contains(e.NullableEnumConvertedToStringWithNonNullableLambda)),
            entryCount: 1);

        AssertSql(
"""
@__array_0={ 'Two', 'Three' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableEnumConvertedToStringWithNonNullableLambda" = ANY (@__array_0) OR (s."NullableEnumConvertedToStringWithNonNullableLambda" IS NULL AND array_position(@__array_0, NULL) IS NOT NULL)
""");
    }

    public override async Task Array_column_Contains_value_converted_param(bool async)
    {
        var item = SomeEnum.Eight;

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedList.Contains(item)),
            entryCount: 1);

        AssertSql(
"""
@__item_0='Eight' (Nullable = false)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."ValueConvertedList" @> ARRAY[@__item_0]::text[]
""");
    }

    public override async Task Array_column_Contains_value_converted_constant(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedList.Contains(SomeEnum.Eight)),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."ValueConvertedList" @> ARRAY['Eight']::text[]
""");
    }

    public override async Task Array_param_Contains_value_converted_array_column(bool async)
    {
        var p = new List<SomeEnum> { SomeEnum.Eight, SomeEnum.Nine };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedArray.All(x => p.Contains(x))),
            entryCount: 1);

        AssertSql(
"""
@__p_0={ 'Eight', 'Nine' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."ValueConvertedList" <@ @__p_0
""");
    }

    public override async Task Array_column_Contains_in_scalar_subquery(bool async)
    {
        await base.Array_column_Contains_in_scalar_subquery(async);

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

    public override async Task Array_Length(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => e.IntList.Count == 2),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."IntList") = 2
""");
    }

    public override async Task Nullable_array_Length(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => e.NullableIntList.Count == 3),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."NullableIntList") = 3
""");
    }

    public override async Task Array_Length_on_EF_Property(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => EF.Property<List<int>>(e, nameof(ArrayEntity.IntList)).Count == 2),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."IntList") = 2
""");
    }

    #endregion Length/Count

    #region Any/All

    public override async Task Any_no_predicate(bool async)
    {
        await base.Any_no_predicate(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE cardinality(s."IntList") > 0
""");
    }

    public override async Task Any_like(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.Like(e.NullableText, p))),
            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "a", "b", "c" }.Any(p => e.NullableText.StartsWith(p, StringComparison.Ordinal))),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" LIKE ANY (ARRAY['a%','b%','c%']::text[])
""");
    }

    public override async Task Any_ilike(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.ILike(e.NullableText, p))),
            ss => ss.Set<ArrayEntity>()
                .Where(e => new[] { "a", "b", "c" }.Any(p => e.NullableText.StartsWith(p, StringComparison.OrdinalIgnoreCase))),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" ILIKE ANY (ARRAY['a%','b%','c%']::text[])
""");
    }

    public override async Task Any_like_anonymous(bool async)
    {
        await using var ctx = CreateContext();

        var patternsActual = new List<string> { "a%", "b%", "c%" };
        var patternsExpected = new List<string> { "a", "b", "c" };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>()
                .Where(e => patternsActual.Any(p => EF.Functions.Like(e.NullableText, p))),
            ss => ss.Set<ArrayEntity>()
                .Where(e => patternsExpected.Any(p => e.NullableText.StartsWith(p, StringComparison.Ordinal))),
            entryCount: 1);

        AssertSql(
"""
@__patternsActual_0={ 'a%', 'b%', 'c%' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" LIKE ANY (@__patternsActual_0)
""");
    }

    public override async Task All_like(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>()
                .Where(e => new List<string> { "b%", "ba%" }.All(p => EF.Functions.Like(e.NullableText, p))),
            ss => ss.Set<ArrayEntity>()
                .Where(e => new List<string> { "b", "ba" }.All(p => e.NullableText.StartsWith(p, StringComparison.Ordinal))),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" LIKE ALL (ARRAY['b%','ba%']::text[])
""");
    }

    public override async Task All_ilike(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>()
                .Where(e => new List<string> { "B%", "ba%" }.All(p => EF.Functions.ILike(e.NullableText, p))),
            ss => ss.Set<ArrayEntity>()
                .Where(e => new List<string> { "B", "ba" }.All(p => e.NullableText.StartsWith(p, StringComparison.OrdinalIgnoreCase))),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."NullableText" ILIKE ALL (ARRAY['B%','ba%']::text[])
""");
    }

    public override async Task Any_Contains_on_constant_array(bool async)
    {
        await base.Any_Contains_on_constant_array(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE ARRAY[2,3]::integer[] && s."IntList"
""");
    }

    public override async Task Any_Contains_between_column_and_List(bool async)
    {
        await base.Any_Contains_between_column_and_List(async);

        AssertSql(
"""
@__ints_0={ '2', '3' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList" && @__ints_0
""");
    }

    public override async Task Any_Contains_between_column_and_array(bool async)
    {
        await base.Any_Contains_between_column_and_array(async);

        AssertSql(
"""
@__ints_0={ '2', '3' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."IntList" && @__ints_0
""");
    }

    public override async Task Any_Contains_between_column_and_other_type(bool async)
    {
        var array = new[] { SomeEnum.Eight };

        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => e.ValueConvertedList.Any(i => array.Contains(i))),
            entryCount: 1);

        AssertSql(
"""
@__array_0={ 'Eight' } (DbType = Object)

SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE s."ValueConvertedList" && @__array_0
""");
    }

    public override async Task All_Contains(bool async)
    {
        await base.All_Contains(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE ARRAY[5,6]::integer[] <@ s."IntList"
""");
    }

    #endregion Any/All

    #region Other translations

    public override async Task Append(bool async)
    {
        // TODO: https://github.com/dotnet/efcore/issues/30669
        await AssertTranslationFailed(() => base.Append(async));

//         await base.Append(async);
//
//         AssertSql(
// """
// SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
// FROM "SomeEntities" AS s
// WHERE array_append(s."IntList", 5) = ARRAY[3,4,5]::integer[]
// """);
    }

    public override async Task Concat(bool async)
    {
        await base.Concat(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE array_cat(s."IntList", ARRAY[5,6]::integer[]) = ARRAY[3,4,5,6]::integer[]
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Array_IndexOf1(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => e.IntList.IndexOf(6) == 1),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE COALESCE(array_position(s."IntList", 6) - 1, -1) = 1
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Array_IndexOf2(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<ArrayEntity>().Where(e => e.IntList.IndexOf(6, 1) == 1),
            entryCount: 1);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE COALESCE(array_position(s."IntList", 6, 2) - 1, -1) = 1
""");
    }

    // Note: see NorthwindFunctionsQueryNpgsqlTest.String_Join_non_aggregate for regular use without an array column/parameter
    public override async Task String_Join_with_array_parameter(bool async)
    {
        await base.String_Join_with_array_parameter(async);

        AssertSql(
"""
SELECT s."Id", s."ArrayContainerEntityId", s."Byte", s."ByteArray", s."Bytea", s."EnumConvertedToInt", s."EnumConvertedToString", s."IntArray", s."IntList", s."NonNullableText", s."NullableEnumConvertedToString", s."NullableEnumConvertedToStringWithNonNullableLambda", s."NullableIntArray", s."NullableIntList", s."NullableStringArray", s."NullableStringList", s."NullableText", s."StringArray", s."StringList", s."ValueConvertedArray", s."ValueConvertedList", s."Varchar10", s."Varchar15"
FROM "SomeEntities" AS s
WHERE array_to_string(s."IntList", ', ', '') = '3, 4'
""");
    }

    #endregion Other translations

    public class ArrayListQueryFixture : ArrayQueryFixture
    {
        protected override string StoreName
            => "ArrayListTest";
    }

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
        => new ArrayToListReplacingExpressionVisitor().Visit(serverQueryExpression);

    private class ArrayToListReplacingExpressionVisitor : ExpressionVisitor
    {
        private static readonly PropertyInfo IntArray
            = typeof(ArrayEntity).GetProperty(nameof(ArrayEntity.IntArray));

        private static readonly PropertyInfo NullableIntArray
            = typeof(ArrayEntity).GetProperty(nameof(ArrayEntity.NullableIntArray));

        private static readonly PropertyInfo IntList
            = typeof(ArrayEntity).GetProperty(nameof(ArrayEntity.IntList));

        private static readonly PropertyInfo NullableIntList
            = typeof(ArrayEntity).GetProperty(nameof(ArrayEntity.NullableIntList));

        private static readonly PropertyInfo StringArray
            = typeof(ArrayEntity).GetProperty(nameof(ArrayEntity.StringArray));

        private static readonly PropertyInfo NullableStringArray
            = typeof(ArrayEntity).GetProperty(nameof(ArrayEntity.NullableStringArray));

        private static readonly PropertyInfo StringList
            = typeof(ArrayEntity).GetProperty(nameof(ArrayEntity.StringList));

        private static readonly PropertyInfo NullableStringList
            = typeof(ArrayEntity).GetProperty(nameof(ArrayEntity.NullableStringList));

        private static readonly PropertyInfo ValueConvertedArray
            = typeof(ArrayEntity).GetProperty(nameof(ArrayEntity.ValueConvertedArray));

        private static readonly PropertyInfo ValueConvertedList
            = typeof(ArrayEntity).GetProperty(nameof(ArrayEntity.ValueConvertedList));

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member == IntArray)
            {
                return Expression.MakeMemberAccess(node.Expression, IntList);
            }

            if (node.Member == NullableIntArray)
            {
                return Expression.MakeMemberAccess(node.Expression, NullableIntList);
            }

            if (node.Member == StringArray)
            {
                return Expression.MakeMemberAccess(node.Expression, StringList);
            }

            if (node.Member == NullableStringArray)
            {
                return Expression.MakeMemberAccess(node.Expression, NullableStringList);
            }

            if (node.Member == ValueConvertedArray)
            {
                return Expression.MakeMemberAccess(node.Expression, ValueConvertedList);
            }

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                var listExpression = Visit(node.Left);
                if (listExpression.Type.IsGenericList())
                {
                    var getItemMethod = listExpression.Type.GetMethod("get_Item", new[] { typeof(int) })!;
                    return Expression.Call(listExpression, getItemMethod, node.Right);
                }
            }

            return base.VisitBinary(node);
        }
    }
}
