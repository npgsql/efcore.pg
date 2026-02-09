namespace Microsoft.EntityFrameworkCore.Query;

public class PrimitiveCollectionsQueryNpgsqlTest : PrimitiveCollectionsQueryRelationalTestBase<
    PrimitiveCollectionsQueryNpgsqlTest.PrimitiveCollectionsQueryNpgsqlFixture>
{
    public PrimitiveCollectionsQueryNpgsqlTest(PrimitiveCollectionsQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Inline_collection_of_ints_Contains()
    {
        await base.Inline_collection_of_ints_Contains();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains()
    {
        await base.Inline_collection_of_nullable_ints_Contains();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableInt" IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains_null()
    {
        await base.Inline_collection_of_nullable_ints_Contains_null();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableInt" IS NULL OR p."NullableInt" = 999
""");
    }

    public override async Task Inline_collection_Count_with_zero_values()
    {
        await base.Inline_collection_Count_with_zero_values();

        AssertSql();
    }

    public override async Task Inline_collection_Count_with_one_value()
    {
        await base.Inline_collection_Count_with_one_value();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (2::int)) AS v("Value")
    WHERE v."Value" > p."Id") = 1
""");
    }

    public override async Task Inline_collection_Count_with_two_values()
    {
        await base.Inline_collection_Count_with_two_values();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (2::int), (999)) AS v("Value")
    WHERE v."Value" > p."Id") = 1
""");
    }

    public override async Task Inline_collection_Count_with_three_values()
    {
        await base.Inline_collection_Count_with_three_values();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (2::int), (999), (1000)) AS v("Value")
    WHERE v."Value" > p."Id") = 2
""");
    }

    public override async Task Inline_collection_Contains_with_zero_values()
    {
        await base.Inline_collection_Contains_with_zero_values();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE FALSE
""");
    }

    public override async Task Inline_collection_Contains_with_one_value()
    {
        await base.Inline_collection_Contains_with_one_value();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" = 2
""");
    }

    public override async Task Inline_collection_Contains_with_two_values()
    {
        await base.Inline_collection_Contains_with_two_values();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (2, 999)
""");
    }

    public override async Task Inline_collection_Contains_with_three_values()
    {
        await base.Inline_collection_Contains_with_three_values();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (2, 999, 1000)
""");
    }

    public override async Task Inline_collection_Contains_with_all_parameters()
    {
        await base.Inline_collection_Contains_with_all_parameters();

        AssertSql(
            """
@i='2'
@j='999'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (@i, @j)
""");
    }

    public override async Task Inline_collection_Contains_with_constant_and_parameter()
    {
        await base.Inline_collection_Contains_with_constant_and_parameter();

        AssertSql(
            """
@j='999'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (2, @j)
""");
    }

    public override async Task Inline_collection_Contains_with_mixed_value_types()
    {
        await base.Inline_collection_Contains_with_mixed_value_types();

        AssertSql(
            """
@i='11'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" IN (999, @i, p."Id", p."Id" + p."Int")
""");
    }

    public override async Task Inline_collection_List_Contains_with_mixed_value_types()
    {
        await base.Inline_collection_List_Contains_with_mixed_value_types();

        AssertSql(
            """
@i='11'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" IN (999, @i, p."Id", p."Id" + p."Int")
""");
    }

    public override async Task Inline_collection_Contains_as_Any_with_predicate()
    {
        await base.Inline_collection_Contains_as_Any_with_predicate();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (2, 999)
""");
    }

    public override async Task Inline_collection_negated_Contains_as_All()
    {
        await base.Inline_collection_negated_Contains_as_All();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" NOT IN (2, 999)
""");
    }

    public override async Task Inline_collection_Min_with_two_values()
    {
        await base.Inline_collection_Min_with_two_values();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE LEAST(30, p."Int") = 30
""");
    }

    public override async Task Inline_collection_List_Min_with_two_values()
    {
        await base.Inline_collection_List_Min_with_two_values();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE LEAST(30, p."Int") = 30
""");
    }

    public override async Task Inline_collection_Max_with_two_values()
    {
        await base.Inline_collection_Max_with_two_values();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE GREATEST(30, p."Int") = 30
""");
    }

    public override async Task Inline_collection_List_Max_with_two_values()
    {
        await base.Inline_collection_List_Max_with_two_values();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE GREATEST(30, p."Int") = 30
""");
    }

    public override async Task Inline_collection_Min_with_three_values()
    {
        await base.Inline_collection_Min_with_three_values();

        AssertSql(
            """
@i='25'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE LEAST(30, p."Int", @i) = 25
""");
    }

    public override async Task Inline_collection_List_Min_with_three_values()
    {
        await base.Inline_collection_List_Min_with_three_values();

        AssertSql(
            """
@i='25'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE LEAST(30, p."Int", @i) = 25
""");
    }

    public override async Task Inline_collection_Max_with_three_values()
    {
        await base.Inline_collection_Max_with_three_values();

        AssertSql(
            """
@i='35'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE GREATEST(30, p."Int", @i) = 35
""");
    }

    public override async Task Inline_collection_List_Max_with_three_values()
    {
        await base.Inline_collection_List_Max_with_three_values();

        AssertSql(
            """
@i='35'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE GREATEST(30, p."Int", @i) = 35
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_Min()
    {
        await base.Inline_collection_of_nullable_value_type_Min();

        AssertSql(
            """
@i='25' (Nullable = true)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE LEAST(30, p."Int", @i) = 25
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_Max()
    {
        await base.Inline_collection_of_nullable_value_type_Max();

        AssertSql(
            """
@i='35' (Nullable = true)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE GREATEST(30, p."Int", @i) = 35
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_with_null_Min()
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Min();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE LEAST(30, p."NullableInt", NULL) = 30
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_with_null_Max()
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Max();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE GREATEST(30, p."NullableInt", NULL) = 30
""");
    }

        public override async Task Inline_collection_with_single_parameter_element_Contains()
    {
        await base.Inline_collection_with_single_parameter_element_Contains();

        AssertSql(
            """
@i='2'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" = @i
""");
    }

    public override async Task Inline_collection_with_single_parameter_element_Count()
    {
        await base.Inline_collection_with_single_parameter_element_Count();

        AssertSql(
            """
@i='2'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (@i::int)) AS v("Value")
    WHERE v."Value" > p."Id") = 1
""");
    }

    public override async Task Inline_collection_Contains_with_EF_Parameter()
    {
        await base.Inline_collection_Contains_with_EF_Parameter();

        AssertSql(
            """
@p={ '2'
'999'
'1000' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" = ANY (@p)
""");
    }

    public override async Task Inline_collection_Count_with_column_predicate_with_EF_Parameter()
    {
        await base.Inline_collection_Count_with_column_predicate_with_EF_Parameter();

        AssertSql(
            """
@p={ '2'
'999'
'1000' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM unnest(@p) AS p0(value)
    WHERE p0.value > p."Id") = 2
""");
    }

    public override async Task Parameter_collection_Count()
    {
        await base.Parameter_collection_Count();

        AssertSql(
            """
@ids={ '2'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM unnest(@ids) AS i(value)
    WHERE i.value > p."Id") = 1
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_int()
    {
        await base.Parameter_collection_of_ints_Contains_int();

        AssertSql(
            """
@ints={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" = ANY (@ints)
""",
            //
            """
@ints={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."Int" = ANY (@ints) AND p."Int" = ANY (@ints) IS NOT NULL)
""");
    }

    public override async Task Parameter_collection_HashSet_of_ints_Contains_int()
    {
        await base.Parameter_collection_HashSet_of_ints_Contains_int();

        AssertSql(
            """
@ints={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" = ANY (@ints)
""",
            //
            """
@ints={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."Int" = ANY (@ints) AND p."Int" = ANY (@ints) IS NOT NULL)
""");
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_HashSet_with_value_converter_Contains()
    {
        HashSet<MyEnum> enums = [MyEnum.Value1, MyEnum.Value4];

        await AssertQuery(ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => enums.Contains(c.Enum)));

        AssertSql(
            """
@enums={ '0'
'3' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Enum" = ANY (@enums)
""");
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_Dictionary_Values_with_value_converter_Contains()
    {
        Dictionary<int, MyEnum> enums = new()
        {
            [0] = MyEnum.Value1,
            [1] = MyEnum.Value4
        };

        // Dictionary<>.ValuesCollection doesn't have a public parameterless constructor, so NpgsqlArrayConverter can't convert to it
        // (see #3050). We still allow NpgsqlArrayConverter to be built to allow one-directional conversion: in the query below,
        // we only need to write enum.Values as a parameter (never read it).
        await AssertQuery(ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => enums.Values.Contains(c.Enum)));

        AssertSql(
            """
@enums_Values={ '0'
'3' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Enum" = ANY (@enums_Values)
""");
    }

    public override async Task Parameter_collection_ImmutableArray_of_ints_Contains_int()
    {
        await base.Parameter_collection_ImmutableArray_of_ints_Contains_int();

        AssertSql(
            """
@ints={ '10'
'999' } (Nullable = false) (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" = ANY (@ints)
""",
            //
            """
@ints={ '10'
'999' } (Nullable = false) (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."Int" = ANY (@ints) AND p."Int" = ANY (@ints) IS NOT NULL)
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_nullable_int()
    {
        await base.Parameter_collection_of_ints_Contains_nullable_int();

        AssertSql(
            """
@ints={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableInt" = ANY (@ints) OR (p."NullableInt" IS NULL AND array_position(@ints, NULL) IS NOT NULL)
""",
            //
            """
@ints={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."NullableInt" = ANY (@ints) AND p."NullableInt" = ANY (@ints) IS NOT NULL) AND (p."NullableInt" IS NOT NULL OR array_position(@ints, NULL) IS NULL)
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_int()
    {
        await base.Parameter_collection_of_nullable_ints_Contains_int();

        AssertSql(
            """
@nullableInts={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" = ANY (@nullableInts)
""",
            //
            """
@nullableInts={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."Int" = ANY (@nullableInts) AND p."Int" = ANY (@nullableInts) IS NOT NULL)
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int()
    {
        await base.Parameter_collection_of_nullable_ints_Contains_nullable_int();

        AssertSql(
            """
@nullableInts={ NULL
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableInt" = ANY (@nullableInts) OR (p."NullableInt" IS NULL AND array_position(@nullableInts, NULL) IS NOT NULL)
""",
            //
            """
@nullableInts={ NULL
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."NullableInt" = ANY (@nullableInts) AND p."NullableInt" = ANY (@nullableInts) IS NOT NULL) AND (p."NullableInt" IS NOT NULL OR array_position(@nullableInts, NULL) IS NULL)
""");
    }

    public override async Task Parameter_collection_of_structs_Contains_struct()
    {
        await base.Parameter_collection_of_structs_Contains_struct();

        AssertSql(
            """
@values={ '22'
'33' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."WrappedId" = ANY (@values)
""",
            //
            """
@values={ '11'
'44' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."WrappedId" = ANY (@values) AND p."WrappedId" = ANY (@values) IS NOT NULL)
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_string()
    {
        await base.Parameter_collection_of_strings_Contains_string();

        AssertSql(
            """
@strings={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."String" = ANY (@strings)
""",
            //
            """
@strings={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."String" = ANY (@strings) AND p."String" = ANY (@strings) IS NOT NULL)
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_nullable_string()
    {
        await base.Parameter_collection_of_strings_Contains_nullable_string();

        AssertSql(
            """
@strings={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableString" = ANY (@strings) OR (p."NullableString" IS NULL AND array_position(@strings, NULL) IS NOT NULL)
""",
            //
            """
@strings={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."NullableString" = ANY (@strings) AND p."NullableString" = ANY (@strings) IS NOT NULL) AND (p."NullableString" IS NOT NULL OR array_position(@strings, NULL) IS NULL)
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_string()
    {
        await base.Parameter_collection_of_nullable_strings_Contains_string();

        AssertSql(
            """
@strings={ '10'
NULL } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."String" = ANY (@strings)
""",
            //
            """
@strings={ '10'
NULL } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."String" = ANY (@strings) AND p."String" = ANY (@strings) IS NOT NULL)
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_nullable_string()
    {
        await base.Parameter_collection_of_nullable_strings_Contains_nullable_string();

        AssertSql(
            """
@strings={ '999'
NULL } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableString" = ANY (@strings) OR (p."NullableString" IS NULL AND array_position(@strings, NULL) IS NOT NULL)
""",
            //
            """
@strings={ '999'
NULL } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."NullableString" = ANY (@strings) AND p."NullableString" = ANY (@strings) IS NOT NULL) AND (p."NullableString" IS NOT NULL OR array_position(@strings, NULL) IS NULL)
""");
    }

    public override async Task Parameter_collection_of_DateTimes_Contains()
    {
        await base.Parameter_collection_of_DateTimes_Contains();

        AssertSql(
            """
@dateTimes={ '2020-01-10T12:30:00.0000000Z'
'9999-01-01T00:00:00.0000000Z' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."DateTime" = ANY (@dateTimes)
""");
    }

    public override async Task Parameter_collection_of_bools_Contains()
    {
        await base.Parameter_collection_of_bools_Contains();

        AssertSql(
            """
@bools={ 'True' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Bool" = ANY (@bools)
""");
    }

    public override async Task Parameter_collection_of_enums_Contains()
    {
        await base.Parameter_collection_of_enums_Contains();

        AssertSql(
            """
@enums={ '0'
'3' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Enum" = ANY (@enums)
""");
    }

    public override async Task Parameter_collection_null_Contains()
    {
        await base.Parameter_collection_null_Contains();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" = ANY (NULL)
""");
    }

    public override async Task Parameter_collection_Contains_with_EF_Constant()
    {
        await base.Parameter_collection_Contains_with_EF_Constant();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" IN (2, 999, 1000)
""");
    }

    public override async Task Parameter_collection_Where_with_EF_Constant_Where_Any()
    {
        await base.Parameter_collection_Where_with_EF_Constant_Where_Any();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE EXISTS (
    SELECT 1
    FROM (VALUES (2::int), (999), (1000)) AS i("Value")
    WHERE i."Value" > 0)
""");
    }

    public override async Task Parameter_collection_Count_with_column_predicate_with_EF_Constant()
    {
        await base.Parameter_collection_Count_with_column_predicate_with_EF_Constant();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (2::int), (999), (1000)) AS i("Value")
    WHERE i."Value" > p."Id") = 2
""");
    }

    // The following test does nothing since we don't override NumberOfValuesForHugeParameterCollectionTests;
    // the PG parameter limit is huge (ushort), so we don't need to test it.
    public override async Task Parameter_collection_Count_with_huge_number_of_values()
    {
        await base.Parameter_collection_Count_with_huge_number_of_values();

        AssertSql();
    }

    // The following test does nothing since we don't override NumberOfValuesForHugeParameterCollectionTests;
    // the PG parameter limit is huge (ushort), so we don't need to test it.
    public override async Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values()
    {
        await base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values();

        AssertSql();
    }

    [ConditionalFact] // #3012
    [MinimumPostgresVersion(14, 0)] // Multiranges were introduced in PostgreSQL 14
    public virtual async Task Parameter_collection_of_ranges_Contains()
    {
        var ranges = new NpgsqlRange<int>[]
        {
            new(5, 15),
            new(40, 50)
        };

        await AssertQuery(
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => ranges.Contains(e.Int)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ranges.Any(p => p.LowerBound <= c.Int && p.UpperBound >= c.Int)));

        AssertSql(
            """
@ranges={ '[5,15]'
'[40,50]' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE @ranges @> p."Int"
""");
    }

    public override async Task Column_collection_of_ints_Contains()
    {
        await base.Column_collection_of_ints_Contains();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE 10 = ANY (p."Ints")
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains()
    {
        await base.Column_collection_of_nullable_ints_Contains();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE 10 = ANY (p."NullableInts")
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains_null()
    {
        await base.Column_collection_of_nullable_ints_Contains_null();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE array_position(p."NullableInts", NULL) IS NOT NULL
""");
    }

    public override async Task Column_collection_of_strings_contains_null()
    {
        await base.Column_collection_of_strings_contains_null();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE array_position(p."Strings", NULL) IS NOT NULL
""");
    }

    public override async Task Column_collection_of_nullable_strings_contains_null()
    {
        await base.Column_collection_of_nullable_strings_contains_null();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE array_position(p."NullableStrings", NULL) IS NOT NULL
""");
    }

    public override async Task Column_collection_of_bools_Contains()
    {
        await base.Column_collection_of_bools_Contains();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE TRUE = ANY (p."Bools")
""");
    }

    public override async Task Column_collection_Count_method()
    {
        await base.Column_collection_Count_method();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(p."Ints") = 2
""");
    }

    public override async Task Column_collection_Length()
    {
        await base.Column_collection_Length();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(p."Ints") = 2
""");
    }

    public override async Task Column_collection_Count_with_predicate()
    {
        await base.Column_collection_Count_with_predicate();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM unnest(p."Ints") AS i(value)
    WHERE i.value > 1) = 2
""");
    }

    public override async Task Column_collection_Where_Count()
    {
        await base.Column_collection_Where_Count();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM unnest(p."Ints") AS i(value)
    WHERE i.value > 1) = 2
""");
    }

    public override async Task Column_collection_index_int()
    {
        await base.Column_collection_index_int();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints"[2] = 10
""");
    }

    public override async Task Column_collection_index_string()
    {
        await base.Column_collection_index_string();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Strings"[2] = '10'
""");
    }

    public override async Task Column_collection_index_datetime()
    {
        await base.Column_collection_index_datetime();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."DateTimes"[2] = TIMESTAMPTZ '2020-01-10T12:30:00Z'
""");
    }

    public override async Task Column_collection_index_beyond_end()
    {
        await base.Column_collection_index_beyond_end();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints"[1000] = 10
""");
    }

    public override async Task Nullable_reference_column_collection_index_equals_nullable_column()
    {
        await base.Nullable_reference_column_collection_index_equals_nullable_column();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableStrings"[3] = p."NullableString" OR (p."NullableStrings"[3] IS NULL AND p."NullableString" IS NULL)
""");
    }

    public override async Task Non_nullable_reference_column_collection_index_equals_nullable_column()
    {
        await base.Non_nullable_reference_column_collection_index_equals_nullable_column();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(p."Strings") > 0 AND p."Strings"[2] = p."NullableString"
""");
    }

    public override async Task Inline_collection_index_Column()
    {
        await base.Inline_collection_index_Column();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT v."Value"
    FROM (VALUES (0, 1::int), (1, 2), (2, 3)) AS v(_ord, "Value")
    ORDER BY v._ord NULLS FIRST
    LIMIT 1 OFFSET p."Int") = 1
""");
    }

    public override async Task Inline_collection_index_Column_with_EF_Constant()
    {
        await base.Inline_collection_index_Column_with_EF_Constant();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT i."Value"
    FROM (VALUES (0, 1::int), (1, 2), (2, 3)) AS i(_ord, "Value")
    ORDER BY i._ord NULLS FIRST
    LIMIT 1 OFFSET p."Int") = 1
""");
    }

    public override async Task Inline_collection_value_index_Column()
    {
        await base.Inline_collection_value_index_Column();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT v."Value"
    FROM (VALUES (0, 1::int), (1, p."Int"), (2, 3)) AS v(_ord, "Value")
    ORDER BY v._ord NULLS FIRST
    LIMIT 1 OFFSET p."Int") = 1
""");
    }

    public override async Task Inline_collection_List_value_index_Column()
    {
        await base.Inline_collection_List_value_index_Column();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT v."Value"
    FROM (VALUES (0, 1::int), (1, p."Int"), (2, 3)) AS v(_ord, "Value")
    ORDER BY v._ord NULLS FIRST
    LIMIT 1 OFFSET p."Int") = 1
""");
    }

    public override async Task Parameter_collection_index_Column_equal_Column()
    {
        await base.Parameter_collection_index_Column_equal_Column();

        AssertSql(
            """
@ints={ '0'
'2'
'3' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE @ints[p."Int" + 1] = p."Int"
""");
    }

    public override async Task Parameter_collection_index_Column_equal_constant()
    {
        await base.Parameter_collection_index_Column_equal_constant();

        AssertSql(
            """
@ints={ '1'
'2'
'3' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE @ints[p."Int" + 1] = 1
""");
    }

    public override async Task Column_collection_ElementAt()
    {
        await base.Column_collection_ElementAt();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints"[2] = 10
""");
    }

        public override async Task Column_collection_First()
    {
        await base.Column_collection_First();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT i.value
    FROM unnest(p."Ints") AS i(value)
    LIMIT 1) = 1
""");
    }

    public override async Task Column_collection_FirstOrDefault()
    {
        await base.Column_collection_FirstOrDefault();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE COALESCE((
    SELECT i.value
    FROM unnest(p."Ints") AS i(value)
    LIMIT 1), 0) = 1
""");
    }

    public override async Task Column_collection_Single()
    {
        await base.Column_collection_Single();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT i.value
    FROM unnest(p."Ints") AS i(value)
    LIMIT 1) = 1
""");
    }

    public override async Task Column_collection_SingleOrDefault()
    {
        await base.Column_collection_SingleOrDefault();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE COALESCE((
    SELECT i.value
    FROM unnest(p."Ints") AS i(value)
    LIMIT 1), 0) = 1
""");
    }

    public override async Task Column_collection_Skip()
    {
        await base.Column_collection_Skip();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(p."Ints"[2:]) = 2
""");
    }

    public override async Task Column_collection_Take()
    {
        await base.Column_collection_Take();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE 11 = ANY (p."Ints"[:2])
""");
    }

    public override async Task Column_collection_Skip_Take()
    {
        await base.Column_collection_Skip_Take();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE 11 = ANY (p."Ints"[2:3])
""");
    }

        public override async Task Column_collection_Where_Skip()
    {
        await base.Column_collection_Where_Skip();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT 1
        FROM unnest(p."Ints") AS i(value)
        WHERE i.value > 1
        OFFSET 1
    ) AS i0) = 3
