using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class StringTranslationsNpgsqlTest : StringTranslationsRelationalTestBase<BasicTypesQueryNpgsqlFixture>
{
    public StringTranslationsNpgsqlTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Equals

    public override async Task Equals()
    {
        await base.Equals();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" = 'Seattle'
""");
    }

    public override async Task Equals_with_OrdinalIgnoreCase()
    {
        await base.Equals_with_OrdinalIgnoreCase();

        AssertSql();
    }

    public override async Task Equals_with_Ordinal()
    {
        await base.Equals_with_Ordinal();

        AssertSql();
    }

    public override async Task Static_Equals()
    {
        await base.Static_Equals();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" = 'Seattle'
""");
    }

    public override async Task Static_Equals_with_OrdinalIgnoreCase()
    {
        await base.Static_Equals_with_OrdinalIgnoreCase();

        AssertSql();
    }

    public override async Task Static_Equals_with_Ordinal()
    {
        await base.Static_Equals_with_Ordinal();

        AssertSql();
    }

    #endregion Equals

    #region Miscellaneous

    public override async Task Length()
    {
        await base.Length();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int = 7
""");
    }

    public override async Task ToUpper()
    {
        await base.ToUpper();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE upper(b."String") = 'SEATTLE'
""",
            //
            """
SELECT upper(b."String")
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task ToLower()
    {
        await base.ToLower();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE lower(b."String") = 'seattle'
""",
            //
            """
SELECT lower(b."String")
FROM "BasicTypesEntities" AS b
""");
    }

    #endregion Miscellaneous

    #region IndexOf

    public override async Task IndexOf()
    {
        await base.IndexOf();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."String", 'eattl') - 1 <> -1
""");
    }

    public override async Task IndexOf_Char()
    {
        await base.IndexOf_Char();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."String", 'e') - 1 <> -1
""");
    }

    public override async Task IndexOf_with_empty_string()
    {
        await base.IndexOf_with_empty_string();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."String", '') - 1 = 0
""");
    }

    public override async Task IndexOf_with_one_parameter_arg()
    {
        await base.IndexOf_with_one_parameter_arg();

        AssertSql(
            """
@pattern='eattl'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."String", @pattern) - 1 = 1
""");
    }

    public override async Task IndexOf_with_one_parameter_arg_char()
    {
        await base.IndexOf_with_one_parameter_arg_char();

        AssertSql(
            """
