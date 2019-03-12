using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using NetTopologySuite.Geometries;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlNetTopologySuiteMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        public virtual IEnumerable<IMethodCallTranslator> Translators { get; } = new IMethodCallTranslator[]
        {
            new NpgsqlGeometryMethodTranslator()
        };
    }

    /// <summary>
    /// Translates methods operating on types implementing the <see cref="IGeometry"/> interface.
    /// </summary>
    public class NpgsqlGeometryMethodTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _collectionItem =
            typeof(IGeometryCollection).GetRuntimeProperty("Item").GetMethod;

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression e, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (!typeof(IGeometry).IsAssignableFrom(e.Method.DeclaringType))
                return null;

            switch (e.Method.Name)
            {
            case "AsBinary":
                return new SqlFunctionExpression("ST_AsBinary",      typeof(byte[]),   new[] { e.Object });
            case "AsText":
                return new SqlFunctionExpression("ST_AsText",        typeof(string),   new[] { e.Object });
            case "Buffer":
                return new SqlFunctionExpression("ST_Buffer",        typeof(Geometry), new[] { e.Object, e.Arguments[0] });
            case "Contains":
                return new SqlFunctionExpression("ST_Contains",      typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "ConvexHull":
                return new SqlFunctionExpression("ST_ConvexHull",    typeof(Geometry), new[] { e.Object });
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
                return GenerateOneBasedFunctionExpression("ST_GeometryN", e.Type, e.Object, e.Arguments[0]);
            case "GetInteriorRingN":
                return GenerateOneBasedFunctionExpression("ST_InteriorRingN", e.Type, e.Object, e.Arguments[0]);
            case "GetPointN":
                return GenerateOneBasedFunctionExpression("ST_PointN", e.Type, e.Object, e.Arguments[0]);
            case "Intersection":
                return new SqlFunctionExpression("ST_Intersection",  typeof(Geometry), new[] { e.Object, e.Arguments[0] });
            case "Intersects":
                return new SqlFunctionExpression("ST_Intersects",    typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "IsWithinDistance":
                return new SqlFunctionExpression("ST_DWithin",       typeof(bool),     new[] { e.Object, e.Arguments[0], e.Arguments[1] });
            case "Overlaps":
                return new SqlFunctionExpression("ST_Overlaps",      typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "Relate":
                return new SqlFunctionExpression("ST_Relate",        typeof(bool),     new[] { e.Object, e.Arguments[0], e.Arguments[1] });
            case "Reverse":
                return new SqlFunctionExpression("ST_Reverse",       typeof(Geometry), new[] { e.Object });
            case "SymmetricDifference":
                return new SqlFunctionExpression("ST_SymDifference", typeof(Geometry), new[] { e.Object, e.Arguments[0] });
            case "ToBinary":
                return new SqlFunctionExpression("ST_AsBinary",      typeof(byte[]),   new[] { e.Object });
            case "ToText":
                return new SqlFunctionExpression("ST_AsText",        typeof(string),   new[] { e.Object });
            case "Touches":
                return new SqlFunctionExpression("ST_Touches",       typeof(bool),     new[] { e.Object, e.Arguments[0] });
            case "Union" when e.Arguments.Count == 0:
                return null;  // ST_Union() with only one parameter is an aggregate function in PostGIS
            case "Union" when e.Arguments.Count == 1:
                return new SqlFunctionExpression("ST_Union",        typeof(Geometry),  new[] { e.Object, e.Arguments[0] });
            case "Within":
                return new SqlFunctionExpression("ST_Within",       typeof(bool),      new[] { e.Object, e.Arguments[0] });
            }

            // IGeometryCollection[index]
            var method = e.Method.OnInterface(typeof(IGeometryCollection));
            if (Equals(method, _collectionItem))
                return GenerateOneBasedFunctionExpression("ST_GeometryN", e.Type, e.Object, e.Arguments[0]);

            return null;
        }

        // NetTopologySuite uses 0-based indexing, but PostGIS uses 1-based
        static SqlFunctionExpression GenerateOneBasedFunctionExpression(
            string functionName, Type returnType, Expression obj, Expression arg)
            => new SqlFunctionExpression(functionName, returnType, new[]
            {
                obj,
                arg is ConstantExpression constant
                    ? (Expression)Expression.Constant((int)constant.Value + 1)
                    : Expression.Add(arg, Expression.Constant(1))
            });
    }
}
