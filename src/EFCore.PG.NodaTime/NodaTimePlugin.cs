using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime
{
    public class NodaTimePlugin : NpgsqlEntityFrameworkPlugin
    {
        public override string Name => "NodaTime";
        public override string Description => "Plugin to map NodaTime types to PostgreSQL date/time datatypes";

        readonly TimestampInstantMapping _timestampInstant = new TimestampInstantMapping();
        readonly TimestampLocalDateTimeMapping _timestampLocalDateTime = new TimestampLocalDateTimeMapping();

        readonly TimestampTzInstantMapping _timestamptzInstant = new TimestampTzInstantMapping();
        readonly TimestampTzZonedDateTimeMapping _timestamptzZonedDateTime = new TimestampTzZonedDateTimeMapping();
        readonly TimestampTzOffsetDateTimeMapping _timestamptzOffsetDateTime = new TimestampTzOffsetDateTimeMapping();

        readonly DateMapping _date = new DateMapping();
        readonly TimeMapping _time = new TimeMapping();
        readonly TimeTzMapping _timetz = new TimeTzMapping();
        readonly IntervalMapping _period = new IntervalMapping();

        readonly NpgsqlRangeTypeMapping<LocalDateTime> _timestampLocalDateTimeRange;
        readonly NpgsqlRangeTypeMapping<Instant> _timestampInstantRange;
        readonly NpgsqlRangeTypeMapping<Instant> _timestamptzInstantRange;
        readonly NpgsqlRangeTypeMapping<ZonedDateTime> _timestamptzZonedDateTimeRange;
        readonly NpgsqlRangeTypeMapping<OffsetDateTime> _timestamptzOffsetDateTimeRange;
        readonly NpgsqlRangeTypeMapping<LocalDate> _dateRange;

        public NodaTimePlugin()
        {
            _timestampLocalDateTimeRange = new NpgsqlRangeTypeMapping<LocalDateTime>(
                "tsrange", typeof(NpgsqlRange<LocalDateTime>), _timestampLocalDateTime, NpgsqlDbType.Range | NpgsqlDbType.Timestamp);
            _timestampInstantRange = new NpgsqlRangeTypeMapping<Instant>(
                "tsrange", typeof(NpgsqlRange<Instant>), _timestampInstant, NpgsqlDbType.Range | NpgsqlDbType.Timestamp);
            _timestamptzInstantRange = new NpgsqlRangeTypeMapping<Instant>(
                "tstzrange", typeof(NpgsqlRange<Instant>), _timestamptzInstant, NpgsqlDbType.Range | NpgsqlDbType.TimestampTz);
            _timestamptzZonedDateTimeRange = new NpgsqlRangeTypeMapping<ZonedDateTime>(
                "tstzrange", typeof(NpgsqlRange<ZonedDateTime>), _timestamptzZonedDateTime, NpgsqlDbType.Range | NpgsqlDbType.TimestampTz);
            _timestamptzOffsetDateTimeRange = new NpgsqlRangeTypeMapping<OffsetDateTime>(
                "tstzrange", typeof(NpgsqlRange<OffsetDateTime>), _timestamptzOffsetDateTime, NpgsqlDbType.Range | NpgsqlDbType.TimestampTz);
            _dateRange = new NpgsqlRangeTypeMapping<LocalDate>(
                "daterange", typeof(NpgsqlRange<LocalDate>), _date, NpgsqlDbType.Range | NpgsqlDbType.Date);;
        }

        public override void AddMappings(NpgsqlTypeMappingSource typeMappingSource)
        {
            typeMappingSource.ClrTypeMappings[typeof(Instant)] = _timestampInstant;
            typeMappingSource.ClrTypeMappings[typeof(LocalDateTime)] = _timestampLocalDateTime;

            typeMappingSource.StoreTypeMappings["timestamp"] =
                typeMappingSource.StoreTypeMappings["timestamp without time zone"] =
                    new RelationalTypeMapping[] { _timestampInstant, _timestampLocalDateTime };

            typeMappingSource.ClrTypeMappings[typeof(ZonedDateTime)] = _timestamptzZonedDateTime;
            typeMappingSource.ClrTypeMappings[typeof(OffsetDateTime)] = _timestamptzOffsetDateTime;

            typeMappingSource.StoreTypeMappings["timestamptz"] =
                typeMappingSource.StoreTypeMappings["timestamp with time zone"] =
                    new RelationalTypeMapping[] { _timestamptzInstant, _timestamptzZonedDateTime, _timestamptzOffsetDateTime };

            typeMappingSource.ClrTypeMappings[typeof(LocalDate)] = _date;
            typeMappingSource.StoreTypeMappings["date"] = new RelationalTypeMapping[] { _date };

            typeMappingSource.ClrTypeMappings[typeof(LocalTime)] = _time;
            typeMappingSource.StoreTypeMappings["time"] = new RelationalTypeMapping[] { _time };

            typeMappingSource.ClrTypeMappings[typeof(OffsetTime)] = _timetz;
            typeMappingSource.StoreTypeMappings["timetz"] = new RelationalTypeMapping[] { _timetz };

            typeMappingSource.ClrTypeMappings[typeof(Period)] = _period;
            typeMappingSource.StoreTypeMappings["interval"] = new RelationalTypeMapping[] { _period };

            // Ranges
            typeMappingSource.ClrTypeMappings[typeof(NpgsqlRange<Instant>)] = _timestampInstantRange;
            typeMappingSource.ClrTypeMappings[typeof(NpgsqlRange<LocalDateTime>)] = _timestampLocalDateTimeRange;

            typeMappingSource.StoreTypeMappings["tsrange"] =
                new RelationalTypeMapping[] { _timestampInstantRange, _timestampLocalDateTimeRange };

            typeMappingSource.ClrTypeMappings[typeof(NpgsqlRange<ZonedDateTime>)] = _timestamptzZonedDateTimeRange;
            typeMappingSource.ClrTypeMappings[typeof(NpgsqlRange<OffsetDateTime>)] = _timestamptzOffsetDateTimeRange;

            typeMappingSource.StoreTypeMappings["tstzrange"] =
                new RelationalTypeMapping[] { _timestamptzInstantRange, _timestamptzZonedDateTimeRange, _timestamptzOffsetDateTimeRange };

            typeMappingSource.ClrTypeMappings[typeof(NpgsqlRange<LocalDate>)] = _dateRange;
            typeMappingSource.StoreTypeMappings["daterange"] = new RelationalTypeMapping[] { _dateRange };
        }

        static readonly IMemberTranslator[] MemberTranslators =
        {
            new NodaTimeMemberTranslator()
        };

        public override void AddMemberTranslators(NpgsqlCompositeMemberTranslator compositeMemberTranslator)
            => compositeMemberTranslator.AddTranslators(MemberTranslators);
    }
}
