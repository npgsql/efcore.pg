using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;
using static Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Utilties.Util;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class DateMapping : NpgsqlTypeMapping
    {
        private static readonly ConstructorInfo Constructor =
            typeof(LocalDate).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) })!;

        public DateMapping() : base("date", typeof(LocalDate), NpgsqlDbType.Date) {}

        protected DateMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Date) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new DateMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new DateMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new DateMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"DATE '{GenerateEmbeddedNonNullSqlLiteral(value)}'";

        protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        {
            var date = (LocalDate)value;

            if (!NpgsqlNodaTimeTypeMappingSourcePlugin.DisableDateTimeInfinityConversions)
            {
                if (date == LocalDate.MinIsoValue)
                {
                    return "-infinity";
                }

                if (date == LocalDate.MaxIsoValue)
                {
                    return "infinity";
                }
            }

            return LocalDatePattern.Iso.Format(date);
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var date = (LocalDate)value;
            return ConstantNew(Constructor, date.Year, date.Month, date.Day);
        }
    }
}
