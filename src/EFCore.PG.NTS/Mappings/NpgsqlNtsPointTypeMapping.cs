using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite.Mappings
{
    public class NpgsqlNtsPointTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlNtsPointTypeMapping() : base("geometry", typeof(Point), NpgsqlDbType.Geometry) {}

        protected NpgsqlNtsPointTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlNtsPointTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlNtsPointTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var p = (Point)value;

            return double.IsNaN(p.M)
                ? double.IsNaN(p.Z)
                    ? $"ST_MakePoint({p.X}, {p.Y})"
                    : $"ST_MakePoint({p.X}, {p.Y}, {p.Z})"
                : double.IsNaN(p.Z)
                    ? $"ST_MakePointM({p.X}, {p.Y}, {p.M})"
                    : $"ST_MakePoint({p.X}, {p.Y}, {p.Z}, {p.M})";
        }
    }
}
