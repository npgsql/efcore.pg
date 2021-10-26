// ReSharper disable once CheckNamespace

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class IntervalMapping : NpgsqlTypeMapping
    {
        private static readonly ConstructorInfo _constructor =
            typeof(Interval).GetConstructor(new[] { typeof(Instant), typeof(Instant) })!;

        private static readonly ConstructorInfo _constructorWithNulls =
            typeof(Interval).GetConstructor(new[] { typeof(Instant?), typeof(Instant?) })!;

        public IntervalMapping()
            : base("tstzrange", typeof(Interval), NpgsqlDbType.TimestampTzRange)
        {
        }

        protected IntervalMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.TimestampTzRange)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new IntervalMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new IntervalMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new IntervalMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var interval = (Interval)value;

            var start = interval.HasStart
                ? InstantPattern.ExtendedIso.Format(interval.Start)
                : "";
            var end = interval.HasEnd
                ? InstantPattern.ExtendedIso.Format(interval.End)
                : "";

            return $"'[{start},{end})'::tstzrange";
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var interval = (Interval)value;

            return interval.HasStart && interval.HasEnd
                ? Expression.New(
                    _constructor,
                    TimestampTzInstantMapping.GenerateCodeLiteral(interval.Start),
                    TimestampTzInstantMapping.GenerateCodeLiteral(interval.End))
                : Expression.New(
                    _constructorWithNulls,
                    interval.HasStart
                        ? Expression.Convert(
                            TimestampTzInstantMapping.GenerateCodeLiteral(interval.Start),
                            typeof(Instant?))
                        : Expression.Constant(null, typeof(Instant?)),
                    interval.HasEnd
                        ? Expression.Convert(
                            TimestampTzInstantMapping.GenerateCodeLiteral(interval.End),
                            typeof(Instant?))
                        : Expression.Constant(null, typeof(Instant?)));
        }
    }
}
