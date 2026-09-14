namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Translates single-argument <c>Parse</c> methods (e.g. <see cref="int.Parse(string)" />) into PostgreSQL CAST expressions.
/// </summary>
public class NpgsqlParseTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    private static readonly Type[] SupportedClrTypes =
    [
        typeof(bool),
        typeof(byte),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(short),
        typeof(int),
        typeof(long)
    ];

    private static readonly MethodInfo[] SupportedMethods
        = SupportedClrTypes
            .Select(t => t.GetMethod(nameof(int.Parse), [typeof(string)])!)
            .ToArray();

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
        => SupportedMethods.Contains(method)
            ? sqlExpressionFactory.Convert(arguments[0], method.ReturnType)
            : null;
}
