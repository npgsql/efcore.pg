using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using NpgsqlTypes;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for <see cref="DateTime"/> members.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-datetime.html
    /// </remarks>
    public class NpgsqlDateTimeMemberTranslator : IMemberTranslator
    {
        private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
        private readonly RelationalTypeMapping _timestampMapping;
        private readonly RelationalTypeMapping _timestampTzMapping;

        public NpgsqlDateTimeMemberTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            NpgsqlSqlExpressionFactory sqlExpressionFactory)
        {
            _timestampMapping = typeMappingSource.FindMapping("timestamp without time zone")!;
            _timestampTzMapping = typeMappingSource.FindMapping("timestamp with time zone")!;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <inheritdoc />
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            var type = member.DeclaringType;

            if (type == typeof(DateTimeOffset)
                && instance is not null
                && TranslateDateTimeOffset(instance, member, returnType) is { } translated)
            {
                return translated;
            }

            if (type != typeof(DateTime)
                && type != typeof(DateTimeOffset)
                && type != typeof(DateOnly)
                && type != typeof(TimeOnly)
#pragma warning disable 618 // NpgsqlDateTime and NpgsqlDate have been obsoleted
                && type != typeof(NpgsqlDateTime)
                && type != typeof(NpgsqlDate))
#pragma warning restore 618
            {
                return null;
            }

            return member.Name switch
            {
                // Legacy behavior
                nameof(DateTime.Now)    when NpgsqlTypeMappingSource.LegacyTimestampBehavior
                    => UtcNow(),
                nameof(DateTime.UtcNow) when NpgsqlTypeMappingSource.LegacyTimestampBehavior
                    => _sqlExpressionFactory.AtUtc(UtcNow()), // Return a UTC timestamp, but as timestamp without time zone

                // We support getting a local DateTime via DateTime.Now (based on PG TimeZone), but there's no way to get a non-UTC
                // DateTimeOffset.
                nameof(DateTime.Now) => type == typeof(DateTimeOffset)
                    ? throw new InvalidOperationException("Cannot translate DateTimeOffset.Now - use UtcNow.")
                    : LocalNow(),
                nameof(DateTime.UtcNow) => UtcNow(),

                nameof(DateTime.Today) => _sqlExpressionFactory.Function(
                    "date_trunc",
                    new SqlExpression[] { _sqlExpressionFactory.Constant("day"), LocalNow() },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    returnType),

                nameof(DateTime.Year)      => GetDatePartExpression(instance!, "year"),
                nameof(DateTime.Month)     => GetDatePartExpression(instance!, "month"),
                nameof(DateTime.DayOfYear) => GetDatePartExpression(instance!, "doy"),
                nameof(DateTime.Day)       => GetDatePartExpression(instance!, "day"),
                nameof(DateTime.Hour)      => GetDatePartExpression(instance!, "hour"),
                nameof(DateTime.Minute)    => GetDatePartExpression(instance!, "minute"),
                nameof(DateTime.Second)    => GetDatePartExpression(instance!, "second"),

                nameof(DateTime.Millisecond) => null, // Too annoying

                // .NET's DayOfWeek is an enum, but its int values happen to correspond to PostgreSQL
                nameof(DateTime.DayOfWeek) => GetDatePartExpression(instance!, "dow", floor: true),

                nameof(DateTime.Date) => _sqlExpressionFactory.Function(
                    "date_trunc",
                    new[] { _sqlExpressionFactory.Constant("day"), instance! },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    returnType,
                    instance!.TypeMapping),

                // TODO: Technically possible simply via casting to PG time, should be better in EF Core 3.0
                // but ExplicitCastExpression only allows casting to PG types that
                // are default-mapped from CLR types (timespan maps to interval,
                // which timestamp cannot be cast into)
                nameof(DateTime.TimeOfDay) => null,

                // TODO: Should be possible
                nameof(DateTime.Ticks) => null,

                _ => null
            };

            SqlExpression UtcNow()
                => _sqlExpressionFactory.Function(
                    "now",
                    Array.Empty<SqlExpression>(),
                    nullable: false,
                    argumentsPropagateNullability: TrueArrays[0],
                    returnType,
                    _timestampTzMapping);

            SqlExpression LocalNow()
                => _sqlExpressionFactory.Convert(UtcNow(), returnType, _timestampMapping);
        }

        private SqlExpression GetDatePartExpression(
            SqlExpression instance,
            string partName,
            bool floor = false)
        {
            if (instance.Type == typeof(DateTimeOffset))
            {
                // date_part exists only for timestamp without time zone, so if we pass in a timestamptz it gets converted to a local
                // timestamp based on TimeZone. Convert to a timestamp without time zone at UTC to get the right values.
                instance = _sqlExpressionFactory.AtUtc(instance);
            }

            var result = _sqlExpressionFactory.Function(
                "date_part",
                new[]
                {
                    _sqlExpressionFactory.Constant(partName),
                    instance
                },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                typeof(double));

            if (floor)
            {
                result = _sqlExpressionFactory.Function(
                    "floor",
                    new[] { result },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(double));
            }

            return _sqlExpressionFactory.Convert(result, typeof(int));
        }

        public virtual SqlExpression? TranslateDateTimeOffset(
            SqlExpression instance,
            MemberInfo member,
            Type returnType)
            => member.Name switch
            {
                // We only support UTC DateTimeOffset, so DateTimeOffset.DateTime is just a matter of converting to timestamp without time zone
                nameof(DateTimeOffset.DateTime) => _sqlExpressionFactory.AtUtc(instance),

                // We only support UTC DateTimeOffset, so DateTimeOffset.UtcDateTime does nothing (type change on CLR change, no change on the
                // PG side.
                nameof(DateTimeOffset.UtcDateTime) => instance,

                // Convert to timestamp without time zone, applying a time zone conversion based on the TimeZone connection parameter.
                nameof(DateTimeOffset.LocalDateTime) => _sqlExpressionFactory.Convert(instance, typeof(DateTime), _timestampMapping),
                _ => null
            };
    }
}
