using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GeoAPI.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite
{
    public class NetTopologySuiteTypeGeographyMapping : NpgsqlTypeMapping
    {
        public NetTopologySuiteTypeGeographyMapping(Type clrType) : base("geography", clrType, NpgsqlDbType.Geography) {}

        protected NetTopologySuiteTypeGeographyMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NetTopologySuiteTypeGeographyMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NetTopologySuiteTypeGeographyMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"GEOGRAPHY '{((IGeometry)value).AsText()}'";
    }
}
