namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Translates methods defined on <see cref="T:System.Convert" /> into PostgreSQL CAST expressions.
/// </summary>
public class NpgsqlConvertTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(Convert))
        {
            return null;
        }

        var isSupported = method.Name is nameof(Convert.ToBoolean) or nameof(Convert.ToByte) or nameof(Convert.ToDecimal)
            or nameof(Convert.ToDouble) or nameof(Convert.ToInt16) or nameof(Convert.ToInt32) or nameof(Convert.ToInt64)
            or nameof(Convert.ToString);

        if (!isSupported
            || arguments is not [var convertArg]
            || !IsSupportedType(convertArg.Type))
        {
            return null;
        }

        return sqlExpressionFactory.Convert(convertArg, method.ReturnType);
    }

    private static bool IsSupportedType(Type type)
        => type == typeof(bool)
            || type == typeof(byte)
            || type == typeof(decimal)
            || type == typeof(double)
            || type == typeof(float)
            || type == typeof(int)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(string)
            || type == typeof(object);
}
