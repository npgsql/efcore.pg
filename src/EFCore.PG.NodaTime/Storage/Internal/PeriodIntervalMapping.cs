// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class PeriodIntervalMapping : NpgsqlTypeMapping
    {
        private static readonly MethodInfo FromYears = typeof(Period).GetRuntimeMethod(nameof(Period.FromYears), new[] { typeof(int) })!;
        private static readonly MethodInfo FromMonths = typeof(Period).GetRuntimeMethod(nameof(Period.FromMonths), new[] { typeof(int) })!;
        private static readonly MethodInfo FromWeeks = typeof(Period).GetRuntimeMethod(nameof(Period.FromWeeks), new[] { typeof(int) })!;
        private static readonly MethodInfo FromDays = typeof(Period).GetRuntimeMethod(nameof(Period.FromDays), new[] { typeof(int) })!;
        private static readonly MethodInfo FromHours = typeof(Period).GetRuntimeMethod(nameof(Period.FromHours), new[] { typeof(long) })!;
        private static readonly MethodInfo FromMinutes = typeof(Period).GetRuntimeMethod(nameof(Period.FromMinutes), new[] { typeof(long) })!;
        private static readonly MethodInfo FromSeconds = typeof(Period).GetRuntimeMethod(nameof(Period.FromSeconds), new[] { typeof(long) })!;
        private static readonly MethodInfo FromMilliseconds = typeof(Period).GetRuntimeMethod(nameof(Period.FromMilliseconds), new[] { typeof(long) })!;
        private static readonly MethodInfo FromNanoseconds = typeof(Period).GetRuntimeMethod(nameof(Period.FromNanoseconds), new[] { typeof(long) })!;

        private static readonly PropertyInfo Zero = typeof(Period).GetProperty(nameof(Period.Zero))!;

        public PeriodIntervalMapping() : base("interval", typeof(Period), NpgsqlDbType.Interval) {}

        protected PeriodIntervalMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Interval) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new PeriodIntervalMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new PeriodIntervalMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new PeriodIntervalMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"INTERVAL '{GenerateLiteralCore(value)}'";

        protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
            => $@"""{GenerateLiteralCore(value)}""";

        private string GenerateLiteralCore(object value)
            => PeriodPattern.NormalizingIso.Format((Period)value);

        public override Expression GenerateCodeLiteral(object value)
        {
            var period = (Period)value;
            Expression? e = null;

            if (period.Years != 0)
            {
                Compose(Expression.Call(FromYears, Expression.Constant(period.Years)));
            }

            if (period.Months != 0)
            {
                Compose(Expression.Call(FromMonths, Expression.Constant(period.Months)));
            }

            if (period.Weeks != 0)
            {
                Compose(Expression.Call(FromWeeks, Expression.Constant(period.Weeks)));
            }

            if (period.Days != 0)
            {
                Compose(Expression.Call(FromDays, Expression.Constant(period.Days)));
            }

            if (period.Hours != 0)
            {
                Compose(Expression.Call(FromHours, Expression.Constant(period.Hours)));
            }

            if (period.Minutes != 0)
            {
                Compose(Expression.Call(FromMinutes, Expression.Constant(period.Minutes)));
            }

            if (period.Seconds != 0)
            {
                Compose(Expression.Call(FromSeconds, Expression.Constant(period.Seconds)));
            }

            if (period.Milliseconds != 0)
            {
                Compose(Expression.Call(FromMilliseconds, Expression.Constant(period.Milliseconds)));
            }

            if (period.Nanoseconds != 0)
            {
                Compose(Expression.Call(FromNanoseconds, Expression.Constant(period.Nanoseconds)));
            }

            return e ?? Expression.MakeMemberAccess(null, Zero);

            void Compose(Expression toAdd) => e = e is null ? toAdd : Expression.Add(e, toAdd);
        }
    }
}
