using NetTopologySuite.Geometries;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlNetTopologySuiteDbFunctionsExtensions
    {
        /// <summary>
        /// Returns a new geometry with its coordinates transformed to a different spatial reference system.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>ST_Transform(geometry, srid)</c>.
        ///
        /// See https://postgis.net/docs/ST_Transform.html.
        /// </remarks>
        public static TGeometry Transform<TGeometry>(this DbFunctions _, TGeometry geometry, int srid)
            where TGeometry : Geometry
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Transform)));

        /// <summary>
        /// Tests whether the distance from the origin geometry to another is less than or equal to a specified value.
        /// </summary>
        /// <param name="geometry">The origin geometry.</param>
        /// <param name="anotherGeometry">The geometry to check the distance to.</param>
        /// <param name="distance">The distance value to compare.</param>
        /// <param name="useSpheroid">Whether to use sphere or spheroid distance measurement.</param>
        /// <returns>True if the geometries are less than distance apart.</returns>
        public static bool IsWithinDistance(this DbFunctions _, Geometry geometry, Geometry anotherGeometry, double distance, bool useSpheroid)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsWithinDistance)));

        /// <summary>
        /// Returns the minimum distance between the origin geometry and another geometry g.
        /// </summary>
        /// <param name="geometry">The origin geometry.</param>
        /// <param name="anotherGeometry">The geometry from which to compute the distance.</param>
        /// <param name="useSpheroid">Whether to use sphere or spheroid distance measurement.</param>
        /// <returns>The distance between the geometries.</returns>
        /// <exception cref="ArgumentException">If g is null</exception>
        public static double Distance(this DbFunctions _, Geometry geometry, Geometry anotherGeometry, bool useSpheroid)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Distance)));
    }
}
