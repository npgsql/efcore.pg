using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class PgRowValueExpressionTest
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Rewriting_preserves_type_mapping(bool useVisitor, bool changeValue)
    {
        var value = new SqlConstantExpression(1, IntTypeMapping.Default);
        var replacement = changeValue ? new SqlConstantExpression(2, IntTypeMapping.Default) : value;
        var typeMapping = new NpgsqlRowValueTypeMapping(typeof(ValueTuple<int>));
        var row = new PgRowValueExpression([value], typeof(ValueTuple<int>), typeMapping);

        var rewritten = useVisitor
            ? (PgRowValueExpression)ReplacingExpressionVisitor.Replace(value, replacement, row)
            : row.Update([replacement]);

        Assert.Same(typeMapping, rewritten.TypeMapping);
        Assert.Same(replacement, Assert.Single(rewritten.Values));
        Assert.Equal(row.Type, rewritten.Type);
        if (!changeValue)
        {
            Assert.Same(row, rewritten);
        }
    }
}
