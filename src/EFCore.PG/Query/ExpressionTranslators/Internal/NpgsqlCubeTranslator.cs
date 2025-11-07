using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using NpgsqlTypes;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlCubeTranslator(
    NpgsqlSqlExpressionFactory sqlExpressionFactory,
    IRelationalTypeMappingSource typeMappingSource) : IMethodCallTranslator, IMemberTranslator
{
    private readonly RelationalTypeMapping _doubleTypeMapping = typeMappingSource.FindMapping(typeof(double))!;

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Handle instance methods on NpgsqlCube
        if (instance is not null && method.DeclaringType == typeof(NpgsqlCube))
        {
            return method.Name switch
            {
                nameof(NpgsqlCube.ToSubset) when arguments is [var indexes]
                    => TranslateSubset([instance, indexes]),

                _ => null
            };
        }

        // Handle NpgsqlCubeDbFunctionsExtensions methods
        if (method.DeclaringType != typeof(NpgsqlCubeDbFunctionsExtensions))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(NpgsqlCubeDbFunctionsExtensions.Overlaps) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.Overlaps(cube1, cube2),

            nameof(NpgsqlCubeDbFunctionsExtensions.Contains) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.Contains(cube1, cube2),

            nameof(NpgsqlCubeDbFunctionsExtensions.ContainedBy) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.ContainedBy(cube1, cube2),

            nameof(NpgsqlCubeDbFunctionsExtensions.Distance) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.Distance,
                    cube1,
                    cube2),

            nameof(NpgsqlCubeDbFunctionsExtensions.DistanceTaxicab) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.CubeDistanceTaxicab,
                    cube1,
                    cube2),

            nameof(NpgsqlCubeDbFunctionsExtensions.DistanceChebyshev) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.CubeDistanceChebyshev,
                    cube1,
                    cube2),

            nameof(NpgsqlCubeDbFunctionsExtensions.NthCoordinate) when arguments is [var cube, var index]
                => sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.CubeNthCoordinate,
                    cube,
                    ConvertToPostgresIndex(index),
                    _doubleTypeMapping),

            nameof(NpgsqlCubeDbFunctionsExtensions.NthCoordinateKnn) when arguments is [var cube, var index]
                => sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.CubeNthCoordinateKnn,
                    cube,
                    ConvertToPostgresIndex(index),
                    _doubleTypeMapping),

            nameof(NpgsqlCubeDbFunctionsExtensions.Union) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.Function(
                    "cube_union",
                    [cube1, cube2],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(NpgsqlCube),
                    typeMappingSource.FindMapping(typeof(NpgsqlCube))),

            nameof(NpgsqlCubeDbFunctionsExtensions.Intersect) when arguments is [var cube1, var cube2]
                => sqlExpressionFactory.Function(
                    "cube_inter",
                    [cube1, cube2],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(NpgsqlCube),
                    typeMappingSource.FindMapping(typeof(NpgsqlCube))),

            nameof(NpgsqlCubeDbFunctionsExtensions.Enlarge) when arguments is [var cube1, var cube2, var dimension]
                => sqlExpressionFactory.Function(
                    "cube_enlarge",
                    [cube1, cube2, dimension],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[3],
                    typeof(NpgsqlCube),
                    typeMappingSource.FindMapping(typeof(NpgsqlCube))),

            _ => null
        };
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (member.DeclaringType != typeof(NpgsqlCube))
        {
            return null;
        }

        return member.Name switch
        {
            nameof(NpgsqlCube.Dimensions)
                => sqlExpressionFactory.Function(
                    "cube_dim",
                    [instance!],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(int)),

            nameof(NpgsqlCube.IsPoint)
                => sqlExpressionFactory.Function(
                    "cube_is_point",
                    [instance!],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(bool)),

            nameof(NpgsqlCube.LowerLeft)
                => throw new InvalidOperationException(
                    $"The '{nameof(NpgsqlCube.LowerLeft)}' property cannot be translated to SQL. " +
                    $"To access individual lower-left coordinates in queries, use indexer syntax (e.g., cube.LowerLeft[index]) instead."),

            nameof(NpgsqlCube.UpperRight)
                => throw new InvalidOperationException(
                    $"The '{nameof(NpgsqlCube.UpperRight)}' property cannot be translated to SQL. " +
                    $"To access individual upper-right coordinates in queries, use indexer syntax (e.g., cube.UpperRight[index]) instead."),

            _ => null
        };
    }

    private SqlExpression? TranslateSubset(IReadOnlyList<SqlExpression> arguments)
    {
        // arguments[0] is the cube
        // arguments[1] is the int[] indexes array

        SqlExpression convertedIndexes;

        switch (arguments[1])
        {
            case SqlConstantExpression { Value: int[] constantArray }:
                // All elements are constants
                var oneBasedValues = constantArray.Select(i => i + 1).ToArray();
                convertedIndexes = sqlExpressionFactory.Constant(oneBasedValues);
                break;

            case PgNewArrayExpression { Expressions: var expressions }:
                // Mixed constants and non-constants
                var convertedExpressions = expressions
                    .Select(e => e is SqlConstantExpression { Value: int index }
                        ? sqlExpressionFactory.Constant(index + 1)  // Constant
                        : sqlExpressionFactory.Add(e, sqlExpressionFactory.Constant(1)))  // Non-constant
                    .ToArray();
                convertedIndexes = sqlExpressionFactory.NewArray(convertedExpressions, typeof(int[]));
                break;

            case ScalarSubqueryExpression:
                // Already converted by NpgsqlSqlTranslatingExpressionVisitor
                convertedIndexes = arguments[1];
                break;

            default:
                // For parameters and columns, let NpgsqlSqlTranslatingExpressionVisitor handle it
                return null;
        }

        return sqlExpressionFactory.Function(
            "cube_subset",
            [arguments[0], convertedIndexes],
            nullable: true,
            argumentsPropagateNullability: TrueArrays[2],
            typeof(NpgsqlCube),
            typeMappingSource.FindMapping(typeof(NpgsqlCube)));
    }

    /// <summary>
    /// Converts a zero-based index to one-based for PostgreSQL cube functions.
    /// For constant indexes, simplifies at translation time to avoid unnecessary addition in SQL.
    /// </summary>
    private SqlExpression ConvertToPostgresIndex(SqlExpression indexExpression)
    {
        var intTypeMapping = typeMappingSource.FindMapping(typeof(int));

        return indexExpression is SqlConstantExpression { Value: int index }
            ? sqlExpressionFactory.Constant(index + 1, intTypeMapping)
            : sqlExpressionFactory.Add(indexExpression, sqlExpressionFactory.Constant(1, intTypeMapping));
    }
}
