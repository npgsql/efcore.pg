using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage;

public class NpgsqlSqlGenerationHelperTest
{
    [Theory]
    [InlineData("name", null, "name")]
    [InlineData("app.name", "app", "name")]
    [InlineData("\"app\".\"name\"", "app", "name")]
    [InlineData("\"app.schema\".\"dotted.name\"", "app.schema", "dotted.name")]
    [InlineData("\"app\"\"schema\".\"quoted\"\"name\"", "app\"schema", "quoted\"name")]
    [InlineData("\"dotted.name\"", null, "dotted.name")]
    public void Parse_schema_qualified_name(string identifier, string schema, string name)
        => Assert.Equal((schema, name), NpgsqlSqlGenerationHelper.ParseSchemaQualifiedName(identifier));

    [Theory]
    [InlineData(null, "dotted.name")]
    [InlineData("app.schema", "dotted.name")]
    [InlineData("app\"schema", "quoted\"name")]
    public void Annotation_names_round_trip(string schema, string name)
    {
        var identifier = schema is null
            ? NpgsqlSqlGenerationHelper.QuoteIfNeeded(name)
            : $"{NpgsqlSqlGenerationHelper.QuoteIfNeeded(schema)}.{NpgsqlSqlGenerationHelper.QuoteIfNeeded(name)}";

        Assert.Equal((schema, name), NpgsqlSqlGenerationHelper.ParseSchemaQualifiedName(identifier));
    }

    [Theory]
    [InlineData(null, "simple", "simple")]
    [InlineData(null, "case.insensitive", "\"case.insensitive\"")]
    [InlineData(null, "embedded\"quote", "\"embedded\"\"quote\"")]
    [InlineData(null, "\"leading", "\"\"\"leading\"")]
    [InlineData(null, "\"quoted\"", "\"\"\"quoted\"\"\"")]
    [InlineData(null, "\"app\".\"name\"", "\"\"\"app\"\".\"\"name\"\"\"")]
    [InlineData("app.schema", "\"leading", "\"app.schema\".\"\"\"leading\"")]
    public void Schema_qualified_names_round_trip(string schema, string name, string expectedReference)
    {
        var reference = NpgsqlSqlGenerationHelper.GetSchemaQualifiedName(schema, name);

        Assert.Equal(expectedReference, reference);
        Assert.Equal((schema, name), NpgsqlSqlGenerationHelper.ResolveCollation(reference, model: null));
        Assert.Equal(
            CreateSqlGenerationHelper().DelimitIdentifier(name, schema),
            CreateSqlGenerationHelper().DelimitCollation(reference));
    }

    [Theory]
    [InlineData("")]
    [InlineData("app..name")]
    [InlineData("app.name.extra")]
    [InlineData("\"unterminated")]
    [InlineData("\"bad\"quote\"\"")]
    public void Malformed_names_are_rejected(string name)
        => Assert.Throws<ArgumentException>(() => NpgsqlSqlGenerationHelper.ParseSchemaQualifiedName(name));

    [Theory]
    [InlineData("case_insensitive", "case_insensitive")]
    [InlineData("en_US.utf8_ci", "\"en_US.utf8_ci\"")]
    [InlineData("\"en_US.utf8_ci\"", "\"en_US.utf8_ci\"")]
    [InlineData("\"app\".\"case_insensitive\"", "app.case_insensitive")]
    [InlineData("\"app.schema\".\"case.insensitive\"", "\"app.schema\".\"case.insensitive\"")]
    [InlineData("\"app\"\"schema\".\"case\"\"insensitive\"", "\"app\"\"schema\".\"case\"\"insensitive\"")]
    public void DelimitCollation_preserves_literal_dots_and_explicit_schemas(string collation, string sql)
        => Assert.Equal(sql, CreateSqlGenerationHelper().DelimitCollation(collation));

    [Theory]
    [InlineData(null, "app.case_insensitive")]
    [InlineData("app", "app.case_insensitive")]
    [InlineData("other", "case_insensitive")]
    public void DelimitCollation_only_resolves_declarations_in_default_schema(string declarationSchema, string sql)
    {
        var model = new ModelBuilder()
            .HasDefaultSchema("app")
            .HasCollation(declarationSchema, "case_insensitive", locale: "POSIX", provider: "libc")
            .Model;

        Assert.Equal(sql, CreateSqlGenerationHelper().DelimitCollation("case_insensitive", (IModel)model));
    }

    [Theory]
    [InlineData("all_lowercase", null, "all_lowercase", "all_lowercase")]
    [InlineData("digit", null, "digit", "digit")]
    [InlineData("under_score", null, "under_score", "under_score")]
    [InlineData("dollar$", null, "dollar$", "dollar$")]
    [InlineData("ALL_CAPS", null, "\"ALL_CAPS\"", "\"ALL_CAPS\"")]
    [InlineData("oneCap", null, "\"oneCap\"", "\"oneCap\"")]
    [InlineData("0starts_with_digit", null, "\"0starts_with_digit\"", "\"0starts_with_digit\"")]
    [InlineData("all_lowercase", "all_lowercase", "all_lowercase.all_lowercase", "all_lowercase")]
    [InlineData("all_lowercase", "oneCap", "\"oneCap\".all_lowercase", "all_lowercase")]
    [InlineData("oneCap", "all_lowercase", "all_lowercase.\"oneCap\"", "\"oneCap\"")]
    [InlineData("CAPS", "CAPS", "\"CAPS\".\"CAPS\"", "\"CAPS\"")]
    [InlineData("select", "null", "\"null\".\"select\"", "\"select\"")]
    public virtual void DelimitIdentifier_quotes_properly(
        string identifier,
        string schema,
        string outputWithSchema,
        string outputWithoutSchema)
    {
        var helper = CreateSqlGenerationHelper();
        Assert.Equal(outputWithoutSchema, helper.DelimitIdentifier(identifier));
        Assert.Equal(outputWithSchema, helper.DelimitIdentifier(identifier, schema));

        var sb = new StringBuilder();
        helper.DelimitIdentifier(sb, identifier);
        Assert.Equal(outputWithoutSchema, sb.ToString());

        sb = new StringBuilder();
        helper.DelimitIdentifier(sb, identifier, schema);
        Assert.Equal(outputWithSchema, sb.ToString());
    }

    private NpgsqlSqlGenerationHelper CreateSqlGenerationHelper()
        => new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
}
