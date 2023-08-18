// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class PrimitiveCollectionsQueryNpgsqlTest : PrimitiveCollectionsQueryTestBase<
    PrimitiveCollectionsQueryNpgsqlTest.PrimitiveCollectionsQueryNpgsqlFixture>
{
    public PrimitiveCollectionsQueryNpgsqlTest(PrimitiveCollectionsQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Inline_collection_of_ints_Contains(bool async)
    {
        await base.Inline_collection_of_ints_Contains(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains(bool async)
    {
        await base.Inline_collection_of_nullable_ints_Contains(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableInt" IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains_null(bool async)
    {
        await base.Inline_collection_of_nullable_ints_Contains_null(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableInt" IS NULL OR p."NullableInt" = 999
""");
    }

    public override Task Inline_collection_Count_with_zero_values(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Inline_collection_Count_with_zero_values(async),
            RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot);

    public override async Task Inline_collection_Count_with_one_value(bool async)
    {
        await base.Inline_collection_Count_with_one_value(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (2::int)) AS v("Value")
    WHERE v."Value" > p."Id") = 1
""");
    }

    public override async Task Inline_collection_Count_with_two_values(bool async)
    {
        await base.Inline_collection_Count_with_two_values(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (2::int), (999)) AS v("Value")
    WHERE v."Value" > p."Id") = 1
""");
    }

    public override async Task Inline_collection_Count_with_three_values(bool async)
    {
        await base.Inline_collection_Count_with_three_values(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (2::int), (999), (1000)) AS v("Value")
    WHERE v."Value" > p."Id") = 2
""");
    }

    public override Task Inline_collection_Contains_with_zero_values(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Inline_collection_Contains_with_zero_values(async),
            RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot);

    public override async Task Inline_collection_Contains_with_one_value(bool async)
    {
        await base.Inline_collection_Contains_with_one_value(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" = 2
""");
    }

    public override async Task Inline_collection_Contains_with_two_values(bool async)
    {
        await base.Inline_collection_Contains_with_two_values(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (2, 999)
""");
    }

    public override async Task Inline_collection_Contains_with_three_values(bool async)
    {
        await base.Inline_collection_Contains_with_three_values(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (2, 999, 1000)
""");
    }

    public override async Task Inline_collection_Contains_with_all_parameters(bool async)
    {
        await base.Inline_collection_Contains_with_all_parameters(async);

        AssertSql(
"""
@__i_0='2'
@__j_1='999'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (@__i_0, @__j_1)
""");
    }

    public override async Task Inline_collection_Contains_with_constant_and_parameter(bool async)
    {
        await base.Inline_collection_Contains_with_constant_and_parameter(async);

        AssertSql(
"""
@__j_0='999'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (2, @__j_0)
""");
    }

    public override async Task Inline_collection_Contains_with_mixed_value_types(bool async)
    {
        await base.Inline_collection_Contains_with_mixed_value_types(async);

        AssertSql(
"""
@__i_0='11'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" IN (999, @__i_0, p."Id", p."Id" + p."Int")
""");    }

    public override async Task Inline_collection_Contains_as_Any_with_predicate(bool async)
    {
        await base.Inline_collection_Contains_as_Any_with_predicate(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (2, 999)
""");
    }

    public override async Task Inline_collection_negated_Contains_as_All(bool async)
    {
        await base.Inline_collection_negated_Contains_as_All(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" NOT IN (2, 999)
""");
    }

    public override async Task Parameter_collection_Count(bool async)
    {
        await base.Parameter_collection_Count(async);

        AssertSql(
"""
@__ids_0={ '2', '999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM unnest(@__ids_0) AS i(value)
    WHERE i.value > p."Id") = 1
""");
    }

    public override async Task Parameter_collection_of_ints_Contains(bool async)
    {
        await base.Parameter_collection_of_ints_Contains(async);

        AssertSql(
"""
@__ints_0={ '10', '999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" = ANY (@__ints_0)
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_int(async);

        AssertSql(
"""
@__nullableInts_0={ '10', '999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" = ANY (@__nullableInts_0)
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_nullable_int(async);

        AssertSql(
"""
@__nullableInts_0={ NULL, '999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableInt" = ANY (@__nullableInts_0) OR (p."NullableInt" IS NULL AND array_position(@__nullableInts_0, NULL) IS NOT NULL)
""");
    }

    public override async Task Parameter_collection_of_strings_Contains(bool async)
    {
        await base.Parameter_collection_of_strings_Contains(async);

        AssertSql(
"""
@__strings_0={ '10', '999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."String" = ANY (@__strings_0) OR (p."String" IS NULL AND array_position(@__strings_0, NULL) IS NOT NULL)
""");
    }

    public override async Task Parameter_collection_of_DateTimes_Contains(bool async)
    {
        await base.Parameter_collection_of_DateTimes_Contains(async);

        AssertSql(
"""
@__dateTimes_0={ '2020-01-10T12:30:00.0000000Z', '9999-01-01T00:00:00.0000000Z' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."DateTime" = ANY (@__dateTimes_0)
""");
    }

    public override async Task Parameter_collection_of_bools_Contains(bool async)
    {
        await base.Parameter_collection_of_bools_Contains(async);

        AssertSql(
"""
@__bools_0={ 'True' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Bool" = ANY (@__bools_0)
""");
    }

    public override async Task Parameter_collection_of_enums_Contains(bool async)
    {
        await base.Parameter_collection_of_enums_Contains(async);

        AssertSql(
"""
@__enums_0={ '0', '3' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Enum" = ANY (@__enums_0)
""");
    }

    public override async Task Parameter_collection_null_Contains(bool async)
    {
        await base.Parameter_collection_null_Contains(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" = ANY (NULL)
""");
    }

    public override async Task Column_collection_of_ints_Contains(bool async)
    {
        await base.Column_collection_of_ints_Contains(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints" @> ARRAY[10]::integer[]
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains(bool async)
    {
        await base.Column_collection_of_nullable_ints_Contains(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableInts" @> ARRAY[10]::integer[]
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains_null(bool async)
    {
        await base.Column_collection_of_nullable_ints_Contains_null(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE array_position(p."NullableInts", NULL) IS NOT NULL
""");
    }

    public override async Task Column_collection_of_strings_contains_null(bool async)
    {
        await base.Column_collection_of_strings_contains_null(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE array_position(p."Strings", NULL) IS NOT NULL
""");
    }

    public override async Task Column_collection_of_bools_Contains(bool async)
    {
        await base.Column_collection_of_bools_Contains(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Bools" @> ARRAY[TRUE]::boolean[]
""");
    }

    public override async Task Column_collection_Count_method(bool async)
    {
        await base.Column_collection_Count_method(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(p."Ints") = 2
""");
    }

    public override async Task Column_collection_Length(bool async)
    {
        await base.Column_collection_Length(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(p."Ints") = 2
""");
    }

        public override async Task Column_collection_index_int(bool async)
    {
        await base.Column_collection_index_int(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints"[2] = 10
""");
    }

    public override async Task Column_collection_index_string(bool async)
    {
        await base.Column_collection_index_string(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Strings"[2] = '10'
""");
    }

    public override async Task Column_collection_index_datetime(bool async)
    {
        await base.Column_collection_index_datetime(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."DateTimes"[2] = TIMESTAMPTZ '2020-01-10 12:30:00Z'
""");
    }

    public override async Task Column_collection_index_beyond_end(bool async)
    {
        await base.Column_collection_index_beyond_end(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints"[1000] = 10
""");
    }

    public override async Task Inline_collection_index_Column(bool async)
    {
        await base.Inline_collection_index_Column(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT v."Value"
    FROM (VALUES (0, 1::int), (1, 2), (2, 3)) AS v(_ord, "Value")
    ORDER BY v._ord NULLS FIRST
    LIMIT 1 OFFSET p."Int") = 1
""");
    }

    public override async Task Parameter_collection_index_Column_equal_Column(bool async)
    {
        await base.Parameter_collection_index_Column_equal_Column(async);

        AssertSql(
"""
@__ints_0={ '0', '2', '3' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE @__ints_0[p."Int" + 1] = p."Int"
""");
    }

    public override async Task Parameter_collection_index_Column_equal_constant(bool async)
    {
        await base.Parameter_collection_index_Column_equal_constant(async);

        AssertSql(
"""
@__ints_0={ '1', '2', '3' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE @__ints_0[p."Int" + 1] = 1
""");
    }

    public override async Task Column_collection_ElementAt(bool async)
    {
        await base.Column_collection_ElementAt(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints"[2] = 10
""");
    }

    public override async Task Column_collection_Skip(bool async)
    {
        await base.Column_collection_Skip(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(p."Ints"[2:]) = 2
""");
    }

    public override async Task Column_collection_Take(bool async)
    {
        await base.Column_collection_Take(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE 11 = ANY (p."Ints"[:2])
""");
    }

    public override async Task Column_collection_Skip_Take(bool async)
    {
        await base.Column_collection_Skip_Take(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE 11 = ANY (p."Ints"[2:3])
""");
    }

    public override async Task Column_collection_OrderByDescending_ElementAt(bool async)
    {
        await base.Column_collection_OrderByDescending_ElementAt(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT i.value
    FROM unnest(p."Ints") AS i(value)
    ORDER BY i.value DESC NULLS LAST
    LIMIT 1 OFFSET 0) = 111
""");
    }

    public override async Task Column_collection_Any(bool async)
    {
        await base.Column_collection_Any(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(p."Ints") > 0
""");
    }

    public override async Task Column_collection_projection_from_top_level(bool async)
    {
        await base.Column_collection_projection_from_top_level(async);

        AssertSql(
"""
SELECT p."Ints"
FROM "PrimitiveCollectionsEntity" AS p
ORDER BY p."Id" NULLS FIRST
""");
    }

    public override async Task Column_collection_Join_parameter_collection(bool async)
    {
        await base.Column_collection_Join_parameter_collection(async);

        AssertSql(
"""
@__ints_0={ '11', '111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM unnest(p."Ints") AS i(value)
    INNER JOIN unnest(@__ints_0) AS i0(value) ON i.value = i0.value) = 2
""");
    }

    public override async Task Inline_collection_Join_ordered_column_collection(bool async)
    {
        await base.Inline_collection_Join_ordered_column_collection(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (11::int), (111)) AS v("Value")
    INNER JOIN unnest(p."Ints") AS i(value) ON v."Value" = i.value) = 2
""");
    }

    public override async Task Parameter_collection_Concat_column_collection(bool async)
    {
        await base.Parameter_collection_Concat_column_collection(async);

        AssertSql(
"""
@__ints_0={ '11', '111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(@__ints_0 || p."Ints") = 2
""");
    }

    public override async Task Column_collection_Union_parameter_collection(bool async)
    {
        await base.Column_collection_Union_parameter_collection(async);

        AssertSql(
"""
@__ints_0={ '11', '111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT i.value
        FROM unnest(p."Ints") AS i(value)
        UNION
        SELECT i0.value
        FROM unnest(@__ints_0) AS i0(value)
    ) AS t) = 2
""");
    }

    public override async Task Column_collection_Intersect_inline_collection(bool async)
    {
        await base.Column_collection_Intersect_inline_collection(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT i.value
        FROM unnest(p."Ints") AS i(value)
        INTERSECT
        VALUES (11::int), (111)
    ) AS t) = 2
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Column_collection_Intersect_Parameter_collection_Any(bool async)
    {
        var ints = new[] { 11, 12 };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Intersect(ints).Any()),
            entryCount: 1);

        AssertSql(
"""
@__ints_0={ '11', '12' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints" && @__ints_0
""");
    }

    public override async Task Inline_collection_Except_column_collection(bool async)
    {
        await base.Inline_collection_Except_column_collection(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT v."Value"
        FROM (VALUES (11::int), (111)) AS v("Value")
        EXCEPT
        SELECT i.value AS "Value"
        FROM unnest(p."Ints") AS i(value)
    ) AS t
    WHERE t."Value" % 2 = 1) = 2
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_Concat_Column_collection_Concat_parameter(bool async)
    {
        var ints1 = new[] { 11 };
        var ints2 = new[] { 12 };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints1.Concat(c.Ints).Concat(ints2).Count() == 4),
            entryCount: 1);

        AssertSql(
"""
@__ints1_0={ '11' } (DbType = Object)
@__ints2_1={ '12' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(@__ints1_0 || p."Ints" || @__ints2_1) = 4
""");
    }

    public override async Task Column_collection_Concat_parameter_collection_equality_inline_collection_not_supported(bool async)
    {
        await base.Column_collection_Concat_parameter_collection_equality_inline_collection_not_supported(async);

        AssertSql();
    }

    public override async Task Column_collection_equality_parameter_collection(bool async)
    {
        await base.Column_collection_equality_parameter_collection(async);

        AssertSql(
"""
@__ints_0={ '1', '10' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints" = @__ints_0
""");
    }

    public override async Task Column_collection_equality_inline_collection(bool async)
    {
        await base.Column_collection_equality_inline_collection(async);

        AssertSql(
"""
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints" = ARRAY[1,10]::integer[]
""");
    }

    public override async Task Column_collection_equality_inline_collection_with_parameters(bool async)
    {
        var (i, j) = (1, 10);

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints == new[] { i, j }),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.SequenceEqual(new[] { i, j })),
            entryCount: 1);

        AssertSql(
"""
@__i_0='1'
@__j_1='10'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints" = ARRAY[@__i_0,@__j_1]::integer[]
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(async);

        AssertSql(
            """
@__ints={ '10', '111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT i.value
        FROM unnest(@__ints[2:]) AS i(value)
        UNION
        SELECT i0.value
        FROM unnest(p."Ints") AS i0(value)
    ) AS t) = 3
""");
    }

    public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        base.Parameter_collection_in_subquery_and_Convert_as_compiled_query();

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Count_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Count_as_compiled_query(async);

        AssertSql(
"""
@__ints={ '10', '111' } (DbType = Object)

SELECT count(*)::int
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM unnest(@__ints[2:]) AS i(value)
    WHERE i.value > p."Id") = 1
""");
    }

    public override async Task Column_collection_in_subquery_Union_parameter_collection(bool async)
    {
        await base.Column_collection_in_subquery_Union_parameter_collection(async);

        AssertSql(
"""
@__ints_0={ '10', '111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."String", p."Strings"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT i.value
        FROM unnest(p."Ints"[2:]) AS i(value)
        UNION
        SELECT i0.value
        FROM unnest(@__ints_0) AS i0(value)
    ) AS t) = 3
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private PrimitiveCollectionsContext CreateContext()
        => Fixture.CreateContext();

    public class PrimitiveCollectionsQueryNpgsqlFixture : PrimitiveCollectionsQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
