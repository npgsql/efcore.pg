using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlFuzzyStringMatchMethodTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<MethodInfo, string> Functions = new()
        {
            [GetRuntimeMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchSoundex), new[] { typeof(DbFunctions), typeof(string) })] = "soundex",
            [GetRuntimeMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchDifference), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "difference",
            [GetRuntimeMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchLevenshtein), new[] { typeof(DbFunctions), typeof(string), typeof(string) })] = "levenshtein",
            [GetRuntimeMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchLevenshtein), new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int) })] = "levenshtein",
            [GetRuntimeMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchLevenshteinLessEqual), new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(int) })] = "levenshtein_less_equal",
            [GetRuntimeMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchLevenshteinLessEqual), new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int) })] = "levenshtein_less_equal",
            [GetRuntimeMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchMetaphone), new[] { typeof(DbFunctions), typeof(string), typeof(int) })] = "metaphone",
            [GetRuntimeMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchDoubleMetaphone), new[] { typeof(DbFunctions), typeof(string) })] = "dmetaphone",
            [GetRuntimeMethod(nameof(NpgsqlFuzzyStringMatchDbFunctionsExtensions.FuzzyStringMatchDoubleMetaphoneAlt), new[] { typeof(DbFunctions), typeof(string) })] = "dmetaphone_alt"
        };

        private static MethodInfo GetRuntimeMethod(string name, params Type[] parameters)
            => typeof(NpgsqlFuzzyStringMatchDbFunctionsExtensions).GetRuntimeMethod(name, parameters)!;

        private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        private static readonly bool[][] TrueArrays =
        {
            Array.Empty<bool>(),
            new[] { true },
            new[] { true, true },
            new[] { true, true, true },
            new[] { true, true, true, true },
            new[] { true, true, true, true, true },
            new[] { true, true, true, true, true, true }
        };

        public NpgsqlFuzzyStringMatchMethodTranslator([NotNull] NpgsqlSqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        /// <inheritdoc />
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => Functions.TryGetValue(method, out var function)
                ? _sqlExpressionFactory.Function(
                    function,
                    arguments.Skip(1),
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[arguments.Count - 1],
                    method.ReturnType)
                : null;
    }
}
