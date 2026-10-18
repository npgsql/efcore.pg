using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     Applies mapping-specific conversions when extracting JSON scalars.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release.
/// </remarks>
public class NpgsqlJsonScalarConversionPostprocessor : ExpressionVisitor
{
    private readonly Dictionary<(string TableAlias, string ColumnName), SqlExpression> _recordsetConversions = [];
    private readonly HashSet<string> _jsonArrayAliases = [];

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression expression)
    {
        switch (expression)
        {
            case ShapedQueryExpression shapedQuery:
                return shapedQuery
                    .UpdateQueryExpression(Visit(shapedQuery.QueryExpression))
                    .UpdateShaperExpression(Visit(shapedQuery.ShaperExpression));

            case SelectExpression select:
                foreach (var table in select.Tables.Select(t => t.UnwrapJoin()))
                {
                    switch (table)
                    {
                        case PgTableValuedFunctionExpression
                        {
                            Name: "json_to_recordset" or "jsonb_to_recordset", ColumnInfos: { } columns
                        } recordset when columns.Any(c => c.TypeMapping is INpgsqlJsonScalarTypeMapping):
                            foreach (var column in columns)
                            {
                                if (column.TypeMapping is INpgsqlJsonScalarTypeMapping mapping)
                                {
                                    _recordsetConversions[(recordset.Alias, column.Name)] = mapping.ConvertFromJsonText(
                                        new ColumnExpression(
                                            column.Name, recordset.Alias, typeof(string), NpgsqlStringTypeMapping.Default, nullable: true));
                                }
                            }

                            break;

                        case TableValuedFunctionExpression { Name: "json_array_elements_text" or "jsonb_array_elements_text" } array:
                            _jsonArrayAliases.Add(array.Alias);
                            break;
                    }
                }

                return base.VisitExtension(select);

            case PgTableValuedFunctionExpression
            {
                Name: "json_to_recordset" or "jsonb_to_recordset", ColumnInfos: { } columns
            } recordset:
                return base.VisitExtension(
                    recordset.WithColumnInfos(
                        columns.Select(
                            c => c.TypeMapping is INpgsqlJsonScalarTypeMapping
                                ? c with { TypeMapping = NpgsqlStringTypeMapping.Default }
                                : c).ToArray()));

            case ColumnExpression column when _recordsetConversions.TryGetValue((column.TableAlias, column.Name), out var conversion):
                return conversion;

            case JsonScalarExpression { Path.Count: > 0, TypeMapping: INpgsqlJsonScalarTypeMapping mapping } scalar:
            {
                var visited = (JsonScalarExpression)base.VisitExtension(scalar);
                return mapping.ConvertFromJsonText(
                    new JsonScalarExpression(
                        visited.Json, visited.Path, typeof(string), NpgsqlStringTypeMapping.Default, visited.IsNullable));
            }

            case SqlUnaryExpression
            {
                OperatorType: ExpressionType.Convert,
                Operand: ColumnExpression column,
                TypeMapping: INpgsqlJsonScalarTypeMapping mapping
            } when _jsonArrayAliases.Contains(column.TableAlias):
                return mapping.ConvertFromJsonText(column);

            default:
                return base.VisitExtension(expression);
        }
    }
}
