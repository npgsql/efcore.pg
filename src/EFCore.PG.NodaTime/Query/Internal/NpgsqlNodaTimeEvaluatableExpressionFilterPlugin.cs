namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;

public class NpgsqlNodaTimeEvaluatableExpressionFilterPlugin : IEvaluatableExpressionFilterPlugin
{
    private static readonly MethodInfo GetCurrentInstantMethod =
        typeof(SystemClock).GetRuntimeMethod(nameof(SystemClock.GetCurrentInstant), Array.Empty<Type>())!;

    private static readonly MemberInfo SystemClockInstanceMember =
        typeof(SystemClock).GetMember(nameof(SystemClock.Instance)).FirstOrDefault()!;

    public virtual bool IsEvaluatableExpression(Expression expression)
        => !(
            expression is MethodCallExpression methodExpression
            && methodExpression.Method == GetCurrentInstantMethod
            ||
            expression is MemberExpression memberExpression
            && memberExpression.Member == SystemClockInstanceMember
        );
}