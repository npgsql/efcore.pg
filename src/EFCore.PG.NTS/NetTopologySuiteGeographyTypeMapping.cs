using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GeoAPI.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite
{
    public class NetTopologySuiteGeographyTypeMapping : NpgsqlTypeMapping
    {
        public NetTopologySuiteGeographyTypeMapping(Type clrType) : base("geography", clrType, NpgsqlDbType.Geography) {}

        protected NetTopologySuiteGeographyTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NetTopologySuiteGeographyTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NetTopologySuiteGeographyTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var geometry = (IGeometry)value;
            var builder = new StringBuilder();

            builder.Append("GEOGRAPHY '");

            if (geometry.SRID > 0)
            {
                builder
                    .Append("SRID=")
                    .Append(geometry.SRID)
                    .Append(";");
            }

            builder
                .Append(geometry.AsText())
                .Append("'");

            return builder.ToString();
        }
    }
}
