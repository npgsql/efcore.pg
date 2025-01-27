namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class ByteArrayTranslationsNpgsqlTest : ByteArrayTranslationsTestBase<BasicTypesQueryNpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public ByteArrayTranslationsNpgsqlTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Length(bool async)
    {
        await base.Length(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."ByteArray") = 4
""");
    }

    public override async Task Index(bool async)
    {
        await base.Index(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."ByteArray") >= 3 AND get_byte(b."ByteArray", 2) = 190
""");
    }

    public override async Task First(bool async)
    {
        await base.First(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."ByteArray") >= 1 AND get_byte(b."ByteArray", 0)::smallint = 222
""");
    }

    public override async Task Contains_with_constant(bool async)
    {
        await base.Contains_with_constant(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE position(BYTEA E'\\x01' IN b."ByteArray") > 0
""");
    }

    public override async Task Contains_with_parameter(bool async)
    {
        await base.Contains_with_parameter(async);

        AssertSql(
            """
@someByte='1' (DbType = Int16)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE position(set_byte(BYTEA E'\\x00', 0, @someByte) IN b."ByteArray") > 0
""");
    }

    public override async Task Contains_with_column(bool async)
    {
        await base.Contains_with_column(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE position(set_byte(BYTEA E'\\x00', 0, b."Byte") IN b."ByteArray") > 0
""");
    }

    public override async Task SequenceEqual(bool async)
    {
        await base.SequenceEqual(async);

        AssertSql(
            """
@byteArrayParam='0xDEADBEEF'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."ByteArray" = @byteArrayParam
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
