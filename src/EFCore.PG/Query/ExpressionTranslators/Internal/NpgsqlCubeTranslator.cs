using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlCubeTranslator : IMethodCallTranslator, IMemberTranslator
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
        if (method.DeclaringType == typeof(NpgsqlCubeDbFunctionsExtensions))
        {
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
                nameof(NpgsqlCubeDbFunctionsExtensions.Union)
                    => _sqlExpressionFactory.Function(
                        "cube_union",
                        new[] { arguments[0], arguments[1] },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        method.ReturnType),
                nameof(NpgsqlCubeDbFunctionsExtensions.Intersect)
                    => _sqlExpressionFactory.Function(
                        "cube_inter",
                        new[] { arguments[0], arguments[1] },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        method.ReturnType),
                nameof(NpgsqlCubeDbFunctionsExtensions.Enlarge)
                    => _sqlExpressionFactory.Function(
                        "cube_enlarge",
                        new[] { arguments[0], arguments[1], arguments[2] },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[3],
                        method.ReturnType),

                _ => null
            };
        }

        if (method.DeclaringType == typeof(NpgsqlCube) && instance != null)
        {
            return method.Name switch
            {
                nameof(NpgsqlCube.LlCoord)
                    => _sqlExpressionFactory.Function(
                        "cube_ll_coord",
                        new[] { instance, arguments[0] },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        method.ReturnType),
                nameof(NpgsqlCube.UrCoord)
                    => _sqlExpressionFactory.Function(
                        "cube_ur_coord",
                        new[] { instance, arguments[0] },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        method.ReturnType),
                nameof(NpgsqlCube.Subset)
                    => _sqlExpressionFactory.Function(
                        "cube_subset",
                        new[] { instance, arguments[0] },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        method.ReturnType),

                _ => null
            };
        }

        // TODO: Implement indexing into lower/upper lists with cube_ll_coord and cube_ur_coord

        return null;
    }

    /// <inheritdoc />
    public SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (member.DeclaringType != typeof(NpgsqlCube) || instance == null)
        {
            return null;
        }

        return member.Name switch
        {
            nameof(NpgsqlCube.Dimensions)
                => _sqlExpressionFactory.Function(
                    "cube_dim",
                    new[] { instance },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    returnType),
            nameof(NpgsqlCube.Point)
                => _sqlExpressionFactory.Function(
                    "cube_is_point",
                    new[] { instance },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    returnType),
            _ => null
        };
    }
}
