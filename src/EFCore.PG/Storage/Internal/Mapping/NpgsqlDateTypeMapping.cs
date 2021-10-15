using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlDateTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlDateTypeMapping(Type clrType) : base("date", clrType, NpgsqlDbType.Date) {}

        protected NpgsqlDateTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Date) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlDateTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"DATE '{GenerateEmbeddedNonNullSqlLiteral(value)}'";

        protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
            => value switch
            {
                DateTime d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                _ => throw new InvalidCastException($"Can't generate a date SQL literal for CLR type {value.GetType()}")
            };
    }
}
