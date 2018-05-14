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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates a method call into a PostgreSQL range operator.
    /// </summary>
    public class NpgsqlRangeOperatorTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// Constructs a <see cref="CustomBinaryExpression"/> from two arguments.
        /// </summary>
        /// <param name="left">
        /// The left-hand argument.
        /// </param>
        /// <param name="right">
        /// The left-hand argument.
        /// </param>
        [NotNull]
        delegate CustomBinaryExpression CustomBinaryExpressionFunction([NotNull] Expression left, [NotNull] Expression right);

        /// <summary>
        /// Maps the generic definitions of the methods supported by this translator to the appropriate PostgreSQL operator.
        /// </summary>
        [NotNull] static readonly Dictionary<MethodInfo, CustomBinaryExpressionFunction> SupportedMethodTranslations;

        /// <summary>
        /// Initialize the <see cref="SupportedMethodTranslations"/> with extension methods from <see cref="NpgsqlRangeExtensions"/>.
        /// </summary>
        static NpgsqlRangeOperatorTranslator()
        {
            SupportedMethodTranslations = new Dictionary<MethodInfo, CustomBinaryExpressionFunction>();

            foreach (MethodInfo methodInfo in typeof(NpgsqlRangeExtensions).GetRuntimeMethods())
            {
                TryAddSupportedMethod(methodInfo);
            }
        }

        /// <summary>
        /// Adds a method that can be translated as a PostgreSQL range operator. This method must declare a <see cref="NpgsqlOperatorAttribute"/>.
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
                throw new ArgumentException($"{nameof(NpgsqlRangeOperatorAttribute)} is not declared on {methodInfo.Name}.");

            NpgsqlOperatorAttribute operatorAttribute =
                typeof(NpgsqlRangeOperatorType)
                    .GetMember(attribute.OperatorType.ToString())
                    .Single()
                    .GetCustomAttribute<NpgsqlOperatorAttribute>();

            if (operatorAttribute is null)
                throw new ArgumentException($"{nameof(NpgsqlOperatorAttribute)} is not declared on the member '{attribute.OperatorType}' of {nameof(NpgsqlRangeOperatorType)}");

            SupportedMethodTranslations.Add(methodInfo, (a, b) => operatorAttribute.Create(a, b));
        }

        /// <summary>
        /// Attempts to add a method that can be translated as a PostgreSQL range operator. This method must declare a <see cref="NpgsqlOperatorAttribute"/>.
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
        /// The method to register.
        /// </param>
        /// <exception cref="ArgumentException" />
        [UsedImplicitly]
        public static void RemoveSupportedMethod([NotNull] MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            SupportedMethodTranslations.Remove(methodInfo);
        }

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression methodCallExpression) =>
            TryGetSupportedTranslation(methodCallExpression.Method, out CustomBinaryExpressionFunction operatorFunction)
                ? operatorFunction(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1])
                : null;

        /// <summary>
        /// Tries to find create a <see cref="CustomBinaryExpression"/> by looking up the <see cref="MethodInfo"/> in <see cref="SupportedMethodTranslations"/>.
        /// </summary>
        /// <param name="methodInfo">
        /// The <see cref="MethodInfo"/> for lookup.
        /// </param>
        /// <param name="operatorFunction">
        /// A delegate that constructs a <see cref="CustomBinaryExpression"/>.
        /// </param>
        /// <returns>
        /// True if the <see cref="MethodInfo"/> was found or registered; otherwise false.
        /// </returns>
        static bool TryGetSupportedTranslation([NotNull] MethodInfo methodInfo, out CustomBinaryExpressionFunction operatorFunction)
        {
            MethodInfo info = methodInfo.IsGenericMethod ? methodInfo.GetGenericMethodDefinition() : methodInfo;

            return SupportedMethodTranslations.TryGetValue(info, out operatorFunction);
        }
    }

    /// <summary>
    /// Indicates that a method can be translated to a PostgreSQL range operator.
    /// </summary>
    /// <remarks>
    /// This attribute allows other extension methods to hook into the range operator translations.
    /// Along with simplifying the code required to identify overloaded generics, this attribute provides
    /// a transparent way in which to transition from extension methods in the EF Core assembly to
    /// instance methods on <see cref="NpgsqlRange{T}"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class NpgsqlRangeOperatorAttribute : Attribute
    {
        /// <summary>
        /// The operator represented by the method.
        /// </summary>
        public NpgsqlRangeOperatorType OperatorType { get; }

        /// <summary>
        /// Indicates that a method can be translated to a PostgreSQL range operator.
        /// </summary>
        /// <param name="operatorType">
        /// The type of operator the method represents.
        /// </param>
        public NpgsqlRangeOperatorAttribute(NpgsqlRangeOperatorType operatorType)
        {
            OperatorType = operatorType;
        }
    }

    /// <summary>
    /// Describes the operator type of a range expression.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    [PublicAPI]
    public enum NpgsqlRangeOperatorType
    {
        /// <summary>
        /// No operator specified.
        /// </summary>
        [NpgsqlOperator] None,

        /// <summary>
        /// The = operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "=", ReturnType = typeof(bool))] Equal,

        /// <summary>
        /// The &lt;> operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "<>", ReturnType = typeof(bool))] NotEqual,

        /// <summary>
        /// The &lt; operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "<", ReturnType = typeof(bool))] LessThan,

        /// <summary>
        /// The > operator.
        /// </summary>
        [NpgsqlOperator(Symbol = ">", ReturnType = typeof(bool))] GreaterThan,

        /// <summary>
        /// The &lt;= operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "<=", ReturnType = typeof(bool))] LessThanOrEqual,

        /// <summary>
        /// The >= operator.
        /// </summary>
        [NpgsqlOperator(Symbol = ">=", ReturnType = typeof(bool))] GreaterThanOrEqual,

        /// <summary>
        /// The @> operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "@>", ReturnType = typeof(bool))] Contains,

        /// <summary>
        /// The &lt;@ operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "<@", ReturnType = typeof(bool))] ContainedBy,

        /// <summary>
        /// The && operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "&&", ReturnType = typeof(bool))] Overlaps,

        /// <summary>
        /// The &lt;&lt; operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "<<", ReturnType = typeof(bool))] IsStrictlyLeftOf,

        /// <summary>
        /// The >> operator.
        /// </summary>
        [NpgsqlOperator(Symbol = ">>", ReturnType = typeof(bool))] IsStrictlyRightOf,

        /// <summary>
        /// The &amp;&lt; operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "&<", ReturnType = typeof(bool))] DoesNotExtendRightOf,

        /// <summary>
        /// The &amp;&gt; operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "&>", ReturnType = typeof(bool))] DoesNotExtendLeftOf,

        /// <summary>
        /// The -|- operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "-|-", ReturnType = typeof(bool))] IsAdjacentTo,

        /// <summary>
        /// The + operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "+", ReturnType = typeof(NpgsqlRange<>))] Union,

        /// <summary>
        /// The * operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "*", ReturnType = typeof(NpgsqlRange<>))] Intersection,

        /// <summary>
        /// The - operator.
        /// </summary>
        [NpgsqlOperator(Symbol = "-", ReturnType = typeof(NpgsqlRange<>))] Difference
    }
}
