#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
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

using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringStartsWithTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        static readonly MethodInfo _concat
            = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        public virtual Expression Translate(MethodCallExpression e)
        {
            if (!e.Method.Equals(_methodInfo) || e.Object == null)
                return null;

            var constantPatternExpr = e.Arguments[0] as ConstantExpression;
            if (constantPatternExpr != null)
            {
                // The pattern is constant. Escape all special characters (%, _, \) in C# and send
                // a simple LIKE
                return new LikeExpression(
                    e.Object,
                    Expression.Constant(Regex.Replace((string)constantPatternExpr.Value, @"([%_\\])", @"\$1") + '%')
                );
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

            // If StartsWith is being invoked on a citext, the LEFT() function above will return a reglar text
            // and the comparison will be case-sensitive. So we need to explicitly cast LEFT()'s return type
            // to citext. See #319.
            if (e.Object.FindProperty(typeof(string))?.GetConfiguredColumnType() == "citext")
                leftExpr = new ExplicitStoreTypeCastExpression(leftExpr, typeof(string), "citext");

            return Expression.AndAlso(
                new LikeExpression(e.Object, Expression.Add(pattern, Expression.Constant("%"), _concat)),
                Expression.Equal(
                    leftExpr,
                    pattern
                )
            );
        }
    }
}
