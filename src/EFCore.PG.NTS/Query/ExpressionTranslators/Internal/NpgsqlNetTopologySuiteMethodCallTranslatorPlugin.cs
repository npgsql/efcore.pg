using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlNetTopologySuiteMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlNetTopologySuiteMethodCallTranslatorPlugin(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        if (sqlExpressionFactory is not NpgsqlSqlExpressionFactory npgsqlSqlExpressionFactory)
        {
            throw new ArgumentException($"Must be an {nameof(NpgsqlSqlExpressionFactory)}", nameof(sqlExpressionFactory));
        }

        Translators = [new NpgsqlGeometryMethodTranslator(npgsqlSqlExpressionFactory, typeMappingSource)];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlGeometryMethodTranslator : IMethodCallTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    private static readonly bool[][] TrueArrays =
    [
        [], [true], [true, true], [true, true, true], [true, true, true, true]
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlGeometryMethodTranslator(
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
        => method.DeclaringType switch
        {
            var t when typeof(Geometry).IsAssignableFrom(t) && instance is not null
                => TranslateGeometryMethod(instance, method, arguments),

            var t when t == typeof(NpgsqlNetTopologySuiteDbFunctionsExtensions)
                => TranslateDbFunction(method, arguments),

            // This handles the collection indexer (geom_collection[x] -> ST_GeometryN(geom_collection, x + 1))
            // This is needed as a special case because EF transforms the indexer into a call to Enumerable.ElementAt
            var t when t == typeof(Enumerable)
                && method.Name is nameof(Enumerable.ElementAt)
                && method.ReturnType == typeof(Geometry)
                && arguments is [var collection, var index]
                && _typeMappingSource.FindMapping(typeof(Geometry), collection.TypeMapping!.StoreType) is RelationalTypeMapping geometryTypeMapping
                    => _sqlExpressionFactory.Function(
                        "ST_GeometryN",
                        [collection, OneBased(index)],
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        method.ReturnType,
                        geometryTypeMapping),

            _ => null
        };

    private SqlExpression? TranslateDbFunction(
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            nameof(NpgsqlNetTopologySuiteDbFunctionsExtensions.Transform) => _sqlExpressionFactory.Function(
                "ST_Transform",
                [arguments[1], arguments[2]],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                method.ReturnType,
                arguments[1].TypeMapping),

            nameof(NpgsqlNetTopologySuiteDbFunctionsExtensions.Force2D) => _sqlExpressionFactory.Function(
                "ST_Force2D",
                [arguments[1]],
                nullable: true,
                TrueArrays[1],
                method.ReturnType,
                arguments[1].TypeMapping),

            nameof(NpgsqlNetTopologySuiteDbFunctionsExtensions.DistanceKnn) => _sqlExpressionFactory.MakePostgresBinary(
                PgExpressionType.Distance,
                arguments[1],
                arguments[2]),

            nameof(NpgsqlNetTopologySuiteDbFunctionsExtensions.Distance) =>
                TranslateGeometryMethod(arguments[1], method, [arguments[2], arguments[3]]),
            nameof(NpgsqlNetTopologySuiteDbFunctionsExtensions.IsWithinDistance) =>
                TranslateGeometryMethod(arguments[1], method, [arguments[2], arguments[3], arguments[4]]),

            _ => null
        };

    private SqlExpression? TranslateGeometryMethod(
        SqlExpression instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        var typeMapping = ExpressionExtensions.InferTypeMapping(
            arguments.Prepend(instance).Where(e => typeof(Geometry).IsAssignableFrom(e.Type)).ToArray());

        Debug.Assert(typeMapping is not null, "At least one argument must have typeMapping.");
        var storeType = typeMapping.StoreType;

        instance = _sqlExpressionFactory.ApplyTypeMapping(instance, _typeMappingSource.FindMapping(instance.Type, storeType));

        var typeMappedArguments = new List<SqlExpression>();
        foreach (var argument in arguments)
        {
            typeMappedArguments.Add(
                _sqlExpressionFactory.ApplyTypeMapping(
                    argument,
                    typeof(Geometry).IsAssignableFrom(argument.Type)
                        ? _typeMappingSource.FindMapping(argument.Type, storeType)
                        : _typeMappingSource.FindMapping(argument.Type)));
        }

        arguments = typeMappedArguments;

        return method.Name switch
        {
            nameof(Geometry.AsBinary)
                => Function("ST_AsBinary", [instance], typeof(byte[])),
            nameof(Geometry.AsText)
                => Function("ST_AsText", [instance], typeof(string)),
            nameof(Geometry.Buffer)
                => Function("ST_Buffer", new[] { instance }.Concat(arguments).ToArray(), typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.Contains)
                => Function("ST_Contains", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.ConvexHull)
                => Function("ST_ConvexHull", [instance], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.CoveredBy)
                => Function("ST_CoveredBy", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.Covers)
                => Function("ST_Covers", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.Crosses)
                => Function("ST_Crosses", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.Disjoint)
                => Function("ST_Disjoint", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.Difference)
                => Function("ST_Difference", [instance, arguments[0]], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.Distance)
                => Function("ST_Distance", new[] { instance }.Concat(arguments).ToArray(), typeof(double)),
            nameof(Geometry.EqualsExact)
                => Function("ST_OrderingEquals", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.EqualsTopologically)
                => Function("ST_Equals", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.GetGeometryN)
                => Function("ST_GeometryN", [instance, OneBased(arguments[0])], typeof(Geometry), ResultGeometryMapping()),
            nameof(Polygon.GetInteriorRingN)
                => Function("ST_InteriorRingN", [instance, OneBased(arguments[0])], typeof(Geometry), ResultGeometryMapping()),
            nameof(LineString.GetPointN)
                => Function("ST_PointN", [instance, OneBased(arguments[0])], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.Intersection)
                => Function("ST_Intersection", [instance, arguments[0]], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.Intersects)
                => Function("ST_Intersects", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.IsWithinDistance)
                => Function("ST_DWithin", new[] { instance }.Concat(arguments).ToArray(), typeof(bool)),
            nameof(Geometry.Normalized)
                => Function("ST_Normalize", [instance], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.Overlaps)
                => Function("ST_Overlaps", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.Relate)
                => Function("ST_Relate", [instance, arguments[0], arguments[1]], typeof(bool)),
            nameof(Geometry.Reverse)
                => Function("ST_Reverse", [instance], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.SymmetricDifference)
                => Function("ST_SymDifference", [instance, arguments[0]], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.ToBinary)
                => Function("ST_AsBinary", [instance], typeof(byte[])),
            nameof(Geometry.ToText)
                => Function("ST_AsText", [instance], typeof(string)),
            nameof(Geometry.Touches)
                => Function("ST_Touches", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.Within)
                => Function("ST_Within", [instance, arguments[0]], typeof(bool)),
            nameof(Geometry.Union) when arguments.Count == 0
                => Function("ST_UnaryUnion", [instance], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.Union) when arguments.Count == 1
                => Function("ST_Union", [instance, arguments[0]], typeof(Geometry), ResultGeometryMapping()),

            _ => null
        };

        SqlExpression Function(string name, SqlExpression[] arguments, Type returnType, RelationalTypeMapping? typeMapping = null)
            => _sqlExpressionFactory.Function(
                name, arguments,
                nullable: true, argumentsPropagateNullability: TrueArrays[arguments.Length],
                returnType, typeMapping);

        RelationalTypeMapping ResultGeometryMapping()
        {
            Debug.Assert(typeof(Geometry).IsAssignableFrom(method.ReturnType));
            return _typeMappingSource.FindMapping(method.ReturnType, storeType)!;
        }
    }

    // NetTopologySuite uses 0-based indexing, but PostGIS uses 1-based
    private SqlExpression OneBased(SqlExpression arg)
        => arg is SqlConstantExpression constant
            ? _sqlExpressionFactory.Constant((int)constant.Value! + 1, constant.TypeMapping)
            : _sqlExpressionFactory.Add(arg, _sqlExpressionFactory.Constant(1));
}
