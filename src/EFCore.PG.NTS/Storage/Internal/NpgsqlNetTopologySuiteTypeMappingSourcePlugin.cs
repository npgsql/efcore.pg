using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;
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

        static readonly Dictionary<string, Type> SubTypeNameToClrType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "POINT",              typeof(Point) },
            { "LINESTRING",         typeof(LineString) },
            { "POLYGON",            typeof(Polygon) },
            { "MULTIPOINT",         typeof(MultiPoint) },
            { "MULTILINESTRING",    typeof(MultiLineString) },
            { "MULTIPOLYGON",       typeof(MultiPolygon) },
            { "GEOMETRYCOLLECTION", typeof(GeometryCollection) }
        };

        public NpgsqlNetTopologySuiteTypeMappingSourcePlugin([NotNull] INpgsqlNetTopologySuiteOptions options)
            => _options = Check.NotNull(options, nameof(options));

        public virtual RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            // TODO: Array
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var isGeography = _options.IsGeographyDefault;

            if (clrType != null && !typeof(Geometry).IsAssignableFrom(clrType))
                return null;

            if (storeTypeName != null)
            {
                if (!TryParseStoreTypeName(storeTypeName, out isGeography, out var parsedSubtype, out var _))
                    return null;
                if (clrType == null)
                    clrType = parsedSubtype;
            }

            return clrType != null || storeTypeName != null
                ? (RelationalTypeMapping)Activator.CreateInstance(
                    typeof(NpgsqlGeometryTypeMapping<>).MakeGenericType(clrType ?? typeof(Geometry)),
                    storeTypeName ?? (isGeography ? "geography" : "geometry"),
                    isGeography)
                : null;
        }

        /// <summary>
        /// Given a PostGIS store type name (e.g. GEOMETRY, GEOGRAPHY(Point, 4326)), attempts to parse it and return its components.
        /// </summary>
        public static bool TryParseStoreTypeName(string storeTypeName, out bool isGeography, out Type clrType, out int srid)
        {
            storeTypeName = storeTypeName.Trim();
            isGeography = false;
            clrType = null;
            srid = -1;

            var openParen = storeTypeName.IndexOf("(", StringComparison.Ordinal);

            var baseType = openParen > 0 ? storeTypeName.Substring(0, openParen).Trim() : storeTypeName;

            if (baseType.Equals("geometry", StringComparison.OrdinalIgnoreCase))
                isGeography = false;
            else if (baseType.Equals("geography", StringComparison.OrdinalIgnoreCase))
                isGeography = true;
            else
                return false;

            if (openParen == -1)
                return true;

            var closeParen = storeTypeName.IndexOf(")", openParen + 1, StringComparison.Ordinal);
            if (closeParen != storeTypeName.Length - 1)
                return false;

            string subTypeString;
            var comma = storeTypeName.IndexOf(",", openParen + 1, StringComparison.Ordinal);
            if (comma == -1)
                subTypeString = storeTypeName.Substring(openParen + 1, closeParen - openParen - 1).Trim();
            else
            {
                subTypeString = storeTypeName.Substring(openParen + 1, comma - openParen - 1).Trim();

                if (!int.TryParse(storeTypeName.Substring(comma + 1, closeParen - comma - 1).Trim(), out srid))
                    return false;
            }

            return SubTypeNameToClrType.TryGetValue(subTypeString, out clrType);
        }
    }
}
