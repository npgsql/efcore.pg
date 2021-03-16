using System;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using JetBrains.Annotations;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    [UsedImplicitly]
    public class NpgsqlGeometryTypeMapping<TGeometry> : RelationalGeometryTypeMapping<TGeometry, TGeometry>, INpgsqlTypeMapping
    {
        private readonly bool _isGeography;

        /// <inheritdoc />
        public virtual NpgsqlDbType NpgsqlDbType
            => _isGeography ? NpgsqlDbType.Geography : NpgsqlDbType.Geometry;

        public NpgsqlGeometryTypeMapping([NotNull] string storeType, bool isGeography) : base(converter: null, storeType)
            => _isGeography = isGeography;

        protected NpgsqlGeometryTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, converter: null) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlGeometryTypeMapping<TGeometry>(parameters);

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            ((NpgsqlParameter)parameter).NpgsqlDbType = NpgsqlDbType;
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var geometry = (Geometry)value;
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

        protected override string AsText(object value) => ((Geometry)value).AsText();

        protected override int GetSrid(object value) => ((Geometry)value).SRID;

        protected override Type WKTReaderType => typeof(WKTReader);
    }
}
