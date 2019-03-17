using System.Collections.Generic;
using System.Linq.Expressions;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using NetTopologySuite.Geometries;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlNetTopologySuiteMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        public virtual IEnumerable<IMemberTranslator> Translators { get; } = new IMemberTranslator[]
        {
            new NpgsqlGeometryMemberTranslator()
        };
    }

    public class NpgsqlGeometryMemberTranslator : IMemberTranslator
    {
        public Expression Translate(MemberExpression e)
        {
            var declaringType = e.Member.DeclaringType;

            if (typeof(IPoint).IsAssignableFrom(declaringType))
            {
                switch (e.Member.Name)
                {
                case "X":
                    return new SqlFunctionExpression("ST_X", typeof(double), new[] { e.Expression });
                case "Y":
                    return new SqlFunctionExpression("ST_Y", typeof(double), new[] { e.Expression });
                case "Z":
                    return new SqlFunctionExpression("ST_Z", typeof(double), new[] { e.Expression });
                case "M":
                    return new SqlFunctionExpression("ST_M", typeof(double), new[] { e.Expression });
                }
            }

            if (typeof(ILineString).IsAssignableFrom(declaringType))
            {
                switch (e.Member.Name)
                {
                case "Count":
                    return new SqlFunctionExpression("ST_NumPoints", typeof(int), new[] { e.Expression });
                }
            }

            if (typeof(IGeometry).IsAssignableFrom(declaringType))
            {
                switch (e.Member.Name)
                {
                case "Area":
                    return new SqlFunctionExpression("ST_Area", typeof(double),         new[] { e.Expression });
                case "Boundary":
                    return new SqlFunctionExpression("ST_Boundary", typeof(IGeometry),  new[] { e.Expression });
                case "Centroid":
                    return new SqlFunctionExpression("ST_Centroid", typeof(Point),      new[] { e.Expression });
                case "Count":
                    return new SqlFunctionExpression("ST_NumGeometries", typeof(int),   new[] { e.Expression });
                case "Dimension":
                    return new SqlFunctionExpression("ST_Dimension", typeof(Dimension), new[] { e.Expression });
                case "EndPoint":
                    return new SqlFunctionExpression("ST_EndPoint", typeof(Point),      new[] { e.Expression });
                case "Envelope":
                    return new SqlFunctionExpression("ST_Envelope", typeof(IGeometry),  new[] { e.Expression });
                case "ExteriorRing":
                    return new SqlFunctionExpression("ST_ExteriorRing", typeof(ILineString),  new[] { e.Expression });
                case "GeometryType":
                    return new SqlFunctionExpression("GeometryType", typeof(string), new[] { e.Expression });
                case "IsClosed":
                    return new SqlFunctionExpression("ST_IsClosed", typeof(bool),       new[] { e.Expression });
                case "IsEmpty":
                    return new SqlFunctionExpression("ST_IsEmpty", typeof(bool),        new[] { e.Expression });
                case "IsRing":
                    return new SqlFunctionExpression("ST_IsRing",  typeof(bool),        new[] { e.Expression });
                case "IsSimple":
                    return new SqlFunctionExpression("ST_IsSimple", typeof(bool),       new[] { e.Expression });
                case "IsValid":
                    return new SqlFunctionExpression("ST_IsValid", typeof(bool),        new[] { e.Expression });
                case "Length":
                    return new SqlFunctionExpression("ST_Length", typeof(double),       new[] { e.Expression });
                case "NumGeometries":
                    return new SqlFunctionExpression("ST_NumGeometries", typeof(int),   new[] { e.Expression });
                case "NumInteriorRings":
                    return new SqlFunctionExpression("ST_NumInteriorRings", typeof(int), new[] { e.Expression });
                case "NumPoints":
                    return new SqlFunctionExpression("ST_NumPoints", typeof(int),       new[] { e.Expression });
                case "OgcGeometryType":
                {
                    var whenThenList = new List<CaseWhenClause>
                    {
                        new CaseWhenClause(Expression.Constant("ST_CircularString"), Expression.Constant(OgcGeometryType.CircularString)),
                        new CaseWhenClause(Expression.Constant("ST_CompoundCurve"), Expression.Constant(OgcGeometryType.CompoundCurve)),
                        new CaseWhenClause(Expression.Constant("ST_CurvePolygon"), Expression.Constant(OgcGeometryType.CurvePolygon)),
                        new CaseWhenClause(Expression.Constant("ST_GeometryCollection"), Expression.Constant(OgcGeometryType.GeometryCollection)),
                        new CaseWhenClause(Expression.Constant("ST_LineString"), Expression.Constant(OgcGeometryType.LineString)),
                        new CaseWhenClause(Expression.Constant("ST_MultiCurve"), Expression.Constant(OgcGeometryType.MultiCurve)),
                        new CaseWhenClause(Expression.Constant("ST_MultiLineString"), Expression.Constant(OgcGeometryType.MultiLineString)),
                        new CaseWhenClause(Expression.Constant("ST_MultiPoint"), Expression.Constant(OgcGeometryType.MultiPoint)),
                        new CaseWhenClause(Expression.Constant("ST_MultiPolygon"), Expression.Constant(OgcGeometryType.MultiPolygon)),
                        new CaseWhenClause(Expression.Constant("ST_MultiSurface"), Expression.Constant(OgcGeometryType.MultiSurface)),
                        new CaseWhenClause(Expression.Constant("ST_Point"), Expression.Constant(OgcGeometryType.Point)),
                        new CaseWhenClause(Expression.Constant("ST_Polygon"), Expression.Constant(OgcGeometryType.Polygon)),
                        new CaseWhenClause(Expression.Constant("ST_PolyhedralSurface"), Expression.Constant(OgcGeometryType.PolyhedralSurface)),
                        new CaseWhenClause(Expression.Constant("ST_Tin"), Expression.Constant(OgcGeometryType.TIN))
                    };

                    return new CaseExpression(
                        new SqlFunctionExpression("ST_GeometryType", typeof(string), new[] { e.Expression }),
                        whenThenList.ToArray());
                }
                case "PointOnSurface":
                case "InteriorPoint":
                    return new SqlFunctionExpression("ST_PointOnSurface", typeof(IGeometry), new[] { e.Expression });
                case "SRID":
                    return new SqlFunctionExpression("ST_SRID", typeof(int),            new[] { e.Expression });
                case "StartPoint":
                    return new SqlFunctionExpression("ST_StartPoint", typeof(IPoint),   new[] { e.Expression });
                default:
                    return null;
                }
            }

            return null;
        }
    }
}
