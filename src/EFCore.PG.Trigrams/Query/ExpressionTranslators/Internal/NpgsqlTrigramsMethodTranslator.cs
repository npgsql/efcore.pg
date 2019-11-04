using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlTrigramsMethodTranslator : IMethodCallTranslator
    {
        static readonly Dictionary<MethodInfo, string> Functions = new Dictionary<MethodInfo, string>
        {
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsGetTrigrams), new[] { typeof(DbFunctions), typeof(string) })] = "show_trgm",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsSimilarity), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "similarity",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsWordSimilarity), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "word_similarity",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsStrictWordSimilarity), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "strict_word_similarity"
        };

        static readonly Dictionary<MethodInfo, string> BoolReturningOperators = new Dictionary<MethodInfo, string>
        {
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreSimilar), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "%",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreWordSimilar), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "<%",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreNotWordSimilar), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "%>",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreStrictWordSimilar), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "<<%",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreNotStrictWordSimilar), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "%>>"
        };

        static readonly Dictionary<MethodInfo, string> FloatReturningOperators = new Dictionary<MethodInfo, string>
        {
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsSimilarityDistance), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "<->",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsWordSimilarityDistance), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "<<->",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsWordSimilarityDistanceInverted), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "<->>",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsStrictWordSimilarityDistance), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "<<<->",
            [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsStrictWordSimilarityDistanceInverted), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "<->>>"
        };

        static MethodInfo GetRuntimeMethod(string name, params Type[] parameters)
            => typeof(NpgsqlTrigramsDbFunctionsExtensions).GetRuntimeMethod(name, parameters);

        readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        readonly RelationalTypeMapping _boolMapping;
        readonly RelationalTypeMapping _floatMapping;

        public NpgsqlTrigramsMethodTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory, IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _boolMapping = typeMappingSource.FindMapping(typeof(bool));
            _floatMapping = typeMappingSource.FindMapping(typeof(float));
        }

#pragma warning disable EF1001
        /// <inheritdoc />
        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (Functions.TryGetValue(method, out var function))
                return _sqlExpressionFactory.Function(function, arguments.Skip(1), method.ReturnType);

            if (BoolReturningOperators.TryGetValue(method, out var boolOperator))
                return new SqlCustomBinaryExpression(
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    boolOperator,
                    _boolMapping.ClrType,
                    _boolMapping);

            if (FloatReturningOperators.TryGetValue(method, out var floatOperator))
                return new SqlCustomBinaryExpression(
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    floatOperator,
                    _floatMapping.ClrType,
                    _floatMapping);

            return null;
        }
#pragma warning restore EF1001
    }
}
