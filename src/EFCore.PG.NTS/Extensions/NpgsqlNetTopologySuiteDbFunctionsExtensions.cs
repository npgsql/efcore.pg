using NetTopologySuite.Geometries;
using System;

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
        public static Geometry Transform(this DbFunctions _, Geometry geometry, int srid) =>
            throw new NotSupportedException();
    }
}
