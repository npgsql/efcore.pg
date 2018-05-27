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
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

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
            if (!typeof(IList).IsAssignableFrom(expression.Method.DeclaringType))
                return null;

            switch (expression.Method.Name)
            {
            case "get_Item" when expression.Object is Expression instance:
                return Expression.MakeIndex(instance, instance.Type.GetRuntimeProperty("Item"), expression.Arguments);
//            case nameof(NpgsqlListExtensions.Contains):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "@>", typeof(bool));
//
//            case nameof(NpgsqlListExtensions.ContainedBy):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "<@", typeof(bool));
//
//            case nameof(NpgsqlListExtensions.Overlaps):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "&&", typeof(bool));
//
//            case nameof(NpgsqlListExtensions.IsStrictlyLeftOf):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "<<", typeof(bool));
//
//            case nameof(NpgsqlListExtensions.IsStrictlyRightOf):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], ">>", typeof(bool));
//
//            case nameof(NpgsqlListExtensions.DoesNotExtendRightOf):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "&<", typeof(bool));
//
//            case nameof(NpgsqlListExtensions.DoesNotExtendLeftOf):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "&>", typeof(bool));
//
//            case nameof(NpgsqlListExtensions.IsAdjacentTo):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "-|-", typeof(bool));
//
//            case nameof(NpgsqlListExtensions.Union):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "+", expression.Arguments[0].Type);
//
//            case nameof(NpgsqlListExtensions.Intersect):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "*", expression.Arguments[0].Type);
//
//            case nameof(NpgsqlListExtensions.Except):
//                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "-", expression.Arguments[0].Type);

            default:
                return null;
            }
        }
    }
}
