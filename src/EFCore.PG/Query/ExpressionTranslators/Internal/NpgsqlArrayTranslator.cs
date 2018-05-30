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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL array operators mapped to methods declared on
    /// <see cref="Array"/>, <see cref="Enumerable"/>, <see cref="List{T}"/>, and <see cref="NpgsqlArrayExtensions"/>.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-array.html
    /// </remarks>
    public class NpgsqlArrayTranslator : IMethodCallTranslator
    {
        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression expression)
        {
            if (!IsTypeSupported(expression))
                return null;

            switch (expression.Method.Name)
            {
            #region EnumerableStaticMethods

            case nameof(Enumerable.ElementAt):
                return Expression.MakeIndex(expression.Arguments[0], expression.Arguments[0].Type.GetRuntimeProperty("Item"), new[] { expression.Arguments[1] });

            // TODO: Currently handled in NpgsqlSqlTranslatingExpressionVisitor.
            case nameof(Enumerable.Append):
            case nameof(Enumerable.Concat) when IsArrayOrList(expression.Arguments[1].Type):
                return new CustomBinaryExpression(expression.Arguments[0], expression.Arguments[1], "||", expression.Arguments[0].Type);

            case nameof(Enumerable.Count):
                return new SqlFunctionExpression("array_length", typeof(int), new[] { expression.Arguments[0], Expression.Constant(1) });

            case nameof(Enumerable.Prepend):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[0], "||", expression.Arguments[0].Type);

            case nameof(Enumerable.SequenceEqual):
                return Expression.MakeBinary(ExpressionType.Equal, expression.Arguments[0], expression.Arguments[1]);

            #endregion

            #region NpgsqlArrayExtensions

            case nameof(NpgsqlArrayExtensions.ArrayToString):
                return new SqlFunctionExpression("array_to_string", typeof(string), expression.Arguments.Skip(1));

            case nameof(NpgsqlArrayExtensions.StringToArray):
                return new SqlFunctionExpression("string_to_array", expression.Method.ReturnType, expression.Arguments.Skip(1));

            case nameof(NpgsqlArrayExtensions.StringToList):
                return new SqlFunctionExpression("string_to_array", expression.Method.ReturnType, expression.Arguments.Skip(1));

            #endregion

            #region ArrayStaticMethods

            case nameof(Array.IndexOf) when expression.Method.DeclaringType == typeof(Array):
                return
                    new SqlFunctionExpression(
                        "COALESCE",
                        typeof(int),
                        new Expression[]
                        {
                            new SqlFunctionExpression("array_position", typeof(int), expression.Arguments),
                            Expression.Constant(-1)
                        });

            #endregion

            #region ListInstanceMethods

            case "get_Item" when expression.Object is Expression instance:
                return Expression.MakeIndex(instance, instance.Type.GetRuntimeProperty("Item"), expression.Arguments);

            case nameof(IList.IndexOf) when IsArrayOrList(expression.Method.DeclaringType):
                return
                    new SqlFunctionExpression(
                        "COALESCE",
                        typeof(int),
                        new Expression[]
                        {
                            new SqlFunctionExpression("array_position", typeof(int), new[] { expression.Object, expression.Arguments[0] }),
                            Expression.Constant(-1)
                        });

            #endregion

            default:
                return null;
            }
        }

        #region Helpers

        /// <summary>
        /// Tests if the instance or argument types are supported.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="MethodCallExpression"/> to test.
        /// </param>
        /// <returns>
        /// True if the instance or argument types are supported; otherwise, false.
        /// </returns>
        static bool IsTypeSupported([NotNull] MethodCallExpression expression)
        {
            Type declaringType = expression.Method.DeclaringType;

            // Methods declared here are always translated.
            if (declaringType == typeof(NpgsqlArrayExtensions))
                return true;

            // Methods not declared here are never translated.
            if (!IsArrayOrList(declaringType) &&
                declaringType != typeof(Array) &&
                declaringType != typeof(Enumerable))
                return false;

            // Instance methods are only translated for T[] and List<T>.
            if (expression.Object is Expression instance)
                return IsArrayOrList(instance.Type);

            // Extension methods may  only be translated when a parameter is T[] or List<T>
            if (expression.Object is null)
            {
                // Static method with no parameters? Skip.
                if (expression.Arguments.Count == 0)
                    return false;

                // Is this an extension method on T[] or List<T>?
                return IsArrayOrList(expression.Arguments[0].Type);
            }

            // Something else? Skip.
            return false;
        }

        /// <summary>
        /// Tests if the type is an array or a <see cref="List{T}"/>.
        /// </summary>
        /// <param name="type">
        /// The type to test.
        /// </param>
        /// <returns>
        /// True if <paramref name="type"/> is an array or a <see cref="List{T}"/>; otherwise, false.
        /// </returns>
        static bool IsArrayOrList(Type type) => type.IsArray || type.IsGenericType && typeof(List<>) == type.GetGenericTypeDefinition();

        #endregion
    }
}
