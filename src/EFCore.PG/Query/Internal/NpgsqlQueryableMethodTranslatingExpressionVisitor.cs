using System.Diagnostics.CodeAnalysis;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        EntityShaperExpression entityShaperExpression,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (!base.IsValidSelectExpressionForExecuteUpdate(selectExpression, entityShaperExpression, out tableExpression))
        {
            return false;
        }

        // PostgreSQL doesn't support referencing the main update table from anywhere except for the UPDATE WHERE clause.
        // This specifically makes it impossible to have JOINs which reference the main table in their predicate (ON ...).
        // We detect these cases and return false to make EF perform subquery pushdown instead.
        OuterReferenceFindingExpressionVisitor? visitor = null;

        var firstTable = true;

        for (var i = 0; i < selectExpression.Tables.Count; i++)
        {
            var table = selectExpression.Tables[i];

            if (ReferenceEquals(table, tableExpression))
            {
                continue;
            }

            visitor ??= new OuterReferenceFindingExpressionVisitor(tableExpression);

            if (firstTable)
            {
                firstTable = false;

                // In UPDATE SQL generation, when the first JOIN is a predicate JOIN (INNER, LEFT), the predicate gets lifted up to the
                // UPDATE's WHERE clause, since the SQL uses a FROM; in other words, the first JOIN isn't really a syntactic JOIN with
                // the main UPDATE table, so ON can't be used.
                // Since the predicate is lifted, any reference to the main table is valid (since it's in main WHERE clause), so we refrain
                // from forcing a subquery pushdown for that case.
                if (table is PredicateJoinExpressionBase predicateJoin)
                {
                    table = predicateJoin.Table;
                }
            }

            if (visitor.ContainsReferenceToMainTable(table))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        EntityShaperExpression entityShaperExpression,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        // The default relational behavior is to allow only single-table expressions, and the only permitted feature is a predicate.
        // Here we extend this to also inner joins to tables, which we generate via the PostgreSQL-specific USING construct.
        if (selectExpression.Offset == null
            && selectExpression.Limit == null
            // If entity type has primary key then Distinct is no-op
            && (!selectExpression.IsDistinct || entityShaperExpression.EntityType.FindPrimaryKey() != null)
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0)
        {
            TableExpressionBase? table = null;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else if (selectExpression.Tables.All(t => t is TableExpression or InnerJoinExpression))
            {
                var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = entityProjectionExpression.BindProperty(entityShaperExpression.EntityType.GetProperties().First());
                table = column.Table;
                if (table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    private sealed class OuterReferenceFindingExpressionVisitor : ExpressionVisitor
    {
        private readonly TableExpression _mainTable;
        private bool _containsReference;

        public OuterReferenceFindingExpressionVisitor(TableExpression mainTable)
            => _mainTable = mainTable;

        public bool ContainsReferenceToMainTable(TableExpressionBase tableExpression)
        {
            _containsReference = false;

            Visit(tableExpression);

            return _containsReference;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (_containsReference)
            {
                return expression;
            }

            if (expression is ColumnExpression columnExpression
                && columnExpression.Table == _mainTable)
            {
                _containsReference = true;

                return expression;
            }

            return base.Visit(expression);
        }
    }
}
