using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlFloatTypeMapping : FloatTypeMapping
    {
        public NpgsqlFloatTypeMapping() : base("real", System.Data.DbType.Single) {}

        protected NpgsqlFloatTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlFloatTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var singleValue = Convert.ToSingle(value);
            if (double.IsNaN(singleValue))
                return "'NaN'";
            if (double.IsPositiveInfinity(singleValue))
                return "'Infinity'";
            if (double.IsNegativeInfinity(singleValue))
                return "'-Infinity'";
            return base.GenerateNonNullSqlLiteral(singleValue);
        }
    }
}
