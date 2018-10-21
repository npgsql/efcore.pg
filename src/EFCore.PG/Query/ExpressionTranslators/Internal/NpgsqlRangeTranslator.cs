using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL range operators.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    public class NpgsqlRangeTranslator : IMethodCallTranslator, IMemberTranslator
    {
        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression e)
        {
            if (e.Method.DeclaringType != typeof(NpgsqlRangeExtensions))
                return null;

            switch (e.Method.Name)
            {
            case nameof(NpgsqlRangeExtensions.Contains):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "@>", typeof(bool));

            case nameof(NpgsqlRangeExtensions.ContainedBy):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "<@", typeof(bool));

            case nameof(NpgsqlRangeExtensions.Overlaps):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&&", typeof(bool));

            case nameof(NpgsqlRangeExtensions.IsStrictlyLeftOf):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "<<", typeof(bool));

            case nameof(NpgsqlRangeExtensions.IsStrictlyRightOf):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], ">>", typeof(bool));

            case nameof(NpgsqlRangeExtensions.DoesNotExtendRightOf):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&<", typeof(bool));

            case nameof(NpgsqlRangeExtensions.DoesNotExtendLeftOf):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&>", typeof(bool));

            case nameof(NpgsqlRangeExtensions.IsAdjacentTo):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "-|-", typeof(bool));

            case nameof(NpgsqlRangeExtensions.Union):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "+", e.Arguments[0].Type);

            case nameof(NpgsqlRangeExtensions.Intersect):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "*", e.Arguments[0].Type);

            case nameof(NpgsqlRangeExtensions.Except):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "-", e.Arguments[0].Type);

            case nameof(NpgsqlRangeExtensions.Merge):
                return new SqlFunctionExpression("range_merge", e.Type, new[] { e.Arguments[0], e.Arguments[1] });

            default:
                return null;
            }
        }

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MemberExpression e)
        {
            var type = e.Member.DeclaringType;
            if (type == null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(NpgsqlRange<>))
                return null;

            switch (e.Member.Name)
            {
            case nameof(NpgsqlRange<int>.LowerBound):
            {
                var lower = new SqlFunctionExpression("lower", e.Type, new[] { e.Expression });

                return e.Type.IsNullableType()
                    ? lower
                    : new SqlFunctionExpression("COALESCE", e.Type, new Expression[] { lower, Expression.Default(e.Type) });
            }

            case nameof(NpgsqlRange<int>.UpperBound):
            {
                var upper = new SqlFunctionExpression("upper", e.Type, new[] { e.Expression });

                return e.Type.IsNullableType()
                    ? upper
                    : new SqlFunctionExpression("COALESCE", e.Type, new Expression[] { upper, Expression.Default(e.Type) });
            }

            case nameof(NpgsqlRange<int>.IsEmpty):
                return new SqlFunctionExpression("isempty", e.Type, new[] { e.Expression });

            case nameof(NpgsqlRange<int>.LowerBoundIsInclusive):
                return new SqlFunctionExpression("lower_inc", e.Type, new[] { e.Expression });

            case nameof(NpgsqlRange<int>.UpperBoundIsInclusive):
                return new SqlFunctionExpression("upper_inc", e.Type, new[] { e.Expression });

            case nameof(NpgsqlRange<int>.LowerBoundInfinite):
                return new SqlFunctionExpression("lower_inf", e.Type, new[] { e.Expression });

            case nameof(NpgsqlRange<int>.UpperBoundInfinite):
                return new SqlFunctionExpression("upper_inf", e.Type, new[] { e.Expression });

            default:
                return null;
            }
        }
    }
}
