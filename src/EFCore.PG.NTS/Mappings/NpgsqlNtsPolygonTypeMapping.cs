using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Mappings
{
    public class NpgsqlNtsPolygonTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlNtsPolygonTypeMapping() : base("geometry", typeof(Polygon), NpgsqlDbType.Geometry) {}

        protected NpgsqlNtsPolygonTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlNtsPolygonTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlNtsPolygonTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        // TODO: Implement with functions?
        //protected override string GenerateNonNullSqlLiteral(object value)
        //    => $"JSON '{EscapeSqlLiteral((string)value)}'";
    }
}
