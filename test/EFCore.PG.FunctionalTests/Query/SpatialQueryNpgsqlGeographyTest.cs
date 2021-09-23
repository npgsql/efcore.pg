using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    // ReSharper disable once UnusedMember.Global
    [RequiresPostgis]
    public class SpatialQueryNpgsqlGeographyTest
        : SpatialQueryRelationalTestBase<SpatialQueryNpgsqlGeographyTest.SpatialQueryNpgsqlGeographyFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public SpatialQueryNpgsqlGeographyTest(SpatialQueryNpgsqlGeographyFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool AssertDistances
            => false;

        public static IEnumerable<object[]> IsAsyncDataAndUseSpheroid = new[]
        {
            new object[] { false, false },
            new object[] { false, true },
            new object[] { true, false },
            new object[] { true, true }
        };

        public override async Task Area(bool async)
        {
            await base.Area(async);

            AssertSql(
                @"SELECT p.""Id"", ST_Area(p.""Polygon"") AS ""Area""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task AsBinary(bool async)
        {
            await base.AsBinary(async);

            AssertSql(
                @"SELECT p.""Id"", ST_AsBinary(p.""Point"") AS ""Binary""
FROM ""PointEntity"" AS p");
        }

        public override async Task AsText(bool async)
        {
            await base.AsText(async);

            AssertSql(
                @"SELECT p.""Id"", ST_AsText(p.""Point"") AS ""Text""
FROM ""PointEntity"" AS p");
        }

        public override async Task Buffer(bool async)
        {
            await base.Buffer(async);

            AssertSql(
                @"SELECT p.""Id"", ST_Buffer(p.""Polygon"", 1.0) AS ""Buffer""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task Buffer_quadrantSegments(bool async)
        {
            await base.Buffer_quadrantSegments(async);

            AssertSql(
                @"SELECT p.""Id"", ST_Buffer(p.""Polygon"", 1.0, 8) AS ""Buffer""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task Centroid(bool async)
        {
            await base.Centroid(async);

            AssertSql(
                @"SELECT p.""Id"", ST_Centroid(p.""Polygon"") AS ""Centroid""
FROM ""PolygonEntity"" AS p");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncDataAndUseSpheroid))]
        public async Task Distance_with_spheroid(bool async, bool useSpheroid)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

            await AssertQuery(
                async,
                ss => ss.Set<PointEntity>().Select(e => new { e.Id, Distance = (double?)EF.Functions.Distance(e.Point, point, useSpheroid) }),
                ss => ss.Set<PointEntity>()
                    .Select(e => new { e.Id, Distance = (e.Point == null ? (double?)null : e.Point.Distance(point)) }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Distance == null, a.Distance == null);
                });

            AssertSql(
                @$"@__point_1='POINT (0 1)' (DbType = Object)
@__useSpheroid_2='{useSpheroid}'

SELECT p.""Id"", ST_Distance(p.""Point"", @__point_1, @__useSpheroid_2) AS ""Distance""
FROM ""PointEntity"" AS p");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task DistanceKnn(bool async)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

            await AssertQuery(
                async,
                ss => ss.Set<PointEntity>().Select(e => new { e.Id, Distance = (double?)EF.Functions.DistanceKnn(e.Point, point) }),
                ss => ss.Set<PointEntity>()
                    .Select(e => new { e.Id, Distance = (e.Point == null ? (double?)null : e.Point.Distance(point)) }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Distance == null, a.Distance == null);
                });

            AssertSql(
                @"@__point_1='POINT (0 1)' (DbType = Object)

SELECT p.""Id"", p.""Point"" <-> @__point_1 AS ""Distance""
FROM ""PointEntity"" AS p");
        }

        public override async Task GeometryType(bool async)
        {
            // PostGIS returns "POINT", NTS returns "Point"
            await AssertQuery(
                async,
                es => es.Set<PointEntity>().Select(
                    e => new { e.Id, GeometryType = e.Point == null ? null : e.Point.GeometryType.ToLower() }),
                x => x.Id);

            AssertSql(
                @"SELECT p.""Id"", CASE
    WHEN (p.""Point"" IS NULL) THEN NULL
    ELSE lower(GeometryType(p.""Point""))
END AS ""GeometryType""
FROM ""PointEntity"" AS p");
        }

        public override async Task Intersection(bool async)
        {
            await base.Intersection(async);

            AssertSql(
                @"@__polygon_0='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p.""Id"", ST_Intersection(p.""Polygon"", @__polygon_0) AS ""Intersection""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task Intersects(bool async)
        {
            await base.Intersects(async);

            AssertSql(
                @"@__lineString_0='LINESTRING (0.5 -0.5, 0.5 0.5)' (DbType = Object)

SELECT l.""Id"", ST_Intersects(l.""LineString"", @__lineString_0) AS ""Intersects""
FROM ""LineStringEntity"" AS l");
        }

        public override async Task IsWithinDistance(bool async)
        {
            await base.IsWithinDistance(async);

            AssertSql(
                @"@__point_0='POINT (0 1)' (DbType = Object)

SELECT p.""Id"", ST_DWithin(p.""Point"", @__point_0, 1.0) AS ""IsWithinDistance""
FROM ""PointEntity"" AS p");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncDataAndUseSpheroid))]
        public async Task IsWithinDistance_with_spheroid(bool async, bool useSpheroid)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

            await AssertQuery(
                async,
                ss => ss.Set<PointEntity>().Select(e => new { e.Id, IsWithinDistance = (bool?)EF.Functions.IsWithinDistance(e.Point, point, 1, useSpheroid) }),
                ss => ss.Set<PointEntity>().Select(
                    e => new { e.Id, IsWithinDistance = e.Point == null ? (bool?)null : e.Point.IsWithinDistance(point, 1) }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    if (e.IsWithinDistance == null)
                    {
                        Assert.False(a.IsWithinDistance ?? false);
                    }
                    else
                    {
                        Assert.NotNull(a.IsWithinDistance);
                    }
                });

            AssertSql(
                @$"@__point_1='POINT (0 1)' (DbType = Object)
@__useSpheroid_2='{useSpheroid}'

SELECT p.""Id"", ST_DWithin(p.""Point"", @__point_1, 1.0, @__useSpheroid_2) AS ""IsWithinDistance""
FROM ""PointEntity"" AS p"
            );
        }

        public override async Task Length(bool async)
        {
            await base.Length(async);

            AssertSql(
                @"SELECT l.""Id"", ST_Length(l.""LineString"") AS ""Length""
FROM ""LineStringEntity"" AS l");
        }

        public override async Task SRID(bool async)
        {
            await base.SRID(async);

            AssertSql(
                @"SELECT p.""Id"", ST_SRID(p.""Point"") AS ""SRID""
FROM ""PointEntity"" AS p");
        }

        public override async Task ToBinary(bool async)
        {
            await base.ToBinary(async);

            AssertSql(
                @"SELECT p.""Id"", ST_AsBinary(p.""Point"") AS ""Binary""
FROM ""PointEntity"" AS p");
        }

        public override async Task ToText(bool async)
        {
            await base.ToText(async);

            AssertSql(
                @"SELECT p.""Id"", ST_AsText(p.""Point"") AS ""Text""
FROM ""PointEntity"" AS p");
        }

        #region Not supported on geography

        public override Task Boundary(bool async)                        => Task.CompletedTask;
        public override Task Contains(bool async)                        => Task.CompletedTask;
        public override Task ConvexHull(bool async)                      => Task.CompletedTask;
        public override Task Crosses(bool async)                         => Task.CompletedTask;
        public override Task Difference(bool async)                      => Task.CompletedTask;
        public override Task Dimension(bool async)                       => Task.CompletedTask;
        public override Task Disjoint_with_cast_to_nullable(bool async)  => Task.CompletedTask;
        public override Task Disjoint_with_null_check(bool async)        => Task.CompletedTask;
        public override Task EndPoint(bool async)                        => Task.CompletedTask;
        public override Task Envelope(bool async)                        => Task.CompletedTask;
        public override Task EqualsTopologically(bool async)             => Task.CompletedTask;
        public override Task ExteriorRing(bool async)                    => Task.CompletedTask;
        public override Task GetGeometryN(bool async)                    => Task.CompletedTask;
        public override Task GetGeometryN_with_null_argument(bool async) => Task.CompletedTask;
        public override Task GetInteriorRingN(bool async)                => Task.CompletedTask;
        public override Task GetPointN(bool async)                       => Task.CompletedTask;
        public override Task ICurve_IsClosed(bool async)                 => Task.CompletedTask;
        public override Task IGeometryCollection_Count(bool async)       => Task.CompletedTask;
        public override Task IMultiCurve_IsClosed(bool async)            => Task.CompletedTask;
        public override Task IsEmpty(bool async)                         => Task.CompletedTask;
        public override Task IsEmpty_equal_to_null(bool async)           => Task.CompletedTask;
        public override Task IsEmpty_not_equal_to_null(bool async)       => Task.CompletedTask;
        public override Task IsRing(bool async)                          => Task.CompletedTask;
        public override Task IsSimple(bool async)                        => Task.CompletedTask;
        public override Task IsValid(bool async)                         => Task.CompletedTask;
        public override Task Item(bool async)                            => Task.CompletedTask;
        public override Task InteriorPoint(bool async)                   => Task.CompletedTask;
        public override Task LineString_Count(bool async)                => Task.CompletedTask;
        public override Task M(bool async)                               => Task.CompletedTask;
        public override Task Normalized(bool async)                      => Task.CompletedTask;
        public override Task NumGeometries(bool async)                   => Task.CompletedTask;
        public override Task NumInteriorRings(bool async)                => Task.CompletedTask;
        public override Task NumPoints(bool async)                       => Task.CompletedTask;
        public override Task OgcGeometryType(bool async)                 => Task.CompletedTask;
        public override Task Overlaps(bool async)                        => Task.CompletedTask;
        public override Task PointOnSurface(bool async)                  => Task.CompletedTask;
        public override Task Relate(bool async)                          => Task.CompletedTask;
        public override Task Reverse(bool async)                         => Task.CompletedTask;
        public override Task StartPoint(bool async)                      => Task.CompletedTask;
        public override Task SymmetricDifference(bool async)             => Task.CompletedTask;
        public override Task Touches(bool async)                         => Task.CompletedTask;
        public override Task Union(bool async)                           => Task.CompletedTask;
        public override Task Union_void(bool async)                      => Task.CompletedTask;
        public override Task Within(bool async)                          => Task.CompletedTask;
        public override Task X(bool async)                               => Task.CompletedTask;
        public override Task Y(bool async)                               => Task.CompletedTask;
        public override Task XY_with_collection_join(bool async)         => Task.CompletedTask;
        public override Task Z(bool async)                               => Task.CompletedTask;

        #endregion

        private void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class SpatialQueryNpgsqlGeographyFixture : SpatialQueryNpgsqlFixture
        {
            protected override string StoreName => "SpatialQueryGeographyTest";

            private NtsGeometryServices _geometryServices;
            private GeometryFactory _geometryFactory;

            public NtsGeometryServices GeometryServices
                => LazyInitializer.EnsureInitialized(
                    ref _geometryServices,
                    () => new NtsGeometryServices(
                        NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory,
                        NtsGeometryServices.Instance.DefaultPrecisionModel,
                        4326));

            public override GeometryFactory GeometryFactory
                => LazyInitializer.EnsureInitialized(
                    ref _geometryFactory,
                    () => GeometryServices.CreateGeometryFactory());

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var optionsBuilder = base.AddOptions(builder);
                new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite(null, null, Ordinates.None, true);

                return optionsBuilder;
            }
        }
    }
}
