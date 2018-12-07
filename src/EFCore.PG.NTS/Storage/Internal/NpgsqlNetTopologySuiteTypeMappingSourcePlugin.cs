using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GeoAPI.Geometries;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlNetTopologySuiteTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
    {
        static readonly Dictionary<string, Type> _storeTypeMappings = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "GEOMETRY", typeof(IGeometry) },
            { "GEOMETRYCOLLECTION", typeof(IGeometryCollection) },
            { "LINESTRING", typeof(ILineString) },
            { "LINESTRINGZ", typeof(ILineString) },
            { "LINESTRINGZM", typeof(ILineString) },
            { "MULTILINESTRING", typeof(IMultiLineString) },
            { "MULTILINESTRINGZ", typeof(IMultiLineString) },
            { "MULTILINESTRINGZM", typeof(IMultiLineString) },
            { "MULTIPOINT", typeof(IMultiPoint) },
            { "MULTIPOINTZ", typeof(IMultiPoint) },
            { "MULTIPOINTZM", typeof(IMultiPoint) },
            { "MULTIPOLYGON", typeof(IMultiPolygon) },
            { "MULTIPOLYGONZ", typeof(IMultiPolygon) },
            { "MULTIPOLYGONZM", typeof(IMultiPolygon) },
            { "POINT", typeof(IPoint) },
            { "POINTZ", typeof(IPoint) },
            { "POINTZM", typeof(IPoint) },
            { "POLYGON", typeof(IPolygon) },
            { "POLYGONZ", typeof(IPolygon) },
            { "POLYGONZM", typeof(IPolygon) }
        };

        // Note: we reference the options rather than copying IsGeographyDefault out, because that field is initialized
        // rather late by SingletonOptionsInitializer
        readonly INpgsqlNetTopologySuiteOptions _options;

        public NpgsqlNetTopologySuiteTypeMappingSourcePlugin([NotNull] INpgsqlNetTopologySuiteOptions options)
            => _options = Check.NotNull(options, nameof(options));

        public virtual RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeType = mappingInfo.StoreTypeName;

            // TODO: Array
            // TODO: SRID (https://github.com/aspnet/EntityFrameworkCore/issues/14000)

            if (clrType != null)
            {
                if (typeof(IGeometry).IsAssignableFrom(clrType) &&
                    (storeType != null || TryGetStoreType(clrType, _options.IsGeographyDefault, _options.Ordinates, null, out storeType)))
                {
                    return (RelationalTypeMapping)Activator.CreateInstance(
                        typeof(NpgsqlGeometryTypeMapping<>).MakeGenericType(clrType),
                        storeType);
                }
            }
            else if (storeType != null)
            {
                var x = ParseGeometryStoreType(storeType);
                if (!x.HasValue)
                    return null;

                if (!_storeTypeMappings.TryGetValue(x.Value.SpatialType ?? "geometry", out clrType))
                    return null;

                return (RelationalTypeMapping)Activator.CreateInstance(
                    typeof(NpgsqlGeometryTypeMapping<>).MakeGenericType(clrType),
                    storeType);
            }

            return null;
        }

        static readonly Regex _storeTypeRegex = new Regex(@"^(GEOMETRY|GEOGRAPHY)\s*(\(\s*(\w+)(\s*,\s*(\d+))?\s*\))?$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Given a PostgreSQL store type, attempts to parse it down to its components.
        /// </summary>
        /// <param name="storeType">A PostgreSQL store type string representation (e.g. GEOMETRY (POINT,4269))</param>
        /// <returns>The different components of the store type, or null if <paramref name="storeType"/> isn't a spatial type.</returns>
        public static (bool IsGeography, string SpatialType, int? Srid)? ParseGeometryStoreType(string storeType)
        {
            var m = _storeTypeRegex.Match(storeType);
            if (!m.Success)
                return null;

            return (
                string.Equals(m.Groups[1].Value, "GEOGRAPHY", StringComparison.OrdinalIgnoreCase),
                m.Groups[3].Success ? m.Groups[3].Value : null,
                m.Groups[5].Success ? (int?)int.Parse(m.Groups[5].Value) : null);
        }

        static bool TryGetStoreType(Type clrType, bool isGeography, Ordinates ordinates, int? srid, out string storeType)
        {
            var sb = new StringBuilder(isGeography ? "GEOGRAPHY" : "GEOMETRY");
            if (typeof(ILineString).IsAssignableFrom(clrType))
                sb.Append(" (LINESTRING");
            else if (typeof(IMultiLineString).IsAssignableFrom(clrType))
                sb.Append(" (MULTILINESTRING");
            else if (typeof(IMultiPoint).IsAssignableFrom(clrType))
                sb.Append(" (MULTIPOINT");
            else if (typeof(IMultiPolygon).IsAssignableFrom(clrType))
                sb.Append(" (MULTIPOLYGON");
            else if (typeof(IPoint).IsAssignableFrom(clrType))
                sb.Append(" (POINT");
            else if (typeof(IPolygon).IsAssignableFrom(clrType))
                sb.Append(" (POLYGON");
            else if (typeof(IGeometryCollection).IsAssignableFrom(clrType))
                sb.Append(" (GEOMETRYCOLLECTION");
            else if (typeof(IGeometry).IsAssignableFrom(clrType))
            {
                if (!srid.HasValue)
                {
                    storeType = sb.ToString();
                    return true;
                }

                sb.Append(" (GEOMETRY");
            }
            else
            {
                storeType = null;
                return false;
            }

            if (ordinates.HasFlag(Ordinates.Z))
                sb.Append('Z');

            if (ordinates.HasFlag(Ordinates.M))
                sb.Append('M');

            if (srid.HasValue)
                sb.Append(',').Append(srid.Value);
            sb.Append(')');

            storeType = sb.ToString();
            return true;
        }
    }
}