""");
    }

    public override async Task Column_collection_Where_Take()
    {
        await base.Column_collection_Where_Take();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT 1
        FROM unnest(p."Ints") AS i(value)
        WHERE i.value > 1
        LIMIT 2
    ) AS i0) = 2
""");
    }

    public override async Task Column_collection_Where_Skip_Take()
    {
        await base.Column_collection_Where_Skip_Take();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT 1
        FROM unnest(p."Ints") AS i(value)
        WHERE i.value > 1
        LIMIT 2 OFFSET 1
    ) AS i0) = 1
""");
    }

    public override async Task Column_collection_Contains_over_subquery()
    {
        await base.Column_collection_Contains_over_subquery();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE 11 IN (
    SELECT i.value
    FROM unnest(p."Ints") AS i(value)
    WHERE i.value > 1
)
""");
    }

    public override async Task Column_collection_OrderByDescending_ElementAt()
    {
        await base.Column_collection_OrderByDescending_ElementAt();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT i.value
    FROM unnest(p."Ints") AS i(value)
    ORDER BY i.value DESC NULLS LAST
    LIMIT 1 OFFSET 0) = 111
""");
    }

    public override async Task Column_collection_Where_ElementAt()
    {
        await base.Column_collection_Where_ElementAt();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT i.value
    FROM unnest(p."Ints") AS i(value)
    WHERE i.value > 1
    LIMIT 1 OFFSET 0) = 11
""");
    }

    public override async Task Column_collection_Any()
    {
        await base.Column_collection_Any();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(p."Ints") > 0
""");
    }

    public override async Task Column_collection_Distinct()
    {
        await base.Column_collection_Distinct();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT DISTINCT i.value
        FROM unnest(p."Ints") AS i(value)
    ) AS i0) = 3
""");
    }

    public override async Task Column_collection_SelectMany()
    {
        await base.Column_collection_SelectMany();

        AssertSql(
            """
SELECT i.value
FROM "PrimitiveCollectionsEntity" AS p
JOIN LATERAL unnest(p."Ints") AS i(value) ON TRUE
""");
    }

    public override async Task Column_collection_SelectMany_with_filter()
    {
        await base.Column_collection_SelectMany_with_filter();

        AssertSql(
            """
SELECT i0.value
FROM "PrimitiveCollectionsEntity" AS p
JOIN LATERAL (
    SELECT i.value
    FROM unnest(p."Ints") AS i(value)
    WHERE i.value > 1
) AS i0 ON TRUE
""");
    }

    public override async Task Column_collection_SelectMany_with_Select_to_anonymous_type()
    {
        await base.Column_collection_SelectMany_with_Select_to_anonymous_type();

        AssertSql(
            """
SELECT i.value AS "Original", i.value + 1 AS "Incremented"
FROM "PrimitiveCollectionsEntity" AS p
JOIN LATERAL unnest(p."Ints") AS i(value) ON TRUE
""");
    }

    public override async Task Column_collection_projection_from_top_level()
    {
        await base.Column_collection_projection_from_top_level();

        AssertSql(
            """
SELECT p."Ints"
FROM "PrimitiveCollectionsEntity" AS p
ORDER BY p."Id" NULLS FIRST
""");
    }

    public override async Task Column_collection_Join_parameter_collection()
    {
        await base.Column_collection_Join_parameter_collection();

        AssertSql(
            """
@ints={ '11'
'111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM unnest(p."Ints") AS i(value)
    INNER JOIN unnest(@ints) AS i0(value) ON i.value = i0.value) = 2
""");
    }

    public override async Task Inline_collection_Join_ordered_column_collection()
    {
        await base.Inline_collection_Join_ordered_column_collection();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (11::int), (111)) AS v("Value")
    INNER JOIN unnest(p."Ints") AS i(value) ON v."Value" = i.value) = 2
