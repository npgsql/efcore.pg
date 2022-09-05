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
        => _sqlExpressionFactory = sqlExpressionFactory;

    private static readonly bool[] FalseTrueArray = { false, true };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        Check.NotNull(member, nameof(member));
        Check.NotNull(returnType, nameof(returnType));

        if (member.DeclaringType == typeof(TimeSpan) && instance is not null)
        {
            return member.Name switch
            {
                nameof(TimeSpan.Days) => Floor(DatePart("day", instance)),
                nameof(TimeSpan.Hours) => Floor(DatePart("hour", instance)),
                nameof(TimeSpan.Minutes) => Floor(DatePart("minute", instance)),
                nameof(TimeSpan.Seconds) => Floor(DatePart("second", instance)),
                nameof(TimeSpan.Milliseconds) => _sqlExpressionFactory.Modulo(
                    Floor(DatePart("millisecond", instance)),
                    _sqlExpressionFactory.Constant(1000)),

                nameof(TimeSpan.TotalDays) => TranslateDurationTotalMember(instance, 86400),
                nameof(TimeSpan.TotalHours) => TranslateDurationTotalMember(instance, 3600),
                nameof(TimeSpan.TotalMinutes) => TranslateDurationTotalMember(instance, 60),
                nameof(TimeSpan.TotalSeconds) => DatePart("epoch", instance),
                nameof(TimeSpan.TotalMilliseconds) => TranslateDurationTotalMember(instance, 0.001),

                _ => null
            };
        }

        return null;

        SqlExpression Floor(SqlExpression value)
            => _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.Function(
                    "floor",
                    new[] { value },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(double)),
                typeof(int));

        SqlFunctionExpression DatePart(string part, SqlExpression value)
            => _sqlExpressionFactory.Function("date_part", new[]
                {
                    _sqlExpressionFactory.Constant(part),
                    value
                },
                nullable: true,
                argumentsPropagateNullability: FalseTrueArray,
                returnType);

        SqlBinaryExpression TranslateDurationTotalMember(SqlExpression instance, double divisor)
            => _sqlExpressionFactory.Divide(DatePart("epoch", instance), _sqlExpressionFactory.Constant(divisor));
    }
}
