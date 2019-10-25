using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlNetTopologySuiteMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        public NpgsqlNetTopologySuiteMethodCallTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
            => Translators = new IMethodCallTranslator[]
            {
                new NpgsqlGeometryMethodTranslator(sqlExpressionFactory, typeMappingSource),
            };

        public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
    }

    /// <summary>
    /// Translates methods operating on types implementing the <see cref="IGeometry"/> interface.
    /// </summary>
    public class NpgsqlGeometryMethodTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _collectionItem =
            typeof(GeometryCollection).GetRuntimeProperty("Item").GetMethod;

        readonly ISqlExpressionFactory _sqlExpressionFactory;
        readonly IRelationalTypeMappingSource _typeMappingSource;

        public NpgsqlGeometryMethodTranslator(ISqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _typeMappingSource = typeMappingSource;
        }

        /// <inheritdoc />
        [CanBeNull]
        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (!typeof(Geometry).IsAssignableFrom(method.DeclaringType))
                return null;

            var typeMapping = ExpressionExtensions.InferTypeMapping(
                arguments.Prepend(instance).Where(e => typeof(Geometry).IsAssignableFrom(e.Type)).ToArray());

            Debug.Assert(typeMapping != null, "At least one argument must have typeMapping.");
            var storeType = typeMapping.StoreType;
            var resultGeometryTypeMapping = typeof(Geometry).IsAssignableFrom(method.ReturnType)
                ? _typeMappingSource.FindMapping(method.ReturnType, storeType)
                : null;

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
            nameof(Geometry.AsBinary)            => _sqlExpressionFactory.Function("ST_AsBinary",       new[] { instance }, typeof(byte[])),
            nameof(Geometry.AsText)              => _sqlExpressionFactory.Function("ST_AsText",         new[] { instance }, typeof(string)),
            nameof(Geometry.Buffer)              => _sqlExpressionFactory.Function("ST_Buffer",         new[] { instance }.Concat(arguments), typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Contains)            => _sqlExpressionFactory.Function("ST_Contains",       new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.ConvexHull)          => _sqlExpressionFactory.Function("ST_ConvexHull",     new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.CoveredBy)           => _sqlExpressionFactory.Function("ST_CoveredBy",      new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Covers)              => _sqlExpressionFactory.Function("ST_Covers",         new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Crosses)             => _sqlExpressionFactory.Function("ST_Crosses",        new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Disjoint)            => _sqlExpressionFactory.Function("ST_Disjoint",       new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Difference)          => _sqlExpressionFactory.Function("ST_Difference",     new[] { instance, arguments[0] }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Distance)            => _sqlExpressionFactory.Function("ST_Distance",       new[] { instance, arguments[0] }, typeof(double)),
            nameof(Geometry.EqualsExact)         => _sqlExpressionFactory.Function("ST_OrderingEquals", new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.EqualsTopologically) => _sqlExpressionFactory.Function("ST_Equals",         new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.GetGeometryN)        => _sqlExpressionFactory.Function("ST_GeometryN",      new[] { instance, OneBased(arguments[0]) }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Polygon.GetInteriorRingN)     => _sqlExpressionFactory.Function("ST_InteriorRingN",  new[] { instance, OneBased(arguments[0]) }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(LineString.GetPointN)         => _sqlExpressionFactory.Function("ST_PointN",         new[] { instance, OneBased(arguments[0]) }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Intersection)        => _sqlExpressionFactory.Function("ST_Intersection",   new[] { instance, arguments[0] }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Intersects)          => _sqlExpressionFactory.Function("ST_Intersects",     new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.IsWithinDistance)    => _sqlExpressionFactory.Function("ST_DWithin",        new[] { instance, arguments[0], arguments[1] }, typeof(bool)),
            nameof(Geometry.Overlaps)            => _sqlExpressionFactory.Function("ST_Overlaps",       new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Relate)              => _sqlExpressionFactory.Function("ST_Relate",         new[] { instance, arguments[0], arguments[1] }, typeof(bool)),
            nameof(Geometry.Reverse)             => _sqlExpressionFactory.Function("ST_Reverse",        new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.SymmetricDifference) => _sqlExpressionFactory.Function("ST_SymDifference",  new[] { instance, arguments[0] }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.ToBinary)            => _sqlExpressionFactory.Function("ST_AsBinary",       new[] { instance }, typeof(byte[])),
            nameof(Geometry.ToText)              => _sqlExpressionFactory.Function("ST_AsText",         new[] { instance }, typeof(string)),
            nameof(Geometry.Touches)             => _sqlExpressionFactory.Function("ST_Touches",        new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Within)              => _sqlExpressionFactory.Function("ST_Within",         new[] { instance, arguments[0] }, typeof(bool)),

            nameof(Geometry.Union) when arguments.Count == 0 => _sqlExpressionFactory.Function("ST_UnaryUnion", new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Union) when arguments.Count == 1 => _sqlExpressionFactory.Function("ST_Union",      new[] { instance, arguments[0] }, typeof(Geometry), resultGeometryTypeMapping),

            _ => method.OnInterface(typeof(GeometryCollection)) is MethodInfo collectionMethod && collectionMethod == null
                 ? _sqlExpressionFactory.Function("ST_GeometryN", new[] { instance, OneBased(arguments[0]) }, typeof(Geometry), resultGeometryTypeMapping)
                 : null
            };

            // NetTopologySuite uses 0-based indexing, but PostGIS uses 1-based
            SqlExpression OneBased(SqlExpression arg)
                => arg is SqlConstantExpression constant
                    ? _sqlExpressionFactory.Constant((int)constant.Value + 1, constant.TypeMapping)
                    : (SqlExpression)_sqlExpressionFactory.Add(arg, _sqlExpressionFactory.Constant(1));
        }
    }
}
