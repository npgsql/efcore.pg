using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     Applies JSON-related post-processing transformations.
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
                // Binary values are stored as base64 strings in JSON, but json_to_recordset/jsonb_to_recordset do not decode
                // them when extracting bytea columns. Find affected recordsets before visiting their column references.
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

                // EF assigns query-wide unique aliases, so keep these registered for correlated references and update setters.
                return base.VisitExtension(node);

            case ColumnExpression { TypeMapping: NpgsqlByteArrayTypeMapping } column
                when _jsonRecordsetAliases.Contains(column.TableAlias):
                // Extract the base64 string as text and decode it, preserving the original binary result mapping.
                // Rewrite every reference, not just projections, so predicates and other server-side operations use decoded bytes.
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
                // Change the recordset's column declarations to match the text inputs used by decode above.
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
