using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlNetTopologySuiteMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        public NpgsqlNetTopologySuiteMemberTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
            => Translators = new IMemberTranslator[]
            {
                new NpgsqlGeometryMemberTranslator(sqlExpressionFactory, typeMappingSource),
            };

        public virtual IEnumerable<IMemberTranslator> Translators { get; }
    }

    public class NpgsqlGeometryMemberTranslator : IMemberTranslator
    {
        readonly ISqlExpressionFactory _sqlExpressionFactory;
        readonly IRelationalTypeMappingSource _typeMappingSource;
        readonly CaseWhenClause[] _ogcGeometryTypeWhenThenList;

        static readonly bool[][] TrueArrays =
        {
            Array.Empty<bool>(),
            new[] { true },
            new[] { true, true },
            new[] { true, true, true }
        };

        public NpgsqlGeometryMemberTranslator(ISqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _typeMappingSource = typeMappingSource;

            _ogcGeometryTypeWhenThenList = new[]
            {
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_CircularString"),     _sqlExpressionFactory.Constant(OgcGeometryType.CircularString)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_CompoundCurve"),      _sqlExpressionFactory.Constant(OgcGeometryType.CompoundCurve)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_CurvePolygon"),       _sqlExpressionFactory.Constant(OgcGeometryType.CurvePolygon)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_GeometryCollection"), _sqlExpressionFactory.Constant(OgcGeometryType.GeometryCollection)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_LineString"),         _sqlExpressionFactory.Constant(OgcGeometryType.LineString)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_MultiCurve"),         _sqlExpressionFactory.Constant(OgcGeometryType.MultiCurve)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_MultiLineString"),    _sqlExpressionFactory.Constant(OgcGeometryType.MultiLineString)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_MultiPoint"),         _sqlExpressionFactory.Constant(OgcGeometryType.MultiPoint)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_MultiPolygon"),       _sqlExpressionFactory.Constant(OgcGeometryType.MultiPolygon)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_MultiSurface"),       _sqlExpressionFactory.Constant(OgcGeometryType.MultiSurface)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_Point"),              _sqlExpressionFactory.Constant(OgcGeometryType.Point)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_Polygon"),            _sqlExpressionFactory.Constant(OgcGeometryType.Polygon)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_PolyhedralSurface"),  _sqlExpressionFactory.Constant(OgcGeometryType.PolyhedralSurface)),
                new CaseWhenClause(_sqlExpressionFactory.Constant("ST_Tin"),                _sqlExpressionFactory.Constant(OgcGeometryType.TIN))
            };
        }

        public SqlExpression Translate(
            SqlExpression instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            var declaringType = member.DeclaringType;

            if (!typeof(Geometry).IsAssignableFrom(declaringType))
                return null;

            var typeMapping = instance.TypeMapping;
            Debug.Assert(typeMapping != null, "Instance must have typeMapping assigned.");
            var storeType = instance.TypeMapping.StoreType;
            var resultGeometryTypeMapping = typeof(Geometry).IsAssignableFrom(returnType)
                ? _typeMappingSource.FindMapping(returnType, storeType)
                : null;

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

                if (function != null)
                    return Function(function, new[] { instance }, typeof(double));
            }

            if (typeof(LineString).IsAssignableFrom(declaringType))
            {
                if (member.Name == "Count")
                    return Function("ST_NumPoints", new[] { instance }, typeof(int));
            }

            return member.Name switch
            {
            nameof(Geometry.Area)            => Function("ST_Area",             new[] { instance }, typeof(double)),
            nameof(Geometry.Boundary)        => Function("ST_Boundary",         new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Centroid)        => Function("ST_Centroid",         new[] { instance }, typeof(Point), resultGeometryTypeMapping),
            nameof(GeometryCollection.Count) => Function("ST_NumGeometries",    new[] { instance }, typeof(int)),
            nameof(Geometry.Dimension)       => Function("ST_Dimension",        new[] { instance }, typeof(Dimension)),
            nameof(LineString.EndPoint)      => Function("ST_EndPoint",         new[] { instance }, typeof(Point), resultGeometryTypeMapping),
            nameof(Geometry.Envelope)        => Function("ST_Envelope",         new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Polygon.ExteriorRing)     => Function("ST_ExteriorRing",     new[] { instance }, typeof(LineString), resultGeometryTypeMapping),
            nameof(Geometry.GeometryType)    => Function("GeometryType",        new[] { instance }, typeof(string)),
            nameof(LineString.IsClosed)      => Function("ST_IsClosed",         new[] { instance }, typeof(bool)),
            nameof(Geometry.IsEmpty)         => Function("ST_IsEmpty",          new[] { instance }, typeof(bool)),
            nameof(LineString.IsRing)        => Function("ST_IsRing",           new[] { instance }, typeof(bool)),
            nameof(Geometry.IsSimple)        => Function("ST_IsSimple",         new[] { instance }, typeof(bool)),
            nameof(Geometry.IsValid)         => Function("ST_IsValid",          new[] { instance }, typeof(bool)),
            nameof(Geometry.Length)          => Function("ST_Length",           new[] { instance }, typeof(double)),
            nameof(Geometry.NumGeometries)   => Function("ST_NumGeometries",    new[] { instance }, typeof(int)),
            nameof(Polygon.NumInteriorRings) => Function("ST_NumInteriorRings", new[] { instance }, typeof(int)),
            nameof(Geometry.NumPoints)       => Function("ST_NumPoints",        new[] { instance }, typeof(int)),
            nameof(Geometry.PointOnSurface)  => Function("ST_PointOnSurface",   new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.InteriorPoint)   => Function("ST_PointOnSurface",   new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.SRID)            => Function("ST_SRID",             new[] { instance }, typeof(int)),
            nameof(LineString.StartPoint)    => Function("ST_StartPoint",       new[] { instance }, typeof(Point), resultGeometryTypeMapping),

            nameof(Geometry.OgcGeometryType)  => _sqlExpressionFactory.Case(
                Function("ST_GeometryType", new[] { instance }, typeof(string)),
                _ogcGeometryTypeWhenThenList,
                elseResult: null),

            _ => null
            };

            SqlFunctionExpression Function(string name, SqlExpression[] arguments, Type returnType, RelationalTypeMapping typeMapping = null)
                => _sqlExpressionFactory.Function(name, arguments,
                    nullable: true, argumentsPropagateNullability: TrueArrays[arguments.Length],
                    returnType, typeMapping);
        }
    }
}
