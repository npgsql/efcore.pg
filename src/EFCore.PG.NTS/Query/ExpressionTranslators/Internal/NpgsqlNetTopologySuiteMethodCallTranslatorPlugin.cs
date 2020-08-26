using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        static readonly bool[][] TrueArrays =
        {
            Array.Empty<bool>(),
            new[] { true },
            new[] { true, true },
            new[] { true, true, true }
        };

        public NpgsqlGeometryMethodTranslator(ISqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _typeMappingSource = typeMappingSource;
        }

        /// <inheritdoc />
        public SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => typeof(Geometry).IsAssignableFrom(method.DeclaringType)
                ? TranslateGeometryMethod(instance, method, arguments)
                : method.DeclaringType == typeof(NpgsqlNetTopologySuiteDbFunctionsExtensions)
                    ? TranslateDbFunction(instance, method, arguments)
                    : null;

        protected virtual SqlExpression TranslateDbFunction(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
            => method.Name switch
            {
                nameof(NpgsqlNetTopologySuiteDbFunctionsExtensions.Transform) => _sqlExpressionFactory.Function(
                    "ST_Transform",
                    new[] { arguments[1], arguments[2] },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    method.ReturnType,
                    arguments[1].TypeMapping),

                _ => null
            };

        protected virtual SqlExpression TranslateGeometryMethod(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
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

            if (Equals(method, _collectionItem))
            {
                return _sqlExpressionFactory.Function(
                    "ST_GeometryN",
                    new[] { instance, OneBased(arguments[0]) },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    method.ReturnType,
                    _typeMappingSource.FindMapping(typeof(Geometry), instance.TypeMapping.StoreType));
            }

            return method.Name switch
            {
            nameof(Geometry.AsBinary)            => Function("ST_AsBinary",       new[] { instance }, typeof(byte[])),
            nameof(Geometry.AsText)              => Function("ST_AsText",         new[] { instance }, typeof(string)),
            nameof(Geometry.Buffer)              => Function("ST_Buffer",         new[] { instance }.Concat(arguments).ToArray(), typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Contains)            => Function("ST_Contains",       new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.ConvexHull)          => Function("ST_ConvexHull",     new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.CoveredBy)           => Function("ST_CoveredBy",      new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Covers)              => Function("ST_Covers",         new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Crosses)             => Function("ST_Crosses",        new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Disjoint)            => Function("ST_Disjoint",       new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Difference)          => Function("ST_Difference",     new[] { instance, arguments[0] }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Distance)            => Function("ST_Distance",       new[] { instance, arguments[0] }, typeof(double)),
            nameof(Geometry.EqualsExact)         => Function("ST_OrderingEquals", new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.EqualsTopologically) => Function("ST_Equals",         new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.GetGeometryN)        => Function("ST_GeometryN",      new[] { instance, OneBased(arguments[0]) }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Polygon.GetInteriorRingN)     => Function("ST_InteriorRingN",  new[] { instance, OneBased(arguments[0]) }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(LineString.GetPointN)         => Function("ST_PointN",         new[] { instance, OneBased(arguments[0]) }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Intersection)        => Function("ST_Intersection",   new[] { instance, arguments[0] }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Intersects)          => Function("ST_Intersects",     new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.IsWithinDistance)    => Function("ST_DWithin",        new[] { instance, arguments[0], arguments[1] }, typeof(bool)),
            nameof(Geometry.Normalized)          => Function("ST_Normalize",      new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Overlaps)            => Function("ST_Overlaps",       new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Relate)              => Function("ST_Relate",         new[] { instance, arguments[0], arguments[1] }, typeof(bool)),
            nameof(Geometry.Reverse)             => Function("ST_Reverse",        new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.SymmetricDifference) => Function("ST_SymDifference",  new[] { instance, arguments[0] }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.ToBinary)            => Function("ST_AsBinary",       new[] { instance }, typeof(byte[])),
            nameof(Geometry.ToText)              => Function("ST_AsText",         new[] { instance }, typeof(string)),
            nameof(Geometry.Touches)             => Function("ST_Touches",        new[] { instance, arguments[0] }, typeof(bool)),
            nameof(Geometry.Within)              => Function("ST_Within",         new[] { instance, arguments[0] }, typeof(bool)),

            nameof(Geometry.Union) when arguments.Count == 0 => Function("ST_UnaryUnion", new[] { instance }, typeof(Geometry), resultGeometryTypeMapping),
            nameof(Geometry.Union) when arguments.Count == 1 => Function("ST_Union",      new[] { instance, arguments[0] }, typeof(Geometry), resultGeometryTypeMapping),

            _ => null
            };

            SqlFunctionExpression Function(string name, SqlExpression[] arguments, Type returnType, RelationalTypeMapping typeMapping = null)
                => _sqlExpressionFactory.Function(name, arguments,
                    nullable: true, argumentsPropagateNullability: TrueArrays[arguments.Length],
                    returnType, typeMapping);

            // NetTopologySuite uses 0-based indexing, but PostGIS uses 1-based
            SqlExpression OneBased(SqlExpression arg)
                => arg is SqlConstantExpression constant
                    ? _sqlExpressionFactory.Constant((int)constant.Value + 1, constant.TypeMapping)
                    : (SqlExpression)_sqlExpressionFactory.Add(arg, _sqlExpressionFactory.Constant(1));
        }
    }
}
