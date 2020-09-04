using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using NodaTime;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal
{
    public class NpgsqlNodaTimeEvaluatableExpressionFilterPlugin : IEvaluatableExpressionFilterPlugin
    {
        static readonly MethodInfo GetCurrentInstantMethod =
            typeof(SystemClock).GetRuntimeMethod(nameof(SystemClock.GetCurrentInstant), Array.Empty<Type>());

        static readonly MemberInfo SystemClockInstanceMember =
            typeof(SystemClock).GetMember(nameof(SystemClock.Instance)).FirstOrDefault();

        public bool IsEvaluatableExpression(Expression expression)
            => !(
                expression is MethodCallExpression methodExpression
                && methodExpression.Method == GetCurrentInstantMethod
                ||
                expression is MemberExpression memberExpression
                && memberExpression.Member == SystemClockInstanceMember
            );
    }
}
