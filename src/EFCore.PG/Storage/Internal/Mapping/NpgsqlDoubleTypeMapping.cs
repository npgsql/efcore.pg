using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlDoubleTypeMapping : DoubleTypeMapping
    {
        public NpgsqlDoubleTypeMapping() : base("double precision", System.Data.DbType.Double) {}

        protected NpgsqlDoubleTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlDoubleTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var doubleValue = Convert.ToDouble(value);
            if (double.IsNaN(doubleValue))
                return "'NaN'";
            if (double.IsPositiveInfinity(doubleValue))
                return "'Infinity'";
            if (double.IsNegativeInfinity(doubleValue))
                return "'-Infinity'";
            return base.GenerateNonNullSqlLiteral(doubleValue);
        }
    }
}
