using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL NULLIF functions.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/functions-conditional.html#FUNCTIONS-NULLIF
    /// </remarks>
    public class NpgsqlNullIfTranslator : IMethodCallTranslator, IExpressionFragmentTranslator
    {
        [NotNull] static readonly MethodInfo NullIfClass =
            typeof(NpgsqlDbFunctionsExtensions)
                .GetMethods()
                .Single(x => x.Name == nameof(NpgsqlDbFunctionsExtensions.NullIf) && x.GetGenericArguments()[0].IsClass);

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression e)
            => e.Method.DeclaringType == typeof(NpgsqlDbFunctionsExtensions) &&
               e.Method.Name == nameof(NpgsqlDbFunctionsExtensions.NullIf)
                ? new SqlFunctionExpression("NULLIF", e.Arguments[1].Type, e.Arguments.Skip(1))
                : null;

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(Expression e)
        {
            // For classes, rewrite `a == b ? null : a` into
            // the equivalent `EF.Functions.NullIf(a, b)`.
            // Value types are complicated due to nullable and
            // conversion wrappers, so skipping for now.

            if (!(e is ConditionalExpression conditional))
                return null;

            if (!(conditional.IfTrue is ConstantExpression constant))
                return null;

            if (constant.Value != null)
                return null;

            if (!(conditional.Test is BinaryExpression b))
                return null;

            if (b.NodeType != ExpressionType.Equal)
                return null;

            if (!ExpressionEqualityComparer.Instance.Equals(conditional.IfFalse, b.Left))
                return null;

            if (conditional.IfFalse.Type.IsClass)
            {
                return Expression.Call(
                    NullIfClass.MakeGenericMethod(conditional.IfFalse.Type),
                    Expression.Constant(EF.Functions),
                    b.Left,
                    b.Right);
            }

            return null;
        }
    }
}
