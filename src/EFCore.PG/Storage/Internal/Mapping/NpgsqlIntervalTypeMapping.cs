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
        => ts.ToString(
            $@"{(ts < TimeSpan.Zero ? "\\-" : "")}{(ts.Days == 0 ? "" : "d\\ ")}hh\:mm\:ss{(ts.Ticks % 10000000 == 0 ? "" : "\\.FFFFFF")}",
            CultureInfo.InvariantCulture);

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

        // Note
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
        if (s[i] == ' ')
        {
            days = int.Parse(s[..i]);
            s = s[i..];
        }

        var timeSpan = TimeSpan.Parse(s, CultureInfo.InvariantCulture);

        // We've already extracted the days, so there shouldn't be a days component in the rest (indicates an incompatible/malformed string)
        if (timeSpan.Days != 0)
        {
            throw new FormatException();
        }

        if (days > 0)
        {
            timeSpan = new TimeSpan(
                days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds, timeSpan.Microseconds);
        }

        if (isNegated)
        {
            timeSpan = -timeSpan;
        }

        return timeSpan;
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
