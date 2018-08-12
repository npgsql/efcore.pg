using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using NetTopologySuite.Geometries;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite
{
    public class NetTopologySuiteMethodCallTranslator : IMethodCallTranslator
    {
        public virtual Expression Translate(MethodCallExpression e)
        {
            if (!typeof(IGeometry).IsAssignableFrom(e.Method.DeclaringType))
                return null;

            switch (e.Method.Name)
            {
            case "AsText":
                return new SqlFunctionExpression("ST_AsText",        typeof(string),   new[] { e.Object });
            case "Contains":
                return new SqlFunctionExpression("ST_Contains",      typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "CoveredBy":
                return new SqlFunctionExpression("ST_CoveredBy",     typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "Covers":
                return new SqlFunctionExpression("ST_Covers",        typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "Crosses":
                return new SqlFunctionExpression("ST_Crosses",       typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "Disjoint":
                return new SqlFunctionExpression("ST_Disjoint",      typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "Difference":
                return new SqlFunctionExpression("ST_Difference",    typeof(Geometry), new[] { e.Object, e.Arguments[0] });
            case "Distance":
                return new SqlFunctionExpression("ST_Distance",      typeof(double),   new[] { e.Object, e.Arguments[0] });
            case "EqualsExact":
                return Expression.Equal(e.Object, e.Arguments[0]);
            case "EqualsTopologically":
                return new SqlFunctionExpression("ST_Equals",        typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "GetGeometryN":
                // NetTopologySuite uses 0-based indexing, but PostGIS uses 1-based
                return new SqlFunctionExpression("ST_GeometryN", typeof(Geometry), new[]
                {
                    e.Object,
                    e.Arguments[0] is ConstantExpression constant
                        ? (Expression)Expression.Constant((int)constant.Value + 1)
                        : Expression.Add(e.Arguments[0], Expression.Constant(1))
                });
            case "Intersection":
                return new SqlFunctionExpression("ST_Intersection",  typeof(Geometry), new[] { e.Object, e.Arguments[0] });
            case "Intersects":
                return new SqlFunctionExpression("ST_Intersects",    typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "Overlaps":
                return new SqlFunctionExpression("ST_Overlaps",      typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "Relate":
                return new SqlFunctionExpression("ST_Relate",        typeof(bool),     new[] { e.Object, e.Arguments[0], e.Arguments[1] });
            case "Reverse":
                return new SqlFunctionExpression("ST_Reverse",       typeof(Geometry), new[] { e.Object });
            case "SymmetricDifference":
                return new SqlFunctionExpression("ST_SymDifference", typeof(Geometry), new[] { e.Object, e.Arguments[0] });
            case "ToText":
                return new SqlFunctionExpression("ST_AsText",        typeof(string),   new[] { e.Object });
            case "Touches":
                return new SqlFunctionExpression("ST_Touches",       typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "Union":
                return new SqlFunctionExpression("ST_Union",        typeof(Geometry),  new[] { e.Object, e.Arguments[0] });
            case "Within":
                return new SqlFunctionExpression("ST_Within",       typeof(bool),      new[] { e.Object, e.Arguments[0] });
            default:
                return null;
            }
        }
    }
}
