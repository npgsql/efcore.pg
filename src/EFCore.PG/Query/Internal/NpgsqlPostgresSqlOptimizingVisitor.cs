using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
/// Performs various PostgreSQL-specific optimizations to the SQL expression tree.
/// </summary>
public class NpgsqlPostgresSqlOptimizingVisitor : ExpressionVisitor
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public NpgsqlPostgresSqlOptimizingVisitor(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    [return: NotNullIfNotNull("expression")]
    public override Expression? Visit(Expression? expression)
    {
        switch (expression)
        {
            case ShapedQueryExpression shapedQueryExpression:
                return shapedQueryExpression.Update(
                    Visit(shapedQueryExpression.QueryExpression),
                    shapedQueryExpression.ShaperExpression);

            case SqlBinaryExpression { OperatorType: ExpressionType.OrElse } sqlBinaryExpression:
            {
                var visited = (SqlBinaryExpression)base.Visit(sqlBinaryExpression);

                var x = TryConvertToRowValueComparison(visited, out var rowComparisonExpression)
                    ? rowComparisonExpression
                    : visited;

                return x;
            }

            default:
                return base.Visit(expression);
        }
    }

    private bool TryConvertToRowValueComparison(
        SqlBinaryExpression sqlOrExpression,
        [NotNullWhen(true)] out SqlBinaryExpression? rowComparisonExpression)
    {
        // This pattern matches x > 5 || x == 5 && y > 6, and converts it to (x, y) > (5, 6)
        Debug.Assert(sqlOrExpression.OperatorType == ExpressionType.OrElse);

        if (TryMatchTopExpression(sqlOrExpression.Left, sqlOrExpression.Right, out rowComparisonExpression)
            || TryMatchTopExpression(sqlOrExpression.Right, sqlOrExpression.Left, out rowComparisonExpression))
        {
            return true;
        }

        rowComparisonExpression = null;
        return false;

        bool TryMatchTopExpression(
            SqlExpression first,
            SqlExpression second,
            [NotNullWhen(true)] out SqlBinaryExpression? rowComparisonExpression)
        {
            if (first is SqlBinaryExpression { OperatorType: ExpressionType.AndAlso } subExpression
                && second is SqlBinaryExpression comparisonExpression1
                && IsComparisonOperator(comparisonExpression1.OperatorType))
            {
                if (TryMatchBottomExpression(subExpression.Left, subExpression.Right, out var equalityExpression, out var comparisonExpression2)
                    || TryMatchBottomExpression(subExpression.Right, subExpression.Left, out equalityExpression, out comparisonExpression2))
                {
                    // We've found a structural match. Now make sure the operands and operators correspond
                    if (comparisonExpression1.Left.Equals(equalityExpression.Left)
                        && comparisonExpression1.Right.Equals(equalityExpression.Right)
                        && comparisonExpression1.OperatorType == comparisonExpression2.OperatorType)
                    {
                        // Bingo.

                        // If we're composing over an existing row value comparison, just prepend the new element to it.
                        // Otherwise create a new row value comparison expression.
                        if (comparisonExpression2.Left is PostgresRowValueExpression leftRowValueExpression)
                        {
                            var rightRowValueExpression = (PostgresRowValueExpression)comparisonExpression2.Right;

                            rowComparisonExpression = _sqlExpressionFactory.MakeBinary(
                                comparisonExpression1.OperatorType,
                                leftRowValueExpression.Prepend(comparisonExpression1.Left),
                                rightRowValueExpression.Prepend(comparisonExpression1.Right),
                                typeMapping: null)!;
                        }
                        else
                        {
                            rowComparisonExpression = _sqlExpressionFactory.MakeBinary(
                                comparisonExpression1.OperatorType,
                                new PostgresRowValueExpression(new[] { comparisonExpression1.Left, comparisonExpression2.Left }),
                                new PostgresRowValueExpression(new[] { comparisonExpression1.Right, comparisonExpression2.Right }),
                                typeMapping: null)!;
                        }

                        return true;
                    }
                }
            }

            rowComparisonExpression = null;
            return false;

            static bool TryMatchBottomExpression(
                SqlExpression first,
                SqlExpression second,
                [NotNullWhen(true)] out SqlBinaryExpression? equalityExpression,
                [NotNullWhen(true)] out SqlBinaryExpression? comparisonExpression)
            {
                // This pattern matches the bottom expression of the pattern: x == 5 && y > 6

                if (first is SqlBinaryExpression { OperatorType: ExpressionType.Equal } equalityExpression2
                    && second is SqlBinaryExpression comparisonExpression2
                    && IsComparisonOperator(comparisonExpression2.OperatorType))
                {
                    equalityExpression = equalityExpression2;
                    comparisonExpression = comparisonExpression2;
                    return true;
                }

                equalityExpression = comparisonExpression = null;
                return false;
            }
        }
    }

    private static bool IsComparisonOperator(ExpressionType expressionType)
        => expressionType switch
        {
            ExpressionType.GreaterThan => true,
            ExpressionType.GreaterThanOrEqual => true,
            ExpressionType.LessThan => true,
            ExpressionType.LessThanOrEqual => true,
            _ => false
        };
}