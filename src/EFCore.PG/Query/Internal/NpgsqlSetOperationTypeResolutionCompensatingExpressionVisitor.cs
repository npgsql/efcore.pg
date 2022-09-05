namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlSetOperationTypeResolutionCompensatingExpressionVisitor : ExpressionVisitor
{
    private State _state;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            ShapedQueryExpression shapedQueryExpression
                => shapedQueryExpression.Update(
                    Visit(shapedQueryExpression.QueryExpression),
                    Visit(shapedQueryExpression.ShaperExpression)),
            SetOperationBase setOperationExpression => VisitSetOperation(setOperationExpression),
            SelectExpression selectExpression => VisitSelect(selectExpression),
            _ => base.VisitExtension(extensionExpression)
        };

    private Expression VisitSetOperation(SetOperationBase setOperationExpression)
    {
        switch (_state)
        {
            case State.Nothing:
                _state = State.InSingleSetOperation;
                var visited = base.VisitExtension(setOperationExpression);
                _state = State.Nothing;
                return visited;

            case State.InSingleSetOperation:
                _state = State.InNestedSetOperation;
                visited = base.VisitExtension(setOperationExpression);
                _state = State.InSingleSetOperation;
                return visited;

            default:
                return base.VisitExtension(setOperationExpression);
        }
    }

    private Expression VisitSelect(SelectExpression selectExpression)
    {
        var changed = false;

        var tables = new List<TableExpressionBase>();
        foreach (var table in selectExpression.Tables)
        {
            var newTable = (TableExpressionBase)Visit(table);
            changed |= newTable != table;
            tables.Add(newTable);
        }

        // Above we visited the tables, which may contain nested set operations - so we retained our state.
        // When visiting the below elements, reset to state to properly handle nested unions inside e.g. the predicate.
        var parentState = _state;
        _state = State.Nothing;

        var projections = new List<ProjectionExpression>();
        foreach (var item in selectExpression.Projection)
        {
            // Inject an explicit cast node around null literals
            var updatedProjection = parentState == State.InNestedSetOperation && item.Expression is SqlConstantExpression { Value : null }
                ? item.Update(
                    new SqlUnaryExpression(ExpressionType.Convert, item.Expression, item.Expression.Type, item.Expression.TypeMapping))
                : (ProjectionExpression)Visit(item);

            projections.Add(updatedProjection);
            changed |= updatedProjection != item;
        }

        var predicate = (SqlExpression?)Visit(selectExpression.Predicate);
        changed |= predicate != selectExpression.Predicate;

        var groupBy = new List<SqlExpression>();
        foreach (var groupingKey in selectExpression.GroupBy)
        {
            var newGroupingKey = (SqlExpression)Visit(groupingKey);
            changed |= newGroupingKey != groupingKey;
            groupBy.Add(newGroupingKey);
        }

        var havingExpression = (SqlExpression?)Visit(selectExpression.Having);
        changed |= havingExpression != selectExpression.Having;

        var orderings = new List<OrderingExpression>();
        foreach (var ordering in selectExpression.Orderings)
        {
            var orderingExpression = (SqlExpression)Visit(ordering.Expression);
            changed |= orderingExpression != ordering.Expression;
            orderings.Add(ordering.Update(orderingExpression));
        }

        var offset = (SqlExpression?)Visit(selectExpression.Offset);
        changed |= offset != selectExpression.Offset;

        var limit = (SqlExpression?)Visit(selectExpression.Limit);
        changed |= limit != selectExpression.Limit;

        // If we were in the InNestedSetOperation state, we've applied all explicit type mappings when visiting the ProjectionExpressions
        // above; change the state to prevent unnecessarily continuing to compensate
        _state = parentState == State.InNestedSetOperation ? State.AlreadyCompensated : parentState;

        return changed
            ? selectExpression.Update(
                projections, tables, predicate, groupBy, havingExpression, orderings, limit, offset)
            : selectExpression;
    }

    private enum State
    {
        Nothing,
        InSingleSetOperation,
        InNestedSetOperation,
        AlreadyCompensated
    }
}
