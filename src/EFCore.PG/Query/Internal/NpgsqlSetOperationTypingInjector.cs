namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     A visitor that injects explicit typing on null projections in set operations, to ensure PostgreSQL gets the typing right.
/// </summary>
/// <remarks>
///     <para>
///         See the <see href="https://www.postgresql.org/docs/current/typeconv-union-case.html">
///         PostgreSQL docs on type conversion and set operations</see>.
///     </para>
///     <para>
///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///         the same compatibility standards as public APIs. It may be changed or removed without notice in
///         any release. You should only use it directly in your code with extreme caution and knowing that
///         doing so can result in application failures when updating to a new Entity Framework Core release.
///     </para>
/// </remarks>
public class NpgsqlSetOperationTypingInjector : ExpressionVisitor
{
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

            _ => base.VisitExtension(extensionExpression)
        };

    private Expression VisitSetOperation(SetOperationBase setOperation)
    {
        var select1 = (SelectExpression)Visit(setOperation.Source1);
        var select2 = (SelectExpression)Visit(setOperation.Source2);

        List<ProjectionExpression>? rewrittenProjections = null;

        for (var i = 0; i < select1.Projection.Count; i++)
        {
            var projection = select1.Projection[i];
            var visitedProjection = projection.Expression is SqlConstantExpression { Value : null }
                && select2.Projection[i].Expression is SqlConstantExpression { Value : null }
                ? projection.Update(
                    new SqlUnaryExpression(
                        ExpressionType.Convert, projection.Expression, projection.Expression.Type, projection.Expression.TypeMapping))
                : (ProjectionExpression)Visit(projection);

            if (visitedProjection != projection && rewrittenProjections is null)
            {
                rewrittenProjections = new List<ProjectionExpression>(select1.Projection.Count);
                rewrittenProjections.AddRange(select1.Projection.Take(i));
            }

            rewrittenProjections?.Add(visitedProjection);
        }

        if (rewrittenProjections is not null)
        {
            select1 = select1.Update(
                select1.Tables,
                select1.Predicate,
                select1.GroupBy,
                select1.Having,
                rewrittenProjections,
                select1.Orderings,
                select1.Offset,
                select1.Limit);
        }

        return setOperation.Update(select1, select2);
    }
}
