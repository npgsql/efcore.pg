using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public NpgsqlSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
            : base(dependencies)
        {
            _typeMappingSource = dependencies.TypeMappingSource;
            _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool));
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
            // TODO: Support List<>
            if (!array.Type.IsArray)
                throw new ArgumentException("Array expression must of an array type", nameof(array));

            var elementType = array.Type.GetElementType();
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
        {
            if (sqlExpression == null
                || sqlExpression.TypeMapping != null)
            {
                return sqlExpression;
            }

            return sqlExpression switch
            {
                RegexMatchExpression e       => ApplyTypeMappingOnRegexMatch(e),
                ArrayAnyAllExpression e      => ApplyTypeMappingOnArrayAnyAll(e),
                ArrayIndexExpression e       => ApplyTypeMappingOnArrayIndex(e),
                ILikeExpression e            => ApplyTypeMappingOnILike(e),
                PgFunctionExpression e       => e.ApplyTypeMapping(typeMapping),

                _ => base.ApplyTypeMapping(sqlExpression, typeMapping)
            };
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

        SqlExpression ApplyTypeMappingOnArrayIndex(ArrayIndexExpression arrayIndexExpression)
            => new ArrayIndexExpression(
                ApplyDefaultTypeMapping(arrayIndexExpression.Array),
                ApplyDefaultTypeMapping(arrayIndexExpression.Index),
                arrayIndexExpression.Type,
                arrayIndexExpression.Array.TypeMapping is NpgsqlArrayTypeMapping arrayMapping
                    ? arrayMapping.ElementMapping
                    : FindMapping(arrayIndexExpression.Type));

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
