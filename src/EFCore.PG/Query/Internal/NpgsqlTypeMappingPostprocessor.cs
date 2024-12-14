using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

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
