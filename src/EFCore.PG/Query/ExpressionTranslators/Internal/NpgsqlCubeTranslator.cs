using System.Security.AccessControl;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlCubeTranslator : IMethodCallTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlCubeTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(NpgsqlCubeDbFunctionsExtensions))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(NpgsqlCubeDbFunctionsExtensions.Overlaps)
                => _sqlExpressionFactory.Overlaps(arguments[0], arguments[1]),
            nameof(NpgsqlCubeDbFunctionsExtensions.Contains)
                => _sqlExpressionFactory.Contains(arguments[0], arguments[1]),
            nameof(NpgsqlCubeDbFunctionsExtensions.ContainedBy)
                => _sqlExpressionFactory.ContainedBy(arguments[0], arguments[1]),
            nameof(NpgsqlCubeDbFunctionsExtensions.NthCoordinate)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.CubeNthCoordinate, arguments[0], arguments[1]),
            nameof(NpgsqlCubeDbFunctionsExtensions.NthCoordinate2)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.CubeNthCoordinate2, arguments[0], arguments[1]),
            nameof(NpgsqlCubeDbFunctionsExtensions.Distance)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.Distance, arguments[0], arguments[1]),
            nameof(NpgsqlCubeDbFunctionsExtensions.DistanceTaxicab)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.CubeDistanceTaxicab, arguments[0], arguments[1]),
            nameof(NpgsqlCubeDbFunctionsExtensions.DistanceChebyshev)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.CubeDistanceChebyshev, arguments[0], arguments[1]),

            _ => null
        };
    }
}
