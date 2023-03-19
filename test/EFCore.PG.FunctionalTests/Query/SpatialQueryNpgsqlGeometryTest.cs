using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

[RequiresPostgis]
public class SpatialQueryNpgsqlGeometryTest
    : SpatialQueryRelationalTestBase<SpatialQueryNpgsqlGeometryTest.SpatialQueryNpgsqlGeometryFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public SpatialQueryNpgsqlGeometryTest(SpatialQueryNpgsqlGeometryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Area(bool async)
    {
        await base.Area(async);

        AssertSql(
"""
SELECT p."Id", ST_Area(p."Polygon") AS "Area"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task AsBinary(bool async)
    {
        await base.AsBinary(async);

        AssertSql(
"""
SELECT p."Id", ST_AsBinary(p."Point") AS "Binary"
FROM "PointEntity" AS p
""");
    }

    public override async Task AsText(bool async)
    {
        await base.AsText(async);

        AssertSql(
"""
SELECT p."Id", ST_AsText(p."Point") AS "Text"
FROM "PointEntity" AS p
""");
    }

    public override async Task Boundary(bool async)
    {
        await base.Boundary(async);

        AssertSql(
"""
SELECT p."Id", ST_Boundary(p."Polygon") AS "Boundary"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Buffer(bool async)
    {
        await base.Buffer(async);

        AssertSql(
"""
SELECT p."Id", ST_Buffer(p."Polygon", 1.0) AS "Buffer"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Buffer_quadrantSegments(bool async)
    {
        await base.Buffer_quadrantSegments(async);

        AssertSql(
"""
SELECT p."Id", ST_Buffer(p."Polygon", 1.0, 8) AS "Buffer"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Centroid(bool async)
    {
        await base.Centroid(async);

        AssertSql(
"""
SELECT p."Id", ST_Centroid(p."Polygon") AS "Centroid"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Combine_aggregate(bool async)
    {
        await base.Combine_aggregate(async);

        AssertSql(
"""
SELECT p."Group" AS "Id", ST_Collect(p."Point") AS "Combined"
FROM "PointEntity" AS p
WHERE p."Point" IS NOT NULL
GROUP BY p."Group"
""");
    }

    public override async Task EnvelopeCombine_aggregate(bool async)
    {
        await base.EnvelopeCombine_aggregate(async);

        AssertSql(
"""
SELECT p."Group" AS "Id", ST_Extent(p."Point")::geometry AS "Combined"
FROM "PointEntity" AS p
WHERE p."Point" IS NOT NULL
GROUP BY p."Group"
""");
    }

    public override async Task Contains(bool async)
    {
        await base.Contains(async);

        AssertSql(
"""
@__point_0='POINT (0.25 0.25)' (DbType = Object)

SELECT p."Id", ST_Contains(p."Polygon", @__point_0) AS "Contains"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task ConvexHull(bool async)
    {
        await base.ConvexHull(async);

        AssertSql(
"""
SELECT p."Id", ST_ConvexHull(p."Polygon") AS "ConvexHull"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task ConvexHull_aggregate(bool async)
    {
        await base.ConvexHull_aggregate(async);

        AssertSql(
"""
SELECT p."Group" AS "Id", ST_ConvexHull(ST_Collect(p."Point")) AS "ConvexHull"
FROM "PointEntity" AS p
WHERE p."Point" IS NOT NULL
GROUP BY p."Group"
""");
    }

    public override async Task IGeometryCollection_Count(bool async)
    {
        await base.IGeometryCollection_Count(async);

        AssertSql(
"""
SELECT m."Id", ST_NumGeometries(m."MultiLineString") AS "Count"
FROM "MultiLineStringEntity" AS m
""");
    }

    public override async Task LineString_Count(bool async)
    {
        await base.LineString_Count(async);

        AssertSql(
"""
SELECT l."Id", ST_NumPoints(l."LineString") AS "Count"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task Crosses(bool async)
    {
        await base.Crosses(async);

        AssertSql(
"""
@__lineString_0='LINESTRING (0.5 -0.5, 0.5 0.5)' (DbType = Object)

SELECT l."Id", ST_Crosses(l."LineString", @__lineString_0) AS "Crosses"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task Difference(bool async)
    {
        await base.Difference(async);

        AssertSql(
"""
@__polygon_0='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_Difference(p."Polygon", @__polygon_0) AS "Difference"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Dimension(bool async)
    {
        await base.Dimension(async);

        AssertSql(
"""
SELECT p."Id", ST_Dimension(p."Point") AS "Dimension"
FROM "PointEntity" AS p
""");
    }

    // PostGIS refuses to operate on points of mixed SRIDs
    public override Task Distance_constant_srid_4326(bool async) => Task.CompletedTask;

    public override async Task EndPoint(bool async)
    {
        await base.EndPoint(async);

        AssertSql(
"""
SELECT l."Id", ST_EndPoint(l."LineString") AS "EndPoint"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task Envelope(bool async)
    {
        await base.Envelope(async);

        AssertSql(
"""
SELECT p."Id", ST_Envelope(p."Polygon") AS "Envelope"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task EqualsTopologically(bool async)
    {
        await base.EqualsTopologically(async);

        AssertSql(
"""
@__point_0='POINT (0 0)' (DbType = Object)

SELECT p."Id", ST_Equals(p."Point", @__point_0) AS "EqualsTopologically"
FROM "PointEntity" AS p
""");
    }

    public override async Task ExteriorRing(bool async)
    {
        await base.ExteriorRing(async);

        AssertSql(
"""
SELECT p."Id", ST_ExteriorRing(p."Polygon") AS "ExteriorRing"
FROM "PolygonEntity" AS p
""");
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
"""
SELECT p."Id", CASE
    WHEN p."Point" IS NULL THEN NULL
    ELSE lower(GeometryType(p."Point"))
END AS "GeometryType"
FROM "PointEntity" AS p
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Force2D(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Force2D = EF.Functions.Force2D(e.PointZ) }),
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Force2D = e.PointZ == null ? null : new Point(e.PointZ.X, e.PointZ.Y) }),
            x => x.Id);

        AssertSql(
"""
SELECT p."Id", ST_Force2D(p."PointZ") AS "Force2D"
FROM "PointEntity" AS p
""");
    }

    public override async Task GetGeometryN(bool async)
    {
        await base.GetGeometryN(async);

        AssertSql(
"""
SELECT m."Id", ST_GeometryN(m."MultiLineString", 1) AS "Geometry0"
FROM "MultiLineStringEntity" AS m
""");
    }

    public override async Task GetInteriorRingN(bool async)
    {
        await base.GetInteriorRingN(async);

        AssertSql(
"""
SELECT p."Id", CASE
    WHEN ST_NumInteriorRings(p."Polygon") = 0 THEN NULL
    ELSE ST_InteriorRingN(p."Polygon", 1)
END AS "InteriorRing0"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task GetPointN(bool async)
    {
        await base.GetPointN(async);

        AssertSql(
"""
SELECT l."Id", ST_PointN(l."LineString", 1) AS "Point0"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task InteriorPoint(bool async)
    {
        await base.InteriorPoint(async);

        AssertSql(
"""
SELECT p."Id", ST_PointOnSurface(p."Polygon") AS "InteriorPoint", p."Polygon"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Intersection(bool async)
    {
        await base.Intersection(async);

        AssertSql(
"""
@__polygon_0='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_Intersection(p."Polygon", @__polygon_0) AS "Intersection"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Intersects(bool async)
    {
        await base.Intersects(async);

        AssertSql(
"""
@__lineString_0='LINESTRING (0.5 -0.5, 0.5 0.5)' (DbType = Object)

SELECT l."Id", ST_Intersects(l."LineString", @__lineString_0) AS "Intersects"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task ICurve_IsClosed(bool async)
    {
        await base.ICurve_IsClosed(async);

        AssertSql(
"""
SELECT l."Id", ST_IsClosed(l."LineString") AS "IsClosed"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task IMultiCurve_IsClosed(bool async)
    {
        await base.IMultiCurve_IsClosed(async);

        AssertSql(
"""
SELECT m."Id", ST_IsClosed(m."MultiLineString") AS "IsClosed"
FROM "MultiLineStringEntity" AS m
""");
    }

    public override async Task IsEmpty(bool async)
    {
        await base.IsEmpty(async);

        AssertSql(
"""
SELECT m."Id", ST_IsEmpty(m."MultiLineString") AS "IsEmpty"
FROM "MultiLineStringEntity" AS m
""");
    }

    public override async Task IsRing(bool async)
    {
        await base.IsRing(async);

        AssertSql(
"""
SELECT l."Id", ST_IsRing(l."LineString") AS "IsRing"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task IsSimple(bool async)
    {
        await base.IsSimple(async);

        AssertSql(
"""
SELECT l."Id", ST_IsSimple(l."LineString") AS "IsSimple"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task IsValid(bool async)
    {
        await base.IsValid(async);

        AssertSql(
"""
SELECT p."Id", ST_IsValid(p."Point") AS "IsValid"
FROM "PointEntity" AS p
""");
    }

    public override async Task IsWithinDistance(bool async)
    {
        await base.IsWithinDistance(async);

        AssertSql(
"""
@__point_0='POINT (0 1)' (DbType = Object)

SELECT p."Id", ST_DWithin(p."Point", @__point_0, 1.0) AS "IsWithinDistance"
FROM "PointEntity" AS p
""");
    }

    public override async Task Item(bool async)
    {
        await base.Item(async);

        AssertSql(
"""
SELECT m."Id", ST_GeometryN(m."MultiLineString", 1) AS "Item0"
FROM "MultiLineStringEntity" AS m
""");
    }

    public override async Task Length(bool async)
    {
        await base.Length(async);

        AssertSql(
"""
SELECT l."Id", ST_Length(l."LineString") AS "Length"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task M(bool async)
    {
        await base.M(async);

        AssertSql(
"""
SELECT p."Id", ST_M(p."Point") AS "M"
FROM "PointEntity" AS p
""");
    }

    public override async Task NumGeometries(bool async)
    {
        await base.NumGeometries(async);

        AssertSql(
"""
SELECT m."Id", ST_NumGeometries(m."MultiLineString") AS "NumGeometries"
FROM "MultiLineStringEntity" AS m
""");
    }

    public override async Task NumInteriorRings(bool async)
    {
        await base.NumInteriorRings(async);

        AssertSql(
"""
SELECT p."Id", ST_NumInteriorRings(p."Polygon") AS "NumInteriorRings"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task NumPoints(bool async)
    {
        await base.NumPoints(async);

        AssertSql(
"""
SELECT l."Id", ST_NumPoints(l."LineString") AS "NumPoints"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task OgcGeometryType(bool async)
    {
        await base.OgcGeometryType(async);

        AssertSql(
"""
SELECT p."Id", CASE ST_GeometryType(p."Point")
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
END AS "OgcGeometryType"
FROM "PointEntity" AS p
""");
    }

    public override async Task Overlaps(bool async)
    {
        await base.Overlaps(async);

        AssertSql(
"""
@__polygon_0='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_Overlaps(p."Polygon", @__polygon_0) AS "Overlaps"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task PointOnSurface(bool async)
    {
        await base.PointOnSurface(async);

        AssertSql(
"""
SELECT p."Id", ST_PointOnSurface(p."Polygon") AS "PointOnSurface", p."Polygon"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Relate(bool async)
    {
        await base.Relate(async);

        AssertSql(
"""
@__polygon_0='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_Relate(p."Polygon", @__polygon_0, '212111212') AS "Relate"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task SRID(bool async)
    {
        await base.SRID(async);

        AssertSql(
"""
SELECT p."Id", ST_SRID(p."Point") AS "SRID"
FROM "PointEntity" AS p
""");
    }

    public override async Task StartPoint(bool async)
    {
        await base.StartPoint(async);

        AssertSql(
"""
SELECT l."Id", ST_StartPoint(l."LineString") AS "StartPoint"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task SymmetricDifference(bool async)
    {
        await base.SymmetricDifference(async);

        AssertSql(
"""
@__polygon_0='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_SymDifference(p."Polygon", @__polygon_0) AS "SymmetricDifference"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task ToBinary(bool async)
    {
        await base.ToBinary(async);

        AssertSql(
"""
SELECT p."Id", ST_AsBinary(p."Point") AS "Binary"
FROM "PointEntity" AS p
""");
    }

    public override async Task ToText(bool async)
    {
        await base.ToText(async);

        AssertSql(
"""
SELECT p."Id", ST_AsText(p."Point") AS "Text"
FROM "PointEntity" AS p
""");
    }

    public override async Task Touches(bool async)
    {
        await base.Touches(async);

        AssertSql(
"""
@__polygon_0='POLYGON ((0 1, 1 0, 1 1, 0 1))' (DbType = Object)

SELECT p."Id", ST_Touches(p."Polygon", @__polygon_0) AS "Touches"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Union(bool async)
    {
        await base.Union(async);

        AssertSql(
"""
@__polygon_0='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_Union(p."Polygon", @__polygon_0) AS "Union"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Union_aggregate(bool async)
    {
        await base.Union_aggregate(async);

        AssertSql(
"""
SELECT p."Group" AS "Id", ST_Union(p."Point") AS "Union"
FROM "PointEntity" AS p
WHERE p."Point" IS NOT NULL
GROUP BY p."Group"
""");
    }

    public override async Task Union_void(bool async)
    {
        await base.Union_void(async);

        AssertSql(
"""
SELECT m."Id", ST_UnaryUnion(m."MultiLineString") AS "Union"
FROM "MultiLineStringEntity" AS m
""");
    }

    public override async Task Within(bool async)
    {
        await base.Within(async);

        AssertSql(
"""
@__polygon_0='POLYGON ((-1 -1, 2 -1, 2 2, -1 2, -1 -1))' (DbType = Object)

SELECT p."Id", ST_Within(p."Point", @__polygon_0) AS "Within"
FROM "PointEntity" AS p
""");
    }

    public override async Task X(bool async)
    {
        await base.X(async);

        AssertSql(
"""
SELECT p."Id", ST_X(p."Point") AS "X"
FROM "PointEntity" AS p
""");
    }

    public override async Task Y(bool async)
    {
        await base.Y(async);

        AssertSql(
"""
SELECT p."Id", ST_Y(p."Point") AS "Y"
FROM "PointEntity" AS p
""");
    }

    public override async Task Z(bool async)
    {
        await base.Z(async);

        AssertSql(
"""
SELECT p."Id", ST_Z(p."Point") AS "Z"
FROM "PointEntity" AS p
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task MultiString_Any(bool async)
    {
        var lineString = Fixture.GeometryFactory.CreateLineString(new[] { new Coordinate(1, 0), new Coordinate(1, 1) });

        // Note the subtle difference between Contains any Any here: Contains resolves to Geometry.Contains, which checks whether a geometry
        // is contained in another; this is different from .NET collection/enumerable Contains, which checks whether an item is in a
        // collection.
        await AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Where(e => e.MultiLineString.Any(ls => ls == lineString)),
            ss => ss.Set<MultiLineStringEntity>().Where(e => e.MultiLineString != null && e.MultiLineString.Any(ls => GeometryComparer.Instance.Equals(ls, lineString))),
            elementSorter: e => e.Id);

        AssertSql(
"""
@__lineString_0='LINESTRING (1 0, 1 1)' (DbType = Object)

SELECT m."Id", m."MultiLineString"
FROM "MultiLineStringEntity" AS m
WHERE EXISTS (
    SELECT 1
    FROM ST_Dump(m."MultiLineString") AS m0
    WHERE m0.geom = @__lineString_0)
""");
    }

    private void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class SpatialQueryNpgsqlGeometryFixture : SpatialQueryNpgsqlFixture
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);
            new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite();

            return optionsBuilder;
        }
    }
}
