using System.Diagnostics.CodeAnalysis;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Extensions.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class NpgsqlShapedQueryExpressionExtensions
{
    /// <summary>
    ///     If the given <paramref name="source" /> wraps an array-returning expression without any additional clauses (e.g. filter,
    ///     ordering...), returns that expression.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static bool TryExtractArray(
        this ShapedQueryExpression source,
        [NotNullWhen(true)] out SqlExpression? array,
        bool ignoreOrderings = false,
        bool ignorePredicate = false)
        => TryExtractArray(source, out array, out _, ignoreOrderings, ignorePredicate);

    /// <summary>
    ///     If the given <paramref name="source" /> wraps an array-returning expression without any additional clauses (e.g. filter,
    ///     ordering...), returns that expression.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static bool TryExtractArray(
        this ShapedQueryExpression source,
        [NotNullWhen(true)] out SqlExpression? array,
        [NotNullWhen(true)] out ColumnExpression? projectedColumn,
        bool ignoreOrderings = false,
        bool ignorePredicate = false)
    {
        if (source.QueryExpression is SelectExpression
            {
                Tables: [PgUnnestExpression { Array: var a } unnest],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            } select
            && (ignorePredicate || select.Predicate is null)
            // We can only apply the indexing if the JSON array is ordered by its natural ordered, i.e. by the "ordinality" column that
            // we created in TranslatePrimitiveCollection. For example, if another ordering has been applied (e.g. by the array elements
            // themselves), we can no longer simply index into the original array.
            && (ignoreOrderings
                || select.Orderings is []
                || (select.Orderings is [{ Expression: ColumnExpression { Name: "ordinality", TableAlias: var orderingTableAlias } }]
                    && orderingTableAlias == unnest.Alias))
            && IsPostgresArray(a)
            && TryGetProjectedColumn(source, out var column))
        {
            array = a;
            projectedColumn = column;
            return true;
        }

        array = null;
        projectedColumn = null;
        return false;
    }

    /// <summary>
    ///     If the given <paramref name="source" /> wraps a <see cref="ValuesExpression" /> without any additional clauses (e.g. filter,
    ///     ordering...), converts that to a <see cref="NewArrayExpression" /> and returns that.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static bool TryConvertValuesToArray(
        this ShapedQueryExpression source,
        [NotNullWhen(true)] out SqlExpression? array,
        bool ignoreOrderings = false,
        bool ignorePredicate = false)
    {
        if (source.QueryExpression is SelectExpression
            {
                Tables: [ValuesExpression { ColumnNames: ["_ord", "Value"], RowValues.Count: > 0 } valuesExpression],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            } select
            && (ignorePredicate || select.Predicate is null)
            && (ignoreOrderings || select.Orderings is []))
        {
            var elements = new SqlExpression[valuesExpression.RowValues.Count];

            for (var i = 0; i < elements.Length; i++)
            {
                // Skip the first column (_ord) and copy the second (Value)
                elements[i] = valuesExpression.RowValues[i].Values[1];
            }

            array = new PgNewArrayExpression(elements, valuesExpression.RowValues[0].Values[1].Type.MakeArrayType(), typeMapping: null);
            return true;
        }

        array = null;
        return false;
    }

    /// <summary>
    ///     Checks whether the given expression maps to a PostgreSQL array, as opposed to a multirange type.
    /// </summary>
    private static bool IsPostgresArray(SqlExpression expression)
        => expression switch
        {
            { TypeMapping: NpgsqlArrayTypeMapping } => true,
            { TypeMapping: NpgsqlMultirangeTypeMapping } => false,
            { Type: var type } when type.IsMultirange() => false,
            _ => true
        };

    private static bool TryGetProjectedColumn(
        ShapedQueryExpression shapedQueryExpression,
        [NotNullWhen(true)] out ColumnExpression? projectedColumn)
    {
        var shaperExpression = shapedQueryExpression.ShaperExpression;
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            && unaryExpression.Operand.Type.IsNullableType()
            && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
        {
            shaperExpression = unaryExpression.Operand;
        }

        if (shaperExpression is ProjectionBindingExpression projectionBindingExpression
            && shapedQueryExpression.QueryExpression is SelectExpression selectExpression
            && selectExpression.GetProjection(projectionBindingExpression) is ColumnExpression c)
        {
            projectedColumn = c;
            return true;
        }

        projectedColumn = null;
        return false;
    }
}
