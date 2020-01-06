using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlSqlExpressionFactory : SqlExpressionFactory
    {
        readonly IRelationalTypeMappingSource _typeMappingSource;
        readonly RelationalTypeMapping _boolTypeMapping;
        readonly RelationalTypeMapping _intervalTypeMapping;

        public NpgsqlSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
            : base(dependencies)
        {
            _typeMappingSource = dependencies.TypeMappingSource;
            _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool));
            _intervalTypeMapping = _typeMappingSource.FindMapping("interval");
        }

        #region Expression factory methods

        public RegexMatchExpression RegexMatch(SqlExpression match, SqlExpression pattern, RegexOptions options)
            => (RegexMatchExpression)ApplyDefaultTypeMapping(new RegexMatchExpression(match, pattern, options, null));

        public ArrayAnyAllExpression ArrayAnyAll(
            SqlExpression operand,
            SqlExpression array,
            ArrayComparisonType arrayComparisonType,
            string @operator)
            => (ArrayAnyAllExpression)ApplyDefaultTypeMapping(new ArrayAnyAllExpression(operand, array, arrayComparisonType, @operator, null));

        public ArrayIndexExpression ArrayIndex(
            SqlExpression array,
            SqlExpression index,
            RelationalTypeMapping typeMapping = null)
        {
            Type elementType;
            if (array.Type.IsArray)
                elementType = array.Type.GetElementType();
            else if (array.Type.IsGenericList())
                elementType = array.Type.GetGenericArguments()[0];
            else
                throw new ArgumentException("Array expression must be of an array or List<> type", nameof(array));

            return (ArrayIndexExpression)ApplyTypeMapping(new ArrayIndexExpression(array, index, elementType, null), typeMapping);
        }

        public AtTimeZoneExpression AtTimeZone(
            SqlExpression timestamp,
            SqlExpression timeZone,
            Type type,
            RelationalTypeMapping typeMapping = null)
        {
            // PostgreSQL AT TIME ZONE flips the given type from timestamptz to timestamp and vice versa
            // See https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT
            typeMapping ??= FlipTimestampTypeMapping(timestamp.TypeMapping ?? _typeMappingSource.FindMapping(timestamp.Type));

            return new AtTimeZoneExpression(
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

        public ILikeExpression ILike(SqlExpression match, SqlExpression pattern, SqlExpression escapeChar = null)
            => (ILikeExpression)ApplyDefaultTypeMapping(new ILikeExpression(match, pattern, escapeChar, null));

        public JsonTraversalExpression JsonTraversal(
            SqlExpression expression,
            IEnumerable<SqlExpression> path,
            bool returnsText,
            Type type,
            RelationalTypeMapping typeMapping = null)
            => new JsonTraversalExpression(
                ApplyDefaultTypeMapping(expression),
                path.Select(ApplyDefaultTypeMapping).ToArray(),
                returnsText,
                type,
                typeMapping);

        #endregion Expression factory methods

        public override SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
            => sqlExpression == null || sqlExpression.TypeMapping != null
                ? sqlExpression
                : sqlExpression switch
                {
                    SqlBinaryExpression e        => ApplyTypeMappingOnSqlBinary(e, typeMapping),

                    // PostgreSQL-specific expression types
                    RegexMatchExpression e       => ApplyTypeMappingOnRegexMatch(e),
                    ArrayAnyAllExpression e      => ApplyTypeMappingOnArrayAnyAll(e),
                    ArrayIndexExpression e       => ApplyTypeMappingOnArrayIndex(e, typeMapping),
                    ILikeExpression e            => ApplyTypeMappingOnILike(e),
                    PgFunctionExpression e       => e.ApplyTypeMapping(typeMapping),

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

        SqlExpression ApplyTypeMappingOnRegexMatch(RegexMatchExpression regexMatchExpression)
        {
            var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(
                                          regexMatchExpression.Match, regexMatchExpression.Pattern)
                                      ?? _typeMappingSource.FindMapping(regexMatchExpression.Match.Type);

            return new RegexMatchExpression(
                ApplyTypeMapping(regexMatchExpression.Match, inferredTypeMapping),
                ApplyTypeMapping(regexMatchExpression.Pattern, inferredTypeMapping),
                regexMatchExpression.Options,
                _boolTypeMapping);
        }

        SqlExpression ApplyTypeMappingOnArrayAnyAll(ArrayAnyAllExpression arrayAnyAllExpression)
        {
            // Attempt type inference either from the operand to the array or the other way around
            var arrayMapping = arrayAnyAllExpression.Array.TypeMapping as NpgsqlArrayTypeMapping;

            var operandMapping = arrayAnyAllExpression.Operand.TypeMapping ??
                                 arrayMapping?.ElementMapping ??
                                 _typeMappingSource.FindMapping(arrayAnyAllExpression.Operand.Type);

            arrayMapping ??= (NpgsqlArrayTypeMapping)_typeMappingSource.FindMapping(arrayAnyAllExpression.Operand.Type.MakeArrayType());

            return new ArrayAnyAllExpression(
                ApplyTypeMapping(arrayAnyAllExpression.Operand, operandMapping),
                ApplyTypeMapping(arrayAnyAllExpression.Array, arrayMapping),
                arrayAnyAllExpression.ArrayComparisonType,
                arrayAnyAllExpression.Operator,
                _boolTypeMapping);
        }

        SqlExpression ApplyTypeMappingOnArrayIndex(
            ArrayIndexExpression arrayIndexExpression, RelationalTypeMapping typeMapping)
            => new ArrayIndexExpression(
                // TODO: Infer the array's mapping from the element
                ApplyDefaultTypeMapping(arrayIndexExpression.Array),
                ApplyDefaultTypeMapping(arrayIndexExpression.Index),
                arrayIndexExpression.Type,
                // If the array has a type mapping (i.e. column), prefer that just like we prefer column mappings in general
                arrayIndexExpression.Array.TypeMapping is NpgsqlArrayTypeMapping arrayMapping
                    ? arrayMapping.ElementMapping
                    : typeMapping ?? FindMapping(arrayIndexExpression.Type));

        SqlExpression ApplyTypeMappingOnILike(ILikeExpression ilikeExpression)
        {
            var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(
                                          ilikeExpression.Match, ilikeExpression.Pattern, ilikeExpression.EscapeChar)
                                      ?? _typeMappingSource.FindMapping(ilikeExpression.Match.Type);

            return new ILikeExpression(
                ApplyTypeMapping(ilikeExpression.Match, inferredTypeMapping),
                ApplyTypeMapping(ilikeExpression.Pattern, inferredTypeMapping),
                ApplyTypeMapping(ilikeExpression.EscapeChar, inferredTypeMapping),
                _boolTypeMapping);
        }
    }
}
