using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NpgsqlSqlExpressionFactory : SqlExpressionFactory
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly RelationalTypeMapping _boolTypeMapping;
        private readonly RelationalTypeMapping _intTypeMapping;
        private readonly RelationalTypeMapping _doubleTypeMapping;

        private static Type? _nodaTimeDurationType;
        private static Type? _nodaTimePeriodType;

        public NpgsqlSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
            : base(dependencies)
        {
            _typeMappingSource = dependencies.TypeMappingSource;
            _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool), dependencies.Model)!;
            _intTypeMapping = _typeMappingSource.FindMapping(typeof(int), dependencies.Model)!;
            _doubleTypeMapping = _typeMappingSource.FindMapping(typeof(double), dependencies.Model)!;
        }

        #region Expression factory methods

        public virtual PostgresRegexMatchExpression RegexMatch(
            SqlExpression match, SqlExpression pattern, RegexOptions options)
            => (PostgresRegexMatchExpression)ApplyDefaultTypeMapping(new PostgresRegexMatchExpression(match, pattern, options, null));

        public virtual PostgresAnyExpression Any(
            SqlExpression item,
            SqlExpression array,
            PostgresAnyOperatorType operatorType)
            => (PostgresAnyExpression)ApplyDefaultTypeMapping(new PostgresAnyExpression(item, array, operatorType, null));

        public virtual PostgresAllExpression All(
            SqlExpression item,
            SqlExpression array,
            PostgresAllOperatorType operatorType)
            => (PostgresAllExpression)ApplyDefaultTypeMapping(new PostgresAllExpression(item, array, operatorType, null));

        public virtual PostgresArrayIndexExpression ArrayIndex(
            SqlExpression array,
            SqlExpression index,
             RelationalTypeMapping? typeMapping = null)
        {
            if (!array.Type.TryGetElementType(out var elementType))
                throw new ArgumentException("Array expression must be of an array or List<> type", nameof(array));

            return (PostgresArrayIndexExpression)ApplyTypeMapping(
                new PostgresArrayIndexExpression(array, index, elementType, typeMapping: null),
                typeMapping);
        }

        public virtual PostgresBinaryExpression AtUtc(
            SqlExpression timestamp,
            RelationalTypeMapping? typeMapping = null)
            => AtTimeZone(timestamp, Constant("UTC"), timestamp.Type);

        public virtual PostgresBinaryExpression AtTimeZone(
            SqlExpression timestamp,
            SqlExpression timeZone,
            Type type,
            RelationalTypeMapping? typeMapping = null)
        {
            // PostgreSQL AT TIME ZONE flips the given type from timestamptz to timestamp and vice versa
            // See https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT
            typeMapping ??= FlipTimestampTypeMapping(
                timestamp.TypeMapping ?? _typeMappingSource.FindMapping(timestamp.Type, Dependencies.Model)!);

            return new PostgresBinaryExpression(
                PostgresExpressionType.AtTimeZone,
                ApplyDefaultTypeMapping(timestamp),
                ApplyDefaultTypeMapping(timeZone),
                type,
                typeMapping);

            RelationalTypeMapping FlipTimestampTypeMapping(RelationalTypeMapping mapping)
            {
                var storeType = mapping.StoreType;
                if (storeType.StartsWith("timestamp with time zone", StringComparison.Ordinal) || storeType.StartsWith("timestamptz", StringComparison.Ordinal))
                    return _typeMappingSource.FindMapping("timestamp without time zone")!;
                if (storeType.StartsWith("timestamp without time zone", StringComparison.Ordinal) || storeType.StartsWith("timestamp", StringComparison.Ordinal))
                    return _typeMappingSource.FindMapping("timestamp with time zone")!;
                throw new ArgumentException($"timestamp argument to AtTimeZone had unknown store type {storeType}", nameof(timestamp));
            }
        }

        public virtual PostgresILikeExpression ILike(
            SqlExpression match,
            SqlExpression pattern,
            SqlExpression? escapeChar = null)
            => (PostgresILikeExpression)ApplyDefaultTypeMapping(new PostgresILikeExpression(match, pattern, escapeChar, null));

        public virtual PostgresJsonTraversalExpression JsonTraversal(
            SqlExpression expression,
            bool returnsText,
            Type type,
            RelationalTypeMapping? typeMapping = null)
            => JsonTraversal(expression, Array.Empty<SqlExpression>(), returnsText, type, typeMapping);

        public virtual PostgresJsonTraversalExpression JsonTraversal(
            SqlExpression expression,
            IEnumerable<SqlExpression> path,
            bool returnsText,
            Type type,
            RelationalTypeMapping? typeMapping = null)
            => new(
                ApplyDefaultTypeMapping(expression),
                path.Select(ApplyDefaultTypeMapping).ToArray()!,
                returnsText,
                type,
                typeMapping);

        /// <summary>
        /// Constructs either a <see cref="PostgresNewArrayExpression"/>, or, if all provided expressions are constants,
        /// a single <see cref="SqlConstantExpression"/> for the entire array.
        /// </summary>
        public virtual SqlExpression NewArrayOrConstant(
            IReadOnlyList<SqlExpression> expressions,
            Type type,
            RelationalTypeMapping? typeMapping = null)
        {
            if (!type.TryGetElementType(out var elementType))
                throw new ArgumentException($"{type.Name} isn't an array or generic List", nameof(type));

            if (expressions.Any(i => i is not SqlConstantExpression))
            {
                return NewArray(expressions, type, typeMapping);
            }

            var array = Array.CreateInstance(elementType, expressions.Count);
            for (var i = 0; i < expressions.Count; i++)
                array.SetValue(((SqlConstantExpression)expressions[i]).Value, i);
            return Constant(array, typeMapping);
        }

        public virtual PostgresNewArrayExpression NewArray(
            IReadOnlyList<SqlExpression> expressions,
            Type type,
            RelationalTypeMapping? typeMapping = null)
            => (PostgresNewArrayExpression)ApplyTypeMapping(new PostgresNewArrayExpression(expressions, type, typeMapping), typeMapping);

        public override SqlBinaryExpression? MakeBinary(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            if (operatorType == ExpressionType.Subtract)
            {
                if (left.Type == typeof(DateTime) && right.Type == typeof(DateTime) ||
                    left.Type == typeof(DateTimeOffset) && right.Type == typeof(DateTimeOffset) ||
                    left.Type == typeof(TimeOnly) && right.Type == typeof(TimeOnly))
                {
                    return (SqlBinaryExpression)ApplyTypeMapping(
                        new SqlBinaryExpression(operatorType, left, right, typeof(TimeSpan), null), typeMapping);
                }

                if (left.Type.FullName == "NodaTime.Instant" && right.Type.FullName == "NodaTime.Instant" ||
                    left.Type.FullName == "NodaTime.ZonedDateTime" && right.Type.FullName == "NodaTime.ZonedDateTime")
                {
                    _nodaTimeDurationType ??= left.Type.Assembly.GetType("NodaTime.Duration");
                    return (SqlBinaryExpression)ApplyTypeMapping(
                        new SqlBinaryExpression(operatorType, left, right, _nodaTimeDurationType!, null), typeMapping);
                }

                if (left.Type.FullName == "NodaTime.LocalDateTime" && right.Type.FullName == "NodaTime.LocalDateTime" ||
                    left.Type.FullName == "NodaTime.LocalTime" && right.Type.FullName == "NodaTime.LocalTime")
                {
                    _nodaTimePeriodType ??= left.Type.Assembly.GetType("NodaTime.Period");
                    return (SqlBinaryExpression)ApplyTypeMapping(
                        new SqlBinaryExpression(operatorType, left, right, _nodaTimePeriodType!, null), typeMapping);
                }

                if (left.Type.FullName == "NodaTime.LocalDate" && right.Type.FullName == "NodaTime.LocalDate")
                {
                    return (SqlBinaryExpression)ApplyTypeMapping(
                        new SqlBinaryExpression(operatorType, left, right, typeof(int), null), typeMapping);
                }
            }

            return base.MakeBinary(operatorType, left, right, typeMapping);
        }

        public virtual PostgresBinaryExpression MakePostgresBinary(
            PostgresExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping = null)
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

            case PostgresExpressionType.PostgisDistanceKnn:
                returnType = typeof(double);
                break;
            }

            return (PostgresBinaryExpression)ApplyTypeMapping(
                new PostgresBinaryExpression(operatorType, left, right, returnType, null), typeMapping);
        }

        public virtual PostgresBinaryExpression Contains(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakePostgresBinary(PostgresExpressionType.Contains, left, right);
        }

        public virtual PostgresBinaryExpression ContainedBy(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakePostgresBinary(PostgresExpressionType.ContainedBy, left, right);
        }

        public virtual PostgresBinaryExpression Overlaps(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakePostgresBinary(PostgresExpressionType.Overlaps, left, right);
        }

        #endregion Expression factory methods

        [return: NotNullIfNotNull("sqlExpression")]
        public override SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, RelationalTypeMapping? typeMapping)
        {
            if (sqlExpression is not null && sqlExpression.TypeMapping is null)
            {
                sqlExpression = sqlExpression switch
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
            }

            if (!NpgsqlTypeMappingSource.LegacyTimestampBehavior
                && (typeMapping is NpgsqlTimestampTypeMapping && sqlExpression?.TypeMapping is NpgsqlTimestampTzTypeMapping
                    || typeMapping is NpgsqlTimestampTzTypeMapping && sqlExpression?.TypeMapping is NpgsqlTimestampTypeMapping))
            {
                throw new NotSupportedException(
                    "Cannot apply binary operation on types 'timestamp with time zone' and 'timestamp without time zone', convert one of the operands first.");
            }

            return sqlExpression;
        }

        private SqlExpression ApplyTypeMappingOnSqlBinary(SqlBinaryExpression binary, RelationalTypeMapping? typeMapping)
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

                // Note that we apply the given type mapping from above to the left operand (which has the same CLR type as
                // the binary expression's)

                // DateTime + TimeSpan => DateTime
                // DateTimeOffset + TimeSpan => DateTimeOffset
                // TimeOnly + TimeSpan => TimeOnly
                if (rightType == typeof(TimeSpan)
                    && (
                        leftType == typeof(DateTime)
                        || leftType == typeof(DateTimeOffset)
                        || leftType == typeof(TimeOnly)
                    )
                    || rightType.FullName == "NodaTime.Period"
                    && (
                        leftType.FullName == "NodaTime.LocalDateTime"
                        || leftType.FullName == "NodaTime.LocalDate"
                        || leftType.FullName == "NodaTime.LocalTime")
                    || rightType.FullName == "NodaTime.Duration"
                    && (
                        leftType.FullName == "NodaTime.Instant"
                        || leftType.FullName == "NodaTime.ZonedDateTime"))
                {
                    var newLeft = ApplyTypeMapping(left, typeMapping);
                    var newRight = ApplyDefaultTypeMapping(right);
                    return new SqlBinaryExpression(binary.OperatorType, newLeft, newRight, binary.Type, newLeft.TypeMapping);
                }

                if (binary.OperatorType == ExpressionType.Subtract)
                {
                    // DateTime - DateTime => TimeSpan
                    // DateTimeOffset - DateTimeOffset => TimeSpan
                    // DateOnly - DateOnly => TimeSpan
                    // TimeOnly - TimeOnly => TimeSpan
                    // Instant - Instant => Duration
                    // LocalDateTime - LocalDateTime => int (days)
                    if (leftType == typeof(DateTime) && rightType == typeof(DateTime)
                        || leftType == typeof(DateTimeOffset) && rightType == typeof(DateTimeOffset)
                        || leftType == typeof(DateOnly) && rightType == typeof(DateOnly)
                        || leftType == typeof(TimeOnly) && rightType == typeof(TimeOnly)
                        || leftType.FullName == "NodaTime.Instant" && rightType.FullName == "NodaTime.Instant"
                        || leftType.FullName == "NodaTime.LocalDateTime" && rightType.FullName == "NodaTime.LocalDateTime"
                        || leftType.FullName == "NodaTime.ZonedDateTime" && rightType.FullName == "NodaTime.ZonedDateTime"
                        || leftType.FullName == "NodaTime.LocalDate" && rightType.FullName == "NodaTime.LocalDate"
                        || leftType.FullName == "NodaTime.LocalTime" && rightType.FullName == "NodaTime.LocalTime")
                    {
                        var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(left, right);

                        return new SqlBinaryExpression(
                            ExpressionType.Subtract,
                            ApplyTypeMapping(left, inferredTypeMapping),
                            ApplyTypeMapping(right, inferredTypeMapping),
                            binary.Type,
                            typeMapping ?? _typeMappingSource.FindMapping(binary.Type, "interval"));
                    }
                }
            }

            return base.ApplyTypeMapping(binary, typeMapping);
        }

        private SqlExpression ApplyTypeMappingOnRegexMatch(PostgresRegexMatchExpression postgresRegexMatchExpression)
        {
            var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(
                                          postgresRegexMatchExpression.Match, postgresRegexMatchExpression.Pattern)
                                      ?? _typeMappingSource.FindMapping(postgresRegexMatchExpression.Match.Type, Dependencies.Model);

            return new PostgresRegexMatchExpression(
                ApplyTypeMapping(postgresRegexMatchExpression.Match, inferredTypeMapping),
                ApplyTypeMapping(postgresRegexMatchExpression.Pattern, inferredTypeMapping),
                postgresRegexMatchExpression.Options,
                _boolTypeMapping);
        }

        private SqlExpression ApplyTypeMappingOnAny(PostgresAnyExpression postgresAnyExpression)
        {
            var (item, array) = ApplyTypeMappingsOnItemAndArray(postgresAnyExpression.Item, postgresAnyExpression.Array);
            return new PostgresAnyExpression(item, array, postgresAnyExpression.OperatorType, _boolTypeMapping);
        }

        private SqlExpression ApplyTypeMappingOnAll(PostgresAllExpression postgresAllExpression)
        {
            var (item, array) = ApplyTypeMappingsOnItemAndArray(postgresAllExpression.Item, postgresAllExpression.Array);
            return new PostgresAllExpression(item, array, postgresAllExpression.OperatorType, _boolTypeMapping);
        }

        private (SqlExpression, SqlExpression) ApplyTypeMappingsOnItemAndArray(SqlExpression itemExpression, SqlExpression arrayExpression)
        {
            // Attempt type inference either from the operand to the array or the other way around
            var arrayMapping = (NpgsqlArrayTypeMapping?)arrayExpression.TypeMapping;

            var itemMapping =
                itemExpression.TypeMapping ??
                // Unwrap convert-to-object nodes - these get added for object[].Contains(x)
                (itemExpression is SqlUnaryExpression { OperatorType: ExpressionType.Convert } unary ? unary.Operand.TypeMapping : null) ??
                // If we couldn't find a type mapping on the item, try inferring it from the array
                arrayMapping?.ElementMapping ??
                _typeMappingSource.FindMapping(itemExpression.Type, Dependencies.Model);

            if (itemMapping is null)
            {
                throw new InvalidOperationException("Couldn't find element type mapping when applying item/array mappings");
            }

            // If the array's type mapping isn't provided (parameter/constant), attempt to infer it from the item.
            if (arrayMapping is null)
            {
                if (itemMapping.Converter is { } converter)
                {
                    // If the item mapping has a value converter, construct an array mapping directly over it - this will build the
                    // corresponding array type converter.
                    arrayMapping = arrayExpression.Type.IsArray
                        ? new NpgsqlArrayArrayTypeMapping(arrayExpression.Type, itemMapping)
                        : new NpgsqlArrayListTypeMapping(arrayExpression.Type, itemMapping);
                }
                else
                {
                    // No value converter on the item mapping - just try to look up an array mapping based on the item type.
                    // Special-case arrays of objects, not taking the array CLR type into account in the lookup (it would never succeed).
                    // Note that we provide both the array CLR type *and* an array store type constructed from the element's store type.
                    // If we use only the array CLR type, byte[] will yield bytea which we don't want.
                    arrayMapping = arrayExpression.Type == typeof(object[]) || arrayExpression.Type == typeof(List<object>)
                        ? (NpgsqlArrayTypeMapping?)_typeMappingSource.FindMapping(itemMapping.StoreType + "[]")
                        : (NpgsqlArrayTypeMapping?)_typeMappingSource.FindMapping(
                            arrayExpression.Type,
                            itemMapping.StoreType + "[]");
                }
            }

            if (arrayMapping == null)
            {
                throw new InvalidOperationException("Couldn't find array type mapping when applying item/array mappings");
            }

            return (ApplyTypeMapping(itemExpression, itemMapping), ApplyTypeMapping(arrayExpression, arrayMapping));
        }

        private SqlExpression ApplyTypeMappingOnArrayIndex(
            PostgresArrayIndexExpression postgresArrayIndexExpression, RelationalTypeMapping? typeMapping)
            => new PostgresArrayIndexExpression(
                // TODO: Infer the array's mapping from the element
                ApplyDefaultTypeMapping(postgresArrayIndexExpression.Array),
                ApplyDefaultTypeMapping(postgresArrayIndexExpression.Index),
                postgresArrayIndexExpression.Type,
                // If the array has a type mapping (i.e. column), prefer that just like we prefer column mappings in general
                postgresArrayIndexExpression.Array.TypeMapping is NpgsqlArrayTypeMapping arrayMapping
                    ? arrayMapping.ElementMapping
                    : typeMapping ?? _typeMappingSource.FindMapping(postgresArrayIndexExpression.Type, Dependencies.Model));

        private SqlExpression ApplyTypeMappingOnILike(PostgresILikeExpression ilikeExpression)
        {
            var inferredTypeMapping = (ilikeExpression.EscapeChar == null
                                          ? ExpressionExtensions.InferTypeMapping(
                                              ilikeExpression.Match, ilikeExpression.Pattern)
                                          : ExpressionExtensions.InferTypeMapping(
                                              ilikeExpression.Match, ilikeExpression.Pattern,
                                              ilikeExpression.EscapeChar))
                                      ?? _typeMappingSource.FindMapping(ilikeExpression.Match.Type, Dependencies.Model);

            return new PostgresILikeExpression(
                ApplyTypeMapping(ilikeExpression.Match, inferredTypeMapping),
                ApplyTypeMapping(ilikeExpression.Pattern, inferredTypeMapping),
                ApplyTypeMapping(ilikeExpression.EscapeChar, inferredTypeMapping),
                _boolTypeMapping);
        }

        private SqlExpression ApplyTypeMappingOnPostgresBinary(
            PostgresBinaryExpression postgresBinaryExpression, RelationalTypeMapping? typeMapping)
        {
            var left = postgresBinaryExpression.Left;
            var right = postgresBinaryExpression.Right;

            Type resultType;
            RelationalTypeMapping? resultTypeMapping;
            RelationalTypeMapping? inferredTypeMapping;
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
                // if (left.Type == right.Type && left.Type.IsArrayOrGenericList())
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

            case PostgresExpressionType.PostgisDistanceKnn:
            {
                inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                resultType = typeof(double);
                resultTypeMapping = _doubleTypeMapping;
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

        private SqlExpression ApplyTypeMappingOnPostgresNewArray(
            PostgresNewArrayExpression postgresNewArrayExpression, RelationalTypeMapping? typeMapping)
        {
            var arrayTypeMapping = typeMapping as NpgsqlArrayTypeMapping;
            if (arrayTypeMapping is null && typeMapping != null)
                throw new ArgumentException($"Type mapping {typeMapping.GetType().Name} isn't an {nameof(NpgsqlArrayTypeMapping)}");

            RelationalTypeMapping? elementTypeMapping = null;

            // First, loop over the expressions to infer the array's type mapping (if not provided), and to make
            // sure we don't have heterogeneous mappings.
            foreach (var expression in postgresNewArrayExpression.Expressions)
            {
                if (expression.TypeMapping is not null)
                {
                    if (elementTypeMapping is null)
                    {
                        elementTypeMapping = expression.TypeMapping;
                    }
                    else if (elementTypeMapping != expression.TypeMapping)
                    {
                        throw new InvalidOperationException(
                            $"Heterogeneous type mappings detected when making new array ({elementTypeMapping}, {expression.TypeMapping})");
                    }
                }
            }

            // None of the array's expressions had a type mapping (i.e. no columns, only parameters/constants)
            // Use the type mapping given externally
            if (elementTypeMapping is null)
            {
                // No type mapping could be inferred from the expressions, nor was one given from the outside -
                // we have no type mapping... Just return the original expression, which has no type mapping and will fail translation.
                if (arrayTypeMapping is null)
                {
                    return postgresNewArrayExpression;
                }

                elementTypeMapping = arrayTypeMapping.ElementMapping;
            }
            else
            {
                // An element type mapping was successfully inferred from one of the expressions (there was a column).
                // Infer the array's type mapping from it.
                arrayTypeMapping = (NpgsqlArrayTypeMapping?)_typeMappingSource.FindMapping(
                    postgresNewArrayExpression.Type,
                    elementTypeMapping.StoreType + "[]");

                // If the array's CLR type doesn't match the type mapping inferred from the element (e.g. CLR object[] with up-casted
                // elements). Just return the original expression, which has no type mapping and will fail translation.
                if (arrayTypeMapping is null)
                {
                    return postgresNewArrayExpression;
                }
            }

            // Now go over all expressions and apply the inferred element type mapping
            List<SqlExpression>? newExpressions = null;
            for (var i = 0; i < postgresNewArrayExpression.Expressions.Count; i++)
            {
                var expression = postgresNewArrayExpression.Expressions[i];
                var newExpression = ApplyTypeMapping(expression, elementTypeMapping);
                if (newExpression != expression && newExpressions is null)
                {
                    newExpressions = new List<SqlExpression>();
                    for (var j = 0; j < i; j++)
                        newExpressions.Add(postgresNewArrayExpression.Expressions[j]);
                }

                newExpressions?.Add(newExpression);
            }

            return new PostgresNewArrayExpression(
                newExpressions ?? postgresNewArrayExpression.Expressions,
                postgresNewArrayExpression.Type, arrayTypeMapping);
        }
    }
}
