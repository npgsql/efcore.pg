using System.Linq.Expressions;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using NetTopologySuite.Geometries;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite
{
    public class NetTopologySuiteMemberTranslator : IMemberTranslator
    {
        public Expression Translate(MemberExpression e)
        {
            var declaringType = e.Member.DeclaringType;

            if (declaringType == typeof(Point))
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
                case "GeometryType":
                    return new SqlFunctionExpression("GeometryType", typeof(string), new[] { e.Expression });
                case "IsClosed":
                    return new SqlFunctionExpression("ST_IsClosed", typeof(bool),       new[] { e.Expression });
                case "IsEmpty":
                    return new SqlFunctionExpression("ST_IsEmpty", typeof(bool),        new[] { e.Expression });
                case "IsSimple":
                    return new SqlFunctionExpression("ST_IsSimple", typeof(bool),       new[] { e.Expression });
                case "IsValid":
                    return new SqlFunctionExpression("ST_IsValid", typeof(bool),        new[] { e.Expression });
                case "Length":
                    return new SqlFunctionExpression("ST_Length", typeof(double),       new[] { e.Expression });
                case "NumGeometries":
                    return new SqlFunctionExpression("ST_NumGeometries", typeof(int),   new[] { e.Expression });
                case "NumPoints":
                    return new SqlFunctionExpression("ST_NumPoints", typeof(int),       new[] { e.Expression });
                default:
                    return null;
                }
            }

            return null;
        }
    }
}
