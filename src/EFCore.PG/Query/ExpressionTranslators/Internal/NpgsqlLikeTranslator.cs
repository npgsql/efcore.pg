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
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates <see cref="T:DbFunctionsExtensions.Like"/> methods into PostgreSQL LIKE expressions.
    /// </summary>
    public class NpgsqlLikeTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo Like =
            typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        static readonly MethodInfo LikeWithEscape =
            typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        // ReSharper disable once InconsistentNaming
        static readonly MethodInfo ILike =
            typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        // ReSharper disable once InconsistentNaming
        static readonly MethodInfo ILikeWithEscape =
            typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression e)
        {
            Check.NotNull(e, nameof(e));

            if (Equals(e.Method, LikeWithEscape))
                return new LikeExpression(e.Arguments[1], e.Arguments[2], e.Arguments[3]);

            if (Equals(e.Method, ILikeWithEscape))
                return new ILikeExpression(e.Arguments[1], e.Arguments[2], e.Arguments[3]);

            bool sensitive;
            if (Equals(e.Method, Like))
                sensitive = true;
            else if (Equals(e.Method, ILike))
                sensitive = false;
            else
                return null;

            // PostgreSQL has backslash as the default LIKE escape character, but EF Core expects
            // no escape character unless explicitly requested (https://github.com/aspnet/EntityFramework/issues/8696).

            // If we have a constant expression, we check that there are no backslashes in order to render with
            // an ESCAPE clause (better SQL). If we have a constant expression with backslashes or a non-constant
            // expression, we render an ESCAPE clause to disable backslash escaping.

            if (e.Arguments[2] is ConstantExpression constantPattern &&
                constantPattern.Value is string pattern &&
                !pattern.Contains("\\"))
            {
                return sensitive
                    ? new LikeExpression(e.Arguments[1], e.Arguments[2])
                    : (Expression)new ILikeExpression(e.Arguments[1], e.Arguments[2]);
            }

            return sensitive
                ? new LikeExpression(e.Arguments[1], e.Arguments[2], Expression.Constant(string.Empty))
                : (Expression)new ILikeExpression(e.Arguments[1], e.Arguments[2], Expression.Constant(string.Empty));
        }
    }
}