""");
    }

    public override async Task Parameter_collection_Concat_column_collection()
    {
        await base.Parameter_collection_Concat_column_collection();

        AssertSql(
            """
@p={ '11'
'111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(@p || p."Ints") = 2
""");
    }

    public override async Task Parameter_collection_with_type_inference_for_JsonScalarExpression()
    {
        await base.Parameter_collection_with_type_inference_for_JsonScalarExpression();

        AssertSql(
            """
@values={ 'one'
'two' } (DbType = Object)

SELECT CASE
    WHEN p."Id" <> 0 THEN @values[p."Int" % 2 + 1]
    ELSE 'foo'
END
FROM "PrimitiveCollectionsEntity" AS p
""");
    }

    public override async Task Column_collection_Union_parameter_collection()
    {
        await base.Column_collection_Union_parameter_collection();

        AssertSql(
            """
@ints={ '11'
'111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT i.value
        FROM unnest(p."Ints") AS i(value)
        UNION
        SELECT i0.value
        FROM unnest(@ints) AS i0(value)
    ) AS u) = 2
""");
    }

    public override async Task Column_collection_Intersect_inline_collection()
    {
        await base.Column_collection_Intersect_inline_collection();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT i.value
        FROM unnest(p."Ints") AS i(value)
        INTERSECT
        VALUES (11::int), (111)
    ) AS i0) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Column_collection_Intersect_Parameter_collection_Any()
    {
        var ints = new[] { 11, 12 };

        await AssertQuery(ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Intersect(ints).Any()));

        AssertSql(
            """
