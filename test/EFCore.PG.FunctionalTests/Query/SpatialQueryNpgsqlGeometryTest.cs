using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.LinearReferencing;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

[RequiresPostgis]
public class SpatialQueryNpgsqlGeometryTest
    : SpatialQueryRelationalTestBase<SpatialQueryNpgsqlGeometryTest.SpatialQueryNpgsqlGeometryFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public SpatialQueryNpgsqlGeometryTest(SpatialQueryNpgsqlGeometryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
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
@point='POINT (0.25 0.25)' (DbType = Object)

SELECT p."Id", ST_Contains(p."Polygon", @point) AS "Contains"
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
@lineString='LINESTRING (0.5 -0.5, 0.5 0.5)' (DbType = Object)

SELECT l."Id", ST_Crosses(l."LineString", @lineString) AS "Crosses"
FROM "LineStringEntity" AS l
""");
    }

    public override async Task Difference(bool async)
    {
        await base.Difference(async);

        AssertSql(
            """
@polygon='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_Difference(p."Polygon", @polygon) AS "Difference"
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
    public override Task Distance_constant_srid_4326(bool async)
        => Task.CompletedTask;

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
@point='POINT (0 0)' (DbType = Object)

SELECT p."Id", ST_Equals(p."Point", @point) AS "EqualsTopologically"
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
SELECT p."Id", lower(GeometryType(p."Point")) AS "GeometryType"
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
@polygon='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_Intersection(p."Polygon", @polygon) AS "Intersection"
FROM "PolygonEntity" AS p
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task IntersectsBbox(bool async)
    {
        var polygon = Fixture.GeometryFactory.CreatePolygon([new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0)]);

        await AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, IntersectsBbox = (bool?)EF.Functions.IntersectsBbox(e.Polygon, polygon) }),
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, IntersectsBbox = (e.Polygon == null ? (bool?)null : e.Polygon.EnvelopeInternal.Intersects(polygon.EnvelopeInternal)) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.IntersectsBbox, a.IntersectsBbox);
            });

        AssertSql(
            """
@polygon='POLYGON ((0 0, 1 0, 0 1, 0 0))' (DbType = Object)

SELECT p."Id", p."Polygon" && @polygon AS "IntersectsBbox"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Intersects(bool async)
    {
        await base.Intersects(async);

        AssertSql(
            """
@lineString='LINESTRING (0.5 -0.5, 0.5 0.5)' (DbType = Object)

SELECT l."Id", ST_Intersects(l."LineString", @lineString) AS "Intersects"
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
@point='POINT (0 1)' (DbType = Object)

SELECT p."Id", ST_DWithin(p."Point", @point, 1.0) AS "IsWithinDistance"
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

        await using var context = Fixture.CreateContext();
        var query = context.Set<PolygonEntity>().Where(e => e.Polygon != null).Select(e => e.Polygon.Length);
        var polygonLengths = async ? await query.ToListAsync() : query.ToList();
        Assert.InRange(Assert.Single(polygonLengths), 3.41421356, 3.41421357);

        var polymorphicQuery = context.Set<PolygonEntity>()
            .Where(e => e.Polygon != null)
            .Select(e => ((Geometry)e.Polygon).Length);
        var lengths = async ? await polymorphicQuery.ToListAsync() : polymorphicQuery.ToList();
        Assert.Equal(polygonLengths, lengths);

        AssertSql(
            """
SELECT l."Id", ST_Length(l."LineString") AS "Length"
FROM "LineStringEntity" AS l
""",
            //
            """
SELECT ST_Perimeter(p."Polygon")
FROM "PolygonEntity" AS p
WHERE p."Polygon" IS NOT NULL
""",
            //
            """
SELECT CASE ST_GeometryType(p."Polygon")
    WHEN 'ST_Polygon' THEN ST_Perimeter(p."Polygon")
    WHEN 'ST_MultiPolygon' THEN ST_Perimeter(p."Polygon")
    WHEN 'ST_GeometryCollection' THEN ST_Length(p."Polygon") + ST_Perimeter(p."Polygon")
    ELSE ST_Length(p."Polygon")
END
FROM "PolygonEntity" AS p
WHERE p."Polygon" IS NOT NULL
""");
    }

    [ConditionalTheory]
    [InlineData("POINT(1 2)", 0d)]
    [InlineData("MULTIPOINT((1 1),(2 2))", 0d)]
    [InlineData("LINESTRING(0 0,3 4)", 5d)]
    [InlineData("MULTILINESTRING((0 0,3 4),(0 0,3 0))", 8d)]
    [InlineData("POLYGON((0 0,4 0,4 3,0 3,0 0),(1 1,2 1,2 2,1 2,1 1))", 18d)]
    [InlineData("MULTIPOLYGON(((0 0,4 0,4 3,0 3,0 0)),((5 0,7 0,7 2,5 2,5 0)))", 22d)]
    [InlineData("GEOMETRYCOLLECTION(POINT(1 2),LINESTRING(0 0,3 4),POLYGON((0 0,4 0,4 3,0 3,0 0)))", 19d)]
    [InlineData("GEOMETRYCOLLECTION(GEOMETRYCOLLECTION(LINESTRING(0 0,3 4),POLYGON((0 0,4 0,4 3,0 3,0 0))),MULTILINESTRING((0 0,3 0)))", 22d)]
    [InlineData("GEOMETRYCOLLECTION(MULTIPOLYGON(((0 0,1 0,1 1,0 1,0 0))))", 4d)]
    [InlineData("GEOMETRYCOLLECTION EMPTY", 0d)]
    [InlineData("POLYGON EMPTY", 0d)]
    [InlineData("POLYGON M((0 0 2,4 0 3,4 3 4,0 3 5,0 0 2))", 14d)]
    [InlineData("POLYGON ZM((0 0 1 2,4 0 2 3,4 3 3 4,0 3 4 5,0 0 1 2))", 14d)]
    [InlineData("MULTIPOLYGON M(((0 0 1,1 0 2,1 1 3,0 1 4,0 0 1)))", 4d)]
    [InlineData(null, null)]
    public async Task Length_polymorphic_geometry_types(string wkt, double? expected)
    {
        await using var context = Fixture.CreateContext();
        var query = context.Set<PointEntity>()
            .FromSql($"SELECT ST_GeomFromText({wkt}, 4326) AS \"Geometry\"")
            .Select(e => e.Geometry == null ? (double?)null : e.Geometry.Length);

        Assert.Equal(expected, query.Single());
        Assert.Equal(expected, await query.SingleAsync());

        var parameter = wkt is null ? "NULL (Nullable = false) (DbType = Object)" : $"'{wkt}'";
        var sql =
            $"""
p0={parameter}

SELECT CASE ST_GeometryType(m."Geometry")
    WHEN 'ST_Polygon' THEN ST_Perimeter(m."Geometry")
    WHEN 'ST_MultiPolygon' THEN ST_Perimeter(m."Geometry")
    WHEN 'ST_GeometryCollection' THEN ST_Length(m."Geometry") + ST_Perimeter(m."Geometry")
    ELSE ST_Length(m."Geometry")
END
FROM (
    SELECT ST_GeomFromText(@p0, 4326) AS "Geometry"
) AS m
LIMIT 2
""";
        AssertSql(sql, sql);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Length_multi_polygon_and_collection(bool async)
    {
        await using var context = Fixture.CreateContext();
        var multiPolygon = "MULTIPOLYGON(((0 0,4 0,4 3,0 3,0 0)),((5 0,7 0,7 2,5 2,5 0)))";
        var multiQuery = context.Set<PointEntity>()
            .FromSql($"SELECT ST_GeomFromText({multiPolygon}, 4326) AS \"Geometry\"")
            .Select(e => new
            {
                MultiPolygon = ((MultiPolygon)e.Geometry).Length,
                Collection = ((GeometryCollection)e.Geometry).Length
            });
        var result = async ? await multiQuery.SingleAsync() : multiQuery.Single();
        Assert.Equal(22d, result.MultiPolygon);
        Assert.Equal(22d, result.Collection);

        var collection = "GEOMETRYCOLLECTION(LINESTRING(0 0,3 4),POLYGON((0 0,4 0,4 3,0 3,0 0)))";
        var collectionQuery = context.Set<PointEntity>()
            .FromSql($"SELECT ST_GeomFromText({collection}, 4326) AS \"Geometry\"")
            .Select(e => ((GeometryCollection)e.Geometry).Length);
        Assert.Equal(19d, async ? await collectionQuery.SingleAsync() : collectionQuery.Single());

        AssertSql(
            $"""
p0='{multiPolygon}'

SELECT ST_Perimeter(m."Geometry") AS "MultiPolygon", ST_Length(m."Geometry") + ST_Perimeter(m."Geometry") AS "Collection"
FROM (
    SELECT ST_GeomFromText(@p0, 4326) AS "Geometry"
) AS m
LIMIT 2
""",
            //
            $"""
p0='{collection}'

SELECT ST_Length(m."Geometry") + ST_Perimeter(m."Geometry")
FROM (
    SELECT ST_GeomFromText(@p0, 4326) AS "Geometry"
) AS m
LIMIT 2
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
@polygon='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_Overlaps(p."Polygon", @polygon) AS "Overlaps"
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
@polygon='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_Relate(p."Polygon", @polygon, '212111212') AS "Relate"
FROM "PolygonEntity" AS p
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task SetSrid(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Point = EF.Functions.SetSrid(e.Point, 4326) }),
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Point = e.Point == null ? null : new Point(e.Point.Coordinate) { SRID = 4326 } }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Point, a.Point, GeometryComparer.Instance);
                Assert.Equal(e.Point?.SRID, a.Point?.SRID);
            });

        AssertSql(
            """
SELECT p."Id", ST_SetSRID(p."Point", 4326) AS "Point"
FROM "PointEntity" AS p
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task SetSrid_parameter(bool async)
    {
        var srid = 3857;

        await AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Point = EF.Functions.SetSrid(e.PointZ, srid) }),
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Point = e.PointZ == null ? null : new Point(e.PointZ.Coordinate) { SRID = srid } }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Point, a.Point, GeometryComparer.Instance);
                Assert.Equal(e.Point?.SRID, a.Point?.SRID);
                Assert.Equal(e.Point?.Z, a.Point?.Z);
            });

        AssertSql(
            """
@srid='3857'

SELECT p."Id", ST_SetSRID(p."PointZ", @srid) AS "Point"
FROM "PointEntity" AS p
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
@polygon='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_SymDifference(p."Polygon", @polygon) AS "SymmetricDifference"
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
@polygon='POLYGON ((0 1, 1 0, 1 1, 0 1))' (DbType = Object)

SELECT p."Id", ST_Touches(p."Polygon", @polygon) AS "Touches"
FROM "PolygonEntity" AS p
""");
    }

    public override async Task Union(bool async)
    {
        await base.Union(async);

        AssertSql(
            """
@polygon='POLYGON ((0 0, 1 0, 1 1, 0 0))' (DbType = Object)

SELECT p."Id", ST_Union(p."Polygon", @polygon) AS "Union"
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
@polygon='POLYGON ((-1 -1, 2 -1, 2 2, -1 2, -1 -1))' (DbType = Object)

SELECT p."Id", ST_Within(p."Point", @polygon) AS "Within"
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

    [ConditionalTheory(Skip = "#2850")]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task MultiString_Any(bool async)
    {
        var lineString = Fixture.GeometryFactory.CreateLineString([new Coordinate(1, 0), new Coordinate(1, 1)]);

        // Note the subtle difference between Contains and Any here: Contains resolves to Geometry.Contains, which checks whether a geometry
        // is contained in another; this is different from .NET collection/enumerable Contains, which checks whether an item is in a
        // collection.
        await AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Where(e => e.MultiLineString.Any(ls => ls == lineString)),
            ss => ss.Set<MultiLineStringEntity>().Where(
                e => e.MultiLineString != null && e.MultiLineString.Any(ls => GeometryComparer.Instance.Equals(ls, lineString))),
            elementSorter: e => e.Id);

        AssertSql(
            """
@__lineString_0='LINESTRING (1 0, 1 1)' (DbType = Object)

SELECT m."Id", m."MultiLineString"
FROM "MultiLineStringEntity" AS m
WHERE @__lineString_0 IN (
    SELECT m0.geom
    FROM ST_Dump(m."MultiLineString") AS m0
)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LineInterpolatePoint(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, Point = EF.Functions.LineInterpolatePoint(e.LineString, 0.5) }),
            ss => ss.Set<LineStringEntity>().Select(
                e => new
                {
                    e.Id,
                    Point = e.LineString == null
                        ? null
                        : e.LineString.Factory.CreatePoint(new LengthIndexedLine(e.LineString).ExtractPoint(e.LineString.Length * 0.5))
                }),
            e => e.Id);

        AssertSql(
            """
SELECT l."Id", ST_LineInterpolatePoint(l."LineString", 0.5) AS "Point"
FROM "LineStringEntity" AS l
""");
    }

    [ConditionalTheory]
    [InlineData(false, 0.0)]
    [InlineData(true, 0.0)]
    [InlineData(false, 0.25)]
    [InlineData(true, 0.25)]
    [InlineData(false, 1.0)]
    [InlineData(true, 1.0)]
    public async Task LineInterpolatePoint_parameter(bool async, double fraction)
    {
        var lineString = Fixture.GeometryFactory.CreateLineString([new Coordinate(0, 0), new Coordinate(0, 4), new Coordinate(3, 4)]);
        var expected = lineString.Factory.CreatePoint(new LengthIndexedLine(lineString).ExtractPoint(lineString.Length * fraction));

        await AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, Point = EF.Functions.LineInterpolatePoint(lineString, fraction) }),
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, Point = expected }),
            e => e.Id);

        AssertSql(
            $"""
@lineString='LINESTRING (0 0, 0 4, 3 4)' (DbType = Object)
@fraction='{fraction}'

SELECT l."Id", ST_LineInterpolatePoint(@lineString, @fraction) AS "Point"
FROM "LineStringEntity" AS l
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LineMerge(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Select(e => new { e.Id, LineMerge = EF.Functions.LineMerge(e.MultiLineString) }),
            ss => ss.Set<MultiLineStringEntity>().Select(e => new { e.Id, LineMerge = (Geometry)e.MultiLineString }),
            e => e.Id);

        AssertSql(
            """
SELECT m."Id", ST_LineMerge(m."MultiLineString") AS "LineMerge"
FROM "MultiLineStringEntity" AS m
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LineMerge_connected_parameter(bool async)
    {
        var geometry = Fixture.GeometryFactory.CreateMultiLineString(
        [
            Fixture.GeometryFactory.CreateLineString([new Coordinate(0, 0), new Coordinate(0, 1)]),
            Fixture.GeometryFactory.CreateLineString([new Coordinate(0, 1), new Coordinate(0, 2)])
        ]);
        var expected = Fixture.GeometryFactory.CreateLineString([new Coordinate(0, 0), new Coordinate(0, 1), new Coordinate(0, 2)]);

        await AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Select(e => new { e.Id, LineMerge = EF.Functions.LineMerge(geometry) }),
            ss => ss.Set<MultiLineStringEntity>().Select(e => new { e.Id, LineMerge = (Geometry)expected }),
            e => e.Id);

        AssertSql(
            """
@geometry='MULTILINESTRING ((0 0, 0 1)
(0 1, 0 2))' (DbType = Object)

SELECT m."Id", ST_LineMerge(@geometry) AS "LineMerge"
FROM "MultiLineStringEntity" AS m
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task LineMerge_non_line(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, LineMerge = EF.Functions.LineMerge(e.Point) }),
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, LineMerge = e.Point == null ? null : (Geometry)e.Point.Factory.CreateGeometryCollection() }),
            e => e.Id);

        AssertSql(
            """
SELECT p."Id", ST_LineMerge(p."Point") AS "LineMerge"
FROM "PointEntity" AS p
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Scale(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Scaled = EF.Functions.Scale(e.Point, 2.0, -0.5) }),
            ss => ss.Set<PointEntity>().Select(
                e => new
                {
                    e.Id,
                    Scaled = e.Point == null ? null : (Point)AffineTransformation.ScaleInstance(2.0, -0.5).Transform(e.Point)
                }),
            e => e.Id);

        AssertSql(
            """
SELECT p."Id", ST_Scale(p."Point", 2.0, -0.5) AS "Scaled"
FROM "PointEntity" AS p
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Scale_preserves_Z_and_M(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Scaled = EF.Functions.Scale(e.PointZM, 2.0, -0.5) }),
            ss => ss.Set<PointEntity>().Select(
                e => new
                {
                    e.Id,
                    Scaled = e.PointZM == null
                        ? null
                        : e.PointZM.Factory.CreatePoint(
                            new CoordinateZM(e.PointZM.X * 2, e.PointZM.Y * -0.5, e.PointZM.Z, e.PointZM.M))
                }),
            e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Scaled, a.Scaled, GeometryComparer.Instance);
                Assert.Equal(e.Scaled?.Z, a.Scaled?.Z);
                Assert.Equal(e.Scaled?.M, a.Scaled?.M);
            });

        AssertSql(
            """
SELECT p."Id", ST_Scale(p."PointZM", 2.0, -0.5) AS "Scaled"
FROM "PointEntity" AS p
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Scale_3D(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Scaled = EF.Functions.Scale(e.PointZM, 0.5, 2.0, -4.0) }),
            ss => ss.Set<PointEntity>().Select(
                e => new
                {
                    e.Id,
                    Scaled = e.PointZM == null
                        ? null
                        : e.PointZM.Factory.CreatePoint(
                            new CoordinateZM(e.PointZM.X * 0.5, e.PointZM.Y * 2, e.PointZM.Z * -4, e.PointZM.M))
                }),
            e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Scaled, a.Scaled, GeometryComparer.Instance);
                Assert.Equal(e.Scaled?.Z, a.Scaled?.Z);
                Assert.Equal(e.Scaled?.M, a.Scaled?.M);
            });

        AssertSql(
            """
SELECT p."Id", ST_Scale(p."PointZM", 0.5, 2.0, -4.0) AS "Scaled"
FROM "PointEntity" AS p
""");
    }

    [ConditionalTheory]
    [InlineData(false, 0.0)]
    [InlineData(true, 0.0)]
    [InlineData(false, 0.5)]
    [InlineData(true, 0.5)]
    public async Task Scale_parameters(bool async, double xFactor)
    {
        var geometry = Fixture.GeometryFactory.CreateLineString([new Coordinate(1, 2), new Coordinate(3, 4)]);
        geometry.SRID = 4326;
        var yFactor = -2.0;
        var expected = (LineString)AffineTransformation.ScaleInstance(xFactor, yFactor).Transform(geometry);

        await AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, Scaled = EF.Functions.Scale(geometry, xFactor, yFactor) }),
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, Scaled = expected }),
            e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Scaled, a.Scaled, GeometryComparer.Instance);
                Assert.Equal(e.Scaled.SRID, a.Scaled.SRID);
                Assert.IsType<LineString>(a.Scaled);
            });

        AssertSql(
            $"""
@geometry='LINESTRING (1 2, 3 4)' (DbType = Object)
@xFactor='{xFactor}'
@yFactor='-2'

SELECT l."Id", ST_Scale(@geometry, @xFactor, @yFactor) AS "Scaled"
FROM "LineStringEntity" AS l
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Missing_point_ordinates_not_equal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<PointEntity>()
                .Where(e => e.Point != null && e.Point.Z != 0 && e.Point.M != 0)
                .Select(e => e.Id));

        AssertSql(
            """
SELECT p."Id"
FROM "PointEntity" AS p
WHERE p."Point" IS NOT NULL AND (ST_Z(p."Point") <> 0.0 OR ST_Z(p."Point") IS NULL) AND (ST_M(p."Point") <> 0.0 OR ST_M(p."Point") IS NULL)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Empty_line_endpoints_are_null(bool async)
    {
        await using var context = Fixture.CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                var entity = await context.Set<LineStringEntity>().AsTracking().SingleAsync(e => e.Id == 1);
                entity.LineString = Fixture.GeometryFactory.CreateLineString();
                await context.SaveChangesAsync();
                Fixture.TestSqlLoggerFactory.Clear();

                var query = context.Set<LineStringEntity>()
                    .Where(e => e.LineString != null && e.LineString.StartPoint == null && e.LineString.EndPoint == null)
                    .Select(e => e.Id);

                Assert.Equal(1, async ? await query.SingleAsync() : query.Single());

                AssertSql(
                    """
SELECT l."Id"
FROM "LineStringEntity" AS l
WHERE l."LineString" IS NOT NULL AND ST_StartPoint(l."LineString") IS NULL AND ST_EndPoint(l."LineString") IS NULL
LIMIT 2
""");
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task GetGeometryN_out_of_range_is_null(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>()
                .Where(e => e.MultiLineString != null && e.MultiLineString.GetGeometryN(99) == null)
                .Select(e => e.Id),
            expectedQuery: ss => ss.Set<MultiLineStringEntity>().Where(e => e.MultiLineString != null).Select(e => e.Id));

        AssertSql(
            """
SELECT m."Id"
FROM "MultiLineStringEntity" AS m
WHERE m."MultiLineString" IS NOT NULL AND ST_GeometryN(m."MultiLineString", 100) IS NULL
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Geometry_collection_indexer_out_of_range_is_null(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>()
                .Where(e => e.MultiLineString != null && e.MultiLineString[99] == null)
                .Select(e => e.Id),
            expectedQuery: ss => ss.Set<MultiLineStringEntity>().Where(e => e.MultiLineString != null).Select(e => e.Id));

        AssertSql(
            """
SELECT m."Id"
FROM "MultiLineStringEntity" AS m
WHERE m."MultiLineString" IS NOT NULL AND ST_GeometryN(m."MultiLineString", 100) IS NULL
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

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
