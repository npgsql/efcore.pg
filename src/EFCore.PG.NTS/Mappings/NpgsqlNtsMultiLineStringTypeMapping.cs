using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Mappings
{
    public class NpgsqlNtsMultiLineStringTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlNtsMultiLineStringTypeMapping() : base("geometry", typeof(MultiLineString), NpgsqlDbType.Geometry) {}

        protected NpgsqlNtsMultiLineStringTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlNtsMultiLineStringTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlNtsMultiLineStringTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        // TODO: Implement with functions?
        //protected override string GenerateNonNullSqlLiteral(object value)
        //    => $"JSON '{EscapeSqlLiteral((string)value)}'";
    }
}
