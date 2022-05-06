namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;

public class NpgsqlNodaTimeEvaluatableExpressionFilterPlugin : IEvaluatableExpressionFilterPlugin
{
    private static readonly MethodInfo GetCurrentInstantMethod =
        typeof(SystemClock).GetRuntimeMethod(nameof(SystemClock.GetCurrentInstant), Array.Empty<Type>())!;

    private static readonly MemberInfo SystemClockInstanceMember =
        typeof(SystemClock).GetMember(nameof(SystemClock.Instance)).FirstOrDefault()!;

    public virtual bool IsEvaluatableExpression(Expression expression)
    {
        switch (expression)
        {
            case MethodCallExpression methodCallExpression when methodCallExpression.Method == GetCurrentInstantMethod:
                return false;

            case MemberExpression memberExpression:
                if (memberExpression.Member == SystemClockInstanceMember)
                {
                    return false;
                }

                // We support translating certain NodaTime patterns which accept a time zone as a parameter,
                // e.g. Instant.InZone(timezone), as long as the timezone is expressed as an access on DateTimeZoneProviders.Tzdb.
                // Prevent this from being evaluated locally and so parameterized, so we can access the member access on
                // DateTimeZoneProviders and extract the constant (see NpgsqlNodaTimeMethodCallTranslatorPlugin)
                if (memberExpression.Member.DeclaringType == typeof(DateTimeZoneProviders))
                {
                    return false;
                }

                break;
        }

        return true;
    }
}