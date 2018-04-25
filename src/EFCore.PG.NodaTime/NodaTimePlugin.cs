using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Mappings;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime
{
    public class NodaTimePlugin : IEntityFrameworkNpgsqlPlugin
    {
        public string Name => "NodaTime";
        public string Description => "Plugin to map NodaTime types to PostgreSQL date/time datatypes";

        readonly NodaTimeTimestampInstantMapping _timestampInstant = new NodaTimeTimestampInstantMapping();
        readonly NodaTimeTimestampLocalDateTimeMapping _timestampLocalDateTime = new NodaTimeTimestampLocalDateTimeMapping();
        readonly NodaTimeTimestampTzMapping _timestamptz = new NodaTimeTimestampTzMapping();
        readonly NodaTimeDateMapping _date = new NodaTimeDateMapping();
        readonly NodaTimeTimeMapping _time = new NodaTimeTimeMapping();
        readonly NodaTimeTimeTzMapping _timetz = new NodaTimeTimeTzMapping();
        readonly NodaTimeIntervalMapping _period = new NodaTimeIntervalMapping();

        public void AddMappings(NpgsqlTypeMappingSource typeMappingSource)
        {
            typeMappingSource.ClrTypeMappings[typeof(Instant)] = _timestampInstant;
            typeMappingSource.ClrTypeMappings[typeof(LocalDateTime)] = _timestampLocalDateTime;

            typeMappingSource.StoreTypeMappings["timestamp"] =
                typeMappingSource.StoreTypeMappings["timestamp without time zone"] =
                    new RelationalTypeMapping[] { _timestampInstant, _timestampLocalDateTime };

            typeMappingSource.ClrTypeMappings[typeof(ZonedDateTime)] = _timestamptz;
            typeMappingSource.StoreTypeMappings["timestamptz"] =
                typeMappingSource.StoreTypeMappings["timestamp with time zone"] =
                    new RelationalTypeMapping[] { _timestamptz };

            typeMappingSource.ClrTypeMappings[typeof(LocalDate)] = _date;
            typeMappingSource.StoreTypeMappings["date"] = new RelationalTypeMapping[] { _date };

            typeMappingSource.ClrTypeMappings[typeof(LocalTime)] = _time;
            typeMappingSource.StoreTypeMappings["time"] = new RelationalTypeMapping[] { _time };

            typeMappingSource.ClrTypeMappings[typeof(OffsetDateTime)] = _timetz;
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