@pattern='e' (DbType = String)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."String", @pattern) - 1 = 1
""");
    }

    // PostgreSQL does not have strpos with starting position
    public override Task IndexOf_with_constant_starting_position()
        => AssertTranslationFailed(() => base.IndexOf_with_constant_starting_position());

    // PostgreSQL does not have strpos with starting position
    public override Task IndexOf_with_constant_starting_position_char()
        => AssertTranslationFailed(() => base.IndexOf_with_constant_starting_position_char());

    // PostgreSQL does not have strpos with starting position
    public override Task IndexOf_with_parameter_starting_position()
        => AssertTranslationFailed(() => base.IndexOf_with_parameter_starting_position());

    // PostgreSQL does not have strpos with starting position
    public override Task IndexOf_with_parameter_starting_position_char()
        => AssertTranslationFailed(() => base.IndexOf_with_parameter_starting_position_char());

    public override async Task IndexOf_after_ToString()
    {
        await base.IndexOf_after_ToString();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."Int"::text, '55') - 1 = 1
""");
    }

    public override async Task IndexOf_over_ToString()
    {
        await base.IndexOf_over_ToString();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos('12559', b."Int"::text) - 1 = 1
""");
    }

    #endregion IndexOf

    #region Replace

    public override async Task Replace()
    {
        await base.Replace();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE replace(b."String", 'Sea', 'Rea') = 'Reattle'
""");
    }

    public override async Task Replace_Char()
    {
        await base.Replace_Char();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE replace(b."String", 'S', 'R') = 'Reattle'
""");
    }

    public override async Task Replace_with_empty_string()
    {
        await base.Replace_with_empty_string();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <> '' AND replace(b."String", b."String", '') = ''
""");
    }

    public override async Task Replace_using_property_arguments()
    {
        await base.Replace_using_property_arguments();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <> '' AND replace(b."String", b."String", b."Int"::text) = b."Int"::text
""");
    }

    #endregion Replace

    #region Substring

    public override async Task Substring()
    {
        await base.Substring();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 3 AND substring(b."String", 2, 2) = 'ea'
""");
    }

    public override async Task Substring_with_one_arg_with_zero_startIndex()
    {
        await base.Substring_with_one_arg_with_zero_startIndex();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE substring(b."String", 1) = 'Seattle'
""");
    }

    public override async Task Substring_with_one_arg_with_constant()
    {
        await base.Substring_with_one_arg_with_constant();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 1 AND substring(b."String", 2) = 'eattle'
""");
    }

    public override async Task Substring_with_one_arg_with_parameter()
    {
        await base.Substring_with_one_arg_with_parameter();

        AssertSql(
            """
@start='2'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 2 AND substring(b."String", @start + 1) = 'attle'
""");
    }

    public override async Task Substring_with_two_args_with_zero_startIndex()
    {
        await base.Substring_with_two_args_with_zero_startIndex();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 3 AND substring(b."String", 1, 3) = 'Sea'
""");
    }

    public override async Task Substring_with_two_args_with_zero_length()
    {
        await base.Substring_with_two_args_with_zero_length();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 2 AND substring(b."String", 3, 0) = ''
""");
    }

    public override async Task Substring_with_two_args_with_parameter()
    {
        await base.Substring_with_two_args_with_parameter();

        AssertSql(
            """
@start='2'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 5 AND substring(b."String", @start + 1, 3) = 'att'
""");
    }

    public override async Task Substring_with_two_args_with_IndexOf()
    {
        await base.Substring_with_two_args_with_IndexOf();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE '%a%' AND substring(b."String", (strpos(b."String", 'a') - 1) + 1, 3) = 'att'
""");
    }

    #endregion Substring

    #region IsNullOrEmpty/Whitespace

    public override async Task IsNullOrEmpty()
    {
        await base.IsNullOrEmpty();

        AssertSql(
            """
SELECT n."Id", n."Bool", n."Byte", n."ByteArray", n."DateOnly", n."DateTime", n."DateTimeOffset", n."Decimal", n."Double", n."Enum", n."FlagsEnum", n."Float", n."Guid", n."Int", n."Long", n."Short", n."String", n."TimeOnly", n."TimeSpan"
FROM "NullableBasicTypesEntities" AS n
WHERE n."String" IS NULL OR n."String" = ''
""",
            //
            """
SELECT n."String" IS NULL OR n."String" = ''
FROM "NullableBasicTypesEntities" AS n
""");
    }

    public override async Task IsNullOrEmpty_negated()
    {
        await base.IsNullOrEmpty_negated();

        AssertSql(
            """
SELECT n."Id", n."Bool", n."Byte", n."ByteArray", n."DateOnly", n."DateTime", n."DateTimeOffset", n."Decimal", n."Double", n."Enum", n."FlagsEnum", n."Float", n."Guid", n."Int", n."Long", n."Short", n."String", n."TimeOnly", n."TimeSpan"
FROM "NullableBasicTypesEntities" AS n
WHERE n."String" IS NOT NULL AND n."String" <> ''
""",
            //
            """
SELECT n."String" IS NOT NULL AND n."String" <> ''
FROM "NullableBasicTypesEntities" AS n
""");
    }

    public override async Task IsNullOrWhiteSpace()
    {
        await base.IsNullOrWhiteSpace();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE btrim(b."String", E' \t\n\r') = ''
""");
    }

    #endregion IsNullOrEmpty/Whitespace

    #region StartsWith

    public override async Task StartsWith_Literal()
    {
        await base.StartsWith_Literal();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE 'Se%'
""");
    }

    public override async Task StartsWith_Literal_Char()
    {
        await base.StartsWith_Literal_Char();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE 'S%'
""");
    }

    public override async Task StartsWith_Parameter()
    {
        await base.StartsWith_Parameter();

        AssertSql(
            """
@pattern_startswith='Se%'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE @pattern_startswith
""");
    }

    public override async Task StartsWith_Parameter_Char()
    {
        await base.StartsWith_Parameter_Char();

        AssertSql(
            """
@pattern_startswith='S%'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE @pattern_startswith
""");
    }

    public override async Task StartsWith_Column()
    {
        await base.StartsWith_Column();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE left(b."String", length(b."String")) = b."String"
""");
    }

    public override async Task StartsWith_with_StringComparison_Ordinal()
    {
        await base.StartsWith_with_StringComparison_Ordinal();

        AssertSql();
    }

    public override async Task StartsWith_with_StringComparison_OrdinalIgnoreCase()
    {
        await base.StartsWith_with_StringComparison_OrdinalIgnoreCase();

        AssertSql();
    }

    public override async Task StartsWith_with_StringComparison_unsupported()
    {
        await base.StartsWith_with_StringComparison_unsupported();

        AssertSql();
    }

    #endregion StartsWith

    #region EndsWith

    public override async Task EndsWith_Literal()
    {
        await base.EndsWith_Literal();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE '%le'
""");
    }

    public override async Task EndsWith_Literal_Char()
    {
        await base.EndsWith_Literal_Char();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE '%e'
""");
    }

    public override async Task EndsWith_Parameter()
    {
        await base.EndsWith_Parameter();

        AssertSql(
            """
@pattern_endswith='%le'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE @pattern_endswith
""");
    }

    public override async Task EndsWith_Parameter_Char()
    {
        await base.EndsWith_Parameter_Char();

        AssertSql(
            """
@pattern_endswith='%e'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE @pattern_endswith
""");
    }

    public override async Task EndsWith_Column()
    {
        // SQL Server trims trailing whitespace for length calculations, making our EndsWith() column translation not work reliably in that
        // case
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String == "Seattle" && b.String.EndsWith(b.String)));

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" = 'Seattle' AND right(b."String", length(b."String")) = b."String"
""");
    }

    public override async Task EndsWith_with_StringComparison_Ordinal()
    {
        await base.EndsWith_with_StringComparison_Ordinal();

        AssertSql();
    }

    public override async Task EndsWith_with_StringComparison_OrdinalIgnoreCase()
    {
        await base.EndsWith_with_StringComparison_OrdinalIgnoreCase();

        AssertSql();
    }

    public override async Task EndsWith_with_StringComparison_unsupported()
    {
        await base.EndsWith_with_StringComparison_unsupported();

        AssertSql();
    }

    #endregion EndsWith

    #region Contains

    public override async Task Contains_Literal()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl")), // SQL Server is case-insensitive by default
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.OrdinalIgnoreCase)));

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE '%eattl%'
""");
    }

    public override async Task Contains_Literal_Char()
    {
        await base.Contains_Literal_Char();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE '%e%'
""");
    }

    public override async Task Contains_Column()
    {
        await base.Contains_Column();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."String", b."String") > 0
