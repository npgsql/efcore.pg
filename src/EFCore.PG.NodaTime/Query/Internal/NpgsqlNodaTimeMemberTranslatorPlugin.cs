using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Query.Internal
{
    /// <summary>
    /// Provides translation services for <see cref="NodaTime"/> members.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-datetime.html
    /// </remarks>
    public class NpgsqlNodaTimeMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        public NpgsqlNodaTimeMemberTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory)
        {
            Translators = new IMemberTranslator[]
            {
                new NpgsqlNodaTimeMemberTranslator((NpgsqlSqlExpressionFactory)sqlExpressionFactory),
            };
        }

        public virtual IEnumerable<IMemberTranslator> Translators { get; }
    }

    public class NpgsqlNodaTimeMemberTranslator : IMemberTranslator
    {
        private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        private static readonly MemberInfo SystemClock_Instance =
            typeof(SystemClock).GetRuntimeProperty(nameof(SystemClock.Instance))!;
        private static readonly MemberInfo ZonedDateTime_LocalDateTime =
            typeof(ZonedDateTime).GetRuntimeProperty(nameof(ZonedDateTime.LocalDateTime))!;

        public NpgsqlNodaTimeMemberTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        private static readonly bool[][] TrueArrays =
        {
            Array.Empty<bool>(),
            new[] { true },
            new[] { true, true }
        };

        /// <inheritdoc />
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            // This is necessary to allow translation of methods on SystemClock.Instance
            if (member == SystemClock_Instance)
                return _sqlExpressionFactory.Constant(SystemClock.Instance);

            if (instance is null)
            {
                return null;
            }

            var declaringType = member.DeclaringType;

            if (declaringType == typeof(LocalDateTime)
                || declaringType == typeof(LocalDate)
                || declaringType == typeof(LocalTime)
                || declaringType == typeof(Period))
            {
                return TranslateDateTime(instance, member, returnType);
            }

            if (declaringType == typeof(ZonedDateTime))
            {
                // date_part, which is used to extract most components, doesn't have an overload for timestamptz, so passing one directly
                // converts it to the local timezone as per TimeZone. Explicitly convert it to a local timestamp in UTC.
                return TranslateDateTime(_sqlExpressionFactory.AtUtc(instance), member, returnType)
                    ?? TranslateZonedDateTime(instance, member);
            }

            if (declaringType == typeof(Duration))
            {
                return TranslateDuration(instance, member);
            }

            return null;
        }

        private SqlExpression? TranslateDuration(SqlExpression instance, MemberInfo member)
        {
            var translateDurationTotalMember = new Func<SqlExpression, double, SqlBinaryExpression>(
                (inst, divisor) =>
                    _sqlExpressionFactory.Divide(GetDatePartExpressionDouble(inst, "epoch"), _sqlExpressionFactory.Constant(divisor)));

            return member.Name switch
            {
                nameof(Duration.TotalDays) => translateDurationTotalMember(instance, 86400),
                nameof(Duration.TotalHours) => translateDurationTotalMember(instance, 3600),
                nameof(Duration.TotalMinutes) => translateDurationTotalMember(instance, 60),
                nameof(Duration.TotalSeconds) => translateDurationTotalMember(instance, 1),
                nameof(Duration.TotalMilliseconds) => translateDurationTotalMember(instance, 0.001),
                nameof(Duration.Days) => GetDatePartExpression(instance, "day"),
                nameof(Duration.Hours) => GetDatePartExpression(instance, "hour"),
                nameof(Duration.Minutes) => GetDatePartExpression(instance, "minute"),
                nameof(Duration.Seconds) => GetDatePartExpression(instance, "second", true),
                nameof(Duration.Milliseconds) => null, // Too annoying, floating point and sub-millisecond handling
                _ => null,
            };
        }

        private SqlExpression? TranslateDateTime(SqlExpression instance, MemberInfo member, Type returnType)
        {
            switch (member.Name)
            {
            case "Year":
            case "Years":
                return GetDatePartExpression(instance, "year");

            case "Month":
            case "Months":
                return GetDatePartExpression(instance, "month");

            case "DayOfYear":
                return GetDatePartExpression(instance, "doy");

            case "Day":
            case "Days":
                return GetDatePartExpression(instance, "day");

            case "Hour":
            case "Hours":
                return GetDatePartExpression(instance, "hour");

            case "Minute":
            case "Minutes":
                return GetDatePartExpression(instance, "minute");

            case "Second":
            case "Seconds":
                return GetDatePartExpression(instance, "second", true);

            case "Millisecond":
            case "Milliseconds":
                return null; // Too annoying

            case "DayOfWeek":
                // Unlike DateTime.DayOfWeek, NodaTime's IsoDayOfWeek enum doesn't exactly correspond to PostgreSQL's
                // values returned by DATE_PART('dow', ...): in NodaTime Sunday is 7 and not 0, which is None.
                // So we generate a CASE WHEN expression to translate PostgreSQL's 0 to 7.
                var getValueExpression = GetDatePartExpression(instance, "dow", true);
                // TODO: Can be simplified once https://github.com/aspnet/EntityFrameworkCore/pull/16726 is in
                return
                    _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.Equal(getValueExpression, _sqlExpressionFactory.Constant(0)),
                                _sqlExpressionFactory.Constant(7))
                        },
                        getValueExpression
                    );

            case "Date":
                return _sqlExpressionFactory.Function(
                    "DATE_TRUNC",
                    new[] { _sqlExpressionFactory.Constant("day"), instance },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    returnType);

            case "TimeOfDay":
                // TODO: Technically possible simply via casting to PG time,
                // but ExplicitCastExpression only allows casting to PG types that
                // are default-mapped from CLR types (timespan maps to interval,
                // which timestamp cannot be cast into)
                return null;

            default:
                return null;
            }
        }

        /// <summary>
        /// Constructs the DATE_PART expression.
        /// </summary>
        /// <param name="e">The member expression.</param>
        /// <param name="partName">The name of the DATE_PART to construct.</param>
        /// <param name="floor">True if the result should be wrapped with FLOOR(...); otherwise, false.</param>
        /// <returns>
        /// The DATE_PART expression.
        /// </returns>
        /// <remarks>
        /// DATE_PART returns doubles, which we floor and cast into ints
        /// This also gets rid of sub-second components when retrieving seconds.
        /// </remarks>
        private SqlExpression GetDatePartExpression(
            SqlExpression instance,
            string partName,
            bool floor = false)
        {
            var result = GetDatePartExpressionDouble(instance, partName, floor);
            return _sqlExpressionFactory.Convert(result, typeof(int));
        }
        
        private SqlExpression GetDatePartExpressionDouble(
            SqlExpression instance,
            string partName,
            bool floor = false)
        {
            var result = _sqlExpressionFactory.Function(
                "DATE_PART",
                new[] { _sqlExpressionFactory.Constant(partName), instance },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                typeof(double));

            if (floor)
                result = _sqlExpressionFactory.Function(
                    "FLOOR",
                    new[] { result },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(double));

            return result;
        }

        private SqlExpression? TranslateZonedDateTime(SqlExpression instance, MemberInfo member)
        {
            if (member == ZonedDateTime_LocalDateTime)
                return _sqlExpressionFactory.AtUtc(instance);

            return null;
        }
    }
}
