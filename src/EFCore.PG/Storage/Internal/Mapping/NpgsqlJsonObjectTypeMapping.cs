using System;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// A mapping for an arbitrary user POCO to PostgreSQL jsonb or json.
    /// For mapping to .NET string, see <see cref="NpgsqlStringTypeMapping"/>.
    /// </summary>
    public class NpgsqlJsonObjectTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlJsonObjectTypeMapping([NotNull] string storeType, [NotNull] Type clrType)
            : base(storeType, clrType, storeType == "jsonb" ? NpgsqlDbType.Jsonb : NpgsqlDbType.Json)
        {
            if (storeType != "json" && storeType != "jsonb")
                throw new ArgumentException($"{nameof(storeType)} must be 'json' or 'jsonb'", nameof(storeType));
        }

        protected NpgsqlJsonObjectTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType)
        {
        }

        public bool IsJsonb => StoreType == "jsonb";

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlJsonObjectTypeMapping(parameters, NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => JsonSerializer.Serialize(value);
    }
}
