using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Union;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlNetTopologySuiteAggregateMethodCallTranslatorPlugin : IAggregateMethodCallTranslatorPlugin
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlNetTopologySuiteAggregateMethodCallTranslatorPlugin(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        if (sqlExpressionFactory is not NpgsqlSqlExpressionFactory npgsqlSqlExpressionFactory)
        {
            throw new ArgumentException($"Must be an {nameof(NpgsqlSqlExpressionFactory)}", nameof(sqlExpressionFactory));
        }

        Translators =
        [
            new NpgsqlNetTopologySuiteAggregateMethodTranslator(npgsqlSqlExpressionFactory, typeMappingSource)
        ];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IAggregateMethodCallTranslator> Translators { get; }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlNetTopologySuiteAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private static readonly MethodInfo GeometryCombineMethod
        = typeof(GeometryCombiner).GetRuntimeMethod(nameof(GeometryCombiner.Combine), [typeof(IEnumerable<Geometry>)])!;

    private static readonly MethodInfo ConvexHullMethod
        = typeof(ConvexHull).GetRuntimeMethod(nameof(ConvexHull.Create), [typeof(IEnumerable<Geometry>)])!;

    private static readonly MethodInfo UnionMethod
        = typeof(UnaryUnionOp).GetRuntimeMethod(nameof(UnaryUnionOp.Union), [typeof(IEnumerable<Geometry>)])!;

    private static readonly MethodInfo EnvelopeCombineMethod
        = typeof(EnvelopeCombiner).GetRuntimeMethod(nameof(EnvelopeCombiner.CombineAsGeometry), [typeof(IEnumerable<Geometry>)])!;

    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlNetTopologySuiteAggregateMethodTranslator(
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (source.Selector is not SqlExpression sqlExpression)
        {
            return null;
        }

        if (method == ConvexHullMethod)
        {
            // PostGIS has no built-in aggregate convex hull, but we can simply apply ST_Collect beforehand as recommended in the docs
            // https://postgis.net/docs/ST_ConvexHull.html
            return _sqlExpressionFactory.Function(
                "ST_ConvexHull",
                [
                    _sqlExpressionFactory.AggregateFunction(
                        "ST_Collect",
                        [sqlExpression],
                        source,
                        nullable: true,
                        argumentsPropagateNullability: [false],
                        typeof(Geometry),
                        GetMapping())
                ],
                nullable: true,
                argumentsPropagateNullability: [true],
                typeof(Geometry),
                GetMapping());
        }

        if (method == EnvelopeCombineMethod)
        {
            // ST_Extent returns a PostGIS box2d, which isn't a geometry and has no binary output function.
            // Convert it to a geometry first.
            return _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.AggregateFunction(
                    "ST_Extent",
                    [sqlExpression],
                    source,
                    nullable: true,
                    argumentsPropagateNullability: [false],
                    typeof(Geometry),
                    GetMapping()),
                typeof(Geometry), GetMapping());
        }

        if (method == UnionMethod || method == GeometryCombineMethod)
        {
            return _sqlExpressionFactory.AggregateFunction(
                method == UnionMethod ? "ST_Union" : "ST_Collect",
                [sqlExpression],
                source,
                nullable: true,
                argumentsPropagateNullability: [false],
                typeof(Geometry),
                GetMapping());
        }

        return null;

        RelationalTypeMapping? GetMapping()
            => _typeMappingSource.FindMapping(typeof(Geometry), sqlExpression.TypeMapping?.StoreType ?? "geometry");
    }
}
