using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class SpatialQueryNpgsqlGeometryTest : SpatialQueryTestBase<SpatialQueryNpgsqlGeometryFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public SpatialQueryNpgsqlGeometryTest(SpatialQueryNpgsqlGeometryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Area(bool isAsync)
        {
            await base.Area(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_Area(p.""Polygon"") AS ""Area""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task AsBinary(bool isAsync)
        {
            await base.AsBinary(isAsync);

//            AssertSql(
//                @"SELECT p.""Id"", ST_AsBinary(p.""Point"") AS ""Binary""
//FROM ""PointEntity"" AS p");
        }

        public override async Task AsText(bool isAsync)
        {
            await base.AsText(isAsync);

//            AssertSql(
//                @"SELECT p.""Id"", ST_AsText(p.""Point"") AS ""Text""
//FROM ""PointEntity"" AS p");
        }

        public override async Task Boundary(bool isAsync)
        {
            await base.Boundary(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_Boundary(p.""Polygon"") AS ""Boundary""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task Buffer(bool isAsync)
        {
            await base.Buffer(isAsync);

//            AssertSql(
//                @"SELECT p.""Id"", ST_Buffer(p.""Polygon"", 1.0) AS ""Buffer""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task Buffer_quadrantSegments(bool isAsync)
        {
            await base.Buffer_quadrantSegments(isAsync);

//            AssertSql(
//                @"SELECT p.""Id"", ST_Buffer(p.""Polygon"", 1.0, 8) AS ""Buffer""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task Centroid(bool isAsync)
        {
            await base.Centroid(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_Centroid(p.""Polygon"") AS ""Centroid""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task Contains(bool isAsync)
        {
            await base.Contains(isAsync);

//            AssertSql(
//                @"@__point_0='POINT (0.25 0.25)' (DbType = Object)
//
//SELECT p.""Id"", ST_Contains(p.""Polygon"", @__point_0) AS ""Contains""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task ConvexHull(bool isAsync)
        {
            await base.ConvexHull(isAsync);

//            AssertSql(
//                @"SELECT p.""Id"", ST_ConvexHull(p.""Polygon"") AS ""ConvexHull""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task IGeometryCollection_Count(bool isAsync)
        {
            await base.IGeometryCollection_Count(isAsync);

            AssertSql(
                @"SELECT m.""Id"", ST_NumGeometries(m.""MultiLineString"") AS ""Count""
FROM ""MultiLineStringEntity"" AS m");
        }

        public override async Task LineString_Count(bool isAsync)
        {
            await base.LineString_Count(isAsync);

            AssertSql(
                @"SELECT l.""Id"", ST_NumPoints(l.""LineString"") AS ""Count""
FROM ""LineStringEntity"" AS l");
        }

        public override async Task Crosses(bool isAsync)
        {
            await base.Crosses(isAsync);

//            AssertSql(
//                @"@__lineString_0='LINESTRING (0.5 -0.5
//0.5 0.5)' (DbType = Object)
//
//SELECT l.""Id"", ST_Crosses(p.""LineString"", @__lineString_0) AS ""Crosses""
//FROM ""LineStringEntity"" AS p");
        }

        public override async Task Difference(bool isAsync)
        {
            await base.Difference(isAsync);

//            AssertSql(
//                @"@__polygon_0='POLYGON ((0 0
//1 0
//1 1
//0 0))' (DbType = Object)
//
//SELECT p.""Id"", ST_Difference(p.""Polygon"", @__polygon_0) AS ""Difference""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task Dimension(bool isAsync)
        {
            await base.Dimension(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_Dimension(p.""Point"") AS ""Dimension""
FROM ""PointEntity"" AS p");
        }

        public override async Task Disjoint(bool isAsync)
        {
            await base.Disjoint(isAsync);

//            AssertSql(
//                @"@__point_0='POINT (1 1)' (DbType = Object)
//
//SELECT p.""Id"", ST_Disjoint(p.""Polygon"", @__point_0) AS ""Disjoint""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task Distance(bool isAsync)
        {
            await base.Distance(isAsync);

//            AssertSql(
//                @"@__point_0='POINT (0 1)' (DbType = Object)
//
//SELECT p.""Id"", ST_Distance(p.""Point"", @__point_0) AS ""Distance""
//FROM ""PointEntity"" AS p");
        }

        // PostGIS refuses to operate on points of mixed SRIDs
        public override Task Distance_constant_srid_4326(bool isAsync) => Task.CompletedTask;

        public override async Task EndPoint(bool isAsync)
        {
            await base.EndPoint(isAsync);

            AssertSql(
                @"SELECT l.""Id"", ST_EndPoint(l.""LineString"") AS ""EndPoint""
FROM ""LineStringEntity"" AS l");
        }

        public override async Task Envelope(bool isAsync)
        {
            await base.Envelope(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_Envelope(p.""Polygon"") AS ""Envelope""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task EqualsTopologically(bool isAsync)
        {
            await base.EqualsTopologically(isAsync);

//            AssertSql(
//                @"@__point_0='POINT (0 0)' (DbType = Object)
//
//SELECT p.""Id"", ST_Equals(p.""Point"", @__point_0) AS ""EqualsTopologically""
//FROM ""PointEntity"" AS p");
        }

        public override async Task ExteriorRing(bool isAsync)
        {
            await base.ExteriorRing(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_ExteriorRing(p.""Polygon"") AS ""ExteriorRing""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task GeometryType(bool isAsync)
        {
            // PostGIS returns "POINT", NTS returns "Point"
            await AssertQuery(
                isAsync,
                es => es.Set<PointEntity>().Select(
                    e => new { e.Id, GeometryType = e.Point == null ? null : e.Point.GeometryType.ToLower() }),
                x => x.Id);

//            AssertSql(
//                @"SELECT p.""Id"", LOWER(GeometryType(p.""Point"")) AS ""GeometryType""
//FROM ""PointEntity"" AS p");
        }

        public override async Task GetGeometryN(bool isAsync)
        {
            await base.GetGeometryN(isAsync);

//            AssertSql(
//                @"SELECT m.""Id"", ST_GeometryN(m.""MultiLineString"", 1) AS ""Geometry0""
//FROM ""MultiLineStringEntity"" AS m");
        }

        public override async Task GetInteriorRingN(bool isAsync)
        {
            await base.GetInteriorRingN(isAsync);

//            AssertSql(
//                @"SELECT p.""Id"", CASE
//    WHEN p.""Polygon"" IS NULL OR (ST_NumInteriorRings(p.""Polygon"") = 0)
//    THEN NULL ELSE ST_InteriorRingN(p.""Polygon"", 1)
//END AS ""InteriorRing0""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task GetPointN(bool isAsync)
        {
            await base.GetPointN(isAsync);

//            AssertSql(
//                @"SELECT l.""Id"", ST_PointN(l.""LineString"", 1) AS ""Point0""
//FROM ""LineStringEntity"" AS l");
        }

        public override async Task InteriorPoint(bool isAsync)
        {
            await base.InteriorPoint(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_PointOnSurface(p.""Polygon"") AS ""InteriorPoint"", p.""Polygon""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task Intersection(bool isAsync)
        {
            await base.Intersection(isAsync);

//            AssertSql(
//                @"@__polygon_0='POLYGON ((0 0
//1 0
//1 1
//0 0))' (DbType = Object)
//
//SELECT p.""Id"", ST_Intersection(p.""Polygon"", @__polygon_0) AS ""Intersection""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task Intersects(bool isAsync)
        {
            await base.Intersects(isAsync);

//            AssertSql(
//                @"@__lineString_0='LINESTRING (0.5 -0.5
//0.5 0.5)' (DbType = Object)
//
//SELECT l.""Id"", ST_Intersects(l.""LineString"", @__lineString_0) AS ""Intersects""
//FROM ""LineStringEntity"" AS l");
        }

        public override async Task ICurve_IsClosed(bool isAsync)
        {
            await base.ICurve_IsClosed(isAsync);

            AssertSql(
                @"SELECT l.""Id"", ST_IsClosed(l.""LineString"") AS ""IsClosed""
FROM ""LineStringEntity"" AS l");
        }

        public override async Task IMultiCurve_IsClosed(bool isAsync)
        {
            await base.IMultiCurve_IsClosed(isAsync);

            AssertSql(
                @"SELECT m.""Id"", ST_IsClosed(m.""MultiLineString"") AS ""IsClosed""
FROM ""MultiLineStringEntity"" AS m");
        }

        public override async Task IsEmpty(bool isAsync)
        {
            await base.IsEmpty(isAsync);

            AssertSql(
                @"SELECT m.""Id"", ST_IsEmpty(m.""MultiLineString"") AS ""IsEmpty""
FROM ""MultiLineStringEntity"" AS m");
        }

        public override async Task IsRing(bool isAsync)
        {
            await base.IsRing(isAsync);

            AssertSql(
                @"SELECT l.""Id"", ST_IsRing(l.""LineString"") AS ""IsRing""
FROM ""LineStringEntity"" AS l");
        }

        public override async Task IsSimple(bool isAsync)
        {
            await base.IsSimple(isAsync);

            AssertSql(
                @"SELECT l.""Id"", ST_IsSimple(l.""LineString"") AS ""IsSimple""
FROM ""LineStringEntity"" AS l");
        }

        public override async Task IsValid(bool isAsync)
        {
            await base.IsValid(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_IsValid(p.""Point"") AS ""IsValid""
FROM ""PointEntity"" AS p");
        }

        public override async Task IsWithinDistance(bool isAsync)
        {
            await base.IsWithinDistance(isAsync);

//            AssertSql(
//                @"@__point_0='POINT (0 1)' (DbType = Object)
//
//SELECT p.""Id"", ST_DWithin(p.""Point"", @__point_0, 1.0) AS ""IsWithinDistance""
//FROM ""PointEntity"" AS p");
        }

        public override async Task Item(bool isAsync)
        {
            await base.Item(isAsync);

//            AssertSql(
//                @"SELECT m.""Id"", ST_GeometryN(m.""MultiLineString"", 1) AS ""Item0""
//FROM ""MultiLineStringEntity"" AS m");
        }

        public override async Task Length(bool isAsync)
        {
            await base.Length(isAsync);

            AssertSql(
                @"SELECT l.""Id"", ST_Length(l.""LineString"") AS ""Length""
FROM ""LineStringEntity"" AS l");
        }

        public override async Task M(bool isAsync)
        {
            await base.M(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_M(p.""Point"") AS ""M""
FROM ""PointEntity"" AS p");
        }

        public override async Task NumGeometries(bool isAsync)
        {
            await base.NumGeometries(isAsync);

            AssertSql(
                @"SELECT m.""Id"", ST_NumGeometries(m.""MultiLineString"") AS ""NumGeometries""
FROM ""MultiLineStringEntity"" AS m");
        }

        public override async Task NumInteriorRings(bool isAsync)
        {
            await base.NumInteriorRings(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_NumInteriorRings(p.""Polygon"") AS ""NumInteriorRings""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task NumPoints(bool isAsync)
        {
            await base.NumPoints(isAsync);

            AssertSql(
                @"SELECT l.""Id"", ST_NumPoints(l.""LineString"") AS ""NumPoints""
FROM ""LineStringEntity"" AS l");
        }

        public override async Task OgcGeometryType(bool isAsync)
        {
            await base.OgcGeometryType(isAsync);

            AssertSql(
                @"SELECT p.""Id"", CASE ST_GeometryType(p.""Point"")
    WHEN 'ST_CircularString' THEN 8
    WHEN 'ST_CompoundCurve' THEN 9
    WHEN 'ST_CurvePolygon' THEN 10
    WHEN 'ST_GeometryCollection' THEN 7
    WHEN 'ST_LineString' THEN 2
    WHEN 'ST_MultiCurve' THEN 11
    WHEN 'ST_MultiLineString' THEN 5
    WHEN 'ST_MultiPoint' THEN 4
    WHEN 'ST_MultiPolygon' THEN 6
    WHEN 'ST_MultiSurface' THEN 12
    WHEN 'ST_Point' THEN 1
    WHEN 'ST_Polygon' THEN 3
    WHEN 'ST_PolyhedralSurface' THEN 15
    WHEN 'ST_Tin' THEN 16
END AS ""OgcGeometryType""
FROM ""PointEntity"" AS p");
        }

        public override async Task Overlaps(bool isAsync)
        {
            await base.Overlaps(isAsync);

//            AssertSql(
//                @"@__polygon_0='POLYGON ((0 0
//1 0
//1 1
//0 0))' (DbType = Object)
//
//SELECT p.""Id"", ST_Overlaps(p.""Polygon"", @__polygon_0) AS ""Overlaps""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task PointOnSurface(bool isAsync)
        {
            await base.PointOnSurface(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_PointOnSurface(p.""Polygon"") AS ""PointOnSurface"", p.""Polygon""
FROM ""PolygonEntity"" AS p");
        }

        public override async Task Relate(bool isAsync)
        {
            await base.Relate(isAsync);

//            AssertSql(
//                @"@__polygon_0='POLYGON ((0 0
//1 0
//1 1
//0 0))' (DbType = Object)
//
//SELECT p.""Id"", ST_Relate(p.""Polygon"", @__polygon_0, '212111212') AS ""Relate""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task SRID(bool isAsync)
        {
            await base.SRID(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_SRID(p.""Point"") AS ""SRID""
FROM ""PointEntity"" AS p");
        }

        public override async Task StartPoint(bool isAsync)
        {
            await base.StartPoint(isAsync);

            AssertSql(
                @"SELECT l.""Id"", ST_StartPoint(l.""LineString"") AS ""StartPoint""
FROM ""LineStringEntity"" AS l");
        }

        public override async Task SymmetricDifference(bool isAsync)
        {
            await base.SymmetricDifference(isAsync);

//            AssertSql(
//                @"@__polygon_0='POLYGON ((0 0
//1 0
//1 1
//0 0))' (DbType = Object)
//
//SELECT l.""Id"", ST_SymDifference(p.""Polygon"", @__polygon_0) AS ""SymmetricDifference""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task ToBinary(bool isAsync)
        {
            await base.ToBinary(isAsync);

//            AssertSql(
//                @"SELECT p.""Id"", ST_AsBinary(p.""Point"") AS ""Binary""
//FROM ""PointEntity"" AS p");
        }

        public override async Task ToText(bool isAsync)
        {
            await base.ToText(isAsync);

//            AssertSql(
//                @"SELECT p.""Id"", ST_AsText(p.""Point"") AS ""Text""
//FROM ""PointEntity"" AS p");
        }

        public override async Task Touches(bool isAsync)
        {
            await base.Touches(isAsync);

//            AssertSql(
//                @"@__polygon_0='POLYGON ((0 1
//1 0
//1 1
//0 1))' (DbType = Object)
//
//SELECT p.""Id"", ST_Touches(p.""Polygon"", @__polygon_0) AS ""Touches""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task Union(bool isAsync)
        {
            await base.Union(isAsync);

//            AssertSql(
//                @"@__polygon_0='POLYGON ((0 0
//1 0
//1 1
//0 0))' (DbType = Object)
//
//SELECT p.""Id"", ST_Union(p.""Polygon"", @__polygon_0) AS ""Union""
//FROM ""PolygonEntity"" AS p");
        }

        public override async Task Union_void(bool isAsync)
        {
            await base.Union_void(isAsync);

//            AssertSql(
//                @"SELECT m.""Id"", ST_UnaryUnion(m.""MultiLineString"") AS ""Union""
//FROM ""MultiLineStringEntity"" AS m");
        }

        public override async Task Within(bool isAsync)
        {
            await base.Within(isAsync);

//            AssertSql(
//                @"@__polygon_0='POLYGON ((-1 -1
//2 -1
//2 2
//-1 2
//-1 -1))' (DbType = Object)
//
//SELECT p.""Id"", ST_Within(p.""Point"", @__polygon_0) AS ""Within""
//FROM ""PointEntity"" AS p");
        }

        public override async Task X(bool isAsync)
        {
            await base.X(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_X(p.""Point"") AS ""X""
FROM ""PointEntity"" AS p");
        }

        public override async Task Y(bool isAsync)
        {
            await base.Y(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_Y(p.""Point"") AS ""Y""
FROM ""PointEntity"" AS p");
        }

        public override async Task Z(bool isAsync)
        {
            await base.Z(isAsync);

            AssertSql(
                @"SELECT p.""Id"", ST_Z(p.""Point"") AS ""Z""
FROM ""PointEntity"" AS p");
        }

        void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
