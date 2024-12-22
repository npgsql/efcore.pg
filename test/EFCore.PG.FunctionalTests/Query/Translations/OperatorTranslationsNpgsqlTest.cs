namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class OperatorTranslationsNpgsqlTest : OperatorTranslationsTestBase<BasicTypesQueryNpgsqlFixture>
{
    public OperatorTranslationsNpgsqlTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Bitwise

    public override async Task Bitwise_or(bool async)
    {
        await base.Bitwise_or(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int"::bigint | b."Long" = 7
""",
            //
            """
SELECT b."Int"::bigint | b."Long"
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Bitwise_or_over_boolean(bool async)
    {
        await base.Bitwise_or_over_boolean(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" = 12 OR b."String" = 'Seattle'
""",
            //
            """
SELECT b."Int" = 12 OR b."String" = 'Seattle'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Bitwise_or_multiple(bool async)
    {
        await base.Bitwise_or_multiple(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CAST(b."Int" | b."Short" AS bigint) | b."Long" = 7
""");
    }

    public override async Task Bitwise_and(bool async)
    {
        await base.Bitwise_and(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" & b."Short" = 2
""",
            //
            """
SELECT b."Int" & b."Short"
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Bitwise_and_over_boolean(bool async)
    {
        await base.Bitwise_and_over_boolean(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" = 8 AND b."String" = 'Seattle'
""",
            //
            """
SELECT b."Int" = 8 AND b."String" = 'Seattle'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Bitwise_xor(bool async)
    {
        await base.Bitwise_xor(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE (b."Int" # b."Short") = 1
""",
            //
            """
SELECT b."Int" # b."Short"
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Bitwise_xor_over_boolean(bool async)
    {
        await base.Bitwise_xor_over_boolean(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE (b."Int" = b."Short") <> (b."String" = 'Seattle')
""");
    }

    public override async Task Bitwise_complement(bool async)
    {
        await base.Bitwise_complement(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE ~b."Int" = -9
""");
    }

    public override async Task Bitwise_and_or_over_boolean(bool async)
    {
        await base.Bitwise_and_or_over_boolean(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE (b."Int" = 12 AND b."Short" = 12) OR b."String" = 'Seattle'
""");
    }

    public override async Task Bitwise_or_with_logical_or(bool async)
    {
        await base.Bitwise_or_with_logical_or(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" = 12 OR b."Short" = 12 OR b."String" = 'Seattle'
""");
    }

    public override async Task Bitwise_and_with_logical_and(bool async)
    {
        await base.Bitwise_and_with_logical_and(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" = 8 AND b."Short" = 8 AND b."String" = 'Seattle'
""");
    }

    public override async Task Bitwise_or_with_logical_and(bool async)
    {
        await base.Bitwise_or_with_logical_and(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE (b."Int" = 8 OR b."Short" = 9) AND b."String" = 'Seattle'
""");
    }

    public override async Task Bitwise_and_with_logical_or(bool async)
    {
        await base.Bitwise_and_with_logical_or(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE (b."Int" = 12 AND b."Short" = 12) OR b."String" = 'Seattle'
""");
    }

    #endregion Bitwise

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