@ints={ '11'
'12' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints" && @ints
""");
    }

    public override async Task Inline_collection_Except_column_collection()
    {
        await base.Inline_collection_Except_column_collection();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT v."Value"
        FROM (VALUES (11::int), (111)) AS v("Value")
        EXCEPT
        SELECT i.value AS "Value"
        FROM unnest(p."Ints") AS i(value)
    ) AS e
    WHERE e."Value" % 2 = 1) = 2
""");
    }

    public override async Task Column_collection_Where_Union()
    {
        await base.Column_collection_Where_Union();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT i.value
        FROM unnest(p."Ints") AS i(value)
        WHERE i.value > 100
        UNION
        VALUES (50::int)
    ) AS u) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_Concat_Column_collection_Concat_parameter()
    {
        var ints1 = new[] { 11 };
        var ints2 = new[] { 12 };

        await AssertQuery(
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints1.Concat(c.Ints).Concat(ints2).Count() == 4));

        AssertSql(
            """
@ints1={ '11' } (DbType = Object)
@ints2={ '12' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(@ints1 || p."Ints" || @ints2) = 4
""");
    }

    public override async Task Column_collection_Concat_parameter_collection_equality_inline_collection()
    {
        await base.Column_collection_Concat_parameter_collection_equality_inline_collection();

        AssertSql();
    }

    public override async Task Column_collection_equality_parameter_collection()
    {
        await base.Column_collection_equality_parameter_collection();

        AssertSql(
            """
@ints={ '1'
'10' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints" = @ints
""");
    }

    public override async Task Column_collection_equality_inline_collection()
    {
        await base.Column_collection_equality_inline_collection();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints" = ARRAY[1,10]::integer[]
""");
    }

    public override async Task Column_collection_equality_inline_collection_with_parameters()
    {
        var (i, j) = (1, 10);

        await AssertQuery(
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints == new[] { i, j }),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.SequenceEqual(new[] { i, j })));

        AssertSql(
            """
@i='1'
@j='10'

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Ints" = ARRAY[@i,@j]::integer[]
""");
    }

    public override async Task Column_collection_Where_equality_inline_collection()
    {
        await base.Column_collection_Where_equality_inline_collection();

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query()
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query();

        AssertSql(
            """
@ints={ '10'
'111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT i.value
        FROM unnest(@ints[2:]) AS i(value)
        UNION
        SELECT i0.value
        FROM unnest(p."Ints") AS i0(value)
    ) AS u) = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection()
    {
        await base.Parameter_collection_in_subquery_Union_column_collection();

        AssertSql(
            """
@Skip={ '111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT s.value
        FROM unnest(@Skip) AS s(value)
        UNION
        SELECT i.value
        FROM unnest(p."Ints") AS i(value)
    ) AS u) = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_nested()
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_nested();

        AssertSql(
            """
@Skip={ '111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT s.value
        FROM unnest(@Skip) AS s(value)
        UNION
        SELECT i2.value
        FROM (
            SELECT i1.value
            FROM (
                SELECT DISTINCT i0.value
                FROM (
                    SELECT i.value
                    FROM unnest(p."Ints") AS i(value)
                    ORDER BY i.value NULLS FIRST
                    OFFSET 1
                ) AS i0
            ) AS i1
            ORDER BY i1.value DESC NULLS LAST
            LIMIT 20
        ) AS i2
    ) AS u) = 3
""");
    }

    public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        base.Parameter_collection_in_subquery_and_Convert_as_compiled_query();

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query()
    {
        await base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query();

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Count_as_compiled_query()
    {
        await base.Parameter_collection_in_subquery_Count_as_compiled_query();

        AssertSql(
            """
@ints={ '10'
'111' } (DbType = Object)

SELECT count(*)::int
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM unnest(@ints[2:]) AS i(value)
    WHERE i.value > p."Id") = 1
