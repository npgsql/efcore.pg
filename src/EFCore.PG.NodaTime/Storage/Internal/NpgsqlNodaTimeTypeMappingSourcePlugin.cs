using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlNodaTimeTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
    {
        public ConcurrentDictionary<string, RelationalTypeMapping[]> StoreTypeMappings { get; }
        public ConcurrentDictionary<Type, RelationalTypeMapping> ClrTypeMappings { get; }

        #region TypeMapping

        [NotNull] readonly TimestampInstantMapping _timestampInstant = new TimestampInstantMapping();
        [NotNull] readonly TimestampLocalDateTimeMapping _timestampLocalDateTime = new TimestampLocalDateTimeMapping();

        [NotNull] readonly TimestampTzInstantMapping _timestamptzInstant = new TimestampTzInstantMapping();
        [NotNull] readonly TimestampTzZonedDateTimeMapping _timestamptzZonedDateTime = new TimestampTzZonedDateTimeMapping();
        [NotNull] readonly TimestampTzOffsetDateTimeMapping _timestamptzOffsetDateTime = new TimestampTzOffsetDateTimeMapping();

        [NotNull] readonly DateMapping _date = new DateMapping();
        [NotNull] readonly TimeMapping _time = new TimeMapping();
        [NotNull] readonly TimeTzMapping _timetz = new TimeTzMapping();
        [NotNull] readonly IntervalMapping _period = new IntervalMapping();

        [NotNull] readonly NpgsqlRangeTypeMapping _timestampLocalDateTimeRange;
        [NotNull] readonly NpgsqlRangeTypeMapping _timestampInstantRange;
        [NotNull] readonly NpgsqlRangeTypeMapping _timestamptzInstantRange;
        [NotNull] readonly NpgsqlRangeTypeMapping _timestamptzZonedDateTimeRange;
        [NotNull] readonly NpgsqlRangeTypeMapping _timestamptzOffsetDateTimeRange;
        [NotNull] readonly NpgsqlRangeTypeMapping _dateRange;

        #endregion

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlNodaTimeTypeMappingSourcePlugin"/> class.
        /// </summary>
        public NpgsqlNodaTimeTypeMappingSourcePlugin()
        {
            _timestampLocalDateTimeRange = new NpgsqlRangeTypeMapping("tsrange", typeof(NpgsqlRange<LocalDateTime>), _timestampLocalDateTime);
            _timestampInstantRange = new NpgsqlRangeTypeMapping("tsrange", typeof(NpgsqlRange<Instant>), _timestampInstant);
            _timestamptzInstantRange = new NpgsqlRangeTypeMapping("tstzrange", typeof(NpgsqlRange<Instant>), _timestamptzInstant);
            _timestamptzZonedDateTimeRange = new NpgsqlRangeTypeMapping("tstzrange", typeof(NpgsqlRange<ZonedDateTime>), _timestamptzZonedDateTime);
            _timestamptzOffsetDateTimeRange = new NpgsqlRangeTypeMapping("tstzrange", typeof(NpgsqlRange<OffsetDateTime>), _timestamptzOffsetDateTime);
            _dateRange = new NpgsqlRangeTypeMapping("daterange", typeof(NpgsqlRange<LocalDate>), _date);

            var storeTypeMappings = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "timestamp", new RelationalTypeMapping[] { _timestampInstant, _timestampLocalDateTime } },
                { "timestamp without time zone", new RelationalTypeMapping[] { _timestampInstant, _timestampLocalDateTime } },
                { "timestamptz", new RelationalTypeMapping[] { _timestamptzInstant, _timestamptzZonedDateTime, _timestamptzOffsetDateTime } },
                { "timestamp with time zone", new RelationalTypeMapping[] { _timestamptzInstant, _timestamptzZonedDateTime, _timestamptzOffsetDateTime } },
                { "date", new RelationalTypeMapping[] { _date } },
                { "time", new RelationalTypeMapping[] { _time } },
                { "time without time zone", new RelationalTypeMapping[] { _time } },
                { "timetz", new RelationalTypeMapping[] { _timetz } },
                { "time with time zone", new RelationalTypeMapping[] { _timetz } },
                { "interval", new RelationalTypeMapping[] { _period } },

                { "tsrange", new RelationalTypeMapping[] { _timestampInstantRange, _timestampLocalDateTimeRange } },
                { "tstzrange", new RelationalTypeMapping[] { _timestamptzInstantRange, _timestamptzZonedDateTimeRange, _timestamptzOffsetDateTimeRange } },
                { "daterange", new RelationalTypeMapping[] { _dateRange} }
            };

            var clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(Instant), _timestampInstant },
                { typeof(LocalDateTime), _timestampLocalDateTime },
                { typeof(ZonedDateTime), _timestamptzZonedDateTime },
                { typeof(OffsetDateTime), _timestamptzOffsetDateTime },
                { typeof(LocalDate), _date },
                { typeof(LocalTime), _time },
                { typeof(OffsetTime), _timetz },
                { typeof(Period), _period },

                { typeof(NpgsqlRange<Instant>), _timestampInstantRange },
                { typeof(NpgsqlRange<LocalDateTime>), _timestampLocalDateTimeRange },
                { typeof(NpgsqlRange<ZonedDateTime>), _timestamptzZonedDateTimeRange },
                { typeof(NpgsqlRange<LocalDate>), _dateRange },
                { typeof(NpgsqlRange<OffsetDateTime>), _timestamptzOffsetDateTimeRange }
            };

            StoreTypeMappings = new ConcurrentDictionary<string, RelationalTypeMapping[]>(storeTypeMappings, StringComparer.OrdinalIgnoreCase);
            ClrTypeMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>(clrTypeMappings);
        }

        public RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => FindExistingMapping(mappingInfo) ?? FindArrayMapping(mappingInfo);

        protected virtual RelationalTypeMapping FindExistingMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                if (StoreTypeMappings.TryGetValue(storeTypeName, out var mappings))
                {
                    if (clrType == null)
                        return mappings[0];

                    foreach (var m in mappings)
                        if (m.ClrType == clrType)
                            return m;

                    return null;
                }

                if (StoreTypeMappings.TryGetValue(storeTypeNameBase, out mappings))
                {
                    if (clrType == null)
                        return mappings[0].Clone(in mappingInfo);

                    foreach (var m in mappings)
                        if (m.ClrType == clrType)
                            return m.Clone(in mappingInfo);

                    return null;
                }
            }

            if (clrType == null || !ClrTypeMappings.TryGetValue(clrType, out var mapping))
                return null;

            // All PostgreSQL date/time types accept a precision except for date
            // TODO: Cache size/precision/scale mappings?
            return mappingInfo.Precision.HasValue && mapping.ClrType != typeof(LocalDate)
                ? mapping.Clone($"{mapping.StoreType}({mappingInfo.Precision.Value})", null)
                : mapping;
        }

        RelationalTypeMapping FindArrayMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            // PostgreSQL array type names are the element plus []
            var storeType = mappingInfo.StoreTypeName;
            if (storeType != null)
            {
                if (!storeType.EndsWith("[]"))
                    return null;

                // Note that we scaffold PostgreSQL arrays to C# arrays, not lists (which are also supported)

                // TODO: In theory support the multiple mappings just like we do with scalars above
                // (e.g. DateTimeOffset[] vs. DateTime[]
                var elementMapping = FindExistingMapping(new RelationalTypeMappingInfo(storeType.Substring(0, storeType.Length - 2)));
                if (elementMapping != null)
                    return StoreTypeMappings.GetOrAdd(storeType,
                        new RelationalTypeMapping[] { new NpgsqlArrayTypeMapping(storeType, elementMapping) })[0];
            }

            var clrType = mappingInfo.ClrType;
            if (clrType == null)
                return null;

            if (clrType.IsArray)
            {
                var elementType = clrType.GetElementType();
                Debug.Assert(elementType != null, "Detected array type but element type is null");

                // If an element isn't supported, neither is its array
                var elementMapping = FindExistingMapping(new RelationalTypeMappingInfo(elementType));
                if (elementMapping == null)
                    return null;

                // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
                if (elementMapping is NpgsqlArrayTypeMapping)
                    return null;

                return ClrTypeMappings.GetOrAdd(clrType, new NpgsqlArrayTypeMapping(elementMapping, clrType));
            }

            if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = clrType.GetGenericArguments()[0];

                // If an element isn't supported, neither is its array
                var elementMapping = FindExistingMapping(new RelationalTypeMappingInfo(elementType));
                if (elementMapping == null)
                    return null;

                // Arrays of arrays aren't supported (as opposed to multidimensional arrays) by PostgreSQL
                if (elementMapping is NpgsqlArrayTypeMapping)
                    return null;

                return ClrTypeMappings.GetOrAdd(clrType, new NpgsqlListTypeMapping(elementMapping, clrType));
            }

            return null;
        }
    }
}
