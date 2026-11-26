// ReSharper disable once CheckNamespace

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlNetTopologySuiteMemberTranslatorPlugin : IMemberTranslatorPlugin
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlNetTopologySuiteMemberTranslatorPlugin(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        Translators = [new NpgsqlGeometryMemberTranslator(sqlExpressionFactory, typeMappingSource)];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IMemberTranslator> Translators { get; }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlGeometryMemberTranslator : IMemberTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly CaseWhenClause[] _ogcGeometryTypeWhenThenList;

    private static readonly bool[][] TrueArrays = [[], [true], [true, true], [true, true, true]];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlGeometryMemberTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;

        _ogcGeometryTypeWhenThenList =
        [
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_CircularString"), _sqlExpressionFactory.Constant(OgcGeometryType.CircularString)),
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_CompoundCurve"), _sqlExpressionFactory.Constant(OgcGeometryType.CompoundCurve)),
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_CurvePolygon"), _sqlExpressionFactory.Constant(OgcGeometryType.CurvePolygon)),
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_GeometryCollection"),
                _sqlExpressionFactory.Constant(OgcGeometryType.GeometryCollection)),
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_LineString"), _sqlExpressionFactory.Constant(OgcGeometryType.LineString)),
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_MultiCurve"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiCurve)),
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_MultiLineString"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiLineString)),
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_MultiPoint"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPoint)),
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_MultiPolygon"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiPolygon)),
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_MultiSurface"), _sqlExpressionFactory.Constant(OgcGeometryType.MultiSurface)),
            new CaseWhenClause(_sqlExpressionFactory.Constant("ST_Point"), _sqlExpressionFactory.Constant(OgcGeometryType.Point)),
            new CaseWhenClause(_sqlExpressionFactory.Constant("ST_Polygon"), _sqlExpressionFactory.Constant(OgcGeometryType.Polygon)),
            new CaseWhenClause(
                _sqlExpressionFactory.Constant("ST_PolyhedralSurface"),
                _sqlExpressionFactory.Constant(OgcGeometryType.PolyhedralSurface)),
            new CaseWhenClause(_sqlExpressionFactory.Constant("ST_Tin"), _sqlExpressionFactory.Constant(OgcGeometryType.TIN))
        ];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var declaringType = member.DeclaringType;

        if (instance is null || !typeof(Geometry).IsAssignableFrom(declaringType))
        {
            return null;
        }

        var instanceType = instance.Type;
        if (member.Name == nameof(Geometry.Length))
        {
            // CLR casts between geometry subtypes must not change a geography operand into geometry.
            while (instance is SqlUnaryExpression { OperatorType: ExpressionType.Convert } unary
                   && unary.Type != unary.Operand.Type
                   && typeof(Geometry).IsAssignableFrom(unary.Operand.Type))
            {
                instance = unary.Operand;
            }
        }

        var typeMapping = instance.TypeMapping;
        Debug.Assert(typeMapping is not null, "Instance must have typeMapping assigned.");
        var storeType = typeMapping!.StoreType;
        var isGeography = member.Name == nameof(Geometry.Length)
            && storeType.AsSpan().TrimStart().StartsWith("geography", StringComparison.OrdinalIgnoreCase);

        if (typeof(Point).IsAssignableFrom(declaringType))
        {
            var function = member.Name switch
            {
                nameof(Point.X) => "ST_X",
                nameof(Point.Y) => "ST_Y",
                nameof(Point.Z) => "ST_Z",
                nameof(Point.M) => "ST_M",
                _ => null
            };

            if (function is not null)
            {
                return Function(function, [instance], typeof(double));
            }
        }

        if (typeof(LineString).IsAssignableFrom(declaringType))
        {
            if (member.Name == "Count")
            {
                return Function("ST_NumPoints", [instance], typeof(int));
            }
        }

        return member.Name switch
        {
            nameof(Geometry.Area) => Function("ST_Area", [instance], typeof(double)),
            nameof(Geometry.Boundary) => Function("ST_Boundary", [instance], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.Centroid) => Function("ST_Centroid", [instance], typeof(Point), ResultGeometryMapping()),
            nameof(GeometryCollection.Count) => Function("ST_NumGeometries", [instance], typeof(int)),
            nameof(Geometry.Dimension) => Function("ST_Dimension", [instance], typeof(Dimension)),
            nameof(LineString.EndPoint) => Function("ST_EndPoint", [instance], typeof(Point), ResultGeometryMapping()),
            nameof(Geometry.Envelope) => Function("ST_Envelope", [instance], typeof(Geometry), ResultGeometryMapping()),
            nameof(Polygon.ExteriorRing) => Function("ST_ExteriorRing", [instance], typeof(LineString), ResultGeometryMapping()),
            nameof(Geometry.GeometryType) => Function("GeometryType", [instance], typeof(string)),
            nameof(LineString.IsClosed) => Function("ST_IsClosed", [instance], typeof(bool)),
            nameof(Geometry.IsEmpty) => Function("ST_IsEmpty", [instance], typeof(bool)),
            nameof(LineString.IsRing) => Function("ST_IsRing", [instance], typeof(bool)),
            nameof(Geometry.IsSimple) => Function("ST_IsSimple", [instance], typeof(bool)),
            nameof(Geometry.IsValid) => Function("ST_IsValid", [instance], typeof(bool)),
            // Collections can contain both lines and polygons; MultiPolygon also inherits GeometryCollection.
            nameof(Geometry.Length) when typeof(Polygon).IsAssignableFrom(instanceType)
                || typeof(MultiPolygon).IsAssignableFrom(instanceType)
                => Function("ST_Perimeter", [instance], typeof(double)),
            nameof(Geometry.Length) when instanceType == typeof(GeometryCollection)
                => _sqlExpressionFactory.Add(
                    Function("ST_Length", [instance], typeof(double)),
                    Function("ST_Perimeter", [instance], typeof(double))),
            nameof(Geometry.Length) when instanceType == typeof(Geometry)
                => LengthByGeometryType(),
            nameof(Geometry.Length) => Function("ST_Length", [instance], typeof(double)),
            nameof(Geometry.NumGeometries) => Function("ST_NumGeometries", [instance], typeof(int)),
            nameof(Polygon.NumInteriorRings) => Function("ST_NumInteriorRings", [instance], typeof(int)),
            nameof(Geometry.NumPoints) => Function("ST_NumPoints", [instance], typeof(int)),
            nameof(Geometry.PointOnSurface) => Function("ST_PointOnSurface", [instance], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.InteriorPoint) => Function("ST_PointOnSurface", [instance], typeof(Geometry), ResultGeometryMapping()),
            nameof(Geometry.SRID) => Function("ST_SRID", [instance], typeof(int)),
            nameof(LineString.StartPoint) => Function("ST_StartPoint", [instance], typeof(Point), ResultGeometryMapping()),

            nameof(Geometry.OgcGeometryType) => _sqlExpressionFactory.Case(
                Function("ST_GeometryType", [instance], typeof(string)),
                _ogcGeometryTypeWhenThenList,
                elseResult: null),

            _ => null
        };

        SqlExpression Function(string name, SqlExpression[] arguments, Type returnType, RelationalTypeMapping? typeMapping = null)
            => _sqlExpressionFactory.Function(
                name, arguments,
                nullable: true, argumentsPropagateNullability: TrueArrays[arguments.Length],
                returnType, typeMapping);

        RelationalTypeMapping ResultGeometryMapping()
        {
            Debug.Assert(typeof(Geometry).IsAssignableFrom(returnType));
            return _typeMappingSource.FindMapping(returnType, storeType)!;
        }

        SqlExpression LengthByGeometryType()
        {
            var length = Function("ST_Length", [instance], typeof(double));
            var perimeter = Function("ST_Perimeter", [instance], typeof(double));

            if (isGeography)
            {
                // GeometryType has a geography overload and appends M for XYM values.
                var collectionLength = _sqlExpressionFactory.Add(length, perimeter);

                return _sqlExpressionFactory.Case(
                    Function("GeometryType", [instance], typeof(string)),
                    [
                        new CaseWhenClause(_sqlExpressionFactory.Constant("POLYGON"), perimeter),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("POLYGONM"), perimeter),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("MULTIPOLYGON"), perimeter),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("MULTIPOLYGONM"), perimeter),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("GEOMETRYCOLLECTION"), collectionLength),
                        new CaseWhenClause(_sqlExpressionFactory.Constant("GEOMETRYCOLLECTIONM"), collectionLength)
                    ],
                    length);
            }

            return _sqlExpressionFactory.Case(
                Function("ST_GeometryType", [instance], typeof(string)),
                [
                    new CaseWhenClause(_sqlExpressionFactory.Constant("ST_Polygon"), perimeter),
                    new CaseWhenClause(_sqlExpressionFactory.Constant("ST_MultiPolygon"), perimeter),
                    new CaseWhenClause(
                        _sqlExpressionFactory.Constant("ST_GeometryCollection"),
                        _sqlExpressionFactory.Add(length, perimeter))
                ],
                length);
        }
    }
}
