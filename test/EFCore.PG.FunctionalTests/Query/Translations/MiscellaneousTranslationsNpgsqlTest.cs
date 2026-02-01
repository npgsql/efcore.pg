using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class MiscellaneousTranslationsNpgsqlTest : MiscellaneousTranslationsRelationalTestBase<BasicTypesQueryNpgsqlFixture>
{
    public MiscellaneousTranslationsNpgsqlTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Random

    public override async Task Random_on_EF_Functions()
    {
        await base.Random_on_EF_Functions();

        AssertSql(
            """
SELECT count(*)::int
FROM "BasicTypesEntities" AS b
WHERE random() >= 0.0 AND random() < 1.0
""");
    }

    public override async Task Random_Shared_Next_with_no_args()
    {
        await base.Random_Shared_Next_with_no_args();

        AssertSql();
    }

    public override async Task Random_Shared_Next_with_one_arg()
    {
        await base.Random_Shared_Next_with_one_arg();

        AssertSql();
    }

    public override async Task Random_Shared_Next_with_two_args()
    {
        await base.Random_Shared_Next_with_two_args();

        AssertSql();
    }

    public override async Task Random_new_Next_with_no_args()
    {
        await base.Random_new_Next_with_no_args();

        AssertSql();
    }

    public override async Task Random_new_Next_with_one_arg()
    {
        await base.Random_new_Next_with_one_arg();

        AssertSql();
    }

    public override async Task Random_new_Next_with_two_args()
    {
        await base.Random_new_Next_with_two_args();

        AssertSql();
    }

    #endregion Random

    #region Convert

    // These tests convert (among other things) to and from boolean, which PostgreSQL
    // does not support (https://github.com/dotnet/efcore/issues/19606)

    public override async Task Convert_ToBoolean()
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToBoolean());
        Assert.Equal("42846", exception.SqlState);
    }

    public override async Task Convert_ToByte()
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToByte());
        Assert.Equal("42846", exception.SqlState);
    }

    public override async Task Convert_ToDecimal()
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToDecimal());
        Assert.Equal("42846", exception.SqlState);
    }

    public override async Task Convert_ToDouble()
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToDouble());
        Assert.Equal("42846", exception.SqlState);
    }

    public override async Task Convert_ToInt16()
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToInt16());
        Assert.Equal("42846", exception.SqlState);
    }

    public override async Task Convert_ToInt32()
    {
        await base.Convert_ToInt32();

AssertSql(
"""
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Bool"::int = 1
""",
                //
                """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Byte"::int = 12
""",
                //
                """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Decimal"::int = 12
""",
                //
                """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Double"::int = 12
""",
                //
                """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Float"::int = 12
""",
                //
                """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Short"::int = 12
""",
                //
                """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int"::int = 12
""",
                //
                """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Long"::int = 12
""",
                //
                """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int"::text::int = 12
""",
                //
                """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int"::int = 12
""");
    }

    public override async Task Convert_ToInt64()
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToInt64());
        Assert.Equal("42846", exception.SqlState);
    }

    // Convert on DateTime not yet supported
    public override Task Convert_ToString()
        => AssertTranslationFailed(() => base.Convert_ToString());

    #endregion Convert

    #region Compare

    public override async Task Int_Compare_to_simple_zero()
    {
        await base.Int_Compare_to_simple_zero();

AssertSql(
    """
@orderId='8'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" = @orderId
""",
    //
    """
@orderId='8'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" <> @orderId
""",
    //
    """
@orderId='8'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" > @orderId
""",
    //
    """
@orderId='8'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" <= @orderId
""",
    //
    """
@orderId='8'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" > @orderId
""",
    //
    """
@orderId='8'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int" <= @orderId
""");
    }

    public override async Task DateTime_Compare_to_simple_zero(bool compareTo)
    {
        // The base test implementation uses an Unspecified DateTime, which isn't supported with PostgreSQL timestamptz
        var dateTime = new DateTime(1998, 5, 4, 15, 30, 10, DateTimeKind.Utc);

        if (compareTo)
        {
            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => c.DateTime.CompareTo(dateTime) == 0));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != c.DateTime.CompareTo(dateTime)));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => c.DateTime.CompareTo(dateTime) > 0));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= c.DateTime.CompareTo(dateTime)));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < c.DateTime.CompareTo(dateTime)));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => c.DateTime.CompareTo(dateTime) <= 0));
        }
        else
        {
            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Compare(c.DateTime, dateTime) == 0));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != DateTime.Compare(c.DateTime, dateTime)));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Compare(c.DateTime, dateTime) > 0));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= DateTime.Compare(c.DateTime, dateTime)));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < DateTime.Compare(c.DateTime, dateTime)));

            await AssertQuery(
                ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Compare(c.DateTime, dateTime) <= 0));
        }

AssertSql(
"""
@dateTime='1998-05-04T15:30:10.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" = @dateTime
""",
                //
                """
@dateTime='1998-05-04T15:30:10.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" <> @dateTime
""",
                //
                """
@dateTime='1998-05-04T15:30:10.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" > @dateTime
""",
                //
                """
@dateTime='1998-05-04T15:30:10.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" <= @dateTime
""",
                //
                """
@dateTime='1998-05-04T15:30:10.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" > @dateTime
""",
                //
                """
@dateTime='1998-05-04T15:30:10.0000000Z' (DbType = DateTime)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."DateTime" <= @dateTime
""");
    }

    public override async Task TimeSpan_Compare_to_simple_zero(bool compareTo)
    {
        await base.TimeSpan_Compare_to_simple_zero(compareTo);

        AssertSql(
            """
@timeSpan='01:02:03' (DbType = Object)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan" = @timeSpan
""",
            //
            """
@timeSpan='01:02:03' (DbType = Object)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan" <> @timeSpan
""",
            //
            """
@timeSpan='01:02:03' (DbType = Object)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan" > @timeSpan
""",
            //
            """
@timeSpan='01:02:03' (DbType = Object)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan" <= @timeSpan
""",
            //
            """
@timeSpan='01:02:03' (DbType = Object)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan" > @timeSpan
""",
            //
            """
@timeSpan='01:02:03' (DbType = Object)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."TimeSpan" <= @timeSpan
""");
    }

    #endregion Compare

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
