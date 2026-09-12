using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

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

    public override async Task Length()
    {
        await base.Length();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."ByteArray") = 4
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."ByteArray") >= 3 AND get_byte(b."ByteArray", 2) = 190
""");
    }

    public override async Task First()
    {
        await base.First();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."ByteArray") >= 1 AND get_byte(b."ByteArray", 0) = 222
""");
    }

    public override async Task Contains_with_constant()
    {
        await base.Contains_with_constant();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE position(BYTEA E'\\x01' IN b."ByteArray") > 0
""");
    }

    public override async Task Contains_with_parameter()
    {
        await base.Contains_with_parameter();

        AssertSql(
            """
@someByte='1'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE position(set_byte(BYTEA E'\\x00', 0, @someByte) IN b."ByteArray") > 0
""");
    }

    public override async Task Contains_with_column()
    {
        await base.Contains_with_column();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE position(set_byte(BYTEA E'\\x00', 0, b."Byte") IN b."ByteArray") > 0
""");
    }

    public override async Task SequenceEqual()
    {
        await base.SequenceEqual();

        AssertSql(
            """
@byteArrayParam='0xDEADBEEF'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."ByteArray" = @byteArrayParam
""");
    }

    public override async Task Any()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Any()));

        AssertSql(
"""
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."ByteArray") > 0
""");
    }

    [ConditionalFact]
    public virtual async Task Concat_with_parameter()
    {
        var suffix = new byte[] { 1, 2 };

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Concat(suffix).ToArray().Length == 6));

        AssertSql(
            """
@suffix='0x0102'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."ByteArray" || @suffix) = 6
""");
    }

    [ConditionalFact]
    public virtual async Task Concat_with_column()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Concat(e.ByteArray).ToArray().Length == 8));

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."ByteArray" || b."ByteArray") = 8
""");
    }

    [ConditionalFact]
    public virtual async Task Concat_in_projection()
    {
        var suffix = new byte[] { 1, 2 };

        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().OrderBy(e => e.Id).Select(e => e.ByteArray.Concat(suffix).ToArray()),
            assertOrder: true);

        AssertSql(
            """
@suffix='0x0102'

SELECT b."ByteArray" || @suffix
FROM "BasicTypesEntities" AS b
ORDER BY b."Id" NULLS FIRST
""");
    }

    [ConditionalFact]
    public virtual async Task Concat_SequenceEqual()
    {
        var suffix = new byte[] { 1, 2 };
        var expected = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 1, 2 };

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.ByteArray.Concat(suffix).SequenceEqual(expected)));

        AssertSql(
            """
@suffix='0x0102'
@expected='0xDEADBEEF0102'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."ByteArray" || @suffix = @expected
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
