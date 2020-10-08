using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NpgsqlSqlExpressionFactory : SqlExpressionFactory
    {
        readonly IRelationalTypeMappingSource _typeMappingSource;
        readonly RelationalTypeMapping _boolTypeMapping;
        readonly RelationalTypeMapping _intervalTypeMapping;

        public NpgsqlSqlExpressionFactory([NotNull] SqlExpressionFactoryDependencies dependencies)
            : base(dependencies)
        {
            _typeMappingSource = dependencies.TypeMappingSource;
            _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool));
            _intervalTypeMapping = _typeMappingSource.FindMapping("interval");
        }

        #region Expression factory methods

        public virtual PostgresRegexMatchExpression RegexMatch(
            [NotNull] SqlExpression match, [NotNull] SqlExpression pattern, RegexOptions options)
            => (PostgresRegexMatchExpression)ApplyDefaultTypeMapping(new PostgresRegexMatchExpression(match, pattern, options, null));

        public virtual PostgresAnyExpression Any(
            [NotNull] SqlExpression item,
            [NotNull] SqlExpression array,
            PostgresAnyOperatorType operatorType)
            => (PostgresAnyExpression)ApplyDefaultTypeMapping(new PostgresAnyExpression(item, array, operatorType, null));

        public virtual PostgresAllExpression All(
            [NotNull] SqlExpression item,
            [NotNull] SqlExpression array,
            PostgresAllOperatorType operatorType)
            => (PostgresAllExpression)ApplyDefaultTypeMapping(new PostgresAllExpression(item, array, operatorType, null));

        public virtual PostgresArrayIndexExpression ArrayIndex(
            [NotNull] SqlExpression array,
            [NotNull] SqlExpression index,
            [CanBeNull]  RelationalTypeMapping typeMapping = null)
        {
            if (!array.Type.TryGetElementType(out var elementType))
                throw new ArgumentException("Array expression must be of an array or List<> type", nameof(array));

            return (PostgresArrayIndexExpression)ApplyTypeMapping(
                new PostgresArrayIndexExpression(array, index, elementType, typeMapping: null),
                typeMapping);
        }

        public virtual PostgresBinaryExpression AtTimeZone(
            [NotNull] SqlExpression timestamp,
            [NotNull] SqlExpression timeZone,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping = null)
        {
            // PostgreSQL AT TIME ZONE flips the given type from timestamptz to timestamp and vice versa
            // See https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT
            typeMapping ??= FlipTimestampTypeMapping(timestamp.TypeMapping ?? _typeMappingSource.FindMapping(timestamp.Type));

            return new PostgresBinaryExpression(
                PostgresExpressionType.AtTimeZone,
                ApplyDefaultTypeMapping(timestamp),
                ApplyDefaultTypeMapping(timeZone),
                type,
                typeMapping);

            RelationalTypeMapping FlipTimestampTypeMapping(RelationalTypeMapping mapping)
            {
                var storeType = mapping.StoreType;
                if (storeType.StartsWith("timestamp with time zone") || storeType.StartsWith("timestamptz"))
                    return _typeMappingSource.FindMapping("timestamp without time zone");
                if (storeType.StartsWith("timestamp without time zone") || storeType.StartsWith("timestamp"))
                    return _typeMappingSource.FindMapping("timestamp with time zone");
                throw new ArgumentException($"timestamp argument to AtTimeZone had unknown store type {storeType}", nameof(timestamp));
            }
        }

        public virtual PostgresILikeExpression ILike(
            [NotNull] SqlExpression match,
            [NotNull] SqlExpression pattern,
            [CanBeNull] SqlExpression escapeChar = null)
            => (PostgresILikeExpression)ApplyDefaultTypeMapping(new PostgresILikeExpression(match, pattern, escapeChar, null));

        public virtual PostgresJsonTraversalExpression JsonTraversal(
            [NotNull] SqlExpression expression,
            bool returnsText,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping = null)
            => JsonTraversal(expression, Array.Empty<SqlExpression>(), returnsText, type, typeMapping);

        public virtual PostgresJsonTraversalExpression JsonTraversal(
            [NotNull] SqlExpression expression,
            [NotNull] IEnumerable<SqlExpression> path,
            bool returnsText,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping = null)
            => new PostgresJsonTraversalExpression(
                ApplyDefaultTypeMapping(expression),
                path.Select(ApplyDefaultTypeMapping).ToArray(),
                returnsText,
                type,
                typeMapping);

        public virtual SqlExpression NewArrayOrConstant(
            [NotNull] IReadOnlyList<SqlExpression> initializers,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping = null)
        {
            if (initializers.All(i => i is SqlConstantExpression))
            {
                if (!type.TryGetElementType(out var elementType))
                    throw new ArgumentException($"{type.Name} isn't an array type", nameof(type));

                var array = Array.CreateInstance(elementType, initializers.Count);
                for (var i = 0; i < initializers.Count; i++)
                    array.SetValue(((SqlConstantExpression)initializers[i]).Value, i);
                return Constant(array, typeMapping);
            }

            return NewArray(initializers, type, typeMapping);
        }

        public virtual PostgresNewArrayExpression NewArray(
            [NotNull] IReadOnlyList<SqlExpression> initializers,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping = null)
            => (PostgresNewArrayExpression)ApplyTypeMapping(new PostgresNewArrayExpression(initializers, type, typeMapping), typeMapping);

        public virtual PostgresBinaryExpression MakePostgresBinary(
            PostgresExpressionType operatorType,
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            var returnType = left.Type;
            switch (operatorType)
            {
            case PostgresExpressionType.Contains:
            case PostgresExpressionType.ContainedBy:
            case PostgresExpressionType.Overlaps:
            case PostgresExpressionType.NetworkContainedByOrEqual:
            case PostgresExpressionType.NetworkContainsOrEqual:
            case PostgresExpressionType.NetworkContainsOrContainedBy:
            case PostgresExpressionType.RangeIsStrictlyLeftOf:
            case PostgresExpressionType.RangeIsStrictlyRightOf:
            case PostgresExpressionType.RangeDoesNotExtendRightOf:
            case PostgresExpressionType.RangeDoesNotExtendLeftOf:
            case PostgresExpressionType.RangeIsAdjacentTo:
            case PostgresExpressionType.TextSearchMatch:
            case PostgresExpressionType.JsonExists:
            case PostgresExpressionType.JsonExistsAny:
            case PostgresExpressionType.JsonExistsAll:
                returnType = typeof(bool);
                break;
            }

            return (PostgresBinaryExpression)ApplyTypeMapping(
                new PostgresBinaryExpression(operatorType, left, right, returnType, null), typeMapping);
        }

        public virtual PostgresBinaryExpression Contains([NotNull] SqlExpression left, [NotNull] SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakePostgresBinary(PostgresExpressionType.Contains, left, right);
        }

        public virtual PostgresBinaryExpression ContainedBy([NotNull] SqlExpression left, [NotNull] SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakePostgresBinary(PostgresExpressionType.ContainedBy, left, right);
        }

        public virtual PostgresBinaryExpression Overlaps([NotNull] SqlExpression left, [NotNull] SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakePostgresBinary(PostgresExpressionType.Overlaps, left, right);
        }

        #endregion Expression factory methods

        public override SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
            => sqlExpression == null || sqlExpression.TypeMapping != null
                ? sqlExpression
                : sqlExpression switch
                {
                    SqlBinaryExpression e => ApplyTypeMappingOnSqlBinary(e, typeMapping),

                    // PostgreSQL-specific expression types
                    PostgresAnyExpression e        => ApplyTypeMappingOnAny(e),
                    PostgresAllExpression e        => ApplyTypeMappingOnAll(e),
                    PostgresArrayIndexExpression e => ApplyTypeMappingOnArrayIndex(e, typeMapping),
                    PostgresBinaryExpression e     => ApplyTypeMappingOnPostgresBinary(e, typeMapping),
                    PostgresFunctionExpression e   => e.ApplyTypeMapping(typeMapping),
                    PostgresILikeExpression e      => ApplyTypeMappingOnILike(e),
                    PostgresNewArrayExpression e   => ApplyTypeMappingOnPostgresNewArray(e, typeMapping),
                    PostgresRegexMatchExpression e => ApplyTypeMappingOnRegexMatch(e),

                    _ => base.ApplyTypeMapping(sqlExpression, typeMapping)
                };

        SqlExpression ApplyTypeMappingOnSqlBinary(SqlBinaryExpression binary, RelationalTypeMapping typeMapping)
        {
            // The default SqlExpressionFactory behavior is to assume that the two added operands have the same type,
            // and so to infer one side's mapping from the other if needed. Here we take care of some heterogeneous
            // operand cases where this doesn't work:
            // * Period + Period (???)

            if (binary.OperatorType == ExpressionType.Add || binary.OperatorType == ExpressionType.Subtract)
            {
                var (left, right) = (binary.Left, binary.Right);
                var leftType = left.Type.UnwrapNullableType();
                var rightType = right.Type.UnwrapNullableType();

                // DateTime + TimeSpan
                // DateTimeOffset + TimeSpan
                // (NodaTime type matching uses string names since NodaTime is a plugin)
                if (rightType == typeof(TimeSpan) && (
                        leftType == typeof(DateTime) ||
                        leftType == typeof(DateTimeOffset)) ||
                    rightType.FullName == "NodaTime.Period" && (
                        leftType.FullName == "NodaTime.LocalDateTime" ||
                        leftType.FullName == "NodaTime.LocalDate" ||
                        leftType.FullName == "NodaTime.LocalTime"
                    ))
                {
                    var newLeft = ApplyDefaultTypeMapping(left);
                    var newRight = ApplyDefaultTypeMapping(right);
                    return new SqlBinaryExpression(binary.OperatorType, newLeft, newRight, binary.Type,
                        newLeft.TypeMapping);
                }

                // * DateTime - DateTime
                // * DateTimeOffset - DateTimeOffset
                if (binary.OperatorType == ExpressionType.Subtract && (
                        leftType == typeof(DateTime) && rightType == typeof(DateTime) ||
                        leftType == typeof(DateTimeOffset) && rightType == typeof(DateTimeOffset)))
                {
                    var inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                    return new SqlBinaryExpression(
                        ExpressionType.Subtract,
                        ApplyTypeMapping(left, inferredTypeMapping),
                        ApplyTypeMapping(right, inferredTypeMapping),
                        binary.Type,
                        _intervalTypeMapping);
                }
            }

            return base.ApplyTypeMapping(binary, typeMapping);
        }

        SqlExpression ApplyTypeMappingOnRegexMatch(PostgresRegexMatchExpression postgresRegexMatchExpression)
        {
            var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(
                                          postgresRegexMatchExpression.Match, postgresRegexMatchExpression.Pattern)
                                      ?? _typeMappingSource.FindMapping(postgresRegexMatchExpression.Match.Type);

            return new PostgresRegexMatchExpression(
                ApplyTypeMapping(postgresRegexMatchExpression.Match, inferredTypeMapping),
                ApplyTypeMapping(postgresRegexMatchExpression.Pattern, inferredTypeMapping),
                postgresRegexMatchExpression.Options,
                _boolTypeMapping);
        }

        SqlExpression ApplyTypeMappingOnAny(PostgresAnyExpression postgresAnyExpression)
        {
            var (item, array) = ApplyTypeMappingsOnItemAndArray(
                postgresAnyExpression.Item,
                postgresAnyExpression.Array);
            return new PostgresAnyExpression(item, array, postgresAnyExpression.OperatorType, _boolTypeMapping);
        }

        SqlExpression ApplyTypeMappingOnAll(PostgresAllExpression postgresAllExpression)
        {
            var (item, array) = ApplyTypeMappingsOnItemAndArray(
                postgresAllExpression.Item,
                postgresAllExpression.Array);
            return new PostgresAllExpression(item, array, postgresAllExpression.OperatorType, _boolTypeMapping);
        }

        (SqlExpression, SqlExpression) ApplyTypeMappingsOnItemAndArray(
            SqlExpression itemExpression, SqlExpression arrayExpression)
        {
            // Attempt type inference either from the operand to the array or the other way around
            var arrayMapping = (NpgsqlArrayTypeMapping)arrayExpression.TypeMapping;

            var itemMapping = itemExpression.TypeMapping ??
                              arrayMapping?.ElementMapping ??
                              _typeMappingSource.FindMapping(itemExpression.Type);

            // Note that we provide both the array CLR type *and* an array store type constructed from the element's
            // store type. If we use only the array CLR type, byte[] will yield bytea which we don't want.
            arrayMapping ??= (NpgsqlArrayTypeMapping)_typeMappingSource.FindMapping(
                arrayExpression.Type, itemMapping.StoreType + "[]");

            if (itemMapping == null || arrayMapping == null)
                throw new InvalidOperationException(
                    "Couldn't find array or element type mapping in ArrayAnyAllExpression");

            return (
                ApplyTypeMapping(itemExpression, itemMapping),
                ApplyTypeMapping(arrayExpression, arrayMapping));
        }

        SqlExpression ApplyTypeMappingOnArrayIndex(
            PostgresArrayIndexExpression postgresArrayIndexExpression, RelationalTypeMapping typeMapping)
            => new PostgresArrayIndexExpression(
                // TODO: Infer the array's mapping from the element
                ApplyDefaultTypeMapping(postgresArrayIndexExpression.Array),
                ApplyDefaultTypeMapping(postgresArrayIndexExpression.Index),
                postgresArrayIndexExpression.Type,
                // If the array has a type mapping (i.e. column), prefer that just like we prefer column mappings in general
                postgresArrayIndexExpression.Array.TypeMapping is NpgsqlArrayTypeMapping arrayMapping
                    ? arrayMapping.ElementMapping
                    : typeMapping ?? _typeMappingSource.FindMapping(postgresArrayIndexExpression.Type));

        SqlExpression ApplyTypeMappingOnILike(PostgresILikeExpression ilikeExpression)
        {
            var inferredTypeMapping = (ilikeExpression.EscapeChar == null
                                          ? ExpressionExtensions.InferTypeMapping(
                                              ilikeExpression.Match, ilikeExpression.Pattern)
                                          : ExpressionExtensions.InferTypeMapping(
                                              ilikeExpression.Match, ilikeExpression.Pattern,
                                              ilikeExpression.EscapeChar))
                                      ?? _typeMappingSource.FindMapping(ilikeExpression.Match.Type);

            return new PostgresILikeExpression(
                ApplyTypeMapping(ilikeExpression.Match, inferredTypeMapping),
                ApplyTypeMapping(ilikeExpression.Pattern, inferredTypeMapping),
                ApplyTypeMapping(ilikeExpression.EscapeChar, inferredTypeMapping),
                _boolTypeMapping);
        }

        SqlExpression ApplyTypeMappingOnPostgresBinary(
            PostgresBinaryExpression postgresBinaryExpression, RelationalTypeMapping typeMapping)
        {
            var left = postgresBinaryExpression.Left;
            var right = postgresBinaryExpression.Right;

            Type resultType;
            RelationalTypeMapping resultTypeMapping;
            RelationalTypeMapping inferredTypeMapping;
            var operatorType = postgresBinaryExpression.OperatorType;
            switch (operatorType)
            {
            case PostgresExpressionType.Overlaps:
            case PostgresExpressionType.RangeIsStrictlyLeftOf:
            case PostgresExpressionType.RangeIsStrictlyRightOf:
            case PostgresExpressionType.RangeDoesNotExtendRightOf:
            case PostgresExpressionType.RangeDoesNotExtendLeftOf:
            case PostgresExpressionType.RangeIsAdjacentTo:
            {
                inferredTypeMapping = ExpressionExtensions.InferTypeMapping(left, right);
                resultType = typeof(bool);
                resultTypeMapping = _boolTypeMapping;
                break;
            }

            case PostgresExpressionType.NetworkContainedByOrEqual:
            case PostgresExpressionType.NetworkContainsOrEqual:
            case PostgresExpressionType.NetworkContainsOrContainedBy:
            case PostgresExpressionType.TextSearchMatch:
            case PostgresExpressionType.JsonExists:
            case PostgresExpressionType.JsonExistsAny:
            case PostgresExpressionType.JsonExistsAll:
            {
                // TODO: For networking, this probably needs to be cleaned up, i.e. we know where the CIDR and INET are
                // based on operator type?
                return new PostgresBinaryExpression(
                    operatorType,
                    ApplyDefaultTypeMapping(left),
                    ApplyDefaultTypeMapping(right),
                    typeof(bool),
                    _boolTypeMapping);
            }

            case PostgresExpressionType.Contains:
            case PostgresExpressionType.ContainedBy:
            {
                // Containment of array within an array, or range within range, or item within range.
                // Note that containment of item within an array is expressed via ArrayAnyAllExpression
                if (left.Type == right.Type)
                    goto case PostgresExpressionType.Overlaps;

                SqlExpression newLeft, newRight;
                if (operatorType == PostgresExpressionType.Contains)
                    (newLeft, newRight) = InferContainmentMappings(left, right);
                else
                    (newRight, newLeft) = InferContainmentMappings(right, left);
                return new PostgresBinaryExpression(operatorType, newLeft, newRight, typeof(bool), _boolTypeMapping);
            }

            case PostgresExpressionType.RangeUnion:
            case PostgresExpressionType.RangeIntersect:
            case PostgresExpressionType.RangeExcept:
            case PostgresExpressionType.TextSearchAnd:
            case PostgresExpressionType.TextSearchOr:
            {
                inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                resultType = inferredTypeMapping?.ClrType ?? left.Type;
                resultTypeMapping = inferredTypeMapping;
                break;
            }

            default:
                throw new InvalidOperationException($"Incorrect {nameof(operatorType)} for {nameof(postgresBinaryExpression)}");
            }

            return new PostgresBinaryExpression(
                operatorType,
                ApplyTypeMapping(left, inferredTypeMapping),
                ApplyTypeMapping(right, inferredTypeMapping),
                resultType,
                resultTypeMapping);

            (SqlExpression, SqlExpression) InferContainmentMappings(SqlExpression container, SqlExpression containee)
            {
                // TODO: We may need container/containee inference for other type, not just range
                // TODO: Do the other way to - infer the container mapping from the containee
                var newContainer = ApplyDefaultTypeMapping(container);
                var newContainee = newContainer.TypeMapping is NpgsqlRangeTypeMapping rangeMapping
                    ? ApplyTypeMapping(containee, rangeMapping.SubtypeMapping)
                    : ApplyDefaultTypeMapping(containee);
                return (newContainer, newContainee);
            }
        }

        SqlExpression ApplyTypeMappingOnPostgresNewArray(
            PostgresNewArrayExpression postgresNewArrayExpression, RelationalTypeMapping typeMapping)
        {
            var arrayTypeMapping = typeMapping as NpgsqlArrayTypeMapping;
            if (arrayTypeMapping is null && typeMapping != null)
                throw new ArgumentException($"Type mapping {typeMapping.GetType().Name} isn't an {nameof(NpgsqlArrayTypeMapping)}");
            var elementTypeMapping = arrayTypeMapping?.ElementMapping;

            List<SqlExpression> newInitializers = null;
            for (var i = 0; i < postgresNewArrayExpression.Initializers.Count; i++)
            {
                var initializer = postgresNewArrayExpression.Initializers[i];
                var newInitializer = ApplyTypeMapping(initializer, elementTypeMapping);
                if (newInitializer != initializer && newInitializers is null)
                {
                    newInitializers = new List<SqlExpression>();
                    for (var j = 0; j < i; j++)
                        newInitializers.Add(newInitializer);
                }

                if (newInitializers != null)
                    newInitializers.Add(newInitializer);
            }

            return new PostgresNewArrayExpression(
                newInitializers ?? postgresNewArrayExpression.Initializers,
                postgresNewArrayExpression.Type, typeMapping);
        }
    }
}
