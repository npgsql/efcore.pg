using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage;

public class NpgsqlSqlGenerationHelperTest
{
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

    private ISqlGenerationHelper CreateSqlGenerationHelper()
        => new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
}