""",
            //
            """
SELECT strpos(b."String", b."String") > 0
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Contains_negated()
    {
        await base.Contains_negated();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" NOT LIKE '%eattle%'
""",
            //
            """
SELECT b."String" NOT LIKE '%eattle%'
FROM "BasicTypesEntities" AS b
""");
    }

    public override async Task Contains_with_StringComparison_Ordinal()
    {
        await base.Contains_with_StringComparison_Ordinal();

        AssertSql();
    }

    public override async Task Contains_with_StringComparison_OrdinalIgnoreCase()
    {
        await base.Contains_with_StringComparison_OrdinalIgnoreCase();

        AssertSql();
    }

    public override async Task Contains_with_StringComparison_unsupported()
    {
        await base.Contains_with_StringComparison_unsupported();

        AssertSql();
    }

    public override async Task Contains_constant_with_whitespace()
    {
        await base.Contains_constant_with_whitespace();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE '%     %'
""");
    }

    public override async Task Contains_parameter_with_whitespace()
    {
        await base.Contains_parameter_with_whitespace();

        AssertSql(
            """
@pattern_contains='%     %'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE @pattern_contains
""");
    }

    #endregion Contains

    #region TrimStart

    public override async Task TrimStart_without_arguments()
    {
        await base.TrimStart_without_arguments();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE ltrim(b."String", E' \t\n\r') = 'Boston  '
""");
    }

    public override async Task TrimStart_with_char_argument()
    {
        await base.TrimStart_with_char_argument();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE ltrim(b."String", 'S') = 'eattle'
""");
    }

    public override async Task TrimStart_with_char_array_argument()
    {
        await base.TrimStart_with_char_array_argument();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE ltrim(b."String", 'Se') = 'attle'
""");
    }

    #endregion TrimStart

    #region TrimEnd

    public override async Task TrimEnd_without_arguments()
    {
        await base.TrimEnd_without_arguments();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE rtrim(b."String", E' \t\n\r') = '  Boston'
""");
    }

    public override async Task TrimEnd_with_char_argument()
    {
        await base.TrimEnd_with_char_argument();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE rtrim(b."String", 'e') = 'Seattl'
""");
    }

    public override async Task TrimEnd_with_char_array_argument()
    {
        await base.TrimEnd_with_char_array_argument();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE rtrim(b."String", 'le') = 'Seatt'
""");
    }

    #endregion TrimEnd

    #region Trim

    public override async Task Trim_without_argument_in_predicate()
    {
        await base.Trim_without_argument_in_predicate();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE btrim(b."String", E' \t\n\r') = 'Boston'
""");
    }

    public override async Task Trim_with_char_argument_in_predicate()
    {
        await base.Trim_with_char_argument_in_predicate();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE btrim(b."String", 'S') = 'eattle'
""");
    }

    public override async Task Trim_with_char_array_argument_in_predicate()
    {
        await base.Trim_with_char_array_argument_in_predicate();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE btrim(b."String", 'Se') = 'attl'
""");
    }

    #endregion Trim

    #region Compare

    public override async Task Compare_simple_zero()
    {
        await base.Compare_simple_zero();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" = 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <> 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= 'Seattle'
""");
    }

    public override async Task Compare_simple_one()
    {
        await base.Compare_simple_one();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" < 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= 'Seattle'
""");
    }

    public override async Task Compare_with_parameter()
    {
        await base.Compare_with_parameter();

        AssertSql(
            """
@basicTypeEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > @basicTypeEntity_String
""",
            //
            """
@basicTypeEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" < @basicTypeEntity_String
""",
            //
            """
@basicTypeEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= @basicTypeEntity_String
""",
            //
            """
@basicTypeEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= @basicTypeEntity_String
""",
            //
            """
@basicTypeEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= @basicTypeEntity_String
""",
            //
            """
@basicTypeEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= @basicTypeEntity_String
""");
    }

    public override async Task Compare_simple_more_than_one()
    {
        await base.Compare_simple_more_than_one();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CASE
    WHEN b."String" = 'Seattle' THEN 0
    WHEN b."String" > 'Seattle' THEN 1
    WHEN b."String" < 'Seattle' THEN -1
END = 42
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CASE
    WHEN b."String" = 'Seattle' THEN 0
    WHEN b."String" > 'Seattle' THEN 1
    WHEN b."String" < 'Seattle' THEN -1
END > 42
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE 42 > CASE
    WHEN b."String" = 'Seattle' THEN 0
    WHEN b."String" > 'Seattle' THEN 1
    WHEN b."String" < 'Seattle' THEN -1
END
""");
    }

    public override async Task Compare_nested()
    {
        await base.Compare_nested();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" = 'M' || b."String"
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <> substring(b."String", 1, 0)
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > replace('Seattle', 'Sea', b."String")
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= 'M' || b."String"
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > substring(b."String", 1, 0)
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" < replace('Seattle', 'Sea', b."String")
""");
    }

    public override async Task Compare_multi_predicate()
    {
        await base.Compare_multi_predicate();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= 'Seattle' AND b."String" < 'Toronto'
""");
    }

    public override async Task CompareTo_simple_zero()
    {
        await base.CompareTo_simple_zero();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" = 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <> 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= 'Seattle'
""");
    }

    public override async Task CompareTo_simple_one()
    {
        await base.CompareTo_simple_one();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" < 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= 'Seattle'
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= 'Seattle'
""");
    }

    public override async Task CompareTo_with_parameter()
    {
        await base.CompareTo_with_parameter();

        AssertSql(
            """
@basicTypesEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > @basicTypesEntity_String
""",
            //
            """
@basicTypesEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" < @basicTypesEntity_String
""",
            //
            """
@basicTypesEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= @basicTypesEntity_String
""",
            //
            """
@basicTypesEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= @basicTypesEntity_String
""",
            //
            """
@basicTypesEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= @basicTypesEntity_String
""",
            //
            """
@basicTypesEntity_String='Seattle'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= @basicTypesEntity_String
""");
    }

    public override async Task CompareTo_simple_more_than_one()
    {
        await base.CompareTo_simple_more_than_one();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CASE
    WHEN b."String" = 'Seattle' THEN 0
    WHEN b."String" > 'Seattle' THEN 1
    WHEN b."String" < 'Seattle' THEN -1
END = 42
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE CASE
    WHEN b."String" = 'Seattle' THEN 0
    WHEN b."String" > 'Seattle' THEN 1
    WHEN b."String" < 'Seattle' THEN -1
END > 42
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE 42 > CASE
    WHEN b."String" = 'Seattle' THEN 0
    WHEN b."String" > 'Seattle' THEN 1
    WHEN b."String" < 'Seattle' THEN -1
END
""");
    }

    public override async Task CompareTo_nested()
    {
        await base.CompareTo_nested();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" = 'M' || b."String"
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <> substring(b."String", 1, 0)
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > replace('Seattle', 'Sea', b."String")
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <= 'M' || b."String"
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" > substring(b."String", 1, 0)
""",
            //
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" < replace('Seattle', 'Sea', b."String")
""");
    }

    public override async Task Compare_to_multi_predicate()
    {
        await base.Compare_to_multi_predicate();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= 'Seattle' AND b."String" < 'Toronto'
""");
    }

    #endregion Compare

    #region Join

    public override async Task Join_over_non_nullable_column()
    {
        await base.Join_over_non_nullable_column();

        AssertSql(
            """
SELECT b."Int" AS "Key", COALESCE(string_agg(b."String", '|'), '') AS "Strings"
FROM "BasicTypesEntities" AS b
GROUP BY b."Int"
""");
    }

    public override async Task Join_over_nullable_column()
    {
        await base.Join_over_nullable_column();

        AssertSql(
            """
SELECT n0."Key", COALESCE(string_agg(COALESCE(n0."String", ''), '|'), '') AS "Regions"
FROM (
    SELECT n."String", COALESCE(n."Int", 0) AS "Key"
    FROM "NullableBasicTypesEntities" AS n
) AS n0
GROUP BY n0."Key"
""");
    }

    public override async Task Join_with_predicate()
    {
        await base.Join_with_predicate();

        AssertSql(
            """
SELECT b."Int" AS "Key", COALESCE(string_agg(b."String", '|') FILTER (WHERE length(b."String")::int > 6), '') AS "Strings"
FROM "BasicTypesEntities" AS b
GROUP BY b."Int"
""");
    }

    public override async Task Join_with_ordering()
    {
        await base.Join_with_ordering();

        AssertSql(
            """
SELECT b."Int" AS "Key", COALESCE(string_agg(b."String", '|' ORDER BY b."Id" DESC NULLS LAST), '') AS "Strings"
FROM "BasicTypesEntities" AS b
GROUP BY b."Int"
""");
    }

    public override async Task Join_non_aggregate()
    {
        await base.Join_non_aggregate();

        AssertSql(
            """
@foo='foo'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE concat_ws('|', b."String", @foo, '', 'bar') = 'Seattle|foo||bar'
""");
    }

    #endregion Join

    #region Concatenation

    public override async Task Concat_operator()
    {
        await base.Concat_operator();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" || 'Boston' = 'SeattleBoston'
""");
    }

    public override async Task Concat_aggregate()
    {
        await base.Concat_aggregate();

        AssertSql(
            """
SELECT b."Int" AS "Key", COALESCE(string_agg(b."String", ''), '') AS "BasicTypesEntitys"
FROM "BasicTypesEntities" AS b
GROUP BY b."Int"
""");
    }

    public override async Task Concat_string_int_comparison1()
    {
        await base.Concat_string_int_comparison1();

        AssertSql(
            """
@i='10'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" || @i::text = 'Seattle10'
""");
    }

    public override async Task Concat_string_int_comparison2()
    {
        await base.Concat_string_int_comparison2();

        AssertSql(
            """
@i='10'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @i::text || b."String" = '10Seattle'
""");
    }

    public override async Task Concat_string_int_comparison3()
    {
        await base.Concat_string_int_comparison3();

        AssertSql(
            """
@p='30'
@j='21'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @p::text || b."String" || @j::text || 42::text = '30Seattle2142'
""");
    }

    public override async Task Concat_string_int_comparison4()
    {
        await base.Concat_string_int_comparison4();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int"::text || b."String" = '8Seattle'
""");
    }

    public override async Task Concat_string_string_comparison()
    {
        await base.Concat_string_string_comparison();

        AssertSql(
            """
@i='A'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @i || b."String" = 'ASeattle'
""");
    }

    public override async Task Concat_method_comparison()
    {
        await base.Concat_method_comparison();

        AssertSql(
            """
@i='A'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @i || b."String" = 'ASeattle'
""");
    }

    public override async Task Concat_method_comparison_2()
    {
        await base.Concat_method_comparison_2();

        AssertSql(
            """
@i='A'
@j='B'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @i || @j || b."String" = 'ABSeattle'
""");
    }

    public override async Task Concat_method_comparison_3()
    {
        await base.Concat_method_comparison_3();

        AssertSql(
            """
@i='A'
@j='B'
@k='C'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @i || @j || @k || b."String" = 'ABCSeattle'
""");
    }

    #endregion Concatenation

    #region LINQ Operators

    public override async Task FirstOrDefault()
    {
        await base.FirstOrDefault();
        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE substr(b."String", 1, 1) = 'S'
""");
    }

    public override async Task LastOrDefault()
    {
        await base.LastOrDefault();
        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE substr(b."String", length(b."String"), 1) = 'e'
""");
    }

    #endregion LINQ Operators

    #region Like

    public override async Task Where_Like_and_comparison()
    {
        await base.Where_Like_and_comparison();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE 'S%' AND b."Int" = 8
""");
    }

    public override async Task Where_Like_or_comparison()
    {
        await base.Where_Like_or_comparison();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE 'S%' OR b."Int" = 2147483647
""");
    }

    public override async Task Like_with_non_string_column_using_ToString()
    {
        await base.Like_with_non_string_column_using_ToString();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int"::text LIKE '%5%'
""");
    }

    public override async Task Like_with_non_string_column_using_double_cast()
    {
        await base.Like_with_non_string_column_using_double_cast();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int"::text LIKE '%5%'
""");
    }

    #endregion Like

    #region Regex

    public override async Task Regex_IsMatch()
    {
        await base.Regex_IsMatch();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" ~ '(?p)^S'
""");
    }

    public override async Task Regex_IsMatch_constant_input()
    {
        await base.Regex_IsMatch_constant_input();

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE 'Seattle' ~ ('(?p)' || b."String")
""");
    }

    #endregion Regex

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
