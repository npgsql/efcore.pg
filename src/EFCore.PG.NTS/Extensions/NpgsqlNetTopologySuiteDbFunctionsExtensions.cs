// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides Npgsql-specific spatial extension methods on <see cref="DbFunctions"/>.
/// </summary>
public static class NpgsqlNetTopologySuiteDbFunctionsExtensions
{
    /// <summary>
    /// Returns a new geometry with its coordinates transformed to a different spatial reference system.
    /// Translates to <c>ST_Transform(geometry, srid)</c>.
    /// </summary>
    /// <remarks>
    /// See https://postgis.net/docs/ST_Transform.html.
    /// </remarks>
    public static TGeometry Transform<TGeometry>(this DbFunctions _, TGeometry geometry, int srid)
        where TGeometry : Geometry
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Transform)));

    /// <summary>
    /// Forces the geometries into a "2-dimensional mode" so that all output representations will only have the X and Y coordinates.
    /// Translates to <c>ST_Force2D(geometry)</c>
    /// </summary>
    /// <remarks>
    /// See https://postgis.net/docs/ST_Force2D.html.
    /// </remarks>
    public static TGeometry Force2D<TGeometry>(this DbFunctions _, TGeometry geometry)
        where TGeometry : Geometry
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Force2D)));

    /// <summary>
    /// Tests whether the distance from the origin geometry to another is less than or equal to a specified value.
    /// Translates to <c>ST_DWithin</c>.
    /// </summary>
    /// <remarks>
    /// See https://postgis.net/docs/ST_DWithin.html.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="geometry">The origin geometry.</param>
    /// <param name="anotherGeometry">The geometry to check the distance to.</param>
    /// <param name="distance">The distance value to compare.</param>
    /// <param name="useSpheroid">Whether to use sphere or spheroid distance measurement.</param>
    /// <returns><see langword="true" /> if the geometries are less than distance apart.</returns>
    public static bool IsWithinDistance(this DbFunctions _, Geometry geometry, Geometry anotherGeometry, double distance, bool useSpheroid)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsWithinDistance)));

    /// <summary>
    /// Returns the minimum distance between the origin geometry and another geometry g.
    /// Translates to <c>ST_Distance</c>.
    /// </summary>
    /// <remarks>
    /// See https://postgis.net/docs/ST_Distance.html.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="geometry">The origin geometry.</param>
    /// <param name="anotherGeometry">The geometry from which to compute the distance.</param>
    /// <param name="useSpheroid">Whether to use sphere or spheroid distance measurement.</param>
    /// <returns>The distance between the geometries.</returns>
    public static double Distance(this DbFunctions _, Geometry geometry, Geometry anotherGeometry, bool useSpheroid)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Distance)));

    /// <summary>
    /// Returns the 2D distance between two geometries. Used in the "ORDER BY" clause, provides index-assisted nearest-neighbor result
    /// sets. Translates to <c>&lt;-&gt;</c>.
    /// </summary>
    /// <remarks>
    /// See https://postgis.net/docs/ST_Distance.html.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="geometry">The origin geometry.</param>
    /// <param name="anotherGeometry">The geometry from which to compute the distance.</param>
    /// <returns>The 2D distance between the geometries.</returns>
    public static double DistanceKnn(this DbFunctions _, Geometry geometry, Geometry anotherGeometry)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DistanceKnn)));
}
