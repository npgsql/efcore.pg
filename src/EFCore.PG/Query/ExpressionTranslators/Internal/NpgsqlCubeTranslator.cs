using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using NpgsqlTypes;
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
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlCubeTranslator(
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Handle indexer access: cube.LowerLeft[index] or cube.UpperRight[index]
        if (method.Name == "get_Item" && instance != null)
        {
            // Check if this is being called on LowerLeft or UpperRight property
            if (instance is SqlFunctionExpression { Name: "cube_ll_coord" or "cube_ur_coord" })
            {
                // This is already a function call, let it pass through
                return null;
            }

            // For member access like cube.LowerLeft or cube.UpperRight, the instance will be a column/parameter
            // and we need to check if this is accessing an IReadOnlyList<double>
            if (instance.Type == typeof(IReadOnlyList<double>))
            {
                // We can't directly determine if this is LowerLeft or UpperRight from here.
                // This case might need special handling in the member translator or we might need
                // to track this through a custom expression type. For now, return null.
                // The indexer translation will be handled by translating member access first.
                return null;
            }
        }

        // Handle NpgsqlCubeDbFunctionsExtensions methods
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

            nameof(NpgsqlCubeDbFunctionsExtensions.Distance)
                => _sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.Distance,
                    arguments[0],
                    arguments[1]),

            nameof(NpgsqlCubeDbFunctionsExtensions.DistanceTaxicab)
                => _sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.CubeDistanceTaxicab,
                    arguments[0],
                    arguments[1]),

            nameof(NpgsqlCubeDbFunctionsExtensions.DistanceChebyshev)
                => _sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.CubeDistanceChebyshev,
                    arguments[0],
                    arguments[1]),

            nameof(NpgsqlCubeDbFunctionsExtensions.NthCoordinate)
                => _sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.CubeNthCoordinate,
                    arguments[0],
                    ConvertToPostgresIndex(arguments[1])),

            nameof(NpgsqlCubeDbFunctionsExtensions.NthCoordinateKnn)
                => _sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.CubeNthCoordinateKnn,
                    arguments[0],
                    ConvertToPostgresIndex(arguments[1])),

            nameof(NpgsqlCubeDbFunctionsExtensions.LlCoord)
                => _sqlExpressionFactory.Function(
                    "cube_ll_coord",
                    [arguments[0], ConvertToPostgresIndex(arguments[1])],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(double)),

            nameof(NpgsqlCubeDbFunctionsExtensions.UrCoord)
                => _sqlExpressionFactory.Function(
                    "cube_ur_coord",
                    [arguments[0], ConvertToPostgresIndex(arguments[1])],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(double)),

            nameof(NpgsqlCubeDbFunctionsExtensions.Union)
                => _sqlExpressionFactory.Function(
                    "cube_union",
                    [arguments[0], arguments[1]],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(NpgsqlCube),
                    _typeMappingSource.FindMapping(typeof(NpgsqlCube))),

            nameof(NpgsqlCubeDbFunctionsExtensions.Intersect)
                => _sqlExpressionFactory.Function(
                    "cube_inter",
                    [arguments[0], arguments[1]],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(NpgsqlCube),
                    _typeMappingSource.FindMapping(typeof(NpgsqlCube))),

            nameof(NpgsqlCubeDbFunctionsExtensions.Enlarge)
                => _sqlExpressionFactory.Function(
                    "cube_enlarge",
                    [arguments[0], arguments[1], arguments[2]],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[3],
                    typeof(NpgsqlCube),
                    _typeMappingSource.FindMapping(typeof(NpgsqlCube))),

            nameof(NpgsqlCubeDbFunctionsExtensions.Subset)
                => TranslateSubset(arguments),

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
                => _sqlExpressionFactory.Function(
                    "cube_dim",
                    [instance!],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(int)),

            nameof(NpgsqlCube.IsPoint)
                => _sqlExpressionFactory.Function(
                    "cube_is_point",
                    [instance!],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(bool)),

            // TODO: Determine if we should remove these list accessors and replace them with UrCoord and LlCoord methods instead.
            nameof(NpgsqlCube.LowerLeft)
                => throw new InvalidOperationException(
                    $"The '{nameof(NpgsqlCube.LowerLeft)}' property cannot be translated to SQL. " +
                    $"To access individual lower-left coordinates in queries, use the LlCoord() extension method instead."),

            nameof(NpgsqlCube.UpperRight)
                => throw new InvalidOperationException(
                    $"The '{nameof(NpgsqlCube.UpperRight)}' property cannot be translated to SQL. " +
                    $"To access individual upper-right coordinates in queries, use the UrCoord() extension method instead."),

            _ => null
        };
    }

    private SqlExpression? TranslateSubset(IReadOnlyList<SqlExpression> arguments)
    {
        // arguments[0] is the cube
        // arguments[1] is the int[] indexes array

        // For subset, we need to convert the C# zero-based indexes to PostgreSQL one-based indexes
        int[]? indexes;

        // Try to extract the int array from various SQL expression types
        switch (arguments[1])
        {
            case SqlConstantExpression { Value: int[] constantIndexes }:
                indexes = constantIndexes;
                break;

            case SqlParameterExpression:
                // Parameterized arrays aren't supported because we can't convert zero-based
                // indexes to one-based at compile time for parameter values
                return null;

            default:
                // Unsupported expression type for array argument
                return null;
        }

        // Convert zero-based indexes to one-based
        var oneBasedIndexes = indexes.Select(i => i + 1).ToArray();

        // Create a PostgreSQL array literal using PgNewArrayExpression
        var arrayExpression = _sqlExpressionFactory.NewArray(
            oneBasedIndexes.Select(i => _sqlExpressionFactory.Constant(i)).ToArray(),
            typeof(int[]));

        return _sqlExpressionFactory.Function(
            "cube_subset",
            [arguments[0], arrayExpression],
            nullable: true,
            argumentsPropagateNullability: TrueArrays[2],
            typeof(NpgsqlCube),
            _typeMappingSource.FindMapping(typeof(NpgsqlCube)));
    }

    /// <summary>
    /// Converts a zero-based index to one-based for PostgreSQL cube functions.
    /// For constant indexes, simplifies at translation time to avoid operator precedence issues.
    /// </summary>
    private SqlExpression ConvertToPostgresIndex(SqlExpression indexExpression)
    {
        var intTypeMapping = _typeMappingSource.FindMapping(typeof(int));

        // If the index is a constant, add 1 at translation time to increment the constant by 1
        return indexExpression is SqlConstantExpression { Value: int index } ? _sqlExpressionFactory.Constant(index + 1, intTypeMapping) :
            // For non-constant expressions (parameters, columns), generate addition expression
            _sqlExpressionFactory.Add(indexExpression, _sqlExpressionFactory.Constant(1, intTypeMapping));
    }
}
