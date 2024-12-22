namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class MathTranslationsNpgsqlTest : MathTranslationsTestBase<BasicTypesQueryNpgsqlFixture>
{
    public MathTranslationsNpgsqlTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Abs_decimal(bool async)
    {
        await base.Abs_decimal(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE abs(b."Decimal") = 9.5
""");
    }

    public override async Task Abs_int(bool async)
    {
        await base.Abs_int(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE abs(b."Int") = 9
""");
    }

    public override async Task Abs_double(bool async)
    {
        await base.Abs_double(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE abs(b."Double") = 9.5
""");
    }

    public override async Task Abs_float(bool async)
    {
        await base.Abs_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE abs(b."Float")::double precision = 9.5
""");
    }

    public override async Task Ceiling(bool async)
    {
        await base.Ceiling(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE ceiling(b."Double") = 9.0
""");
    }

    public override async Task Ceiling_float(bool async)
    {
        await base.Ceiling_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE ceiling(b."Float") = 9
""");
    }

    public override async Task Floor_decimal(bool async)
    {
        await base.Floor_decimal(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(b."Decimal") = 8.0
""");
    }

    public override async Task Floor_double(bool async)
    {
        await base.Floor_double(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(b."Double") = 8.0
""");
    }

    public override async Task Floor_float(bool async)
    {
        await base.Floor_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE floor(b."Float") = 8
""");
    }

    public override async Task Power(bool async)
    {
        await base.Power(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE power(b."Int"::double precision, 2.0) = 64.0
""");
    }

    public override async Task Power_float(bool async)
    {
        await base.Power_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE power(b."Float", 2) > 73 AND power(b."Float", 2) < 74
""");
    }

    public override async Task Round_decimal(bool async)
    {
        await base.Round_decimal(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE round(b."Decimal") = 9.0
""",
            //
            """
SELECT round(b."Decimal")
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Round_double(bool async)
    {
        await base.Round_double(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE round(b."Double") = 9.0
""",
            //
            """
SELECT round(b."Double")
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Round_float(bool async)
    {
        await base.Round_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE round(b."Float")::real = 9
""",
            //
            """
SELECT round(b."Float")::real
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Round_with_digits_decimal(bool async)
    {
        await base.Round_with_digits_decimal(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE round(b."Decimal", 1) = 255.1
""");
    }

    // PostgreSQL only has round(v, s) over numeric, may be possible to cast back and forth though
    public override Task Round_with_digits_double(bool async)
        => AssertTranslationFailed(() => base.Round_with_digits_double(async));

    // PostgreSQL only has round(v, s) over numeric, may be possible to cast back and forth though
    public override Task Round_with_digits_float(bool async)
        => AssertTranslationFailed(() => base.Round_with_digits_float(async));

    public override async Task Truncate_decimal(bool async)
    {
        await base.Truncate_decimal(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE trunc(b."Decimal") = 8.0
""",
            //
            """
SELECT trunc(b."Decimal")
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Truncate_double(bool async)
    {
        await base.Truncate_double(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE trunc(b."Double") = 8.0
""",
            //
            """
SELECT trunc(b."Double")
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Truncate_float(bool async)
    {
        await base.Truncate_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE trunc(b."Float")::real = 8
""",
            //
            """
SELECT trunc(b."Float")::real
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Truncate_project_and_order_by_it_twice(bool async)
    {
        await base.Truncate_project_and_order_by_it_twice(async);

        AssertSql(
            """
SELECT trunc(b."Double") AS "A"
FROM "BasicTypesEntities" AS b
ORDER BY trunc(b."Double") NULLS FIRST
""");
    }

    // issue #16038
    //            AssertSql(
    //                @"SELECT ROUND(CAST([o].[OrderID] AS float), 0, 1) AS [A]
    //FROM [Orders] AS [o]
    //WHERE [o].[OrderID] < 10250
    //ORDER BY [A]");
    public override async Task Truncate_project_and_order_by_it_twice2(bool async)
    {
        await base.Truncate_project_and_order_by_it_twice2(async);

        AssertSql(
            """
SELECT trunc(b."Double") AS "A"
FROM "BasicTypesEntities" AS b
ORDER BY trunc(b."Double") DESC NULLS LAST
""");
    }

    // issue #16038
    //            AssertSql(
    //                @"SELECT ROUND(CAST([o].[OrderID] AS float), 0, 1) AS [A]
    //FROM [Orders] AS [o]
    //WHERE [o].[OrderID] < 10250
    //ORDER BY [A] DESC");
    public override async Task Truncate_project_and_order_by_it_twice3(bool async)
    {
        await base.Truncate_project_and_order_by_it_twice3(async);

        AssertSql(
            """
SELECT trunc(b."Double") AS "A"
FROM "BasicTypesEntities" AS b
ORDER BY trunc(b."Double") DESC NULLS LAST
""");
    }

    public override async Task Exp(bool async)
    {
        await base.Exp(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE exp(b."Double") > 1.0
""");
    }

    public override async Task Exp_float(bool async)
    {
        await base.Exp_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE exp(b."Float") > 1
""");
    }

    public override async Task Log(bool async)
    {
        await base.Log(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Double" > 0.0 AND ln(b."Double") <> 0.0
""");
    }

    public override async Task Log_float(bool async)
    {
        await base.Log_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Float" > 0 AND ln(b."Float") <> 0
""");
    }

    // PostgreSQL only has log(x, base) over numeric, may be possible to cast back and forth though
    public override Task Log_with_newBase(bool async)
        => AssertTranslationFailed(() => base.Log_with_newBase(async));

    // PostgreSQL only has log(x, base) over numeric, may be possible to cast back and forth though
    public override Task Log_with_newBase_float(bool async)
        => AssertTranslationFailed(() => base.Log_with_newBase_float(async));

    public override async Task Log10(bool async)
    {
        await base.Log10(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Double" > 0.0 AND log(b."Double") <> 0.0
""");
    }

    public override async Task Log10_float(bool async)
    {
        await base.Log10_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Float" > 0 AND log(b."Float") <> 0
""");
    }

    public override async Task Log2(bool async)
        => await AssertTranslationFailed(() => base.Log2(async));

    public override async Task Sqrt(bool async)
    {
        await base.Sqrt(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Double" > 0.0 AND sqrt(b."Double") > 0.0
""");
    }

    public override async Task Sqrt_float(bool async)
    {
        await base.Sqrt_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Float" > 0 AND sqrt(b."Float") > 0
""");
    }

    public override async Task Sign(bool async)
    {
        await base.Sign(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE sign(b."Double")::int > 0
""");
    }

    public override async Task Sign_float(bool async)
    {
        await base.Sign_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE sign(b."Float")::int > 0
""");
    }

    public override async Task Max(bool async)
    {
        await base.Max(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE GREATEST(b."Int", b."Short" - 3) = b."Int"
""");
    }

    public override async Task Max_nested(bool async)
    {
        await base.Max_nested(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE GREATEST(b."Short" - 3, b."Int", 1) = b."Int"
""");
    }

    public override async Task Max_nested_twice(bool async)
    {
        await base.Max_nested_twice(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE GREATEST(1, b."Int", 2, b."Short" - 3) = b."Int"
""");
    }

    public override async Task Min(bool async)
    {
        await base.Min(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE LEAST(b."Int", b."Short" + 3) = b."Int"
""");
    }

    public override async Task Min_nested(bool async)
    {
        await base.Min_nested(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE LEAST(b."Short" + 3, b."Int", 99999) = b."Int"
""");
    }

    public override async Task Min_nested_twice(bool async)
    {
        await base.Min_nested_twice(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE LEAST(99999, b."Int", 99998, b."Short" + 3) = b."Int"
""");
    }

    public override async Task Degrees(bool async)
    {
        await base.Degrees(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE degrees(b."Double") > 0.0
""");
    }

    public override async Task Degrees_float(bool async)
    {
        await base.Degrees_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE degrees(b."Float") > 0
""");
    }

    public override async Task Radians(bool async)
    {
        await base.Radians(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE radians(b."Double") > 0.0
""");
    }

    public override async Task Radians_float(bool async)
    {
        await base.Radians_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE radians(b."Float") > 0
""");
    }

    #region Trigonometry

    public override async Task Acos(bool async)
    {
        await base.Acos(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Double" >= -1.0 AND b."Double" <= 1.0 AND acos(b."Double") > 1.0
""");
    }

    public override async Task Acos_float(bool async)
    {
        await base.Acos_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Float" >= -1 AND b."Float" <= 1 AND acos(b."Float") > 0
""");
    }

    public override async Task Acosh(bool async)
        => await AssertTranslationFailed(() => base.Acosh(async));

    public override async Task Asin(bool async)
    {
        await base.Asin(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Double" >= -1.0 AND b."Double" <= 1.0 AND asin(b."Double") > -1.7976931348623157E+308
""");
    }

    public override async Task Asin_float(bool async)
    {
        await base.Asin_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Float" >= -1 AND b."Float" <= 1 AND asin(b."Float")::double precision > -1.7976931348623157E+308
""");
    }

    public override async Task Asinh(bool async)
        => await AssertTranslationFailed(() => base.Asinh(async));

    public override async Task Atan(bool async)
    {
        await base.Atan(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE atan(b."Double") > 0.0
""");
    }

    public override async Task Atan_float(bool async)
    {
        await base.Atan_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE atan(b."Float") > 0
""");
    }

    public override async Task Atanh(bool async)
        => await AssertTranslationFailed(() => base.Atanh(async));

    public override async Task Atan2(bool async)
    {
        await base.Atan2(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE atan2(b."Double", 1.0) > 0.0
""");
    }

    public override async Task Atan2_float(bool async)
    {
        await base.Atan2_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE atan2(b."Float", 1) > 0
""");
    }

    public override async Task Cos(bool async)
    {
        await base.Cos(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE cos(b."Double") > 0.0
""");
    }

    public override async Task Cos_float(bool async)
    {
        await base.Cos_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE cos(b."Float") > 0
""");
    }

    public override async Task Cosh(bool async)
        => await AssertTranslationFailed(() => base.Cosh(async));

    public override async Task Sin(bool async)
    {
        await base.Sin(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE sin(b."Double") > 0.0
""");
    }

    public override async Task Sin_float(bool async)
    {
        await base.Sin_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE sin(b."Float") > 0
""");
    }

    public override async Task Sinh(bool async)
        => await AssertTranslationFailed(() => base.Sinh(async));

    public override async Task Tan(bool async)
    {
        await base.Tan(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE tan(b."Double") > 0.0
""");
    }

    public override async Task Tan_float(bool async)
    {
        await base.Tan_float(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE tan(b."Float") > 0
""");
    }

    public override async Task Tanh(bool async)
        => await AssertTranslationFailed(() => base.Tanh(async));

    #endregion Trigonometry

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
