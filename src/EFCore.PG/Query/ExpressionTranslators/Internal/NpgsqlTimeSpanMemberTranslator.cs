using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlTimeSpanMemberTranslator : IMemberTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlTimeSpanMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    private static readonly bool[] FalseTrueArray = [false, true];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        Check.NotNull(member, nameof(member));
        Check.NotNull(returnType, nameof(returnType));

        return member.DeclaringType == typeof(TimeSpan) && instance is not null
            ? TranslateIntervalMember(_sqlExpressionFactory, instance, member.Name)
            : null;
    }

    /// <summary>
    ///     Translates a member of a type representing an exact amount of time which is mapped to PostgreSQL interval,
    ///     i.e. <see cref="TimeSpan" /> and NodaTime's Duration, which have the same component (e.g. <c>Hours</c>)
    ///     and total (e.g. <c>TotalHours</c>) members, with the same semantics.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static SqlExpression? TranslateIntervalMember(
        ISqlExpressionFactory sqlExpressionFactory,
        SqlExpression interval,
        string memberName)
    {
        // A TimeSpan can also be mapped to PostgreSQL time (e.g. DateTime.TimeOfDay), which is a time of day from 00:00 to 24:00,
        // so its date_part() values are the components already (except for 24:00, i.e. one day), and justify_hours() doesn't accept it.
        var isTime = interval.TypeMapping is INpgsqlTypeMapping { NpgsqlDbType: NpgsqlDbType.Time };

        return memberName switch
        {
            // The components are truncated towards zero, and all have the value's sign.
            // An interval's day and time fields are kept separate (e.g. '1 day -02:00:00'), and the time field can exceed a day.
            // justify_hours() moves whole days into the day field and gives both fields the same sign,
            // so that each date_part() returns the corresponding component
            nameof(TimeSpan.Days) when isTime => sqlExpressionFactory.Divide(
                ToInt(DatePart("hour", interval)), sqlExpressionFactory.Constant(24)),
            nameof(TimeSpan.Hours) when isTime => sqlExpressionFactory.Modulo(
                ToInt(DatePart("hour", interval)), sqlExpressionFactory.Constant(24)),
            nameof(TimeSpan.Days) => ToInt(DatePart("day", JustifyHours())),
            nameof(TimeSpan.Hours) => ToInt(DatePart("hour", JustifyHours())),
            nameof(TimeSpan.Minutes) => ToInt(DatePart("minute", JustifyHours())),
            // date_part('second') includes the fractional seconds
            nameof(TimeSpan.Seconds) => ToInt(Truncate(DatePart("second", JustifyHours()))),
            // date_part('millisecond') includes the seconds and PostgreSQL's integer % truncates
            nameof(TimeSpan.Milliseconds) => sqlExpressionFactory.Modulo(
                ToInt(Truncate(DatePart("millisecond", JustifyHours()))),
                sqlExpressionFactory.Constant(1000)),

            nameof(TimeSpan.TotalDays) => TotalMember(86400),
            nameof(TimeSpan.TotalHours) => TotalMember(3600),
            nameof(TimeSpan.TotalMinutes) => TotalMember(60),
            nameof(TimeSpan.TotalSeconds) => DatePart("epoch", interval),
            nameof(TimeSpan.TotalMilliseconds) => TotalMember(0.001),

            _ => null
        };

        SqlExpression JustifyHours()
            => isTime
                ? interval
                : sqlExpressionFactory.Function(
                    "justify_hours",
                    [interval],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    interval.Type,
                    interval.TypeMapping);

        SqlExpression DatePart(string part, SqlExpression value)
            => sqlExpressionFactory.Function(
                "date_part",
                [sqlExpressionFactory.Constant(part), value],
                nullable: true,
                argumentsPropagateNullability: FalseTrueArray,
                typeof(double));

        SqlExpression Truncate(SqlExpression value)
            => sqlExpressionFactory.Function(
                "trunc",
                [value],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(double));

        SqlExpression ToInt(SqlExpression value)
            => sqlExpressionFactory.Convert(value, typeof(int));

        SqlExpression TotalMember(double divisor)
            => sqlExpressionFactory.Divide(DatePart("epoch", interval), sqlExpressionFactory.Constant(divisor));
    }
}
