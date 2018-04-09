using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Mappings
{
    public class NpgsqlNtsMultiPointTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlNtsMultiPointTypeMapping() : base("geometry", typeof(MultiPoint), NpgsqlDbType.Geometry) {}

        protected NpgsqlNtsMultiPointTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlNtsMultiPointTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlNtsMultiPointTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        // TODO: Implement with functions?
        //protected override string GenerateNonNullSqlLiteral(object value)
        //    => $"JSON '{EscapeSqlLiteral((string)value)}'";
    }
}