""");
    }

    public override async Task Column_collection_in_subquery_Union_parameter_collection()
    {
        await base.Column_collection_in_subquery_Union_parameter_collection();

        AssertSql(
            """
@ints={ '10'
'111' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (
        SELECT i.value
        FROM unnest(p."Ints"[2:]) AS i(value)
        UNION
        SELECT i0.value
        FROM unnest(@ints) AS i0(value)
    ) AS u) = 3
""");
    }

    public override async Task Project_collection_of_ints_simple()
    {
        await base.Project_collection_of_ints_simple();

        AssertSql(
            """
SELECT p."Ints"
FROM "PrimitiveCollectionsEntity" AS p
ORDER BY p."Id" NULLS FIRST
""");
    }

    public override async Task Project_collection_of_ints_ordered()
    {
        await base.Project_collection_of_ints_ordered();

        AssertSql(
            """
SELECT p."Id", i.value, i.ordinality
FROM "PrimitiveCollectionsEntity" AS p
LEFT JOIN LATERAL unnest(p."Ints") WITH ORDINALITY AS i(value) ON TRUE
ORDER BY p."Id" NULLS FIRST, i.value DESC NULLS LAST
""");
    }

    public override async Task Project_collection_of_datetimes_filtered()
    {
        await base.Project_collection_of_datetimes_filtered();

        AssertSql(
            """
