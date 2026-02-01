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
            // We can only apply the indexing if the array is ordered by its natural ordered, i.e. by the "ordinality" column that
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
    ///     If the given <paramref name="source" /> wraps a JSON-array-returning expression without any additional clauses (e.g. filter,
    ///     ordering...), returns that expression.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static bool TryExtractJsonArray(
        this ShapedQueryExpression source,
        [NotNullWhen(true)] out SqlExpression? jsonArray,
        [NotNullWhen(true)] out SqlExpression? projectedElement,
        out bool isElementNullable,
        bool ignoreOrderings = false,
        bool ignorePredicate = false)
    {
        if (source.QueryExpression is SelectExpression
            {
                Tables:
                [
                    TableValuedFunctionExpression
                {
                    Name: "jsonb_array_elements_text" or "json_array_elements_text",
                    Arguments: [var json]
                } tvf
                ],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            } select
            && (ignorePredicate || select.Predicate is null)
            // We can only apply the indexing if the array is ordered by its natural ordered, i.e. by the "ordinality" column that
            // we created in TranslatePrimitiveCollection. For example, if another ordering has been applied (e.g. by the array elements
            // themselves), we can no longer simply index into the original array.
            && (ignoreOrderings
                || select.Orderings is []
                || (select.Orderings is [{ Expression: ColumnExpression { Name: "ordinality", TableAlias: var orderingTableAlias } }]
                    && orderingTableAlias == tvf.Alias))
            && TryGetScalarProjection(source, out var projectedScalar))
        {
            jsonArray = json;

            // The projected ColumnExpression is wrapped in a Convert to apply the element type mapping - unless it happens to be text.
            switch (projectedScalar)
            {
                case SqlUnaryExpression
                {
                    OperatorType: ExpressionType.Convert,
                    Operand: ColumnExpression { IsNullable: var isNullable }
                } convert:
                    projectedElement = convert;
                    isElementNullable = isNullable;
                    return true;
                case ColumnExpression { IsNullable: var isNullable } column:
                    projectedElement = column;
                    isElementNullable = isNullable;
                    return true;
                default:
                    throw new UnreachableException();
            }
        }

        jsonArray = null;
        projectedElement = null;
        isElementNullable = false;
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
    public static bool TryConvertToArray(
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
            { TypeMapping: NpgsqlJsonTypeMapping } => false,
            { Type: var type } when type.IsMultirange() => false,
            _ => true
        };

    private static bool TryGetProjectedColumn(
        ShapedQueryExpression shapedQueryExpression,
        [NotNullWhen(true)] out ColumnExpression? projectedColumn)
    {
        if (TryGetScalarProjection(shapedQueryExpression, out var scalar) && scalar is ColumnExpression column)
        {
            projectedColumn = column;
            return true;
        }

        projectedColumn = null;
        return false;
    }

    private static bool TryGetScalarProjection(
        ShapedQueryExpression shapedQueryExpression,
        [NotNullWhen(true)] out SqlExpression? projectedScalar)
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
            && selectExpression.GetProjection(projectionBindingExpression) is SqlExpression scalar)
        {
            projectedScalar = scalar;
            return true;
        }

        projectedScalar = null;
        return false;
    }
}
