using System.Collections.Concurrent;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

public class NpgsqlNodaTimeTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
#if DEBUG
    internal static bool LegacyTimestampBehavior;
    internal static bool DisableDateTimeInfinityConversions;
#else
        internal static readonly bool LegacyTimestampBehavior;
        internal static readonly bool DisableDateTimeInfinityConversions;
#endif

    static NpgsqlNodaTimeTypeMappingSourcePlugin()
    {
        LegacyTimestampBehavior = AppContext.TryGetSwitch("Npgsql.EnableLegacyTimestampBehavior", out var enabled) && enabled;
        DisableDateTimeInfinityConversions = AppContext.TryGetSwitch("Npgsql.DisableDateTimeInfinityConversions", out enabled) && enabled;
    }

    public virtual ConcurrentDictionary<string, RelationalTypeMapping[]> StoreTypeMappings { get; }
    public virtual ConcurrentDictionary<Type, RelationalTypeMapping> ClrTypeMappings { get; }

    #region TypeMapping

    private readonly TimestampLocalDateTimeMapping _timestampLocalDateTime = new();
    private readonly LegacyTimestampInstantMapping _legacyTimestampInstant = new();

    private readonly TimestampTzInstantMapping _timestamptzInstant = new();
    private readonly TimestampTzZonedDateTimeMapping _timestamptzZonedDateTime = new();
    private readonly TimestampTzOffsetDateTimeMapping _timestamptzOffsetDateTime = new();

    private readonly DateMapping _date = new();
    private readonly TimeMapping _time = new();
    private readonly TimeTzMapping _timetz = new();
    private readonly PeriodIntervalMapping _periodInterval = new();
    private readonly DurationIntervalMapping _durationInterval = new();

    private readonly NpgsqlRangeTypeMapping _timestampLocalDateTimeRange;
    private readonly NpgsqlRangeTypeMapping _legacyTimestampInstantRange;
    private readonly NpgsqlRangeTypeMapping _timestamptzInstantRange;
    private readonly NpgsqlRangeTypeMapping _timestamptzZonedDateTimeRange;
    private readonly NpgsqlRangeTypeMapping _timestamptzOffsetDateTimeRange;
    private readonly NpgsqlRangeTypeMapping _dateRange;

    private readonly DateIntervalMapping _dateInterval = new();
    private readonly IntervalMapping _interval = new();

    #endregion

    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlNodaTimeTypeMappingSourcePlugin"/> class.
    /// </summary>
    public NpgsqlNodaTimeTypeMappingSourcePlugin(ISqlGenerationHelper sqlGenerationHelper)
    {
        _legacyTimestampInstantRange
            = new NpgsqlRangeTypeMapping("tsrange", typeof(NpgsqlRange<Instant>), _legacyTimestampInstant, sqlGenerationHelper);
        _timestampLocalDateTimeRange
            = new NpgsqlRangeTypeMapping("tsrange", typeof(NpgsqlRange<LocalDateTime>), _timestampLocalDateTime, sqlGenerationHelper);
        _timestamptzInstantRange
            = new NpgsqlRangeTypeMapping("tstzrange", typeof(NpgsqlRange<Instant>), _timestamptzInstant, sqlGenerationHelper);
        _timestamptzZonedDateTimeRange
            = new NpgsqlRangeTypeMapping("tstzrange", typeof(NpgsqlRange<ZonedDateTime>), _timestamptzZonedDateTime, sqlGenerationHelper);
        _timestamptzOffsetDateTimeRange
            = new NpgsqlRangeTypeMapping("tstzrange", typeof(NpgsqlRange<OffsetDateTime>), _timestamptzOffsetDateTime, sqlGenerationHelper);
        _dateRange
            = new NpgsqlRangeTypeMapping("daterange", typeof(NpgsqlRange<LocalDate>), _date, sqlGenerationHelper);

        var storeTypeMappings = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
        {
            {
                // We currently allow _legacyTimestampInstant even in non-legacy mode, since when upgrading to 6.0 with existing
                // migrations, model snapshots still contain old mappings (Instant mapped to timestamp), and EF Core's model differ
                // expects type mappings to be found for these. See https://github.com/dotnet/efcore/issues/26168.
                "timestamp without time zone", LegacyTimestampBehavior
                    ? new RelationalTypeMapping[] { _legacyTimestampInstant, _timestampLocalDateTime }
                    : new RelationalTypeMapping[] { _timestampLocalDateTime, _legacyTimestampInstant }
            },
            { "timestamp with time zone", new RelationalTypeMapping[] { _timestamptzInstant, _timestamptzZonedDateTime, _timestamptzOffsetDateTime } },
            { "date", new RelationalTypeMapping[] { _date } },
            { "time without time zone", new RelationalTypeMapping[] { _time } },
            { "time with time zone", new RelationalTypeMapping[] { _timetz } },
            { "interval", new RelationalTypeMapping[] { _periodInterval } },

            { "tsrange", LegacyTimestampBehavior
                ? new RelationalTypeMapping[] { _legacyTimestampInstantRange, _timestampLocalDateTimeRange }
                : new RelationalTypeMapping[] { _timestampLocalDateTimeRange, _legacyTimestampInstantRange }
            },
            { "tstzrange", new RelationalTypeMapping[] { _interval, _timestamptzInstantRange, _timestamptzZonedDateTimeRange, _timestamptzOffsetDateTimeRange } },
            { "daterange", new RelationalTypeMapping[] { _dateInterval, _dateRange } }
        };

        // Set up aliases
        storeTypeMappings["timestamp"] = storeTypeMappings["timestamp without time zone"];
        storeTypeMappings["timestamptz"] = storeTypeMappings["timestamp with time zone"];
        storeTypeMappings["time"] = storeTypeMappings["time without time zone"];
        storeTypeMappings["timetz"] = storeTypeMappings["time with time zone"];

        var clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
        {
            { typeof(Instant), LegacyTimestampBehavior ? _legacyTimestampInstant : _timestamptzInstant },

            { typeof(LocalDateTime), _timestampLocalDateTime },
            { typeof(ZonedDateTime), _timestamptzZonedDateTime },
            { typeof(OffsetDateTime), _timestamptzOffsetDateTime },
            { typeof(LocalDate), _date },
            { typeof(LocalTime), _time },
            { typeof(OffsetTime), _timetz },
            { typeof(Period), _periodInterval },
            { typeof(Duration), _durationInterval },

            { typeof(NpgsqlRange<Instant>), LegacyTimestampBehavior ? _legacyTimestampInstantRange : _timestamptzInstantRange },
            { typeof(NpgsqlRange<LocalDateTime>), _timestampLocalDateTimeRange },
            { typeof(NpgsqlRange<ZonedDateTime>), _timestamptzZonedDateTimeRange },
            { typeof(NpgsqlRange<OffsetDateTime>), _timestamptzOffsetDateTimeRange },
            { typeof(NpgsqlRange<LocalDate>), _dateRange },
            { typeof(DateInterval), _dateInterval },
            { typeof(Interval), _interval }
        };

        StoreTypeMappings = new ConcurrentDictionary<string, RelationalTypeMapping[]>(storeTypeMappings, StringComparer.OrdinalIgnoreCase);
        ClrTypeMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>(clrTypeMappings);
    }

    public virtual RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        => FindExistingMapping(mappingInfo) ?? FindArrayMapping(mappingInfo);

    protected virtual RelationalTypeMapping? FindExistingMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;
        var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

        if (storeTypeName is not null)
        {
            if (StoreTypeMappings.TryGetValue(storeTypeName, out var mappings))
            {
                if (clrType is null)
                {
                    return mappings[0];
                }

                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m;
                    }
                }

                return null;
            }

            if (StoreTypeMappings.TryGetValue(storeTypeNameBase!, out mappings))
            {
                if (clrType is null)
                {
                    return mappings[0].Clone(in mappingInfo);
                }

                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m.Clone(in mappingInfo);
                    }
                }

                return null;
            }
        }

        if (clrType is null || !ClrTypeMappings.TryGetValue(clrType, out var mapping))
        {
            return null;
        }

        // All PostgreSQL date/time types accept a precision except for date
        // TODO: Cache size/precision/scale mappings?
        return mappingInfo.Precision.HasValue && mapping.ClrType != typeof(LocalDate)
            ? mapping.Clone($"{mapping.StoreType}({mappingInfo.Precision.Value})", null)
            : mapping;
    }

    // TODO: This is duplicated from NpgsqlTypeMappingSource
    protected virtual RelationalTypeMapping? FindArrayMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        Type? elementClrType = null;

        if (clrType is not null && !clrType.TryGetElementType(out elementClrType))
        {
            return null; // Not an array/list
        }

        var storeType = mappingInfo.StoreTypeName;
        var storeTypeNameBase = mappingInfo.StoreTypeNameBase;
        if (storeType is not null)
        {
            // PostgreSQL array type names are the element plus []
            if (!storeType.EndsWith("[]", StringComparison.Ordinal))
            {
                return null;
            }

            var elementStoreType = storeType.Substring(0, storeType.Length - 2);
            var elementStoreTypeNameBase = storeTypeNameBase!.Substring(0, storeTypeNameBase.Length - 2);

            RelationalTypeMapping? elementMapping;

            if (elementClrType is null)
            {
                elementMapping = FindMapping(new RelationalTypeMappingInfo(
                    elementStoreType, elementStoreTypeNameBase,
                    mappingInfo.IsUnicode, mappingInfo.Size, mappingInfo.Precision, mappingInfo.Scale));
            }
            else
            {
                elementMapping = FindMapping(new RelationalTypeMappingInfo(
                    elementClrType, elementStoreType, elementStoreTypeNameBase,
                    mappingInfo.IsKeyOrIndex, mappingInfo.IsUnicode, mappingInfo.Size, mappingInfo.IsRowVersion,
                    mappingInfo.IsFixedLength, mappingInfo.Precision, mappingInfo.Scale));

                // If an element mapping was found only with the help of a value converter, return null and EF will
                // construct the corresponding array mapping with a value converter.
                if (elementMapping?.Converter is not null)
                {
                    return null;
                }
            }

            // If no mapping was found for the element, there's no mapping for the array.
            // Also, arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
            if (elementMapping is null || elementMapping is NpgsqlArrayTypeMapping)
            {
                return null;
            }

            return new NpgsqlArrayArrayTypeMapping(storeType, elementMapping);
        }

        if (clrType is null)
        {
            return null;
        }

        if (clrType.IsArray)
        {
            var elementType = clrType.GetElementType();
            Debug.Assert(elementType is not null, "Detected array type but element type is null");

            // If an element isn't supported, neither is its array. If the element is only supported via value
            // conversion we also don't support it.
            var elementMapping = FindMapping(new RelationalTypeMappingInfo(elementType));
            if (elementMapping is null || elementMapping.Converter is not null)
            {
                return null;
            }

            // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
            if (elementMapping is NpgsqlArrayTypeMapping)
            {
                return null;
            }

            return new NpgsqlArrayArrayTypeMapping(clrType, elementMapping);
        }

        if (clrType.IsGenericList())
        {
            var elementType = clrType.GetGenericArguments()[0];

            // If an element isn't supported, neither is its array
            var elementMapping = FindMapping(new RelationalTypeMappingInfo(elementType));
            if (elementMapping is null)
            {
                return null;
            }

            // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
            if (elementMapping is NpgsqlArrayTypeMapping)
            {
                return null;
            }

            return new NpgsqlArrayListTypeMapping(clrType, elementMapping);
        }

        return null;
    }
}