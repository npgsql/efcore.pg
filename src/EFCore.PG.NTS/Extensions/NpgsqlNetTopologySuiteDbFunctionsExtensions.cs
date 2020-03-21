using NetTopologySuite.Geometries;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
    }
}
