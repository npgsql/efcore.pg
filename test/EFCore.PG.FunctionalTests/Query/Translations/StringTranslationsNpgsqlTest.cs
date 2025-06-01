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

    public override async Task Equals(bool async)
    {
        await base.Equals(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" = 'Seattle'
""");
    }

    public override async Task Equals_with_OrdinalIgnoreCase(bool async)
    {
        await base.Equals_with_OrdinalIgnoreCase(async);

        AssertSql();
    }

    public override async Task Equals_with_Ordinal(bool async)
    {
        await base.Equals_with_Ordinal(async);

        AssertSql();
    }

    public override async Task Static_Equals(bool async)
    {
        await base.Static_Equals(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" = 'Seattle'
""");
    }

    public override async Task Static_Equals_with_OrdinalIgnoreCase(bool async)
    {
        await base.Static_Equals_with_OrdinalIgnoreCase(async);

        AssertSql();
    }

    public override async Task Static_Equals_with_Ordinal(bool async)
    {
        await base.Static_Equals_with_Ordinal(async);

        AssertSql();
    }

    #endregion Equals

    #region Miscellaneous

    public override async Task Length(bool async)
    {
        await base.Length(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int = 7
""");
    }

    public override async Task ToUpper(bool async)
    {
        await base.ToUpper(async);

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

    public override async Task ToLower(bool async)
    {
        await base.ToLower(async);

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

    public override async Task IndexOf(bool async)
    {
        await base.IndexOf(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."String", 'eattl') - 1 <> -1
""");
    }

    // TODO: #3547
    public override Task IndexOf_Char(bool async)
        => Assert.ThrowsAsync<InvalidCastException>(() => base.IndexOf_Char(async));

    public override async Task IndexOf_with_empty_string(bool async)
    {
        await base.IndexOf_with_empty_string(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."String", '') - 1 = 0
""");
    }

    public override async Task IndexOf_with_one_parameter_arg(bool async)
    {
        await base.IndexOf_with_one_parameter_arg(async);

        AssertSql(
            """
@pattern='eattl'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."String", @pattern) - 1 = 1
""");
    }

    public override async Task IndexOf_with_one_parameter_arg_char(bool async)
    {
        await base.IndexOf_with_one_parameter_arg_char(async);

        AssertSql(
            """
