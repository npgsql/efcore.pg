using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlTypeMappingPostprocessor : RelationalTypeMappingPostprocessor
{
    private readonly IModel _model;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlTypeMappingPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        RelationalQueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _model = queryCompilationContext.Model;
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression expression)
    {
        switch (expression)
        {
            case NpgsqlCubeTranslator.ArrayIncrementSubqueryExpression arrayIncrementExpression:
            {
                // Transform the marker expression into a proper scalar subquery:
                // (SELECT array_agg(x + 1) FROM unnest(arrayExpression) AS x)

                // Apply type mapping to the input array expression
                var arrayTypeMapping = _typeMappingSource.FindMapping(typeof(int[]), _model);
                if (arrayTypeMapping is null)
                {
                    throw new InvalidOperationException(RelationalStrings.NullTypeMappingInSqlTree(expression.Print()));
                }

                var typedArrayExpression = _sqlExpressionFactory.ApplyTypeMapping(
                    arrayIncrementExpression.ArrayExpression, arrayTypeMapping);

                // Generate table alias and create the unnest table expression
                var tableAlias = ((RelationalQueryCompilationContext)QueryCompilationContext).SqlAliasManager.GenerateTableAlias("u");
                var unnestTable = new PgUnnestExpression(tableAlias, typedArrayExpression, "x", withOrdinality: false);

                // Create column reference for the unnested value
                var intTypeMapping = _typeMappingSource.FindMapping(typeof(int), _model);
                var xColumn = new ColumnExpression("x", tableAlias, typeof(int), intTypeMapping, nullable: false);

                // Create the increment expression: x + 1
                var xPlusOne = ((NpgsqlSqlExpressionFactory)_sqlExpressionFactory).Add(
                    xColumn,
                    _sqlExpressionFactory.Constant(1, intTypeMapping));

                // Create array_agg(x + 1) function call
                var arrayAggFunction = _sqlExpressionFactory.Function(
                    "array_agg",
                    new[] { xPlusOne },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(int[]),
                    arrayIncrementExpression.TypeMapping ?? arrayTypeMapping);

                // Construct the SelectExpression (mutable state)
#pragma warning disable EF1001 // SelectExpression constructors are pubternal
                var selectExpression = new SelectExpression(
                    new List<TableExpressionBase> { unnestTable },
                    arrayAggFunction,
                    new List<(ColumnExpression, ValueComparer)>(),
                    ((RelationalQueryCompilationContext)QueryCompilationContext).SqlAliasManager);
#pragma warning restore EF1001

                // Finalize: convert _projectionMapping → Projection list (immutable state)
                selectExpression.ApplyProjection();

                // Wrap in ScalarSubqueryExpression
                return new ScalarSubqueryExpression(selectExpression);
            }

            case PgUnnestExpression unnestExpression
                when TryGetInferredTypeMapping(unnestExpression.Alias, unnestExpression.ColumnName, out var elementTypeMapping):
            {
                var collectionTypeMapping = _typeMappingSource.FindMapping(unnestExpression.Array.Type, _model, elementTypeMapping);

                if (collectionTypeMapping is null)
                {
                    throw new InvalidOperationException(RelationalStrings.NullTypeMappingInSqlTree(expression.Print()));
                }

                return unnestExpression.Update(
                    _sqlExpressionFactory.ApplyTypeMapping(unnestExpression.Array, collectionTypeMapping));
            }

            default:
                return base.VisitExtension(expression);
        }
    }
}
