using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;

public class NpgsqlNodaTimeAggregateMethodCallTranslatorPlugin : IAggregateMethodCallTranslatorPlugin
{
    public NpgsqlNodaTimeAggregateMethodCallTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory)
    {
        if (sqlExpressionFactory is not NpgsqlSqlExpressionFactory npgsqlSqlExpressionFactory)
        {
            throw new ArgumentException($"Must be an {nameof(NpgsqlSqlExpressionFactory)}", nameof(sqlExpressionFactory));
        }

        Translators = new IAggregateMethodCallTranslator[]
        {
            new NpgsqlNodaTimeAggregateMethodTranslator(npgsqlSqlExpressionFactory)
        };
    }

    public virtual IEnumerable<IAggregateMethodCallTranslator> Translators { get; }
}

public class NpgsqlNodaTimeAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private static readonly bool[][] FalseArrays = { Array.Empty<bool>(), new[] { false } };

    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

    public NpgsqlNodaTimeAggregateMethodTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (source.Selector is not SqlExpression sqlExpression || method.DeclaringType != typeof(NpgsqlNodaTimeDbFunctionsExtensions))
        {
            return null;
        }

        switch (method.Name)
        {
            case nameof(NpgsqlNodaTimeDbFunctionsExtensions.Sum):
                return _sqlExpressionFactory.AggregateFunction(
                    "sum",
                    new[] { sqlExpression },
                    source,
                    nullable: true,
                    argumentsPropagateNullability: FalseArrays[1],
                    returnType: sqlExpression.Type,
                    sqlExpression.TypeMapping);

            case nameof(NpgsqlNodaTimeDbFunctionsExtensions.Average):
                return _sqlExpressionFactory.AggregateFunction(
                    "avg",
                    new[] { sqlExpression },
                    source,
                    nullable: true,
                    argumentsPropagateNullability: FalseArrays[1],
                    returnType: sqlExpression.Type,
                    sqlExpression.TypeMapping);

            default:
                return null;
        }
    }
}
