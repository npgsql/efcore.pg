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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates a method call into a PostgreSQL range operator.
    /// </summary>
    /// <remarks>
    /// By default, this class supports translation for methods declared on <see cref="NpgsqlRangeExtensions"/>.
    /// Additional methods can be supported so long as they declare a <see cref="NpgsqlRangeOperatorAttribute"/>.
    /// </remarks>
    public class NpgsqlRangeOperatorTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// Constructs a <see cref="Expression"/> from two arguments.
        /// </summary>
        /// <param name="left">
        /// The left-hand argument.
        /// </param>
        /// <param name="right">
        /// The left-hand argument.
        /// </param>
        [NotNull]
        delegate Expression BinaryExpressionFunction([NotNull] Expression left, [NotNull] Expression right);

        /// <summary>
        /// Maps the generic definitions of the methods supported by this translator to the appropriate PostgreSQL operator.
        /// </summary>
        [NotNull] static readonly Dictionary<MethodInfo, BinaryExpressionFunction> SupportedMethodTranslations;

        /// <summary>
        /// Initialize the <see cref="SupportedMethodTranslations"/> with extension methods from <see cref="NpgsqlRangeExtensions"/>.
        /// </summary>
        static NpgsqlRangeOperatorTranslator()
        {
            SupportedMethodTranslations = new Dictionary<MethodInfo, BinaryExpressionFunction>();

            foreach (MethodInfo methodInfo in typeof(NpgsqlRangeExtensions).GetRuntimeMethods())
            {
                TryAddSupportedMethod(methodInfo);
            }
        }

        /// <summary>
        /// Adds a method that can be translated as a PostgreSQL range operator. This method must declare a <see cref="NpgsqlBinaryOperatorAttribute"/>.
        /// </summary>
        /// <param name="methodInfo">
        /// The method to register.
        /// </param>
        /// <exception cref="ArgumentException" />
        public static void AddSupportedMethod([NotNull] MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            NpgsqlRangeOperatorAttribute attribute =
                methodInfo.GetCustomAttribute<NpgsqlRangeOperatorAttribute>();

            if (attribute is null)
                throw new ArgumentException($"{nameof(NpgsqlRangeOperatorAttribute)} is not declared on the method '{methodInfo.Name}' of {nameof(methodInfo.DeclaringType)}.");

            NpgsqlBinaryOperatorAttribute operatorAttribute =
                typeof(NpgsqlRangeOperatorType)
                    .GetRuntimeField(attribute.OperatorType.ToString())
                    .GetCustomAttribute<NpgsqlBinaryOperatorAttribute>();

            if (operatorAttribute is null)
                throw new ArgumentException($"{nameof(NpgsqlBinaryOperatorAttribute)} is not declared on the member '{attribute.OperatorType}' of {nameof(NpgsqlRangeOperatorType)}");

            SupportedMethodTranslations.Add(methodInfo, (a, b) => operatorAttribute.Create(a, b));
        }

        /// <summary>
        /// Attempts to add a method that can be translated as a PostgreSQL range operator. This method must declare a <see cref="NpgsqlBinaryOperatorAttribute"/>.
        /// </summary>
        /// <param name="methodInfo">
        /// The method to register.
        /// </param>
        /// <returns>
        /// True if the method was added successfully; otherwise, false.
        /// </returns>
        public static bool TryAddSupportedMethod([NotNull] MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            try
            {
                AddSupportedMethod(methodInfo);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a method from the list of supported translations.
        /// </summary>
        /// <param name="methodInfo">
        /// The method to unregister.
        /// </param>
        /// <exception cref="ArgumentException" />
        [UsedImplicitly]
        public static void RemoveSupportedMethod([NotNull] MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            SupportedMethodTranslations.Remove(methodInfo);
        }

        /// <summary>
        /// Attempts to remove a method from the list of supported translations.
        /// </summary>
        /// <param name="methodInfo">
        /// The method to unregister.
        /// </param>
        /// <exception cref="ArgumentException" />
        [UsedImplicitly]
        public static bool TryRemoveSupportedMethod([NotNull] MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            try
            {
                RemoveSupportedMethod(methodInfo);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression methodCallExpression) =>
            TryGetSupportedTranslation(methodCallExpression.Method, out BinaryExpressionFunction operatorFunction)
                ? operatorFunction(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1])
                : null;

        /// <summary>
        /// Tries to find create a <see cref="Expression"/> by looking up the <see cref="MethodInfo"/> in <see cref="SupportedMethodTranslations"/>.
        /// </summary>
        /// <param name="methodInfo">
        /// The <see cref="MethodInfo"/> for lookup.
        /// </param>
        /// <param name="operatorFunction">
        /// A delegate that constructs a <see cref="Expression"/>.
        /// </param>
        /// <returns>
        /// True if the <see cref="MethodInfo"/> was found or registered; otherwise false.
        /// </returns>
        static bool TryGetSupportedTranslation([NotNull] MethodInfo methodInfo, out BinaryExpressionFunction operatorFunction) =>
            methodInfo.IsGenericMethod
                ? SupportedMethodTranslations.TryGetValue(methodInfo.GetGenericMethodDefinition(), out operatorFunction)
                : SupportedMethodTranslations.TryGetValue(methodInfo, out operatorFunction);
    }
}
