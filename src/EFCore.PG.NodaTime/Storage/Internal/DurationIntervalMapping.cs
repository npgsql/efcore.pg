using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NpgsqlTypes;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class DurationIntervalMapping : NpgsqlTypeMapping
    {
        private static readonly MethodInfo FromDays = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromDays), new[] { typeof(int) })!;
        private static readonly MethodInfo FromHours = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromHours), new[] { typeof(int) })!;
        private static readonly MethodInfo FromMinutes = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromMinutes), new[] { typeof(long) })!;
        private static readonly MethodInfo FromSeconds = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromSeconds), new[] { typeof(long) })!;
        private static readonly MethodInfo FromMilliseconds = typeof(Duration).GetRuntimeMethod(nameof(Duration.FromMilliseconds), new[] { typeof(long) })!;

        private static readonly PropertyInfo Zero = typeof(Duration).GetProperty(nameof(Duration.Zero))!;

        public DurationIntervalMapping() : base("interval", typeof(Duration), NpgsqlDbType.Interval) {}

        protected DurationIntervalMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Interval) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new DurationIntervalMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new DurationIntervalMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new DurationIntervalMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"INTERVAL '{NpgsqlIntervalTypeMapping.FormatTimeSpanAsInterval(((Duration)value).ToTimeSpan())}'";

        public override Expression GenerateCodeLiteral(object value)
        {
            var duration = (Duration)value;
            Expression? e = null;

            if (duration.Days != 0)
            {
                Compose(Expression.Call(FromDays, Expression.Constant(duration.Days)));
            }

            if (duration.Hours != 0)
            {
                Compose(Expression.Call(FromHours, Expression.Constant(duration.Hours)));
            }

            if (duration.Minutes != 0)
            {
                Compose(Expression.Call(FromMinutes, Expression.Constant((long)duration.Minutes)));
            }

            if (duration.Seconds != 0)
            {
                Compose(Expression.Call(FromSeconds, Expression.Constant((long)duration.Seconds)));
            }

            if (duration.Milliseconds != 0)
            {
                Compose(Expression.Call(FromMilliseconds, Expression.Constant((long)duration.Milliseconds)));
            }

            return e ?? Expression.MakeMemberAccess(null, Zero);

            void Compose(Expression toAdd) => e = e is null ? toAdd : Expression.Add(e, toAdd);
        }
    }
}
