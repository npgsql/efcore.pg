using NetTopologySuite.Geometries;
using ProjNet;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite-specific extension methods on <see cref="DbFunctions"/>.
    /// </summary>
    public static class NpgsqlNetTopologySuiteDbFunctionsExtensions
    {
        static readonly CoordinateSystemServices _coordinateSystemServices =
            new CoordinateSystemServices(
                new CoordinateSystemFactory(),
                new CoordinateTransformationFactory());

        /// <summary>
        /// An implementation of the PostgreSQL ST_Transform operation, which transforms coordinates of a geometry to a different spatial reference system.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="geometry">The Geometry to transform.</param>
        /// <param name="srid">SRID of the new spatial reference system.</param>
        /// <returns>New geometry with its coordinates transformed to the new spatial reference system.</returns>
        public static Geometry Transform(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] Geometry geometry,
            int srid)
        {
            if (geometry is null)
                throw new ArgumentNullException(nameof(geometry));

            var transformation = _coordinateSystemServices.CreateTransformation(geometry.SRID, srid);

            var result = geometry.Copy();
            result.Apply(new MathTransformFilter(transformation.MathTransform));

            return result;
        }

        class MathTransformFilter : ICoordinateSequenceFilter
        {
            readonly MathTransform _transform;

            public MathTransformFilter(MathTransform transform)
                => _transform = transform;

            public bool Done => false;
            public bool GeometryChanged => true;

            public void Filter(CoordinateSequence seq, int i)
            {
                var result = _transform.Transform(
                    new[]
                    {
                        seq.GetOrdinate(i, Ordinate.X),
                        seq.GetOrdinate(i, Ordinate.Y)
                    });
                seq.SetOrdinate(i, Ordinate.X, result[0]);
                seq.SetOrdinate(i, Ordinate.Y, result[1]);
            }
        }
    }
}
