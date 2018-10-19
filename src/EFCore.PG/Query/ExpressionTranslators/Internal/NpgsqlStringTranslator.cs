#region License

// The PostgreSQL License
//
// Copyright (C) 2018 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL string functions.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-string.html
    /// </remarks>
    public class NpgsqlStringTranslator : IMethodCallTranslator
    {
        #region MethodInfo

        [NotNull] static readonly MethodInfo Concat = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });
        [NotNull] static readonly MethodInfo Contains = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });
        [NotNull] static readonly MethodInfo EndsWith = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });
        [NotNull] static readonly MethodInfo StartsWith = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });
        [NotNull] static readonly MethodInfo IndexOfString = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string) });
        [NotNull] static readonly MethodInfo IndexOfChar = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(char) });
        [NotNull] static readonly MethodInfo IsNullOrWhiteSpace = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) });
        [NotNull] static readonly MethodInfo Trim = typeof(string).GetRuntimeMethod(nameof(string.Trim), Type.EmptyTypes);
        [NotNull] static readonly MethodInfo TrimWithChars = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) });
        [NotNull] static readonly MethodInfo TrimEndWithChars = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char[]) });
        [NotNull] static readonly MethodInfo TrimStartWithChars = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char[]) });
        [NotNull] static readonly MethodInfo Substring = typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Substring)).Single(m => m.GetParameters().Length == 2);
        [NotNull] static readonly MethodInfo Replace = typeof(string).GetRuntimeMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) });

        // The following exist as optimizations in netcoreapp20
        [NotNull] static readonly MethodInfo TrimWithSingleChar = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char) });
        [NotNull] static readonly MethodInfo TrimEndNoParam = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new Type[0]);
        [NotNull] static readonly MethodInfo TrimEndSingleChar = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char) });
        [NotNull] static readonly MethodInfo TrimStartNoParam = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new Type[0]);
        [NotNull] static readonly MethodInfo TrimStartSingleChar = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char) });

        #endregion

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression e)
        {
            if (e.Method.DeclaringType != typeof(string))
                return null;

            return TranslateContains(e) ??
                   TranslateEndsWith(e) ??
                   TranslateStartsWith(e) ??
                   TranslateIndexOf(e) ??
                   TranslateIsNullOrWhiteSpace(e) ??
                   TranslateTrim(e) ??
                   TranslateTrimEnd(e) ??
                   TranslateTrimStart(e) ??
                   TranslateSubstring(e) ??
                   TranslateReplace(e);
        }

        #region Contains

        [CanBeNull]
        static Expression TranslateContains([NotNull] MethodCallExpression e)
        {
            if (e.Object == null || e.Method != Contains)
                return null;

            var argument0 = e.Arguments[0];

            // If Contains() is being invoked on a citext, ensure that the string argument is explicitly cast into a
            // citext (instead of the default text for a CLR string) as otherwise PostgreSQL prefers the text variant of
            // the ambiguous call STRPOS(citext, text) and the search will be case-sensitive. See #384.
            if (argument0 != null && e.Object?.FindProperty(typeof(string))?.GetConfiguredColumnType() == "citext")
                argument0 = new ExplicitStoreTypeCastExpression(argument0, typeof(string), "citext");

            return Expression.GreaterThan(
                new SqlFunctionExpression("STRPOS", typeof(int), new[] { e.Object, argument0 }),
                Expression.Constant(0));
        }

        #endregion

        #region EndsWith, StartsWith

        [CanBeNull]
        static Expression TranslateEndsWith([NotNull] MethodCallExpression e)
        {
            if (e.Object == null || e.Method != EndsWith)
                return null;

            return Expression.Equal(
                new SqlFunctionExpression(
                    "RIGHT",
                    e.Object.Type,
                    new[] { e.Object, new SqlFunctionExpression("LENGTH", typeof(int), new[] { e.Arguments[0] }) }),
                e.Arguments[0]);
        }

        [CanBeNull]
        static Expression TranslateStartsWith([NotNull] MethodCallExpression e)
        {
            if (e.Object == null || e.Method != StartsWith)
                return null;

            if (e.Arguments[0] is ConstantExpression constantPatternExpr)
            {
                // The pattern is constant. Escape all special characters (%, _, \) in C# and send
                // a simple LIKE
                return new LikeExpression(
                    e.Object,
                    Expression.Constant(Regex.Replace((string)constantPatternExpr.Value, @"([%_\\])", @"\$1") + '%'));
            }

            // The pattern isn't a constant (i.e. parameter, database column...).
            // First run LIKE against the *unescaped* pattern (which will efficiently use indices),
            // but then add another test to filter out false positives.
            var pattern = e.Arguments[0];

            Expression leftExpr = new SqlFunctionExpression("LEFT", typeof(string), new[]
            {
                e.Object,
                new SqlFunctionExpression("LENGTH", typeof(int), new[] { pattern }),
            });

            // If StartsWith is being invoked on a citext, the LEFT() function above will return a regular text
            // and the comparison will be case-sensitive. So we need to explicitly cast LEFT()'s return type
            // to citext. See #319.
            if (e.Object.FindProperty(typeof(string))?.GetConfiguredColumnType() == "citext")
                leftExpr = new ExplicitStoreTypeCastExpression(leftExpr, typeof(string), "citext");

            return Expression.AndAlso(
                new LikeExpression(e.Object, Expression.Add(pattern, Expression.Constant("%"), Concat)),
                Expression.Equal(leftExpr, pattern));
        }

        #endregion

        #region IndexOf

        [CanBeNull]
        static Expression TranslateIndexOf([NotNull] MethodCallExpression e)
        {
            if (e.Object == null || e.Method != IndexOfString && e.Method != IndexOfChar)
                return null;

            return Expression.Subtract(
                new SqlFunctionExpression("STRPOS", e.Type, new[] { e.Object }.Concat(e.Arguments)),
                Expression.Constant(1)
            );
        }

        #endregion

        #region IsNullOrWhiteSpace

        [CanBeNull]
        static Expression TranslateIsNullOrWhiteSpace([NotNull] MethodCallExpression e)
        {
            if (e.Method != IsNullOrWhiteSpace)
                return null;

            return Expression.MakeBinary(
                ExpressionType.OrElse,
                new IsNullExpression(e.Arguments[0]),
                new RegexMatchExpression(e.Arguments[0], Expression.Constant(@"^\s*$"), RegexOptions.Singleline));
        }

        #endregion

        #region Trim, TrimEnd, TrimStart

        [CanBeNull]
        static Expression TranslateTrim([NotNull] MethodCallExpression e)
        {
            if (e.Object == null ||
                e.Method != Trim &&
                e.Method != TrimWithChars &&
                e.Method != TrimWithSingleChar)
                return null;

            if (e.Method == Trim)
            {
                // Note that PostgreSQL TRIM() does spaces only, not all whitespace, so we use a regex
                return new SqlFunctionExpression(
                    "REGEXP_REPLACE",
                    typeof(string),
                    new[]
                    {
                        e.Object,
                        Expression.Constant(@"^\s*(.*?)\s*$"),
                        Expression.Constant(@"\1")
                    });
            }

            if (e.Method == TrimWithChars)
            {
                if (!(e.Arguments[0] is ConstantExpression constantTrimChars))
                    return null;

                return new SqlFunctionExpression(
                    "BTRIM",
                    typeof(string),
                    new[]
                    {
                        e.Object,
                        Expression.Constant(new string((char[])constantTrimChars.Value))
                    });
            }

            if (e.Method != TrimWithSingleChar)
                return null;

            if (!(e.Arguments[0] is ConstantExpression constantTrimChar))
                return null;

            return new SqlFunctionExpression(
                "BTRIM",
                typeof(string),
                new[]
                {
                    e.Object,
                    Expression.Constant(new string((char)constantTrimChar.Value, 1))
                });
        }

        [CanBeNull]
        static Expression TranslateTrimEnd([NotNull] MethodCallExpression e)
        {
            if (e.Object == null ||
                e.Method != TrimEndWithChars &&
                e.Method != TrimEndNoParam &&
                e.Method != TrimEndSingleChar)
                return null;

            char[] trimChars;

            if (e.Method == TrimEndSingleChar)
            {
                var constantTrimChars = e.Arguments[0] as ConstantExpression;
                if (constantTrimChars == null)
                    return null; // Don't translate if trim chars isn't a constant

                var trimChar = (char)constantTrimChars.Value;
                return new SqlFunctionExpression(
                    "RTRIM",
                    typeof(string),
                    new[]
                    {
                        e.Object,
                        Expression.Constant(new string(trimChar, 1))
                    });
            }

            if (e.Method == TrimEndNoParam)
            {
                trimChars = null;
            }
            else if (e.Method == TrimEndWithChars)
            {
                var constantTrimChars = e.Arguments[0] as ConstantExpression;
                if (constantTrimChars == null)
                {
                    return null; // Don't translate if trim chars isn't a constant
                }

                trimChars = (char[])constantTrimChars.Value;
            }
            else
            {
                throw new Exception($"{nameof(NpgsqlStringTranslator)} does not support {e}");
            }

            if (trimChars == null || trimChars.Length == 0)
            {
                // Trim whitespace
                return new SqlFunctionExpression(
                    "REGEXP_REPLACE",
                    typeof(string),
                    new[]
                    {
                        e.Object,
                        Expression.Constant(@"\s*$"),
                        Expression.Constant(string.Empty)
                    });
            }

            return new SqlFunctionExpression(
                "RTRIM",
                typeof(string),
                new[]
                {
                    e.Object,
                    Expression.Constant(new string(trimChars))
                });
        }

        [CanBeNull]
        static Expression TranslateTrimStart([NotNull] MethodCallExpression e)
        {
            if (e.Object == null ||
                e.Method != TrimStartWithChars &&
                e.Method != TrimStartNoParam &&
                e.Method != TrimStartSingleChar)
                return null;

            char[] trimChars;

            if (e.Method == TrimStartSingleChar)
            {
                var constantTrimChars = e.Arguments[0] as ConstantExpression;
                if (constantTrimChars == null)
                    return null; // Don't translate if trim chars isn't a constant
                var trimChar = (char)constantTrimChars.Value;
                return new SqlFunctionExpression(
                    "LTRIM",
                    typeof(string),
                    new[]
                    {
                        e.Object,
                        Expression.Constant(new string(trimChar, 1))
                    });
            }

            if (e.Method == TrimStartNoParam)
            {
                trimChars = null;
            }
            else if (e.Method == TrimStartWithChars)
            {
                var constantTrimChars = e.Arguments[0] as ConstantExpression;
                if (constantTrimChars == null)
                    return null; // Don't translate if trim chars isn't a constant
                trimChars = (char[])constantTrimChars.Value;
            }
            else
            {
                throw new Exception($"{nameof(NpgsqlStringTranslator)} does not support {e}");
            }

            if (trimChars == null || trimChars.Length == 0)
            {
                // Trim whitespace
                return new SqlFunctionExpression(
                    "REGEXP_REPLACE",
                    typeof(string),
                    new[]
                    {
                        e.Object,
                        Expression.Constant(@"^\s*"),
                        Expression.Constant(string.Empty)
                    });
            }

            return new SqlFunctionExpression(
                "LTRIM",
                typeof(string),
                new[]
                {
                    e.Object,
                    Expression.Constant(new string(trimChars))
                });
        }

        #endregion

        #region Substring

        [CanBeNull]
        static Expression TranslateSubstring([NotNull] MethodCallExpression e)
        {
            if (e.Object == null || e.Method != Substring)
                return null;

            return new SqlFunctionExpression(
                "SUBSTRING",
                e.Type,
                new[]
                {
                    e.Object,
                    // Accommodate for SQL Server assumption of 1-based string indexes
                    e.Arguments[0].NodeType == ExpressionType.Constant
                        ? (Expression)Expression.Constant((int)((ConstantExpression)e.Arguments[0]).Value + 1)
                        : Expression.Add(e.Arguments[0], Expression.Constant(1)),
                    e.Arguments[1]
                });
        }

        #endregion

        #region Replace

        [CanBeNull]
        static Expression TranslateReplace([NotNull] MethodCallExpression e)
        {
            if (e.Object == null || e.Method != Replace)
                return null;

            return new SqlFunctionExpression("REPLACE", e.Type, new[] { e.Object }.Concat(e.Arguments));
        }

        #endregion
    }
}
