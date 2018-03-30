using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <remarks>
    /// This mapping is only used in Npgsql 3.2 and below.
    /// Later versions use type plugins to set up mappings, and corresponding EF Core
    /// plugins need to be used.
    /// </remarks>
    public class NpgsqlLegacyPostgisTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlLegacyPostgisTypeMapping() : base("geometry", typeof(PostgisGeometry), NpgsqlDbType.Geometry) {}

        protected NpgsqlLegacyPostgisTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlLegacyPostgisTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlLegacyPostgisTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);
    }
}
