using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL string functions.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-string.html
    /// </remarks>
    public class NpgsqlStringMethodTranslator : IMethodCallTranslator
    {
        // Note: This is the PostgreSQL default and does not need to be explicitly specified
        const char LikeEscapeChar = '\\';

        readonly ISqlExpressionFactory _sqlExpressionFactory;
        readonly SqlConstantExpression _whitespace;

        #region MethodInfo

        [NotNull] static readonly MethodInfo Contains = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });
        [NotNull] static readonly MethodInfo EndsWith = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });
        [NotNull] static readonly MethodInfo StartsWith = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });
        [NotNull] static readonly MethodInfo IndexOfString = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string) });
        [NotNull] static readonly MethodInfo IndexOfChar = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(char) });
        [NotNull] static readonly MethodInfo IsNullOrWhiteSpace = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) });
        [NotNull] static readonly MethodInfo TrimStartWithNoParam = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new Type[0]);
        [NotNull] static readonly MethodInfo TrimStartWithChars = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char[]) });
        [NotNull] static readonly MethodInfo TrimStartWithSingleChar = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char) });
        [NotNull] static readonly MethodInfo TrimEndWithNoParam = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new Type[0]);
        [NotNull] static readonly MethodInfo TrimEndWithChars = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char[]) });
        [NotNull] static readonly MethodInfo TrimEndWithSingleChar = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char) });
        [NotNull] static readonly MethodInfo TrimBothWithNoParam = typeof(string).GetRuntimeMethod(nameof(string.Trim), Type.EmptyTypes);
        [NotNull] static readonly MethodInfo TrimBothWithChars = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) });
        [NotNull] static readonly MethodInfo TrimBothWithSingleChar = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char) });
        [NotNull] static readonly MethodInfo Substring = typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Substring)).Single(m => m.GetParameters().Length == 2);
        [NotNull] static readonly MethodInfo Replace = typeof(string).GetRuntimeMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) });
        [NotNull] static readonly MethodInfo PadLeft = typeof(string).GetRuntimeMethod(nameof(string.PadLeft), new[] { typeof(int) });
        [NotNull] static readonly MethodInfo PadLeftWithChar = typeof(string).GetRuntimeMethod(nameof(string.PadLeft), new[] { typeof(int), typeof(char) });
        [NotNull] static readonly MethodInfo PadRight = typeof(string).GetRuntimeMethod(nameof(string.PadRight), new[] { typeof(int) });
        [NotNull] static readonly MethodInfo PadRightWithChar = typeof(string).GetRuntimeMethod(nameof(string.PadRight), new[] { typeof(int), typeof(char) });
        [NotNull] static readonly MethodInfo ToLower = typeof(string).GetRuntimeMethod(nameof(string.ToLower), Array.Empty<Type>());
        [NotNull] static readonly MethodInfo ToUpper = typeof(string).GetRuntimeMethod(nameof(string.ToUpper), Array.Empty<Type>());

        #endregion

        public NpgsqlStringMethodTranslator(ISqlExpressionFactory sqlExpressionFactory, NpgsqlTypeMappingSource npgsqlTypeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _whitespace = _sqlExpressionFactory.Constant(
                @" \t\n\r",  // TODO: Complete this
                npgsqlTypeMappingSource.EStringTypeMapping);
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (method == IndexOfString || method == IndexOfChar)
            {
                var argument = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, argument);

                return _sqlExpressionFactory.Subtract(
                    _sqlExpressionFactory.Function(
                        "STRPOS",
                        new[]
                        {
                            _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                            _sqlExpressionFactory.ApplyTypeMapping(argument, stringTypeMapping)
                        },
                        method.ReturnType),
                    _sqlExpressionFactory.Constant(1));
            }

            if (method == Replace)
            {
                var oldValue = arguments[0];
                var newValue = arguments[1];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, oldValue, newValue);

                return _sqlExpressionFactory.Function(
                    "REPLACE",
                    new[]
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(oldValue, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(newValue, stringTypeMapping)
                    },
                    method.ReturnType,
                    stringTypeMapping);
            }

            if (method == ToLower || method == ToUpper)
            {
                return _sqlExpressionFactory.Function(
                    method == ToLower ? "LOWER" : "UPPER",
                    new[] { instance },
                    method.ReturnType,
                    instance.TypeMapping);
            }

            if (method == Substring)
            {
                return _sqlExpressionFactory.Function(
                    "SUBSTRING",
                    new[]
                    {
                        instance,
                        _sqlExpressionFactory.Add(
                            arguments[0],
                            _sqlExpressionFactory.Constant(1)),
                        arguments[1]
                    },
                    method.ReturnType,
                    instance.TypeMapping);
            }

            if (method == IsNullOrWhiteSpace)
            {
                var argument = arguments[0];

                return _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.IsNull(argument),
                    _sqlExpressionFactory.Equal(
                        _sqlExpressionFactory.Function(
                            "BTRIM",
                            new[]
                            {
                                argument,
                                _whitespace
                            },
                            argument.Type,
                            argument.TypeMapping),
                        _sqlExpressionFactory.Constant(string.Empty, argument.TypeMapping)));
            }

            var isTrimStart = method == TrimStartWithNoParam || method == TrimStartWithChars || method == TrimStartWithSingleChar;
            var isTrimEnd   = method == TrimEndWithNoParam   || method == TrimEndWithChars   || method == TrimEndWithSingleChar;
            var isTrimBoth  = method == TrimBothWithNoParam  || method == TrimBothWithChars  || method == TrimBothWithSingleChar;
            if (isTrimStart || isTrimEnd || isTrimBoth)
            {
                char[] trimChars = null;

                if (method == TrimStartWithChars || method == TrimStartWithSingleChar ||
                    method == TrimEndWithChars   || method == TrimEndWithSingleChar   ||
                    method == TrimBothWithChars  || method == TrimBothWithSingleChar)
                {
                    var constantTrimChars = arguments[0] as SqlConstantExpression;
                    if (constantTrimChars == null)
                        return null; // Don't translate if trim chars isn't a constant

                    trimChars = constantTrimChars.Value is char c
                        ? new[] { c }
                        : (char[])constantTrimChars.Value;
                }

                return _sqlExpressionFactory.Function(
                    isTrimStart ? "LTRIM" : isTrimEnd ? "RTRIM" : "BTRIM",
                    new[]
                    {
                        instance,
                        trimChars == null || trimChars.Length == 0
                            ? _whitespace
                            : _sqlExpressionFactory.Constant(new string(trimChars))
                    },
                    instance.Type,
                    instance.TypeMapping);
            }

            if (method == Contains)
            {
                var pattern = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);
                instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
                pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping);

                return _sqlExpressionFactory.GreaterThan(
                    _sqlExpressionFactory.Function(
                        "STRPOS",
                        new[]
                        {
                            _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping),
                            _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping)
                        },
                        typeof(int)),
                    _sqlExpressionFactory.Constant(0));
            }

            if (method == PadLeft || method == PadLeftWithChar || method == PadRight || method == PadRightWithChar)
            {
                var args =
                    method == PadLeft || method == PadRight
                        ? new[] { instance, arguments[0] }
                        : new[] { instance, arguments[0], arguments[1] };

                return _sqlExpressionFactory.Function(
                    method == PadLeft || method == PadLeftWithChar ? "lpad" : "rpad",
                    args,
                    instance.Type,
                    instance.TypeMapping);
            }

            if (method == StartsWith)
                return TranslateStartsEndsWith(instance, arguments[0], true);
            if (method == EndsWith)
                return TranslateStartsEndsWith(instance, arguments[0], false);

            return null;
        }

        SqlExpression TranslateStartsEndsWith(SqlExpression instance, SqlExpression pattern, bool startsWith)
        {
            var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);

            instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
            pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping);

            if (pattern is SqlConstantExpression constantExpression)
            {
                // The pattern is constant. Aside from null, we escape all special characters (%, _, \)
                // in C# and send a simple LIKE
                if (!(constantExpression.Value is string constantString))
                    return _sqlExpressionFactory.Like(instance, _sqlExpressionFactory.Constant(null, stringTypeMapping));

                return _sqlExpressionFactory.Like(
                    instance,
                    _sqlExpressionFactory.Constant(
                        startsWith
                            ? EscapeLikePattern(constantString) + '%'
                            : '%' + EscapeLikePattern(constantString)));
            }

            // The pattern is non-constant, we use LEFT or RIGHT to extract substring and compare.
            // For StartsWith we also first run a LIKE to quickly filter out most non-matching results (sargable, but imprecise
            // because of wildchars).
            if (startsWith)
            {
                return _sqlExpressionFactory.AndAlso(
                    _sqlExpressionFactory.Like(
                        instance,
                        _sqlExpressionFactory.Add(
                            instance,
                            _sqlExpressionFactory.Constant("%"))),
                    _sqlExpressionFactory.Equal(
                        _sqlExpressionFactory.Function(
                            "LEFT",
                            new[] {
                                instance,
                                _sqlExpressionFactory.Function("LENGTH", new[] { pattern }, typeof(int))
                            },
                            typeof(string),
                            stringTypeMapping),
                        pattern));
            }

            return _sqlExpressionFactory.Equal(
                _sqlExpressionFactory.Function(
                    "RIGHT",
                    new[] {
                        instance,
                        _sqlExpressionFactory.Function("LENGTH", new[] { pattern }, typeof(int))
                    },
                    typeof(string),
                    stringTypeMapping),
                pattern);
        }

        bool IsLikeWildChar(char c) => c == '%' || c == '_';

        string EscapeLikePattern(string pattern)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < pattern.Length; i++)
            {
                var c = pattern[i];
                if (IsLikeWildChar(c) || c == LikeEscapeChar)
                {
                    builder.Append(LikeEscapeChar);
                }

                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
