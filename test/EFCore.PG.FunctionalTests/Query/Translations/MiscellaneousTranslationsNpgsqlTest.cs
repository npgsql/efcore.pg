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

    public override async Task Random_on_EF_Functions(bool async)
    {
        await base.Random_on_EF_Functions(async);

        AssertSql(
            """
SELECT count(*)::int
FROM "BasicTypesEntities" AS b
WHERE random() >= 0.0 AND random() < 1.0
""");
    }

    public override async Task Random_Shared_Next_with_no_args(bool async)
    {
        await base.Random_Shared_Next_with_no_args(async);

        AssertSql();
    }

    public override async Task Random_Shared_Next_with_one_arg(bool async)
    {
        await base.Random_Shared_Next_with_one_arg(async);

        AssertSql();
    }

    public override async Task Random_Shared_Next_with_two_args(bool async)
    {
        await base.Random_Shared_Next_with_two_args(async);

        AssertSql();
    }

    public override async Task Random_new_Next_with_no_args(bool async)
    {
        await base.Random_new_Next_with_no_args(async);

        AssertSql();
    }

    public override async Task Random_new_Next_with_one_arg(bool async)
    {
        await base.Random_new_Next_with_one_arg(async);

        AssertSql();
    }

    public override async Task Random_new_Next_with_two_args(bool async)
    {
        await base.Random_new_Next_with_two_args(async);

        AssertSql();
    }

    #endregion Random

    #region Convert

    // These tests convert (among other things) to and from boolean, which PostgreSQL
    // does not support (https://github.com/dotnet/efcore/issues/19606)

    public override async Task Convert_ToBoolean(bool async)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToBoolean(async));
        Assert.Equal("42846", exception.SqlState);
    }

    public override async Task Convert_ToByte(bool async)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToByte(async));
        Assert.Equal("42846", exception.SqlState);
    }

    public override async Task Convert_ToDecimal(bool async)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToDecimal(async));
        Assert.Equal("42846", exception.SqlState);
    }

    public override async Task Convert_ToDouble(bool async)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToDouble(async));
        Assert.Equal("42846", exception.SqlState);
    }

    public override async Task Convert_ToInt16(bool async)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToInt16(async));
        Assert.Equal("42846", exception.SqlState);
    }

    public override async Task Convert_ToInt32(bool async)
    {
        await base.Convert_ToInt32(async);

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

    public override async Task Convert_ToInt64(bool async)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => base.Convert_ToInt64(async));
        Assert.Equal("42846", exception.SqlState);
    }

    // Convert on DateTime not yet supported
    public override Task Convert_ToString(bool async)
        => AssertTranslationFailed(() => base.Convert_ToString(async));

    #endregion Convert

    #region Compare

    public override async Task Int_Compare_to_simple_zero(bool async)
    {
        await base.Int_Compare_to_simple_zero(async);

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

    public override async Task DateTime_Compare_to_simple_zero(bool async, bool compareTo)
    {
        // The base test implementation uses an Unspecified DateTime, which isn't supported with PostgreSQL timestamptz
        var dateTime = new DateTime(1998, 5, 4, 15, 30, 10, DateTimeKind.Utc);

        if (compareTo)
        {
            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => c.DateTime.CompareTo(dateTime) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != c.DateTime.CompareTo(dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => c.DateTime.CompareTo(dateTime) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= c.DateTime.CompareTo(dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < c.DateTime.CompareTo(dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => c.DateTime.CompareTo(dateTime) <= 0));
        }
        else
        {
            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Compare(c.DateTime, dateTime) == 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 != DateTime.Compare(c.DateTime, dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Compare(c.DateTime, dateTime) > 0));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 >= DateTime.Compare(c.DateTime, dateTime)));

            await AssertQuery(
                async,
                ss => ss.Set<BasicTypesEntity>().Where(c => 0 < DateTime.Compare(c.DateTime, dateTime)));

            await AssertQuery(
                async,
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

    public override async Task TimeSpan_Compare_to_simple_zero(bool async, bool compareTo)
    {
        await base.TimeSpan_Compare_to_simple_zero(async, compareTo);

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
