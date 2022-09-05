using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlNetTopologySuiteTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    // Note: we reference the options rather than copying IsGeographyDefault out, because that field is initialized
    // rather late by SingletonOptionsInitializer
    private readonly INpgsqlNetTopologySuiteOptions _options;

    private static bool TryGetClrType(string subtypeName, [NotNullWhen(true)] out Type? clrType)
    {
        clrType = subtypeName switch
        {
            "POINT"              => typeof(Point),
            "LINESTRING"         => typeof(LineString),
            "POLYGON"            => typeof(Polygon),
            "MULTIPOINT"         => typeof(MultiPoint),
            "MULTILINESTRING"    => typeof(MultiLineString),
            "MULTIPOLYGON"       => typeof(MultiPolygon),
            "GEOMETRYCOLLECTION" => typeof(GeometryCollection),
            "GEOMETRY"           => typeof(Geometry),
            _                    => null
        };

        return clrType is not null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlNetTopologySuiteTypeMappingSourcePlugin(INpgsqlNetTopologySuiteOptions options)
        => _options = Check.NotNull(options, nameof(options));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        // TODO: Array
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;
        var isGeography = _options.IsGeographyDefault;

        if (clrType is not null && !typeof(Geometry).IsAssignableFrom(clrType))
        {
            return null;
        }

        if (storeTypeName is not null)
        {
            if (!TryParseStoreTypeName(storeTypeName, out _, out isGeography, out var parsedSubtype, out _, out _))
            {
                return null;
            }

            if (clrType is null)
            {
                clrType = parsedSubtype;
            }
        }

        return clrType is not null || storeTypeName is not null
            ? (RelationalTypeMapping)Activator.CreateInstance(
                typeof(NpgsqlGeometryTypeMapping<>).MakeGenericType(clrType ?? typeof(Geometry)),
                storeTypeName ?? (isGeography ? "geography" : "geometry"),
                isGeography)!
            : null;
    }

    /// <summary>
    /// Given a PostGIS store type name (e.g. GEOMETRY, GEOGRAPHY(Point, 4326), GEOMETRY(LineStringM, 4326)),
    /// attempts to parse it and return its components.
    /// </summary>
    public static bool TryParseStoreTypeName(
        string storeTypeName,
        out string subtypeName,
        out bool isGeography,
        out Type? clrType,
        out int srid,
        out Ordinates ordinates)
    {
        storeTypeName = storeTypeName.Trim();
        subtypeName = storeTypeName;
        isGeography = false;
        clrType = null;
        srid = -1;
        ordinates = Ordinates.AllOrdinates;

        var openParen = storeTypeName.IndexOf("(", StringComparison.Ordinal);

        var baseType = openParen > 0 ? storeTypeName.Substring(0, openParen).Trim() : storeTypeName;

        if (baseType.Equals("GEOMETRY", StringComparison.OrdinalIgnoreCase))
        {
            isGeography = false;
        }
        else if (baseType.Equals("GEOGRAPHY", StringComparison.OrdinalIgnoreCase))
        {
            isGeography = true;
        }
        else
        {
            return false;
        }

        if (openParen == -1)
        {
            return true;
        }

        var closeParen = storeTypeName.IndexOf(")", openParen + 1, StringComparison.Ordinal);
        if (closeParen != storeTypeName.Length - 1)
        {
            return false;
        }

        var comma = storeTypeName.IndexOf(",", openParen + 1, StringComparison.Ordinal);
        if (comma == -1)
        {
            subtypeName = storeTypeName.Substring(openParen + 1, closeParen - openParen - 1).Trim();
        }
        else
        {
            subtypeName = storeTypeName.Substring(openParen + 1, comma - openParen - 1).Trim();

            if (!int.TryParse(storeTypeName.Substring(comma + 1, closeParen - comma - 1).Trim(), out srid))
            {
                return false;
            }
        }

        subtypeName = subtypeName.ToUpper(CultureInfo.InvariantCulture);

        // We have geometry(subtype, srid), parse the subtype (POINT, POINTZ, POINTM, POINTZM...)

        if (TryGetClrType(subtypeName, out clrType))
        {
            return true;
        }

        if (subtypeName.EndsWith("ZM", StringComparison.Ordinal) && TryGetClrType(subtypeName[..^2], out clrType))
        {
            ordinates = Ordinates.XYZM;
            return true;
        }

        if (subtypeName.EndsWith("M", StringComparison.Ordinal) && TryGetClrType(subtypeName[..^1], out clrType))
        {
            ordinates = Ordinates.XYM;
            return true;
        }

        if (subtypeName.EndsWith("Z", StringComparison.Ordinal) && TryGetClrType(subtypeName[..^1], out clrType))
        {
            ordinates = Ordinates.XYZ;
            return true;
        }

        return false;
    }
}
