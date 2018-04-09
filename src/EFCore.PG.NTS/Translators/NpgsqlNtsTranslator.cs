using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using NetTopologySuite.Geometries;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Translators
{
    public class NpgsqlNtsTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo Contains
            = typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Contains), new[] { typeof(IGeometry) });
        static readonly MethodInfo Covers
            = typeof(Geometry).GetRuntimeMethod(nameof(Geometry.Covers), new[] { typeof(IGeometry) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Geometry))
            {
                if (methodCallExpression.Method.Equals(Covers))
                {
                    return new SqlFunctionExpression("ST_Covers", typeof(bool), new[]
                    {
                        methodCallExpression.Object,
                        methodCallExpression.Arguments[0]
                    });
                }
                if (methodCallExpression.Method.Equals(Contains))
                {
                    return new SqlFunctionExpression("ST_Contains", typeof(bool), new[]
                    {
                        methodCallExpression.Object,
                        methodCallExpression.Arguments[0]
                    });
                }
            }

            return null;
        }
    }
}
