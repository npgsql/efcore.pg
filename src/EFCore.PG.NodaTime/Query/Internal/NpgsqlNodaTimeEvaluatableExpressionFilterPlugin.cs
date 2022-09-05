namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlNodaTimeEvaluatableExpressionFilterPlugin : IEvaluatableExpressionFilterPlugin
{
    private static readonly MethodInfo GetCurrentInstantMethod =
        typeof(SystemClock).GetRuntimeMethod(nameof(SystemClock.GetCurrentInstant), Array.Empty<Type>())!;

    private static readonly MemberInfo SystemClockInstanceMember =
        typeof(SystemClock).GetMember(nameof(SystemClock.Instance)).FirstOrDefault()!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
