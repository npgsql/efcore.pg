using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime
{
    public class NodaTimePlugin : IEntityFrameworkNpgsqlPlugin
    {
        public string Name => "NodaTime";
        public string Description => "Plugin to map NodaTime types to PostgreSQL date/time datatypes";

        readonly TimestampInstantMapping _timestampInstant = new TimestampInstantMapping();
        readonly TimestampLocalDateTimeMapping _timestampLocalDateTime = new TimestampLocalDateTimeMapping();

        readonly TimestampTzInstantMapping _timestamptzInstant = new TimestampTzInstantMapping();
        readonly TimestampTzZonedDateTimeMapping _timestamptzZonedDateTime = new TimestampTzZonedDateTimeMapping();
        readonly TimestampTzOffsetDateTimeMapping _timestamptzOffsetDateTime = new TimestampTzOffsetDateTimeMapping();

        readonly DateMapping _date = new DateMapping();
        readonly TimeMapping _time = new TimeMapping();
        readonly TimeTzMapping _timetz = new TimeTzMapping();
        readonly IntervalMapping _period = new IntervalMapping();

        public void AddMappings(NpgsqlTypeMappingSource typeMappingSource)
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
                    new RelationalTypeMapping[] { _timestamptzInstant, _timestamptzOffsetDateTime, _timestamptzZonedDateTime };

            typeMappingSource.ClrTypeMappings[typeof(LocalDate)] = _date;
            typeMappingSource.StoreTypeMappings["date"] = new RelationalTypeMapping[] { _date };

            typeMappingSource.ClrTypeMappings[typeof(LocalTime)] = _time;
            typeMappingSource.StoreTypeMappings["time"] = new RelationalTypeMapping[] { _time };

            typeMappingSource.ClrTypeMappings[typeof(OffsetTime)] = _timetz;
            typeMappingSource.StoreTypeMappings["timetz"] = new RelationalTypeMapping[] { _timetz };

            typeMappingSource.ClrTypeMappings[typeof(Period)] = _period;
            typeMappingSource.StoreTypeMappings["interval"] = new RelationalTypeMapping[] { _period };
        }

        static readonly IMemberTranslator[] MemberTranslators =
        {
            new NodaTimeMemberTranslator()
        };

        public void AddMethodCallTranslators(NpgsqlCompositeMethodCallTranslator compositeMethodCallTranslator) {}

        public void AddMemberTranslators(NpgsqlCompositeMemberTranslator compositeMemberTranslator)
            => compositeMemberTranslator.AddTranslators(MemberTranslators);
    }
}
