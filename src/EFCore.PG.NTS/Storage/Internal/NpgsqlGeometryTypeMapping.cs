using System;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using NetTopologySuite.IO;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    [UsedImplicitly]
    public class NpgsqlGeometryTypeMapping<TGeometry> : RelationalGeometryTypeMapping<TGeometry, TGeometry>
    {
        readonly bool _isGeography;

        public NpgsqlGeometryTypeMapping(string storeType) : base(new NullValueConverter(), storeType)
            => _isGeography = IsGeography(storeType);

        protected NpgsqlGeometryTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, new NullValueConverter()) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlGeometryTypeMapping<TGeometry>(parameters);

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            ((NpgsqlParameter)parameter).NpgsqlDbType = _isGeography ? NpgsqlDbType.Geography : NpgsqlDbType.Geometry;
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var geometry = (IGeometry)value;
            var builder = new StringBuilder();

            builder
                .Append(_isGeography ? "GEOGRAPHY" : "GEOMETRY")
                .Append(" '");

            if (geometry.SRID > 0)
            {
                builder
                    .Append("SRID=")
                    .Append(geometry.SRID)
                    .Append(';');
            }

            builder
                .Append(geometry.AsText())
                .Append('\'');

            return builder.ToString();
        }

        static bool IsGeography(string storeType)
            => string.Equals(storeType, "geography", StringComparison.OrdinalIgnoreCase);

        class NullValueConverter : ValueConverter<TGeometry, TGeometry>
        {
            public NullValueConverter() : base(t => t, t => t) {}
        }

        protected override string AsText(object value) => ((IGeometry)value).AsText();

        protected override int GetSrid(object value) => ((IGeometry)value).SRID;

        protected override Type WKTReaderType => typeof(WKTReader);
    }
}