SELECT p."Id", d0.value, d0.ordinality
FROM "PrimitiveCollectionsEntity" AS p
LEFT JOIN LATERAL (
    SELECT d.value, d.ordinality
    FROM unnest(p."DateTimes") WITH ORDINALITY AS d(value)
    WHERE date_part('day', d.value AT TIME ZONE 'UTC')::int <> 1
) AS d0 ON TRUE
ORDER BY p."Id" NULLS FIRST, d0.ordinality NULLS FIRST
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging()
    {
        await base.Project_collection_of_nullable_ints_with_paging();

        AssertSql(
            """
SELECT p."Id", n.value, n.ordinality
FROM "PrimitiveCollectionsEntity" AS p
LEFT JOIN LATERAL unnest(p."NullableInts"[:20]) WITH ORDINALITY AS n(value) ON TRUE
ORDER BY p."Id" NULLS FIRST
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging2()
    {
        await base.Project_collection_of_nullable_ints_with_paging2();

        AssertSql(
            """
SELECT p."Id", n0.value, n0.ordinality
FROM "PrimitiveCollectionsEntity" AS p
LEFT JOIN LATERAL (
    SELECT n.value, n.ordinality
    FROM unnest(p."NullableInts") WITH ORDINALITY AS n(value)
    ORDER BY n.value NULLS FIRST
    OFFSET 1
) AS n0 ON TRUE
ORDER BY p."Id" NULLS FIRST, n0.value NULLS FIRST
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging3()
    {
        await base.Project_collection_of_nullable_ints_with_paging3();

        AssertSql(
            """
SELECT p."Id", n.value, n.ordinality
FROM "PrimitiveCollectionsEntity" AS p
LEFT JOIN LATERAL unnest(p."NullableInts"[3:]) WITH ORDINALITY AS n(value) ON TRUE
ORDER BY p."Id" NULLS FIRST
""");
    }

    public override async Task Project_collection_of_ints_with_distinct()
    {
        await base.Project_collection_of_ints_with_distinct();

        AssertSql(
            """
SELECT p."Id", i0.value
FROM "PrimitiveCollectionsEntity" AS p
LEFT JOIN LATERAL (
    SELECT DISTINCT i.value
    FROM unnest(p."Ints") AS i(value)
) AS i0 ON TRUE
ORDER BY p."Id" NULLS FIRST
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_distinct()
    {
        await base.Project_collection_of_nullable_ints_with_distinct();

        AssertSql();
    }

    public override async Task Project_collection_of_ints_with_ToList_and_FirstOrDefault()
    {
        await base.Project_collection_of_ints_with_ToList_and_FirstOrDefault();

        AssertSql(
            """
SELECT p0."Id", i.value, i.ordinality
FROM (
    SELECT p."Id", p."Ints"
    FROM "PrimitiveCollectionsEntity" AS p
    ORDER BY p."Id" NULLS FIRST
    LIMIT 1
) AS p0
LEFT JOIN LATERAL unnest(p0."Ints") WITH ORDINALITY AS i(value) ON TRUE
ORDER BY p0."Id" NULLS FIRST, i.ordinality NULLS FIRST
""");
    }

    public override async Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls()
    {
        await base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls();

        AssertSql(
            """
SELECT p."Id", n1.value, n1.ordinality, n2.value, n2.ordinality
FROM "PrimitiveCollectionsEntity" AS p
LEFT JOIN LATERAL (
    SELECT n.value, n.ordinality
    FROM unnest(p."NullableInts") WITH ORDINALITY AS n(value)
    WHERE FALSE
) AS n1 ON TRUE
LEFT JOIN LATERAL (
    SELECT n0.value, n0.ordinality
    FROM unnest(p."NullableInts") WITH ORDINALITY AS n0(value)
    WHERE n0.value IS NULL
) AS n2 ON TRUE
ORDER BY p."Id" NULLS FIRST, n1.ordinality NULLS FIRST, n2.ordinality NULLS FIRST
""");
    }

    public override async Task Project_multiple_collections()
    {
        // Base implementation currently uses an Unspecified DateTime in the query, but we require a Utc one.
        await AssertQuery(
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(x => x.Id).Select(
                x => new
                {
                    Ints = x.Ints.ToList(),
                    OrderedInts = x.Ints.OrderByDescending(xx => xx).ToList(),
                    FilteredDateTimes = x.DateTimes.Where(xx => xx.Day != 1).ToList(),
                    FilteredDateTimes2 = x.DateTimes.Where(xx => xx > new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).ToList()
                }),
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.Ints, a.Ints, ordered: true);
                AssertCollection(e.OrderedInts, a.OrderedInts, ordered: true);
                AssertCollection(e.FilteredDateTimes, a.FilteredDateTimes, elementSorter: ee => ee);
                AssertCollection(e.FilteredDateTimes2, a.FilteredDateTimes2, elementSorter: ee => ee);
            },
            assertOrder: true);

        AssertSql(
            """
SELECT p."Id", i.value, i.ordinality, i0.value, i0.ordinality, d1.value, d1.ordinality, d2.value, d2.ordinality
FROM "PrimitiveCollectionsEntity" AS p
LEFT JOIN LATERAL unnest(p."Ints") WITH ORDINALITY AS i(value) ON TRUE
LEFT JOIN LATERAL unnest(p."Ints") WITH ORDINALITY AS i0(value) ON TRUE
LEFT JOIN LATERAL (
    SELECT d.value, d.ordinality
    FROM unnest(p."DateTimes") WITH ORDINALITY AS d(value)
    WHERE date_part('day', d.value AT TIME ZONE 'UTC')::int <> 1
) AS d1 ON TRUE
LEFT JOIN LATERAL (
    SELECT d0.value, d0.ordinality
    FROM unnest(p."DateTimes") WITH ORDINALITY AS d0(value)
    WHERE d0.value > TIMESTAMPTZ '2000-01-01T00:00:00Z'
) AS d2 ON TRUE
ORDER BY p."Id" NULLS FIRST, i.ordinality NULLS FIRST, i0.value DESC NULLS LAST, i0.ordinality NULLS FIRST, d1.ordinality NULLS FIRST, d2.ordinality NULLS FIRST
""");
    }

    public override async Task Project_primitive_collections_element()
    {
        await base.Project_primitive_collections_element();

        AssertSql(
            """
SELECT p."Ints"[1] AS "Indexer", p."DateTimes"[1] AS "EnumerableElementAt", p."Strings"[2] AS "QueryableElementAt"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Id" < 4
ORDER BY p."Id" NULLS FIRST
""");
    }

    public override async Task Project_inline_collection()
    {
        await base.Project_inline_collection();

        AssertSql(
            """
SELECT ARRAY[p."String",'foo']::text[]
FROM "PrimitiveCollectionsEntity" AS p
""");
    }

    public override async Task Project_inline_collection_with_Union()
    {
        await base.Project_inline_collection_with_Union();

        AssertSql(
            """
SELECT p."Id", u."Value"
FROM "PrimitiveCollectionsEntity" AS p
LEFT JOIN LATERAL (
    SELECT v."Value"
    FROM (VALUES (p."String")) AS v("Value")
    UNION
    SELECT p0."String" AS "Value"
    FROM "PrimitiveCollectionsEntity" AS p0
) AS u ON TRUE
ORDER BY p."Id" NULLS FIRST
""");
    }

    public override async Task Project_inline_collection_with_Concat()
    {
        await base.Project_inline_collection_with_Concat();

        AssertSql();
    }

    public override async Task Nested_contains_with_Lists_and_no_inferred_type_mapping()
    {
        await base.Nested_contains_with_Lists_and_no_inferred_type_mapping();

        AssertSql(
            """
@ints={ '1'
'2'
'3' } (DbType = Object)
@strings={ 'one'
'two'
'three' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE CASE
    WHEN p."Int" = ANY (@ints) THEN 'one'
    ELSE 'two'
END = ANY (@strings)
""");
    }

    public override async Task Nested_contains_with_arrays_and_no_inferred_type_mapping()
    {
        await base.Nested_contains_with_arrays_and_no_inferred_type_mapping();

        AssertSql(
            """
@ints={ '1'
'2'
'3' } (DbType = Object)
@strings={ 'one'
'two'
'three' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE CASE
    WHEN p."Int" = ANY (@ints) THEN 'one'
    ELSE 'two'
END = ANY (@strings)
""");
    }

    public override async Task Values_of_enum_casted_to_underlying_value()
    {
        await base.Values_of_enum_casted_to_underlying_value();

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE (
    SELECT count(*)::int
    FROM (VALUES (0::int), (1), (2), (3)) AS v("Value")
    WHERE v."Value" = p."Int") > 0
""");
    }

    [ConditionalFact]
    public virtual async Task Array_remove()
    {
        await AssertQuery(
            // ReSharper disable once ReplaceWithSingleCallToCount
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => e.Ints.Where(i => i != 1).Count() == 1));

        AssertSql(
            """
SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE cardinality(array_remove(p."Ints", 1)) = 1
""");
    }

        public override async Task Parameter_collection_of_structs_Contains_nullable_struct()
    {
        await base.Parameter_collection_of_structs_Contains_nullable_struct();

        AssertSql(
            """
@values={ '22'
'33' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableWrappedId" = ANY (@values) OR (p."NullableWrappedId" IS NULL AND array_position(@values, NULL) IS NOT NULL)
""",
            //
            """
@values={ '11'
'44' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."NullableWrappedId" = ANY (@values) AND p."NullableWrappedId" = ANY (@values) IS NOT NULL) AND (p."NullableWrappedId" IS NOT NULL OR array_position(@values, NULL) IS NULL)
""");
    }

    public override Task Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer()
        => Assert.ThrowsAnyAsync<TargetInvocationException>(
            () => base.Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer());

    public override async Task Parameter_collection_of_nullable_structs_Contains_struct()
    {
        await base.Parameter_collection_of_nullable_structs_Contains_struct();

        AssertSql(
            """
@values={ NULL
'22' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."WrappedId" = ANY (@values)
""",
            //
            """
@values={ '11'
'44' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."WrappedId" = ANY (@values) AND p."WrappedId" = ANY (@values) IS NOT NULL)
""");
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct()
    {
        await base.Parameter_collection_of_nullable_structs_Contains_nullable_struct();

        AssertSql(
            """
@values={ NULL
'22' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableWrappedId" = ANY (@values) OR (p."NullableWrappedId" IS NULL AND array_position(@values, NULL) IS NOT NULL)
""",
            //
            """
@values={ '11'
'44' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."NullableWrappedId" = ANY (@values) AND p."NullableWrappedId" = ANY (@values) IS NOT NULL) AND (p."NullableWrappedId" IS NOT NULL OR array_position(@values, NULL) IS NULL)
""");
    }

    public override Task Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer()
        => Assert.ThrowsAnyAsync<TargetInvocationException>(
            () => base.Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer());


    public override async Task Inline_collection_Contains_with_IEnumerable_EF_Parameter()
    {
        await base.Inline_collection_Contains_with_IEnumerable_EF_Parameter();

        AssertSql(
            """
@Select={ '10'
'a'
'aa' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."NullableString" = ANY (@Select) OR (p."NullableString" IS NULL AND array_position(@Select, NULL) IS NOT NULL)
""");
    }


    public override async Task Parameter_collection_FrozenSet_of_ints_Contains_int()
    {
        await base.Parameter_collection_FrozenSet_of_ints_Contains_int();

        AssertSql(
            """
@ints={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" = ANY (@ints)
""",
            //
            """
@ints={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."Int" = ANY (@ints) AND p."Int" = ANY (@ints) IS NOT NULL)
""");
    }


    public override async Task Parameter_collection_IReadOnlySet_of_ints_Contains_int()
    {
        await base.Parameter_collection_IReadOnlySet_of_ints_Contains_int();

        AssertSql(
            """
@ints={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE p."Int" = ANY (@ints)
""",
            //
            """
@ints={ '10'
'999' } (DbType = Object)

SELECT p."Id", p."Bool", p."Bools", p."DateTime", p."DateTimes", p."Enum", p."Enums", p."Int", p."Ints", p."NullableInt", p."NullableInts", p."NullableString", p."NullableStrings", p."NullableWrappedId", p."NullableWrappedIdWithNullableComparer", p."String", p."Strings", p."WrappedId"
FROM "PrimitiveCollectionsEntity" AS p
WHERE NOT (p."Int" = ANY (@ints) AND p."Int" = ANY (@ints) IS NOT NULL)
""");
    }

    public override Task Parameter_collection_ReadOnlyCollectionWithContains_of_ints_Contains_int()
        => base.Parameter_collection_ReadOnlyCollectionWithContains_of_ints_Contains_int();

    public override Task Parameter_collection_empty_Contains()
        => base.Parameter_collection_empty_Contains();

    public override Task Parameter_collection_empty_Join()
        => base.Parameter_collection_empty_Join();

    public override Task Parameter_collection_Count_with_huge_number_of_values_over_5_operations()
        => base.Parameter_collection_Count_with_huge_number_of_values_over_5_operations();

    public override Task Parameter_collection_Count_with_huge_number_of_values_over_5_operations_same_parameter()
        => base.Parameter_collection_Count_with_huge_number_of_values_over_5_operations_same_parameter();

    public override Task Parameter_collection_Count_with_huge_number_of_values_over_2_operations_same_parameter_different_type_mapping()
        => base.Parameter_collection_Count_with_huge_number_of_values_over_2_operations_same_parameter_different_type_mapping();

    public override Task Parameter_collection_Count_with_huge_number_of_values_over_5_operations_forced_constants()
        => base.Parameter_collection_Count_with_huge_number_of_values_over_5_operations_forced_constants();

    public override Task Parameter_collection_Count_with_huge_number_of_values_over_5_operations_mixed_parameters_constants()
        => base.Parameter_collection_Count_with_huge_number_of_values_over_5_operations_mixed_parameters_constants();

    public override Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations()
        => base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations();

    public override Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_same_parameter()
        => base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_same_parameter();

    public override Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_2_operations_same_parameter_different_type_mapping()
        => base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_2_operations_same_parameter_different_type_mapping();

    public override Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_forced_constants()
        => base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_forced_constants();

    public override Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_mixed_parameters_constants()
        => base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_mixed_parameters_constants();

    public override Task Static_readonly_collection_List_of_ints_Contains_int()
        => base.Static_readonly_collection_List_of_ints_Contains_int();

    public override Task Static_readonly_collection_FrozenSet_of_ints_Contains_int()
        => base.Static_readonly_collection_FrozenSet_of_ints_Contains_int();

    public override Task Static_readonly_collection_ImmutableArray_of_ints_Contains_int()
        => base.Static_readonly_collection_ImmutableArray_of_ints_Contains_int();

    public override Task Contains_on_Enumerable()
        => base.Contains_on_Enumerable();

    public override Task Contains_on_MemoryExtensions()
        => base.Contains_on_MemoryExtensions();

    public override Task Contains_with_MemoryExtensions_with_null_comparer()
        => base.Contains_with_MemoryExtensions_with_null_comparer();

    public override Task Compiled_query_with_uncorrelated_parameter_collection_expression()
        => base.Compiled_query_with_uncorrelated_parameter_collection_expression();

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private PrimitiveCollectionsContext CreateContext()
        => Fixture.CreateContext();

    public class PrimitiveCollectionsQueryNpgsqlFixture : PrimitiveCollectionsQueryFixtureBase, ITestSqlLoggerFactory
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;
    }
}
