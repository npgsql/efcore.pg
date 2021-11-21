namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

public class NpgsqlRandomTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo _methodInfo
        = typeof(DbFunctionsExtensions).GetRuntimeMethod(nameof(DbFunctionsExtensions.Random), new[] { typeof(DbFunctions) })!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public NpgsqlRandomTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        Check.NotNull(method, nameof(method));
        Check.NotNull(arguments, nameof(arguments));
        Check.NotNull(logger, nameof(logger));

        return _methodInfo.Equals(method)
            ? _sqlExpressionFactory.Function(
                "random",
                Array.Empty<SqlExpression>(),
                nullable: false,
                argumentsPropagateNullability: Array.Empty<bool>(),
                method.ReturnType)
            : null;
    }
}