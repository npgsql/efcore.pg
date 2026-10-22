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
    private readonly Dictionary<TableExpressionBase, SelectExpression> _jsonRecordsets = new(ReferenceEqualityComparer.Instance);

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
                // them when extracting bytea columns. Find recordsets before visiting their column references.
                RegisterRecordsets(selectExpression);
                var visitedSelect = (SelectExpression)base.VisitExtension(node);
                // Visiting a select can replace its tables; register those too for references in the shaper.
                RegisterRecordsets(visitedSelect);
                return visitedSelect;

            case ColumnExpression { TypeMapping: NpgsqlByteArrayTypeMapping } column
                when _jsonRecordsets.TryGetValue(column.Table, out var select):
                // Extract the base64 string as text and decode it, preserving the original binary result mapping.
                // Rewrite every reference, not just projections, so predicates and other server-side operations use decoded bytes.
                return sqlExpressionFactory.Function(
                    "decode",
                    [
                        select.CreateColumnExpression(
                            column.Table, column.Name, typeof(string), NpgsqlStringTypeMapping.Default, column.IsNullable),
                        sqlExpressionFactory.Constant("base64")
                    ],
                    nullable: true,
                    argumentsPropagateNullability: [true, false],
                    column.Type,
                    column.TypeMapping);

            case PgTableValuedFunctionExpression
                {
                    Name: "json_to_recordset" or "jsonb_to_recordset",
                    ColumnInfos: { } columnInfos
                } tvf when columnInfos.Any(c => c.TypeMapping is NpgsqlByteArrayTypeMapping):
                // Change the recordset's column declarations to match the text inputs used by decode above.
                return new PgTableValuedFunctionExpression(
                    tvf.Alias,
                    tvf.Name,
                    this.VisitAndConvert(tvf.Arguments),
                    columnInfos.Select(
                        c => c.TypeMapping is NpgsqlByteArrayTypeMapping
                            ? c with { TypeMapping = NpgsqlStringTypeMapping.Default }
                            : c).ToArray(),
                    tvf.WithOrdinality);

            default:
                return base.VisitExtension(node);
        }
    }

    private void RegisterRecordsets(SelectExpression selectExpression)
    {
        foreach (var table in selectExpression.Tables)
        {
            if (table.UnwrapJoin() is PgTableValuedFunctionExpression { Name: "json_to_recordset" or "jsonb_to_recordset" })
            {
                // Use table identity to distinguish query scopes and retain the owning select to create EF8 text columns,
                // including correlated references.
                _jsonRecordsets[table] = selectExpression;
            }
        }
    }
}
