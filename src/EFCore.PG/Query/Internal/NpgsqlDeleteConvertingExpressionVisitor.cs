using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     Converts the relational <see cref="NonQueryExpression" /> into a PG-specific <see cref="PostgresDeleteExpression" />, which
///     precisely models a DELETE statement in PostgreSQL. This is done to handle the PG-specific USING syntax for table joining.
/// </summary>
public class NpgsqlDeleteConvertingExpressionVisitor : ExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Process(Expression node)
        => node switch
        {
            DeleteExpression deleteExpression => VisitDelete(deleteExpression),

            _ => node
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression VisitDelete(DeleteExpression deleteExpression)
    {
        var selectExpression = deleteExpression.SelectExpression;

        if (selectExpression.Offset != null
            || selectExpression.Limit != null
            || selectExpression.Having != null
            || selectExpression.Orderings.Count > 0
            || selectExpression.GroupBy.Count > 0
            || selectExpression.Projection.Count > 0)
        {
            throw new InvalidOperationException(
                RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                    nameof(RelationalQueryableExtensions.ExecuteDelete)));
        }

        var fromItems = new List<TableExpressionBase>();
        SqlExpression? joinPredicates = null;

        // The SelectExpression also contains the target table being modified (same as deleteExpression.Table).
        // If it has additional inner joins, use the PostgreSQL-specific USING syntax to express the join.
        // Note that the non-join TableExpression isn't necessary the target table - through projection the last table being
        // joined may be the one being modified.
        foreach (var tableBase in selectExpression.Tables)
        {
            switch (tableBase)
            {
                case TableExpression tableExpression:
                    if (tableExpression != deleteExpression.Table)
                    {
                        fromItems.Add(tableExpression);
                    }

                    break;

                case InnerJoinExpression { Table: { } tableExpression } innerJoinExpression:
                    if (tableExpression != deleteExpression.Table)
                    {
                        fromItems.Add(tableExpression);
                    }

                    joinPredicates = joinPredicates is null
                        ? innerJoinExpression.JoinPredicate
                        : new SqlBinaryExpression(
                            ExpressionType.AndAlso, joinPredicates, innerJoinExpression.JoinPredicate, typeof(bool),
                            innerJoinExpression.JoinPredicate.TypeMapping);
                    break;

                default:
                    throw new InvalidOperationException(
                        RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                            nameof(RelationalQueryableExtensions.ExecuteDelete)));
            }
        }

        // Combine the join predicates (if any) before the user-provided predicate
        var predicate = (joinPredicates, selectExpression.Predicate) switch
        {
            (null, not null) => selectExpression.Predicate,
            (not null, null) => joinPredicates,
            (null, null) => null,
            (not null, not null) => new SqlBinaryExpression(
                ExpressionType.AndAlso, joinPredicates, selectExpression.Predicate, typeof(bool), joinPredicates.TypeMapping)
        };

        return new PostgresDeleteExpression(deleteExpression.Table, fromItems, predicate, deleteExpression.Tags);
    }
}
