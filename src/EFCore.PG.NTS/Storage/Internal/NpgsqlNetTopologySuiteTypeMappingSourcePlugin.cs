using System;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlNetTopologySuiteTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
    {
        // Note: we reference the options rather than copying IsGeographyDefault out, because that field is initialized
        // rather late by SingletonOptionsInitializer
        readonly INpgsqlNetTopologySuiteOptions _options;

        public NpgsqlNetTopologySuiteTypeMappingSourcePlugin([NotNull] INpgsqlNetTopologySuiteOptions options)
            => _options = Check.NotNull(options, nameof(options));

        public virtual RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            // TODO: Array
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var isGeography = _options.IsGeographyDefault;

            if (storeTypeName != null && !TryParseStoreTypeName(storeTypeName, out isGeography, out var _, out var _))
                return null;
            if (clrType != null && !typeof(IGeometry).IsAssignableFrom(clrType))
                return null;

            return clrType != null || storeTypeName != null
                ? (RelationalTypeMapping)Activator.CreateInstance(
                    typeof(NpgsqlGeometryTypeMapping<>).MakeGenericType(clrType ?? typeof(IGeometry)),
                    storeTypeName ?? (isGeography ? "geography" : "geometry"),
                    isGeography)
                : null;
        }

        /// <summary>
        /// Given a PostGIS store type name (e.g. GEOMETRY, GEOGRAPHY(Point, 4326)), attempts to parse it and return its components.
        /// </summary>
        public static bool TryParseStoreTypeName(string storeTypeName, out bool isGeography, out string subType, out int srid)
        {
            isGeography = false;
            subType = null;
            srid = -1;

            var openParen = storeTypeName.IndexOf("(", StringComparison.Ordinal);

            var baseType = (openParen > 0
                ? storeTypeName.Substring(0, openParen)
                : storeTypeName)
                .Trim();

            if (baseType.Equals("geometry", StringComparison.OrdinalIgnoreCase))
                isGeography = false;
            else if (baseType.Equals("geography", StringComparison.OrdinalIgnoreCase))
                isGeography = true;
            else
                return false;

            if (openParen == -1)
                return true;

            var closeParen = storeTypeName.IndexOf(")", openParen + 1, StringComparison.Ordinal);
            if (closeParen > openParen)
            {
                var comma = storeTypeName.IndexOf(",", openParen + 1, StringComparison.Ordinal);
                if (comma > openParen && comma < closeParen)
                {
                    subType = storeTypeName.Substring(openParen + 1, comma - openParen - 1).Trim();

                    if (!int.TryParse(storeTypeName.Substring(comma + 1, closeParen - comma - 1).Trim(), out srid))
                        return false;
                }
                else
                    subType = storeTypeName.Substring(openParen + 1, closeParen - openParen - 1).Trim();
            }

            return true;
        }
    }
}
