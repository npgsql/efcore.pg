using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Union;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

public class NpgsqlNetTopologySuiteAggregateMethodCallTranslatorPlugin : IAggregateMethodCallTranslatorPlugin
{
    public NpgsqlNetTopologySuiteAggregateMethodCallTranslatorPlugin(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        if (sqlExpressionFactory is not NpgsqlSqlExpressionFactory npgsqlSqlExpressionFactory)
        {
            throw new ArgumentException($"Must be an {nameof(NpgsqlSqlExpressionFactory)}", nameof(sqlExpressionFactory));
        }

        Translators = new IAggregateMethodCallTranslator[]
        {
            new NpgsqlNetTopologySuiteAggregateMethodTranslator(npgsqlSqlExpressionFactory, typeMappingSource)
        };
    }

    public virtual IEnumerable<IAggregateMethodCallTranslator> Translators { get; }
}

public class NpgsqlNetTopologySuiteAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private static readonly MethodInfo GeometryCombineMethod
        = typeof(GeometryCombiner).GetRuntimeMethod(nameof(GeometryCombiner.Combine), new[] { typeof(IEnumerable<Geometry>) })!;

    private static readonly MethodInfo ConvexHullMethod
        = typeof(ConvexHull).GetRuntimeMethod(nameof(ConvexHull.Create), new[] { typeof(IEnumerable<Geometry>) })!;

    private static readonly MethodInfo UnionMethod
        = typeof(UnaryUnionOp).GetRuntimeMethod(nameof(UnaryUnionOp.Union), new[] { typeof(IEnumerable<Geometry>) })!;

    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    public NpgsqlNetTopologySuiteAggregateMethodTranslator(
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    public virtual SqlExpression? Translate(
        MethodInfo method, EnumerableExpression source, IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (source.Selector is not SqlExpression sqlExpression
            || (method != GeometryCombineMethod && method != UnionMethod && method != ConvexHullMethod))
        {
            return null;
        }

        var resultTypeMapping = _typeMappingSource.FindMapping(typeof(Geometry), sqlExpression.TypeMapping?.StoreType ?? "geometry");

        if (method == GeometryCombineMethod || method == UnionMethod)
        {
            return _sqlExpressionFactory.AggregateFunction(
                method == GeometryCombineMethod ? "ST_Collect" : "ST_Union",
                new[] { sqlExpression },
                nullable: true,
                argumentsPropagateNullability: new[] { false },
                source,
                typeof(Geometry),
                resultTypeMapping);
        }

        // PostGIS has no built-in aggregate convex hull, but we can simply apply ST_Collect beforehand as recommended in the docs
        // https://postgis.net/docs/ST_ConvexHull.html
        return _sqlExpressionFactory.Function(
            "ST_ConvexHull",
            new[]
            {
                _sqlExpressionFactory.AggregateFunction(
                    "ST_Collect",
                    new[] { sqlExpression },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false },
                    source,
                    typeof(Geometry),
                    resultTypeMapping)
            },
            nullable: true,
            argumentsPropagateNullability: new[] { true },
            typeof(Geometry),
            resultTypeMapping);
    }
}
