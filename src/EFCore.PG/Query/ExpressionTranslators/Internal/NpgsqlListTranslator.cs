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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for <see cref="List{T}"/> methods as PostgreSQL array operators.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-array.html
    /// </remarks>
    public class NpgsqlListTranslator : IMethodCallTranslator
    {
        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression expression)
        {
            if (expression.Object != null && !typeof(IList).IsAssignableFrom(expression.Object.Type))
                return null;
            if (expression.Object == null && expression.Arguments.Count > 0 && !typeof(IList).IsAssignableFrom(expression.Arguments[0].Type))
                return null;

            // TODO: use #430 to map @> to source.All(x => other.Contains(x));
            // TODO: use #430 to map && to soucre.Any(x => other.Contains(x));

            switch (expression.Method.Name)
            {
            // TODO: Currently handled in NpgsqlSqlTranslatingExpressionVisitor.
            case nameof(Enumerable.Append):
            case nameof(Enumerable.Concat):
                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "||", expression.Arguments[0].Type);

            // TODO: Currently handled in NpgsqlSqlTranslatingExpressionVisitor.
            case nameof(Enumerable.Count):
                return new SqlFunctionExpression("array_length", typeof(int), new[] { expression.Arguments[0], Expression.Constant(1) });

            case "get_Item" when expression.Object is Expression instance:
                return Expression.MakeIndex(instance, instance.Type.GetRuntimeProperty("Item"), expression.Arguments);

            case nameof(Enumerable.ElementAt):
                return Expression.MakeIndex(expression.Arguments[0], expression.Arguments[0].Type.GetRuntimeProperty("Item"), new[] { expression.Arguments[1] });

            case nameof(Enumerable.Prepend):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[0], "||", expression.Arguments[0].Type);

            case nameof(Enumerable.SequenceEqual):
                return Expression.MakeBinary(ExpressionType.Equal, expression.Arguments[0], expression.Arguments[1]);

            case nameof(ToString):
                return new SqlFunctionExpression("array_to_string", typeof(string), new[] { expression.Object, Expression.Constant(",") });

            case nameof(IList.IndexOf):
                return
                    new SqlFunctionExpression(
                        "COALESCE",
                        typeof(int),
                        new Expression[]
                        {
                            new SqlFunctionExpression(
                                "array_position",
                                typeof(int),
                                expression.Object is null
                                    ? (IEnumerable<Expression>)expression.Arguments
                                    : new[] { expression.Object, expression.Arguments[0] }),
                            Expression.Constant(-1)
                        });

            default:
                return null;
            }
        }
    }
}
