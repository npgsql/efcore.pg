using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Represents information about a PostgreSQL operator.
    /// </summary>
    /// <remarks>
    /// This attribute should be applied to the members of an enum to record metadata for database translation.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class NpgsqlOperatorAttribute : Attribute
    {
        /// <summary>
        /// The operator symbol.
        /// </summary>
        [NotNull]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// The operator represented by the method.
        /// </summary>
        [NotNull]
        public Type ReturnType { get; set; } = typeof(void);

        /// <summary>
        /// Creates a <see cref="CustomBinaryExpression"/> representing the operator.
        /// </summary>
        /// <param name="left">
        /// The left-hand expression.
        /// </param>
        /// <param name="right">
        /// The right-hand expression.
        /// </param>
        /// <returns>
        /// A <see cref="CustomBinaryExpression"/> representing the operator.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        [NotNull]
        public CustomBinaryExpression Create([NotNull] Expression left, [NotNull] Expression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            // TODO: will the left type arguments always be valid?
            Type type = ReturnType.IsGenericType ? ReturnType.MakeGenericType(left.Type.GetGenericArguments()) : ReturnType;

            return new CustomBinaryExpression(left, right, Symbol, type);
        }
    }
}