@pattern='e' (DbType = String)

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."String", @pattern) - 1 = 1
""");
    }

    // PostgreSQL does not have strpos with starting position
    public override Task IndexOf_with_constant_starting_position(bool async)
        => AssertTranslationFailed(() => base.IndexOf_with_constant_starting_position(async));

    // PostgreSQL does not have strpos with starting position
    public override Task IndexOf_with_constant_starting_position_char(bool async)
        => AssertTranslationFailed(() => base.IndexOf_with_constant_starting_position_char(async));

    // PostgreSQL does not have strpos with starting position
    public override Task IndexOf_with_parameter_starting_position(bool async)
        => AssertTranslationFailed(() => base.IndexOf_with_parameter_starting_position(async));

    // PostgreSQL does not have strpos with starting position
    public override Task IndexOf_with_parameter_starting_position_char(bool async)
        => AssertTranslationFailed(() => base.IndexOf_with_parameter_starting_position_char(async));

    public override async Task IndexOf_after_ToString(bool async)
    {
        await base.IndexOf_after_ToString(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos(b."Int"::text, '55') - 1 = 1
""");
    }

    public override async Task IndexOf_over_ToString(bool async)
    {
        await base.IndexOf_over_ToString(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE strpos('12559', b."Int"::text) - 1 = 1
""");
    }

    #endregion IndexOf

    #region Replace

    public override async Task Replace(bool async)
    {
        await base.Replace(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE replace(b."String", 'Sea', 'Rea') = 'Reattle'
""");
    }

    // TODO: #3547
    public override Task Replace_Char(bool async)
        => AssertTranslationFailed(() => base.Replace_Char(async));

    public override async Task Replace_with_empty_string(bool async)
    {
        await base.Replace_with_empty_string(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <> '' AND replace(b."String", b."String", '') = ''
""");
    }

    public override async Task Replace_using_property_arguments(bool async)
    {
        await base.Replace_using_property_arguments(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" <> '' AND replace(b."String", b."String", b."Int"::text) = b."Int"::text
""");
    }

    #endregion Replace

    #region Substring

    public override async Task Substring(bool async)
    {
        await base.Substring(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 3 AND substring(b."String", 2, 2) = 'ea'
""");
    }

    public override async Task Substring_with_one_arg_with_zero_startIndex(bool async)
    {
        await base.Substring_with_one_arg_with_zero_startIndex(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE substring(b."String", 1) = 'Seattle'
""");
    }

    public override async Task Substring_with_one_arg_with_constant(bool async)
    {
        await base.Substring_with_one_arg_with_constant(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 1 AND substring(b."String", 2) = 'eattle'
""");
    }

    public override async Task Substring_with_one_arg_with_parameter(bool async)
    {
        await base.Substring_with_one_arg_with_parameter(async);

        AssertSql(
            """
@start='2'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 2 AND substring(b."String", @start + 1) = 'attle'
""");
    }

    public override async Task Substring_with_two_args_with_zero_startIndex(bool async)
    {
        await base.Substring_with_two_args_with_zero_startIndex(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 3 AND substring(b."String", 1, 3) = 'Sea'
""");
    }

    public override async Task Substring_with_two_args_with_zero_length(bool async)
    {
        await base.Substring_with_two_args_with_zero_length(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 2 AND substring(b."String", 3, 0) = ''
""");
    }

    public override async Task Substring_with_two_args_with_parameter(bool async)
    {
        await base.Substring_with_two_args_with_parameter(async);

        AssertSql(
            """
@start='2'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE length(b."String")::int >= 5 AND substring(b."String", @start + 1, 3) = 'att'
""");
    }

    public override async Task Substring_with_two_args_with_IndexOf(bool async)
    {
        await base.Substring_with_two_args_with_IndexOf(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE '%a%' AND substring(b."String", (strpos(b."String", 'a') - 1) + 1, 3) = 'att'
""");
    }

    #endregion Substring

    #region IsNullOrEmpty/Whitespace

    public override async Task IsNullOrEmpty(bool async)
    {
        await base.IsNullOrEmpty(async);

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

    public override async Task IsNullOrEmpty_negated(bool async)
    {
        await base.IsNullOrEmpty_negated(async);

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

    public override async Task IsNullOrWhiteSpace(bool async)
    {
        await base.IsNullOrWhiteSpace(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE btrim(b."String", E' \t\n\r') = ''
""");
    }

    #endregion IsNullOrEmpty/Whitespace

    #region StartsWith

    public override async Task StartsWith_Literal(bool async)
    {
        await base.StartsWith_Literal(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE 'Se%'
""");
    }

    // TODO: #3547
    public override Task StartsWith_Literal_Char(bool async)
        => AssertTranslationFailed(() => base.StartsWith_Literal_Char(async));

    public override async Task StartsWith_Parameter(bool async)
    {
        await base.StartsWith_Parameter(async);

        AssertSql(
            """
@pattern_startswith='Se%'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE @pattern_startswith
""");
    }

    public override Task StartsWith_Parameter_Char(bool async)
        => AssertTranslationFailed(() => base.StartsWith_Parameter_Char(async));

    public override async Task StartsWith_Column(bool async)
    {
        await base.StartsWith_Column(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE left(b."String", length(b."String")) = b."String"
""");
    }

    public override async Task StartsWith_with_StringComparison_Ordinal(bool async)
    {
        await base.StartsWith_with_StringComparison_Ordinal(async);

        AssertSql();
    }

    public override async Task StartsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
    {
        await base.StartsWith_with_StringComparison_OrdinalIgnoreCase(async);

        AssertSql();
    }

    public override async Task StartsWith_with_StringComparison_unsupported(bool async)
    {
        await base.StartsWith_with_StringComparison_unsupported(async);

        AssertSql();
    }

    #endregion StartsWith

    #region EndsWith

    public override async Task EndsWith_Literal(bool async)
    {
        await base.EndsWith_Literal(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE '%le'
""");
    }

    // TODO: #3547
    public override Task EndsWith_Literal_Char(bool async)
        => AssertTranslationFailed(() => base.EndsWith_Literal_Char(async));

    public override async Task EndsWith_Parameter(bool async)
    {
        await base.EndsWith_Parameter(async);

        AssertSql(
            """
@pattern_endswith='%le'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE @pattern_endswith
""");
    }

    // TODO: #3547
    public override Task EndsWith_Parameter_Char(bool async)
        => AssertTranslationFailed(() => base.EndsWith_Parameter_Char(async));

    public override async Task EndsWith_Column(bool async)
    {
        // SQL Server trims trailing whitespace for length calculations, making our EndsWith() column translation not work reliably in that
        // case
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.String == "Seattle" && b.String.EndsWith(b.String)));

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" = 'Seattle' AND right(b."String", length(b."String")) = b."String"
""");
    }

    public override async Task EndsWith_with_StringComparison_Ordinal(bool async)
    {
        await base.EndsWith_with_StringComparison_Ordinal(async);

        AssertSql();
    }

    public override async Task EndsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
    {
        await base.EndsWith_with_StringComparison_OrdinalIgnoreCase(async);

        AssertSql();
    }

    public override async Task EndsWith_with_StringComparison_unsupported(bool async)
    {
        await base.EndsWith_with_StringComparison_unsupported(async);

        AssertSql();
    }

    #endregion EndsWith

    #region Contains

    public override async Task Contains_Literal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl")), // SQL Server is case-insensitive by default
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.Contains("eattl", StringComparison.OrdinalIgnoreCase)));

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE '%eattl%'
""");
    }

    // TODO: #3547
    public override Task Contains_Literal_Char(bool async)
        => AssertTranslationFailed(() => base.Contains_Literal_Char(async));

    public override async Task Contains_Column(bool async)
    {
        await base.Contains_Column(async);

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

    public override async Task Contains_negated(bool async)
    {
        await base.Contains_negated(async);

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

    public override async Task Contains_with_StringComparison_Ordinal(bool async)
    {
        await base.Contains_with_StringComparison_Ordinal(async);

        AssertSql();
    }

    public override async Task Contains_with_StringComparison_OrdinalIgnoreCase(bool async)
    {
        await base.Contains_with_StringComparison_OrdinalIgnoreCase(async);

        AssertSql();
    }

    public override async Task Contains_with_StringComparison_unsupported(bool async)
    {
        await base.Contains_with_StringComparison_unsupported(async);

        AssertSql();
    }

    public override async Task Contains_constant_with_whitespace(bool async)
    {
        await base.Contains_constant_with_whitespace(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE '%     %'
""");
    }

    public override async Task Contains_parameter_with_whitespace(bool async)
    {
        await base.Contains_parameter_with_whitespace(async);

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

    public override async Task TrimStart_without_arguments(bool async)
    {
        await base.TrimStart_without_arguments(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE ltrim(b."String", E' \t\n\r') = 'Boston  '
""");
    }

    public override async Task TrimStart_with_char_argument(bool async)
    {
        await base.TrimStart_with_char_argument(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE ltrim(b."String", 'S') = 'eattle'
""");
    }

    public override async Task TrimStart_with_char_array_argument(bool async)
    {
        await base.TrimStart_with_char_array_argument(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE ltrim(b."String", 'Se') = 'attle'
""");
    }

    #endregion TrimStart

    #region TrimEnd

    public override async Task TrimEnd_without_arguments(bool async)
    {
        await base.TrimEnd_without_arguments(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE rtrim(b."String", E' \t\n\r') = '  Boston'
""");
    }

    public override async Task TrimEnd_with_char_argument(bool async)
    {
        await base.TrimEnd_with_char_argument(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE rtrim(b."String", 'e') = 'Seattl'
""");
    }

    public override async Task TrimEnd_with_char_array_argument(bool async)
    {
        await base.TrimEnd_with_char_array_argument(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE rtrim(b."String", 'le') = 'Seatt'
""");
    }

    #endregion TrimEnd

    #region Trim

    public override async Task Trim_without_argument_in_predicate(bool async)
    {
        await base.Trim_without_argument_in_predicate(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE btrim(b."String", E' \t\n\r') = 'Boston'
""");
    }

    public override async Task Trim_with_char_argument_in_predicate(bool async)
    {
        await base.Trim_with_char_argument_in_predicate(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE btrim(b."String", 'S') = 'eattle'
""");
    }

    public override async Task Trim_with_char_array_argument_in_predicate(bool async)
    {
        await base.Trim_with_char_array_argument_in_predicate(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE btrim(b."String", 'Se') = 'attl'
""");
    }

    #endregion Trim

    #region Compare

    public override async Task Compare_simple_zero(bool async)
    {
        await base.Compare_simple_zero(async);

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

    public override async Task Compare_simple_one(bool async)
    {
        await base.Compare_simple_one(async);

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

    public override async Task Compare_with_parameter(bool async)
    {
        await base.Compare_with_parameter(async);

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

    public override async Task Compare_simple_more_than_one(bool async)
    {
        await base.Compare_simple_more_than_one(async);

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

    public override async Task Compare_nested(bool async)
    {
        await base.Compare_nested(async);

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

    public override async Task Compare_multi_predicate(bool async)
    {
        await base.Compare_multi_predicate(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= 'Seattle' AND b."String" < 'Toronto'
""");
    }

    public override async Task CompareTo_simple_zero(bool async)
    {
        await base.CompareTo_simple_zero(async);

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

    public override async Task CompareTo_simple_one(bool async)
    {
        await base.CompareTo_simple_one(async);

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

    public override async Task CompareTo_with_parameter(bool async)
    {
        await base.CompareTo_with_parameter(async);

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

    public override async Task CompareTo_simple_more_than_one(bool async)
    {
        await base.CompareTo_simple_more_than_one(async);

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

    public override async Task CompareTo_nested(bool async)
    {
        await base.CompareTo_nested(async);

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

    public override async Task Compare_to_multi_predicate(bool async)
    {
        await base.Compare_to_multi_predicate(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" >= 'Seattle' AND b."String" < 'Toronto'
""");
    }

    #endregion Compare

    #region Join

    public override async Task Join_over_non_nullable_column(bool async)
    {
        await base.Join_over_non_nullable_column(async);

        AssertSql(
            """
SELECT b."Int" AS "Key", COALESCE(string_agg(b."String", '|'), '') AS "Strings"
FROM "BasicTypesEntities" AS b
GROUP BY b."Int"
""");
    }

    public override async Task Join_over_nullable_column(bool async)
    {
        await base.Join_over_nullable_column(async);

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

    public override async Task Join_with_predicate(bool async)
    {
        await base.Join_with_predicate(async);

        AssertSql(
            """
SELECT b."Int" AS "Key", COALESCE(string_agg(b."String", '|') FILTER (WHERE length(b."String")::int > 6), '') AS "Strings"
FROM "BasicTypesEntities" AS b
GROUP BY b."Int"
""");
    }

    public override async Task Join_with_ordering(bool async)
    {
        await base.Join_with_ordering(async);

        AssertSql(
            """
SELECT b."Int" AS "Key", COALESCE(string_agg(b."String", '|' ORDER BY b."Id" DESC NULLS LAST), '') AS "Strings"
FROM "BasicTypesEntities" AS b
GROUP BY b."Int"
""");
    }

    public override async Task Join_non_aggregate(bool async)
    {
        await base.Join_non_aggregate(async);

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

    public override async Task Concat_operator(bool async)
    {
        await base.Concat_operator(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" || 'Boston' = 'SeattleBoston'
""");
    }

    public override async Task Concat_aggregate(bool async)
    {
        await base.Concat_aggregate(async);

        AssertSql(
            """
SELECT b."Int" AS "Key", COALESCE(string_agg(b."String", ''), '') AS "BasicTypesEntitys"
FROM "BasicTypesEntities" AS b
GROUP BY b."Int"
""");
    }

    public override async Task Concat_string_int_comparison1(bool async)
    {
        await base.Concat_string_int_comparison1(async);

        AssertSql(
            """
@i='10'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" || @i::text = 'Seattle10'
""");
    }

    public override async Task Concat_string_int_comparison2(bool async)
    {
        await base.Concat_string_int_comparison2(async);

        AssertSql(
            """
@i='10'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @i::text || b."String" = '10Seattle'
""");
    }

    public override async Task Concat_string_int_comparison3(bool async)
    {
        await base.Concat_string_int_comparison3(async);

        AssertSql(
            """
@p='30'
@j='21'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @p::text || b."String" || @j::text || 42::text = '30Seattle2142'
""");
    }

    public override async Task Concat_string_int_comparison4(bool async)
    {
        await base.Concat_string_int_comparison4(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int"::text || b."String" = '8Seattle'
""");
    }

    public override async Task Concat_string_string_comparison(bool async)
    {
        await base.Concat_string_string_comparison(async);

        AssertSql(
            """
@i='A'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @i || b."String" = 'ASeattle'
""");
    }

    public override async Task Concat_method_comparison(bool async)
    {
        await base.Concat_method_comparison(async);

        AssertSql(
            """
@i='A'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @i || b."String" = 'ASeattle'
""");
    }

    public override async Task Concat_method_comparison_2(bool async)
    {
        await base.Concat_method_comparison_2(async);

        AssertSql(
            """
@i='A'
@j='B'

SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE @i || @j || b."String" = 'ABSeattle'
""");
    }

    public override async Task Concat_method_comparison_3(bool async)
    {
        await base.Concat_method_comparison_3(async);

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

    public override async Task FirstOrDefault(bool async)
    {
        await base.FirstOrDefault(async);
        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE substr(b."String", 1, 1) = 'S'
""");
    }

    public override async Task LastOrDefault(bool async)
    {
        await base.LastOrDefault(async);
        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE substr(b."String", length(b."String"), 1) = 'e'
""");
    }

    #endregion LINQ Operators

    #region Like

    public override async Task Where_Like_and_comparison(bool async)
    {
        await base.Where_Like_and_comparison(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE 'S%' AND b."Int" = 8
""");
    }

    public override async Task Where_Like_or_comparison(bool async)
    {
        await base.Where_Like_or_comparison(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" LIKE 'S%' OR b."Int" = 2147483647
""");
    }

    public override async Task Like_with_non_string_column_using_ToString(bool async)
    {
        await base.Like_with_non_string_column_using_ToString(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int"::text LIKE '%5%'
""");
    }

    public override async Task Like_with_non_string_column_using_double_cast(bool async)
    {
        await base.Like_with_non_string_column_using_double_cast(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."Int"::text LIKE '%5%'
""");
    }

    #endregion Like

    #region Regex

    public override async Task Regex_IsMatch(bool async)
    {
        await base.Regex_IsMatch(async);

        AssertSql(
            """
SELECT b."Id", b."Bool", b."Byte", b."ByteArray", b."DateOnly", b."DateTime", b."DateTimeOffset", b."Decimal", b."Double", b."Enum", b."FlagsEnum", b."Float", b."Guid", b."Int", b."Long", b."Short", b."String", b."TimeOnly", b."TimeSpan"
FROM "BasicTypesEntities" AS b
WHERE b."String" ~ '(?p)^S'
""");
    }

    public override async Task Regex_IsMatch_constant_input(bool async)
    {
        await base.Regex_IsMatch_constant_input(async);

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
