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

using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates a method call into a <see cref="NpgsqlRangeOperatorExpression"/>.
    /// </summary>
    public class NpgsqlRangeOperatorTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// Maps the generic definitions of the methods supported by this translator to the appropriate PostgreSQL operator.
        /// </summary>
        [NotNull] static readonly ConcurrentDictionary<MethodInfo, NpgsqlRangeOperatorType> SupportedMethodTranslations =
            new ConcurrentDictionary<MethodInfo, NpgsqlRangeOperatorType>(
                typeof(NpgsqlRangeExtensions)
                    .GetRuntimeMethods()
                    .Select(x => (MethodInfo: x, Attribute: x.GetCustomAttribute<NpgsqlRangeOperatorAttribute>()))
                    .Where(x => x.Attribute != null)
                    .ToDictionary(x => x.MethodInfo, x => x.Attribute.OperatorType));

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression methodCallExpression) =>
            TryGetSupportedTranslation(methodCallExpression.Method, out NpgsqlRangeOperatorType operatorType)
                ? new NpgsqlRangeOperatorExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], operatorType)
                : null;

        /// <summary>
        /// Tries to find the <see cref="NpgsqlRangeOperatorType"/> by looking up the <see cref="MethodInfo"/> in <see cref="SupportedMethodTranslations"/>.
        /// If the <see cref="MethodInfo"/> is not found, but the method is checked for a <see cref="NpgsqlRangeOperatorAttribute"/>.
        /// If one is found, it is registered with <see cref="SupportedMethodTranslations"/> and the <see cref="NpgsqlRangeOperatorType"/> is returned.
        /// </summary>
        /// <param name="methodInfo">
        /// The <see cref="MethodInfo"/> for lookup.
        /// </param>
        /// <param name="operatorType">
        /// The <see cref="NpgsqlRangeOperatorType"/> if successful.
        /// </param>
        /// <returns>
        /// True if the <see cref="MethodInfo"/> was found or registered; otherwise false.
        /// </returns>
        static bool TryGetSupportedTranslation([NotNull] MethodInfo methodInfo, out NpgsqlRangeOperatorType operatorType)
        {
            MethodInfo info = methodInfo.IsGenericMethod ? methodInfo.GetGenericMethodDefinition() : methodInfo;

            if (SupportedMethodTranslations.TryGetValue(info, out operatorType))
            {
                return true;
            }

            if (info.GetCustomAttribute<NpgsqlRangeOperatorAttribute>() is NpgsqlRangeOperatorAttribute attribute)
            {
                operatorType = attribute.OperatorType;
                return SupportedMethodTranslations.TryAdd(info, operatorType);
            }

            return false;
        }
    }
}
