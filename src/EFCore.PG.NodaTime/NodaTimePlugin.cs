#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.EvaluatableExpressionFilters.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime
{
    /// <summary>
    /// The NodaTime plugin for the Npgsql Entity Framework Core provider.
    /// </summary>
    public class NodaTimePlugin : NpgsqlEntityFrameworkPlugin
    {
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

        [NotNull] readonly NpgsqlRangeTypeMapping<LocalDateTime> _timestampLocalDateTimeRange;
        [NotNull] readonly NpgsqlRangeTypeMapping<Instant> _timestampInstantRange;
        [NotNull] readonly NpgsqlRangeTypeMapping<Instant> _timestamptzInstantRange;
        [NotNull] readonly NpgsqlRangeTypeMapping<ZonedDateTime> _timestamptzZonedDateTimeRange;
        [NotNull] readonly NpgsqlRangeTypeMapping<OffsetDateTime> _timestamptzOffsetDateTimeRange;
        [NotNull] readonly NpgsqlRangeTypeMapping<LocalDate> _dateRange;

        #endregion

        /// <summary>
        /// The default member translators registered by the Npgsql NodaTime plugin.
        /// </summary>
        [NotNull] static readonly IMemberTranslator[] MemberTranslators = { new NodaTimeMemberTranslator() };

        /// <summary>
        /// The default method call translators registered by the Npgsql NodaTime plugin.
        /// </summary>
        [NotNull] static readonly IMethodCallTranslator[] MethodCallTranslators = { new NodaTimeMethodCallTranslator() };

        /// <summary>
        /// The default evaluatable expression filters registered by the Npgsql NodaTime plugin.
        /// </summary>
        [NotNull] static readonly IEvaluatableExpressionFilter[] EvaluatableExpressionFilters = { new NodaTimeEvaluatableExpressionFilter() };

        /// <inheritdoc />
        public override string Name => "NodaTime";

        /// <inheritdoc />
        public override string Description => "Plugin to map NodaTime types to PostgreSQL date/time datatypes";

        /// <summary>
        /// Constructs an instance of the <see cref="NodaTimePlugin"/> class.
        /// </summary>
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
                "daterange", typeof(NpgsqlRange<LocalDate>), _date, NpgsqlDbType.Range | NpgsqlDbType.Date);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void AddMemberTranslators(NpgsqlCompositeMemberTranslator compositeMemberTranslator)
            => compositeMemberTranslator.AddTranslators(MemberTranslators);

        /// <inheritdoc />
        public override void AddMethodCallTranslators(NpgsqlCompositeMethodCallTranslator compositeMethodCallTranslator)
            => compositeMethodCallTranslator.AddTranslators(MethodCallTranslators);

        /// <inheritdoc />
        public override void AddEvaluatableExpressionFilters(NpgsqlCompositeEvaluatableExpressionFilter compositeEvaluatableExpressionFilter)
            => compositeEvaluatableExpressionFilter.AddFilters(EvaluatableExpressionFilters);
    }
}
