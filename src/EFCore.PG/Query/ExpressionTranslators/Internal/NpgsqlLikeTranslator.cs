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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlLikeTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _like
            = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private static readonly MethodInfo _likeWithEscape
            = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        private static readonly MethodInfo _iLike
            = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private static readonly MethodInfo _iLikeWithEscape
            = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (Equals(methodCallExpression.Method, _likeWithEscape))
                return new LikeExpression(methodCallExpression.Arguments[1], methodCallExpression.Arguments[2], methodCallExpression.Arguments[3]);

            if (Equals(methodCallExpression.Method, _iLikeWithEscape))
                return new ILikeExpression(methodCallExpression.Arguments[1], methodCallExpression.Arguments[2], methodCallExpression.Arguments[3]);

            bool sensitive;
            if (Equals(methodCallExpression.Method, _like))
                sensitive = true;
            else if (Equals(methodCallExpression.Method, _iLike))
                sensitive = false;
            else
                return null;

            // PostgreSQL has backslash as the default LIKE escape character, but EF Core expects
            // no escape character unless explicitly requested (https://github.com/aspnet/EntityFramework/issues/8696).

            // If we have a constant expression, we check that there are no backslashes in order to render with
            // an ESCAPE clause (better SQL). If we have a constant expression with backslashes or a non-constant
            // expression, we render an ESCAPE clause to disable backslash escaping.

            if (methodCallExpression.Arguments[2] is ConstantExpression constantPattern &&
                constantPattern.Value is string pattern &&
                !pattern.Contains("\\"))
            {
                return sensitive
                    ? new LikeExpression(methodCallExpression.Arguments[1], methodCallExpression.Arguments[2])
                    : (Expression)new ILikeExpression(methodCallExpression.Arguments[1], methodCallExpression.Arguments[2]);
            }

            return sensitive
                ? new LikeExpression(methodCallExpression.Arguments[1], methodCallExpression.Arguments[2], Expression.Constant(string.Empty))
                : (Expression)new ILikeExpression(methodCallExpression.Arguments[1], methodCallExpression.Arguments[2], Expression.Constant(string.Empty));
        }
    }
}
