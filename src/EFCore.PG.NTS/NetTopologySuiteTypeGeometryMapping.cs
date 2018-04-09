using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite
{
    public class NetTopologySuiteTypeGeometryMapping : NpgsqlTypeMapping
    {
        public NetTopologySuiteTypeGeometryMapping(Type clrType) : base("geometry", clrType, NpgsqlDbType.Geometry) {}

        protected NetTopologySuiteTypeGeometryMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NetTopologySuiteTypeGeometryMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NetTopologySuiteTypeGeometryMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"GEOMETRY '{((IGeometry)value).AsText()}'";
    }
}
