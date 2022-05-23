using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

public class NpgsqlSetOperationTypeResolutionCompensatingExpressionVisitor : ExpressionVisitor
{
    private State _state;

    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            ShapedQueryExpression shapedQueryExpression
                => shapedQueryExpression.Update(
                    Visit(shapedQueryExpression.QueryExpression),
                    Visit(shapedQueryExpression.ShaperExpression)),
            SetOperationBase setOperationExpression => VisitSetOperation(setOperationExpression),
            SelectExpression selectExpression => VisitSelect(selectExpression),
            TpcTablesExpression tpcTablesExpression => VisitTpcTablesExpression(tpcTablesExpression),
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

#pragma warning disable EF1001
    private Expression VisitTpcTablesExpression(TpcTablesExpression tpcTablesExpression)
    {
        var parentState = _state;

        if (tpcTablesExpression.SelectExpressions.Count < 3)
        {
            return base.VisitExtension(tpcTablesExpression);
        }

        var changed = false;
        var visitedSelectExpressions = new SelectExpression[tpcTablesExpression.SelectExpressions.Count];

        _state = State.InNestedSetOperation;
        visitedSelectExpressions[0] = (SelectExpression)Visit(tpcTablesExpression.SelectExpressions[0]);
        changed |= visitedSelectExpressions[0] != tpcTablesExpression.SelectExpressions[0];
        _state = State.AlreadyCompensated;

        for (var i = 1; i < tpcTablesExpression.SelectExpressions.Count; i++)
        {
            var selectExpression = tpcTablesExpression.SelectExpressions[i];
            var visitedSelectExpression = (SelectExpression)Visit(tpcTablesExpression.SelectExpressions[i]);
            visitedSelectExpressions[i] = visitedSelectExpression;
            changed |= selectExpression != visitedSelectExpression;
        }

        _state = parentState;

        return changed
            ? new TpcTablesExpression(tpcTablesExpression.Alias, tpcTablesExpression.EntityType, visitedSelectExpressions)
            : tpcTablesExpression;
    }
#pragma warning restore EF1001

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
