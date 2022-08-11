namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

public class NpgsqlParameterBasedSqlProcessor : RelationalParameterBasedSqlProcessor
{
    public NpgsqlParameterBasedSqlProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        bool useRelationalNulls)
        : base(dependencies, useRelationalNulls)
    {
    }

    public override Expression Optimize(
        Expression queryExpression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
    {
        queryExpression = base.Optimize(queryExpression, parametersValues, out canCache);

        queryExpression = new NpgsqlDeleteConvertingExpressionVisitor().Process(queryExpression);

        return queryExpression;
    }

    /// <inheritdoc />
    protected override Expression ProcessSqlNullability(
        Expression selectExpression, IReadOnlyDictionary<string, object?> parametersValues, out bool canCache)
    {
        Check.NotNull(selectExpression, nameof(selectExpression));
        Check.NotNull(parametersValues, nameof(parametersValues));

        return new NpgsqlSqlNullabilityProcessor(Dependencies, UseRelationalNulls).Process(selectExpression, parametersValues, out canCache);
    }
}