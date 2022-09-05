using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DurationIntervalMapping : NpgsqlTypeMapping
{
    private static readonly MethodInfo FromDays = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromDays), new[] { typeof(int) })!;
    private static readonly MethodInfo FromHours = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromHours), new[] { typeof(int) })!;
    private static readonly MethodInfo FromMinutes = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromMinutes), new[] { typeof(long) })!;
    private static readonly MethodInfo FromSeconds = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromSeconds), new[] { typeof(long) })!;
    private static readonly MethodInfo FromMilliseconds = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromMilliseconds), new[] { typeof(long) })!;

    private static readonly PropertyInfo Zero = typeof(Duration).GetProperty(nameof(Duration.Zero))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DurationIntervalMapping() : base("interval", typeof(Duration), NpgsqlDbType.Interval) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected DurationIntervalMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Interval) {}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new DurationIntervalMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override RelationalTypeMapping Clone(string storeType, int? size)
        => new DurationIntervalMapping(Parameters.WithStoreTypeAndSize(storeType, size));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override CoreTypeMapping Clone(ValueConverter? converter)
        => new DurationIntervalMapping(Parameters.WithComposedConverter(converter));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"INTERVAL '{NpgsqlIntervalTypeMapping.FormatTimeSpanAsInterval(((Duration)value).ToTimeSpan())}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        var duration = (Duration)value;
        Expression? e = null;

        if (duration.Days != 0)
        {
            Compose(Expression.Call(FromDays, Expression.Constant(duration.Days)));
        }

        if (duration.Hours != 0)
        {
            Compose(Expression.Call(FromHours, Expression.Constant(duration.Hours)));
        }

        if (duration.Minutes != 0)
        {
            Compose(Expression.Call(FromMinutes, Expression.Constant((long)duration.Minutes)));
        }

        if (duration.Seconds != 0)
        {
            Compose(Expression.Call(FromSeconds, Expression.Constant((long)duration.Seconds)));
        }

        if (duration.Milliseconds != 0)
        {
            Compose(Expression.Call(FromMilliseconds, Expression.Constant((long)duration.Milliseconds)));
        }

        return e ?? Expression.MakeMemberAccess(null, Zero);

        void Compose(Expression toAdd) => e = e is null ? toAdd : Expression.Add(e, toAdd);
    }
}
