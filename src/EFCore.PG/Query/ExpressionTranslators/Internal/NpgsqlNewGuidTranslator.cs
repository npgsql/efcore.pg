using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
/// Provides translation services for PostgreSQL UUID functions.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/datatype-uuid.html
/// </remarks>
public class NpgsqlNewGuidTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo MethodInfo = typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Array.Empty<Type>())!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly string _uuidGenerationFunction;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlNewGuidTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        INpgsqlSingletonOptions npgsqlSingletonOptions)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _uuidGenerationFunction = npgsqlSingletonOptions.PostgresVersion.AtLeast(13) ? "gen_random_uuid" : "uuid_generate_v4";
    }

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
        => MethodInfo.Equals(method)
            ? _sqlExpressionFactory.Function(
                _uuidGenerationFunction,
                Array.Empty<SqlExpression>(),
                nullable: false,
                argumentsPropagateNullability: FalseArrays[0],
                method.ReturnType)
            : null;
}
