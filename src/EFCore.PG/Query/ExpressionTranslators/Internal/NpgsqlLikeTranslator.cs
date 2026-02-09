namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Translates <see cref="T:DbFunctionsExtensions.Like" /> methods into PostgreSQL LIKE expressions.
/// </summary>
public class NpgsqlLikeTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        bool sensitive;
        if (method.DeclaringType == typeof(DbFunctionsExtensions)
            && method.Name == nameof(DbFunctionsExtensions.Like))
        {
            sensitive = true;
        }
        else if (method.DeclaringType == typeof(NpgsqlDbFunctionsExtensions)
            && method.Name == nameof(NpgsqlDbFunctionsExtensions.ILike))
        {
            sensitive = false;
        }
        else
        {
            return null;
        }

        // The 4-argument overloads have an escape char parameter
        if (arguments is [_, _, _, var escapeChar])
        {
            return sensitive
                ? sqlExpressionFactory.Like(arguments[1], arguments[2], escapeChar)
                : sqlExpressionFactory.ILike(arguments[1], arguments[2], escapeChar);
        }

        // PostgreSQL has backslash as the default LIKE escape character, but EF Core expects
        // no escape character unless explicitly requested (https://github.com/aspnet/EntityFramework/issues/8696).

        // If we have a constant expression, we check that there are no backslashes in order to render with
        // an ESCAPE clause (better SQL). If we have a constant expression with backslashes or a non-constant
        // expression, we render an ESCAPE clause to disable backslash escaping.

        var (match, pattern) = (arguments[1], arguments[2]);

        if (pattern is SqlConstantExpression { Value: string patternValue }
            && !patternValue.Contains('\\'))
        {
            return sensitive
                ? sqlExpressionFactory.Like(match, pattern)
                : sqlExpressionFactory.ILike(match, pattern);
        }

        return sensitive
            ? sqlExpressionFactory.Like(match, pattern, sqlExpressionFactory.Constant(string.Empty))
            : sqlExpressionFactory.ILike(match, pattern, sqlExpressionFactory.Constant(string.Empty));
    }
}
