namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
/// Translates methods defined on <see cref="T:System.Convert"/> into PostgreSQL CAST expressions.
/// </summary>
public class NpgsqlConvertTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<string, string> TypeMapping = new()
    {
        [nameof(Convert.ToBoolean)] = "bool",
        [nameof(Convert.ToByte)]    = "smallint",
        [nameof(Convert.ToDecimal)] = "numeric",
        [nameof(Convert.ToDouble)]  = "double precision",
        [nameof(Convert.ToInt16)]   = "smallint",
        [nameof(Convert.ToInt32)]   = "int",
        [nameof(Convert.ToInt64)]   = "bigint",
        [nameof(Convert.ToString)]  = "text"
    };

    private static readonly List<Type> SupportedTypes = new()
    {
        typeof(bool),
        typeof(byte),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(int),
        typeof(long),
        typeof(short),
        typeof(string)
    };

    private static readonly List<MethodInfo> SupportedMethods
        = TypeMapping.Keys
            .SelectMany(
                t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                    .Where(
                        m => m.GetParameters().Length == 1
                            && SupportedTypes.Contains(m.GetParameters().First().ParameterType)))
            .ToList();

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlConvertTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

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
            ? _sqlExpressionFactory.Convert(arguments[0], method.ReturnType)
            : null;
}
