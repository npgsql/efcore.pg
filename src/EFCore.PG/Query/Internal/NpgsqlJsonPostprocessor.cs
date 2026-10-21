using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     Applies base64 decoding to binary properties extracted by json_to_recordset/jsonb_to_recordset.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class NpgsqlJsonPostprocessor(ISqlExpressionFactory sqlExpressionFactory) : ExpressionVisitor
{
    private readonly HashSet<string> _jsonRecordsetAliases = [];

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression node)
    {
        switch (node)
        {
            case ShapedQueryExpression shapedQueryExpression:
                return shapedQueryExpression.Update(
                    Visit(shapedQueryExpression.QueryExpression),
                    Visit(shapedQueryExpression.ShaperExpression));

            case SelectExpression selectExpression:
                foreach (var table in selectExpression.Tables)
                {
                    if (table.UnwrapJoin() is PgTableValuedFunctionExpression
                        {
                            Name: "json_to_recordset" or "jsonb_to_recordset",
                            ColumnInfos: { } columnInfos
                        } tvf
                        && columnInfos.Any(c => c.TypeMapping is NpgsqlByteArrayTypeMapping))
                    {
                        _jsonRecordsetAliases.Add(tvf.Alias);
                    }
                }

                return base.VisitExtension(node);

            case ColumnExpression { TypeMapping: NpgsqlByteArrayTypeMapping } column
                when _jsonRecordsetAliases.Contains(column.TableAlias):
                return sqlExpressionFactory.Function(
                    "decode",
                    [
                        new ColumnExpression(
                            column.Name, column.TableAlias, typeof(string), NpgsqlStringTypeMapping.Default, column.IsNullable),
                        sqlExpressionFactory.Constant("base64")
                    ],
                    nullable: true,
                    argumentsPropagateNullability: [true, false],
                    column.Type,
                    column.TypeMapping);

            case PgTableValuedFunctionExpression { ColumnInfos: { } columnInfos } tvf
                when _jsonRecordsetAliases.Contains(tvf.Alias):
                return tvf.Update(this.Visit(tvf.Arguments)).WithColumnInfos(
                    columnInfos.Select(
                        c => c.TypeMapping is NpgsqlByteArrayTypeMapping
                            ? c with { TypeMapping = NpgsqlStringTypeMapping.Default }
                            : c).ToArray());

            default:
                return base.VisitExtension(node);
        }
    }
}
