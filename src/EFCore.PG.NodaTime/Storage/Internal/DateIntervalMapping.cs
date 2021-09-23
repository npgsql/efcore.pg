// ReSharper disable once CheckNamespace

using System;
using System.Data.Common;
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
    public class DateIntervalMapping : NpgsqlTypeMapping
    {
        private static readonly ConstructorInfo _constructorWithDates =
            typeof(DateInterval).GetConstructor(new[] { typeof(LocalDate), typeof(LocalDate) })!;

        private static readonly ConstructorInfo _localDateConstructor =
            typeof(LocalDate).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) })!;

        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        public DateIntervalMapping()
            : base("daterange", typeof(DateInterval), NpgsqlDbType.Range |  NpgsqlDbType.Date)
        {
        }

        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        protected DateIntervalMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Range |  NpgsqlDbType.Date)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new DateIntervalMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new DateIntervalMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new DateIntervalMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var range = (DateInterval)value;
            return $"'[{LocalDatePattern.Iso.Format(range.Start)}, {LocalDatePattern.Iso.Format(range.End)}]'::daterange";
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var (start, end) = (DateInterval)value;
            return Expression.New(
                _constructorWithDates,
                Expression.New(_localDateConstructor, Expression.Constant(start.Year), Expression.Constant(start.Month), Expression.Constant(start.Day)),
                Expression.New(_localDateConstructor, Expression.Constant(end.Year), Expression.Constant(end.Month), Expression.Constant(end.Day))
            );
        }
    }
}
