using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class SpatialQueryNpgsqlGeometryTest : SpatialQueryTestBase<SpatialQueryNpgsqlGeometryFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public SpatialQueryNpgsqlGeometryTest(SpatialQueryNpgsqlGeometryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
            => Fixture.TestSqlLoggerFactory.Clear();

        public override async Task Area(bool isAsync)
        {
            await base.Area(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_Area(e.""Polygon"") AS ""Area""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task AsBinary(bool isAsync)
        {
            await base.AsBinary(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_AsBinary(e.""Point"") AS ""Binary""
FROM ""PointEntity"" AS e");
        }

        public override async Task AsText(bool isAsync)
        {
            await base.AsText(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_AsText(e.""Point"") AS ""Text""
FROM ""PointEntity"" AS e");
        }

        public override async Task Boundary(bool isAsync)
        {
            await base.Boundary(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_Boundary(e.""Polygon"") AS ""Boundary""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task Buffer(bool isAsync)
        {
            await base.Buffer(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_Buffer(e.""Polygon"", 1.0) AS ""Buffer""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task Centroid(bool isAsync)
        {
            await base.Centroid(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_Centroid(e.""Polygon"") AS ""Centroid""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task Contains(bool isAsync)
        {
            await base.Contains(isAsync);

            AssertSql(
                @"@__point_0='POINT (0.25 0.25)' (DbType = Object)

SELECT e.""Id"", ST_Contains(e.""Polygon"", @__point_0) AS ""Contains""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task ConvexHull(bool isAsync)
        {
            await base.ConvexHull(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_ConvexHull(e.""Polygon"") AS ""ConvexHull""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task IGeometryCollection_Count(bool isAsync)
        {
            await base.IGeometryCollection_Count(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_NumGeometries(e.""MultiLineString"") AS ""Count""
FROM ""MultiLineStringEntity"" AS e");
        }

        public override async Task LineString_Count(bool isAsync)
        {
            await base.LineString_Count(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_NumPoints(e.""LineString"") AS ""Count""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task Crosses(bool isAsync)
        {
            await base.Crosses(isAsync);

            AssertSql(
                @"@__lineString_0='LINESTRING (0.5 -0.5
0.5 0.5)' (DbType = Object)

SELECT e.""Id"", ST_Crosses(e.""LineString"", @__lineString_0) AS ""Crosses""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task Difference(bool isAsync)
        {
            await base.Difference(isAsync);

            AssertSql(
                @"@__polygon_0='POLYGON ((0 0
1 0
1 1
0 0))' (DbType = Object)

SELECT e.""Id"", ST_Difference(e.""Polygon"", @__polygon_0) AS ""Difference""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task Dimension(bool isAsync)
        {
            await base.Dimension(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_Dimension(e.""Point"") AS ""Dimension""
FROM ""PointEntity"" AS e");
        }

        public override async Task Disjoint(bool isAsync)
        {
            await base.Disjoint(isAsync);

            AssertSql(
                @"@__point_0='POINT (1 1)' (DbType = Object)

SELECT e.""Id"", ST_Disjoint(e.""Polygon"", @__point_0) AS ""Disjoint""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task Distance(bool isAsync)
        {
            await base.Distance(isAsync);

            AssertSql(
                @"@__point_0='POINT (0 1)' (DbType = Object)

SELECT e.""Id"", ST_Distance(e.""Point"", @__point_0) AS ""Distance""
FROM ""PointEntity"" AS e");
        }

        // PostGIS refuses to operate on points of mixed SRIDs
        public override Task Distance_constant_srid_4326(bool isAsync) => Task.CompletedTask;

        public override async Task EndPoint(bool isAsync)
        {
            await base.EndPoint(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_EndPoint(e.""LineString"") AS ""EndPoint""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task Envelope(bool isAsync)
        {
            await base.Envelope(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_Envelope(e.""Polygon"") AS ""Envelope""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task EqualsTopologically(bool isAsync)
        {
            await base.EqualsTopologically(isAsync);

            AssertSql(
                @"@__point_0='POINT (0 0)' (DbType = Object)

SELECT e.""Id"", ST_Equals(e.""Point"", @__point_0) AS ""EqualsTopologically""
FROM ""PointEntity"" AS e");
        }

        public override async Task ExteriorRing(bool isAsync)
        {
            await base.ExteriorRing(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_ExteriorRing(e.""Polygon"") AS ""ExteriorRing""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task GeometryType(bool isAsync)
        {
            // PostGIS returns "POINT", NTS returns "Point"
            await AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new { e.Id, GeometryType = e.Point == null ? null : e.Point.GeometryType.ToLower() }),
                elementSorter: x => x.Id);

            AssertSql(
                @"SELECT e.""Id"", LOWER(GeometryType(e.""Point"")) AS ""GeometryType""
FROM ""PointEntity"" AS e");
        }

        public override async Task GetGeometryN(bool isAsync)
        {
            await base.GetGeometryN(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_GeometryN(e.""MultiLineString"", 1) AS ""Geometry0""
FROM ""MultiLineStringEntity"" AS e");
        }

        public override async Task GetInteriorRingN(bool isAsync)
        {
            await base.GetInteriorRingN(isAsync);

            AssertSql(
                @"SELECT e.""Id"", CASE
    WHEN e.""Polygon"" IS NULL OR (ST_NumInteriorRings(e.""Polygon"") = 0)
    THEN NULL ELSE ST_InteriorRingN(e.""Polygon"", 1)
END AS ""InteriorRing0""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task GetPointN(bool isAsync)
        {
            await base.GetPointN(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_PointN(e.""LineString"", 1) AS ""Point0""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task Intersection(bool isAsync)
        {
            await base.Intersection(isAsync);

            AssertSql(
                @"@__polygon_0='POLYGON ((0 0
1 0
1 1
0 0))' (DbType = Object)

SELECT e.""Id"", ST_Intersection(e.""Polygon"", @__polygon_0) AS ""Intersection""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task Intersects(bool isAsync)
        {
            await base.Intersects(isAsync);

            AssertSql(
                @"@__lineString_0='LINESTRING (0.5 -0.5
0.5 0.5)' (DbType = Object)

SELECT e.""Id"", ST_Intersects(e.""LineString"", @__lineString_0) AS ""Intersects""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task ICurve_IsClosed(bool isAsync)
        {
            await base.ICurve_IsClosed(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_IsClosed(e.""LineString"") AS ""IsClosed""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task IMultiCurve_IsClosed(bool isAsync)
        {
            await base.IMultiCurve_IsClosed(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_IsClosed(e.""MultiLineString"") AS ""IsClosed""
FROM ""MultiLineStringEntity"" AS e");
        }

        public override async Task IsEmpty(bool isAsync)
        {
            await base.IsEmpty(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_IsEmpty(e.""MultiLineString"") AS ""IsEmpty""
FROM ""MultiLineStringEntity"" AS e");
        }

        public override async Task IsRing(bool isAsync)
        {
            await base.IsRing(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_IsRing(e.""LineString"") AS ""IsRing""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task IsSimple(bool isAsync)
        {
            await base.IsSimple(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_IsSimple(e.""LineString"") AS ""IsSimple""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task IsValid(bool isAsync)
        {
            await base.IsValid(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_IsValid(e.""Point"") AS ""IsValid""
FROM ""PointEntity"" AS e");
        }

        public override async Task IsWithinDistance(bool isAsync)
        {
            await base.IsWithinDistance(isAsync);

            AssertSql(
                @"@__point_0='POINT (0 1)' (DbType = Object)

SELECT e.""Id"", ST_DWithin(e.""Point"", @__point_0, 1.0) AS ""IsWithinDistance""
FROM ""PointEntity"" AS e");
        }

        public override async Task Item(bool isAsync)
        {
            await base.Item(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_GeometryN(e.""MultiLineString"", 1) AS ""Item0""
FROM ""MultiLineStringEntity"" AS e");
        }

        public override async Task Length(bool isAsync)
        {
            await base.Length(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_Length(e.""LineString"") AS ""Length""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task M(bool isAsync)
        {
            await base.M(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_M(e.""Point"") AS ""M""
FROM ""PointEntity"" AS e");
        }

        public override async Task NumGeometries(bool isAsync)
        {
            await base.NumGeometries(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_NumGeometries(e.""MultiLineString"") AS ""NumGeometries""
FROM ""MultiLineStringEntity"" AS e");
        }

        public override async Task NumInteriorRings(bool isAsync)
        {
            await base.NumInteriorRings(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_NumInteriorRings(e.""Polygon"") AS ""NumInteriorRings""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task NumPoints(bool isAsync)
        {
            await base.NumPoints(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_NumPoints(e.""LineString"") AS ""NumPoints""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task Overlaps(bool isAsync)
        {
            await base.Overlaps(isAsync);

            AssertSql(
                @"@__polygon_0='POLYGON ((0 0
1 0
1 1
0 0))' (DbType = Object)

SELECT e.""Id"", ST_Overlaps(e.""Polygon"", @__polygon_0) AS ""Overlaps""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task PointOnSurface(bool isAsync)
        {
            await base.PointOnSurface(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_PointOnSurface(e.""Polygon"") AS ""PointOnSurface"", e.""Polygon""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task Relate(bool isAsync)
        {
            await base.Relate(isAsync);

            AssertSql(
                @"@__polygon_0='POLYGON ((0 0
1 0
1 1
0 0))' (DbType = Object)

SELECT e.""Id"", ST_Relate(e.""Polygon"", @__polygon_0, '212111212') AS ""Relate""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task SRID(bool isAsync)
        {
            await base.SRID(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_SRID(e.""Point"") AS ""SRID""
FROM ""PointEntity"" AS e");
        }

        public override async Task StartPoint(bool isAsync)
        {
            await base.StartPoint(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_StartPoint(e.""LineString"") AS ""StartPoint""
FROM ""LineStringEntity"" AS e");
        }

        public override async Task SymmetricDifference(bool isAsync)
        {
            await base.SymmetricDifference(isAsync);

            AssertSql(
                @"@__polygon_0='POLYGON ((0 0
1 0
1 1
0 0))' (DbType = Object)

SELECT e.""Id"", ST_SymDifference(e.""Polygon"", @__polygon_0) AS ""SymmetricDifference""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task ToBinary(bool isAsync)
        {
            await base.ToBinary(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_AsBinary(e.""Point"") AS ""Binary""
FROM ""PointEntity"" AS e");
        }

        public override async Task ToText(bool isAsync)
        {
            await base.ToText(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_AsText(e.""Point"") AS ""Text""
FROM ""PointEntity"" AS e");
        }

        public override async Task Touches(bool isAsync)
        {
            await base.Touches(isAsync);

            AssertSql(
                @"@__polygon_0='POLYGON ((0 1
1 0
1 1
0 1))' (DbType = Object)

SELECT e.""Id"", ST_Touches(e.""Polygon"", @__polygon_0) AS ""Touches""
FROM ""PolygonEntity"" AS e");
        }

        public override async Task Union(bool isAsync)
        {
            await base.Union(isAsync);

            AssertSql(
                @"@__polygon_0='POLYGON ((0 0
1 0
1 1
0 0))' (DbType = Object)

SELECT e.""Id"", ST_Union(e.""Polygon"", @__polygon_0) AS ""Union""
FROM ""PolygonEntity"" AS e");
        }

        [ConditionalTheory(Skip="ST_Union() with only one parameter is an aggregate function in PostGIS")]
        public override Task Union_void(bool isAsync) => null;

        public override async Task Within(bool isAsync)
        {
            await base.Within(isAsync);

            AssertSql(
                @"@__polygon_0='POLYGON ((-1 -1
2 -1
2 2
-1 2
-1 -1))' (DbType = Object)

SELECT e.""Id"", ST_Within(e.""Point"", @__polygon_0) AS ""Within""
FROM ""PointEntity"" AS e");
        }

        public override async Task X(bool isAsync)
        {
            await base.X(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_X(e.""Point"") AS ""X""
FROM ""PointEntity"" AS e");
        }

        public override async Task Y(bool isAsync)
        {
            await base.Y(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_Y(e.""Point"") AS ""Y""
FROM ""PointEntity"" AS e");
        }

        public override async Task Z(bool isAsync)
        {
            await base.Z(isAsync);

            AssertSql(
                @"SELECT e.""Id"", ST_Z(e.""Point"") AS ""Z""
FROM ""PointEntity"" AS e");
        }

        void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
