using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlIntervalTypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static NpgsqlIntervalTypeMapping Default { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlIntervalTypeMapping()
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(TimeSpan), jsonValueReaderWriter: NpgsqlJsonTimeSpanReaderWriter.Instance),
                "interval"))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlIntervalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Interval)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlIntervalTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"interval({parameters.Precision})";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"INTERVAL '{FormatTimeSpanAsInterval((TimeSpan)value)}'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => $"""
            "{FormatTimeSpanAsInterval((TimeSpan)value)}"
            """;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string FormatTimeSpanAsInterval(TimeSpan ts)
    {
        // PostgreSQL intervals have microsecond precision, but TimeSpan has 100ns tick precision. Truncate the sub-microsecond part,
        // otherwise a value with only sub-microsecond ticks gets formatted with a trailing decimal separator (e.g. 00:23:00.).
        ts = new TimeSpan(ts.Ticks - ts.Ticks % TimeSpan.TicksPerMicrosecond);

        // TimeSpan's custom format specifiers output absolute values, so the sign is added explicitly.
        // PostgreSQL applies a sign to the field it precedes only ('-1 02:00:00' is -1 day +2 hours, i.e. -22 hours),
        // so a negative value with a days component needs a sign on both the days and the time ('-1 -02:00:00').
        var sign = ts < TimeSpan.Zero ? "-" : "";
        var time = ts.ToString(
            $@"hh\:mm\:ss{(ts.Ticks % 10000000 == 0 ? "" : "\\.FFFFFF")}",
            CultureInfo.InvariantCulture);

        return ts.Days == 0
            ? sign + time
            : FormattableString.Invariant($"{sign}{Math.Abs(ts.Days)} {sign}{time}");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TimeSpan ParseIntervalAsTimeSpan(ReadOnlySpan<char> s)
    {
        // We store the a PG-compatible interval representation in JSON so it can be queried out, but unfortunately this isn't
        // compatible with TimeSpan, so we have to parse manually.

        // A leading sign applies to the days if there are any, otherwise to the time
        var isNegated = false;
        if (s[0] == '-')
        {
            isNegated = true;
            s = s[1..];
        }

        int i;
        for (i = 0; i < s.Length; i++)
        {
            if (!char.IsDigit(s[i]))
            {
                break;
            }
        }

        if (i == s.Length)
        {
            throw new FormatException();
        }

        // Space is the separator between the days and the hours:minutes:seconds components
        var days = 0;
        var isTimeNegated = isNegated;
        if (s[i] == ' ')
        {
            days = int.Parse(s[..i]);
            s = s[(i + 1)..];

            // PostgreSQL applies a sign to the field it precedes only
            // i.e. negative values are written with a sign on both the days and the time (-1 -02:00:00 for -26 hours).
            // Older versions of the provider wrote a sign on the days only (-1 02:00:00),
            // which to PostgreSQL is -1 day +2 hours, i.e. -22 hours.
            // The Npgsql.ParseOldEFCoreJsonTimeSpans AppContext switch (on by default) determines which meaning such a value is given.
            if (s.Length > 0 && s[0] == '-')
            {
                isTimeNegated = true;
                s = s[1..];
            }
            else
            {
                isTimeNegated = isNegated && NpgsqlTypeMappingSource.ParseOldEFCoreJsonTimeSpans;
            }
        }

        var time = TimeSpan.Parse(s, CultureInfo.InvariantCulture);

        // We've already extracted the days, so there shouldn't be a days component in the rest (indicates an incompatible/malformed string)
        if (time.Days != 0 || time < TimeSpan.Zero)
        {
            throw new FormatException("Error parsing interval: invalid format");
        }

        // The days and the time can have opposite signs, so only the combined value is range-checked
        // (e.g. 10675200 -23:00:00 fits in a TimeSpan, but 10675200 days don't)
        var ticks = (Int128)(isNegated ? -days : days) * TimeSpan.TicksPerDay + (isTimeNegated ? -time.Ticks : time.Ticks);
        if (ticks < TimeSpan.MinValue.Ticks || ticks > TimeSpan.MaxValue.Ticks)
        {
            throw new OverflowException("Error parsing interval: the value is out of range for a TimeSpan");
        }

        return new TimeSpan((long)ticks);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class NpgsqlJsonTimeSpanReaderWriter : JsonValueReaderWriter<TimeSpan>
    {
        private static readonly PropertyInfo InstanceProperty = typeof(NpgsqlJsonTimeSpanReaderWriter).GetProperty(nameof(Instance))!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static NpgsqlJsonTimeSpanReaderWriter Instance { get; } = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override TimeSpan FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => ParseIntervalAsTimeSpan(manager.CurrentReader.GetString()!);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void ToJsonTyped(Utf8JsonWriter writer, TimeSpan value)
            => writer.WriteStringValue(FormatTimeSpanAsInterval(value));

        /// <inheritdoc />
        public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
    }
}
