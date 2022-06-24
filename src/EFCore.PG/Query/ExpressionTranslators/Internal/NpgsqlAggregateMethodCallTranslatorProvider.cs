namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

public class NpgsqlAggregateMethodCallTranslatorProvider : RelationalAggregateMethodCallTranslatorProvider
{
    public NpgsqlAggregateMethodCallTranslatorProvider(RelationalAggregateMethodCallTranslatorProviderDependencies dependencies)
        : base(dependencies)
    {
        var sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
        var typeMappingSource = dependencies.RelationalTypeMappingSource;

        AddTranslators(
            new IAggregateMethodCallTranslator[]
            {
                new NpgsqlQueryableAggregateMethodTranslator(sqlExpressionFactory, typeMappingSource),
                new NpgsqlStatisticsAggregateMethodTranslator(sqlExpressionFactory, typeMappingSource),
                new NpgsqlMiscAggregateMethodTranslator(sqlExpressionFactory, typeMappingSource)
            });
    }
}
