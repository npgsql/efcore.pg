using System;

// ReSharper disable once CheckNamespace
namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Provides additional geometry methods which are not in NetTopologySuite.
    /// </summary>
    public static class NpgsqlNetTopologySuiteGeometryExtensions
    {
        /// <summary>
        /// Tests whether the distance from this Geometry to another is less than or equal to a specified value.
        /// </summary>
        /// <param name="geometry">the Geometry to check the distance to.</param>
        /// <param name="anotherGeometry">the distance value to compare.</param>
        /// <param name="distance">whether to use sphere or spheroid distance measurement.</param>
        /// <param name="useSpheroid">true if the geometries are less than distance apart.</param>
        /// <returns>true if the geometries are less than distance apart.</returns>
        public static bool IsWithinDistance(this Geometry geometry, Geometry anotherGeometry, double distance, bool useSpheroid) => default;

        /// <summary>
        /// Returns the minimum distance between this Geometry and another Geometry g.
        /// </summary>
        /// <param name="geometry">the Geometry from which to compute the distance.</param>
        /// <param name="anotherGeometry">whether to use sphere or spheroid distance measurement.</param>
        /// <param name="useSpheroid">the distance between the geometries</param>
        /// <returns>the distance between the geometries</returns>
        /// <exception cref="ArgumentException">if g is null</exception>
        public static double Distance(this Geometry geometry, Geometry anotherGeometry, bool useSpheroid) => default;
    }
}
