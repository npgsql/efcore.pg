using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

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
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override bool AssertDistances
        => false;

    public static IEnumerable<object[]> IsAsyncDataAndUseSpheroid =
    [
        [false, false], [false, true], [true, false], [true, true]
    ];

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
                Assert.Equal(e.Distance is null, a.Distance is null);
            });

        AssertSql(
            $"""
@point='POINT (0 1)' (DbType = Object)
@useSpheroid='{useSpheroid}'

SELECT p."Id", ST_Distance(p."Point", @point, @useSpheroid) AS "Distance"
FROM "PointEntity" AS p
""");
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
                Assert.Equal(e.Distance is null, a.Distance is null);
            });

        AssertSql(
            """
@point='POINT (0 1)' (DbType = Object)

SELECT p."Id", p."Point" <-> @point AS "Distance"
FROM "PointEntity" AS p
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncDataAndUseSpheroid))]
    public async Task IsWithinDistance_with_spheroid(bool async, bool useSpheroid)
    {
        var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

        await AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, IsWithinDistance = (bool?)EF.Functions.IsWithinDistance(e.Point, point, 1, useSpheroid) }),
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, IsWithinDistance = e.Point == null ? (bool?)null : e.Point.IsWithinDistance(point, 1) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.IsWithinDistance is null)
                {
                    Assert.False(a.IsWithinDistance ?? false);
                }
                else
                {
                    Assert.NotNull(a.IsWithinDistance);
                }
            });

        AssertSql(
            $"""
@point='POINT (0 1)' (DbType = Object)
@useSpheroid='{useSpheroid}'

SELECT p."Id", ST_DWithin(p."Point", @point, 1.0, @useSpheroid) AS "IsWithinDistance"
FROM "PointEntity" AS p
""");
    }

    public override async Task Length(bool async)
    {
        await base.Length(async);

        await using var context = Fixture.CreateContext();
        var query = context.Set<PolygonEntity>().Where(e => e.Polygon != null).Select(e => e.Polygon.Length);
        var polygonLengths = async ? await query.ToListAsync() : query.ToList();
        Assert.InRange(Assert.Single(polygonLengths), 378_792, 378_795);

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
SELECT CASE GeometryType(p."Polygon")
    WHEN 'POLYGON' THEN ST_Perimeter(p."Polygon")
    WHEN 'POLYGONM' THEN ST_Perimeter(p."Polygon")
    WHEN 'MULTIPOLYGON' THEN ST_Perimeter(p."Polygon")
    WHEN 'MULTIPOLYGONM' THEN ST_Perimeter(p."Polygon")
    WHEN 'GEOMETRYCOLLECTION' THEN ST_Length(p."Polygon") + ST_Perimeter(p."Polygon")
    WHEN 'GEOMETRYCOLLECTIONM' THEN ST_Length(p."Polygon") + ST_Perimeter(p."Polygon")
    ELSE ST_Length(p."Polygon")
END
FROM "PolygonEntity" AS p
WHERE p."Polygon" IS NOT NULL
""");
    }

    [ConditionalTheory]
    [InlineData("POINT(1 2)", 0d)]
    [InlineData("MULTIPOINT((1 1),(2 2))", 0d)]
    [InlineData("LINESTRING(0 0,3 4)", 554058.92)]
    [InlineData("MULTILINESTRING((0 0,3 4),(0 0,3 0))", 888017.40)]
    [InlineData("POLYGON((0 0,4 0,4 3,0 3,0 0),(1 1,2 1,2 2,1 2,1 1))", 1997106.17)]
    [InlineData("MULTIPOLYGON(((0 0,4 0,4 3,0 3,0 0)),((5 0,7 0,7 2,5 2,5 0)))", 2440843.40)]
    [InlineData("GEOMETRYCOLLECTION(POINT(1 2),LINESTRING(0 0,3 4),POLYGON((0 0,4 0,4 3,0 3,0 0)))", 2107460.18)]
    [InlineData("GEOMETRYCOLLECTION(GEOMETRYCOLLECTION(LINESTRING(0 0,3 4),POLYGON((0 0,4 0,4 3,0 3,0 0))),MULTILINESTRING((0 0,3 0)))", 2441418.65)]
    [InlineData("GEOMETRYCOLLECTION(MULTIPOLYGON(((0 0,1 0,1 1,0 1,0 0))))", 443770.92)]
    [InlineData("GEOMETRYCOLLECTION M(LINESTRING M(0 0 1,3 4 2),POLYGON M((0 0 1,4 0 2,4 3 3,0 3 4,0 0 1)))", 2107460.18)]
    [InlineData("GEOMETRYCOLLECTION EMPTY", 0d)]
    [InlineData("POLYGON EMPTY", 0d)]
    [InlineData("POLYGON M((0 0 2,4 0 3,4 3 4,0 3 5,0 0 2))", 1553401.26)]
    [InlineData("POLYGON ZM((0 0 1 2,4 0 2 3,4 3 3 4,0 3 4 5,0 0 1 2))", 1553401.26)]
    [InlineData("MULTIPOLYGON M(((0 0 1,1 0 2,1 1 3,0 1 4,0 0 1)))", 443770.92)]
    [InlineData(null, null)]
    public async Task Length_polymorphic_geography_types(string wkt, double? expected)
    {
        await using var context = Fixture.CreateContext();
        var query = context.Set<PointEntity>()
            .FromSql($"SELECT ST_GeogFromText({wkt}) AS \"Geometry\"")
            .Select(e => e.Geometry == null ? (double?)null : e.Geometry.Length);

        var result = query.Single();
        Assert.Equal(result, await query.SingleAsync());
        if (expected is null or 0)
        {
            Assert.Equal(expected, result);
        }
        else
        {
            Assert.InRange(result.Value, expected.Value - 1, expected.Value + 1);
        }

        var parameter = wkt is null ? "NULL (Nullable = false) (DbType = Object)" : $"'{wkt}'";
        var sql =
            $"""
p0={parameter}

SELECT CASE GeometryType(m."Geometry")
    WHEN 'POLYGON' THEN ST_Perimeter(m."Geometry")
    WHEN 'POLYGONM' THEN ST_Perimeter(m."Geometry")
    WHEN 'MULTIPOLYGON' THEN ST_Perimeter(m."Geometry")
    WHEN 'MULTIPOLYGONM' THEN ST_Perimeter(m."Geometry")
    WHEN 'GEOMETRYCOLLECTION' THEN ST_Length(m."Geometry") + ST_Perimeter(m."Geometry")
    WHEN 'GEOMETRYCOLLECTIONM' THEN ST_Length(m."Geometry") + ST_Perimeter(m."Geometry")
    ELSE ST_Length(m."Geometry")
END
FROM (
    SELECT ST_GeogFromText(@p0) AS "Geometry"
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
            .FromSql($"SELECT ST_GeogFromText({multiPolygon}) AS \"Geometry\"")
            .Select(e => new
            {
                MultiPolygon = ((MultiPolygon)e.Geometry).Length,
                Collection = ((GeometryCollection)e.Geometry).Length
            });
        var result = async ? await multiQuery.SingleAsync() : multiQuery.Single();
        Assert.InRange(result.MultiPolygon, 2440842d, 2440845d);
        Assert.Equal(result.MultiPolygon, result.Collection);

        var collection = "GEOMETRYCOLLECTION(LINESTRING(0 0,3 4),POLYGON((0 0,4 0,4 3,0 3,0 0)))";
        var collectionQuery = context.Set<PointEntity>()
            .FromSql($"SELECT ST_GeogFromText({collection}) AS \"Geometry\"")
            .Select(e => ((GeometryCollection)e.Geometry).Length);
        Assert.InRange(async ? await collectionQuery.SingleAsync() : collectionQuery.Single(), 2107459d, 2107462d);

        AssertSql(
            $"""
p0='{multiPolygon}'

SELECT ST_Perimeter(m."Geometry") AS "MultiPolygon", ST_Length(m."Geometry") + ST_Perimeter(m."Geometry") AS "Collection"
FROM (
    SELECT ST_GeogFromText(@p0) AS "Geometry"
) AS m
LIMIT 2
""",
            //
            $"""
p0='{collection}'

SELECT ST_Length(m."Geometry") + ST_Perimeter(m."Geometry")
FROM (
    SELECT ST_GeogFromText(@p0) AS "Geometry"
) AS m
LIMIT 2
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Length_geography_configured_per_property(bool async)
    {
        var options = new DbContextOptionsBuilder<GeographyPropertyContext>()
            .UseNpgsql(Fixture.TestStore.ConnectionString, o => o.UseNetTopologySuite())
            .UseLoggerFactory(Fixture.TestSqlLoggerFactory)
            .EnableSensitiveDataLogging()
            .Options;
        await using var context = new GeographyPropertyContext(options);

        var polygon = "POLYGON((0 0,1 0,1 1,0 1,0 0))";
        var polygonQuery = context.Set<PolygonEntity>()
            .FromSql($"SELECT ST_GeogFromText({polygon}) AS \"Polygon\"")
            .Select(e => ((Geometry)e.Polygon).Length);
        Assert.InRange(async ? await polygonQuery.SingleAsync() : polygonQuery.Single(), 443770d, 443772d);

        var multiPolygon = "MULTIPOLYGON M(((0 0 1,1 0 2,1 1 3,0 1 4,0 0 1)))";
        var collectionQuery = context.Set<PointEntity>()
            .FromSql($"SELECT ST_GeogFromText({multiPolygon}) AS \"Collection\"")
            .Select(e => EF.Property<GeometryCollection>(e, "Collection").Length);
        Assert.InRange(async ? await collectionQuery.SingleAsync() : collectionQuery.Single(), 443770d, 443772d);

        var nestedCastQuery = context.Set<PointEntity>()
            .FromSql($"SELECT ST_GeogFromText({multiPolygon}) AS \"Geometry\"")
            .Select(e => ((GeometryCollection)(Geometry)(MultiPolygon)e.Geometry).Length);
        Assert.InRange(async ? await nestedCastQuery.SingleAsync() : nestedCastQuery.Single(), 443770d, 443772d);

        AssertSql(
            $"""
p0='{polygon}'

SELECT CASE GeometryType(m."Polygon")
    WHEN 'POLYGON' THEN ST_Perimeter(m."Polygon")
    WHEN 'POLYGONM' THEN ST_Perimeter(m."Polygon")
    WHEN 'MULTIPOLYGON' THEN ST_Perimeter(m."Polygon")
    WHEN 'MULTIPOLYGONM' THEN ST_Perimeter(m."Polygon")
    WHEN 'GEOMETRYCOLLECTION' THEN ST_Length(m."Polygon") + ST_Perimeter(m."Polygon")
    WHEN 'GEOMETRYCOLLECTIONM' THEN ST_Length(m."Polygon") + ST_Perimeter(m."Polygon")
    ELSE ST_Length(m."Polygon")
END
FROM (
    SELECT ST_GeogFromText(@p0) AS "Polygon"
) AS m
LIMIT 2
""",
            //
            $"""
p0='{multiPolygon}'

SELECT ST_Length(m."Collection") + ST_Perimeter(m."Collection")
FROM (
    SELECT ST_GeogFromText(@p0) AS "Collection"
) AS m
LIMIT 2
""",
            //
            $"""
p0='{multiPolygon}'

SELECT ST_Length(m."Geometry") + ST_Perimeter(m."Geometry")
FROM (
    SELECT ST_GeogFromText(@p0) AS "Geometry"
) AS m
LIMIT 2
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

    #region Not supported on geography

    public override Task Boundary(bool async)
        => Task.CompletedTask;

    public override Task Combine_aggregate(bool async)
        => Task.CompletedTask;

    public override Task EnvelopeCombine_aggregate(bool async)
        => Task.CompletedTask;

    public override Task Contains(bool async)
        => Task.CompletedTask;

    public override Task ConvexHull(bool async)
        => Task.CompletedTask;

    public override Task ConvexHull_aggregate(bool async)
        => Task.CompletedTask;

    public override Task Crosses(bool async)
        => Task.CompletedTask;

    public override Task Difference(bool async)
        => Task.CompletedTask;

    public override Task Dimension(bool async)
        => Task.CompletedTask;

    public override Task Disjoint_with_cast_to_nullable(bool async)
        => Task.CompletedTask;

    public override Task Disjoint_with_null_check(bool async)
        => Task.CompletedTask;

    public override Task EndPoint(bool async)
        => Task.CompletedTask;

    public override Task Envelope(bool async)
        => Task.CompletedTask;

    public override Task EqualsTopologically(bool async)
        => Task.CompletedTask;

    public override Task ExteriorRing(bool async)
        => Task.CompletedTask;

    public override Task GetGeometryN(bool async)
        => Task.CompletedTask;

    public override Task GetGeometryN_with_null_argument(bool async)
        => Task.CompletedTask;

    public override Task GetInteriorRingN(bool async)
        => Task.CompletedTask;

    public override Task GetPointN(bool async)
        => Task.CompletedTask;

    public override Task ICurve_IsClosed(bool async)
        => Task.CompletedTask;

    public override Task IGeometryCollection_Count(bool async)
        => Task.CompletedTask;

    public override Task IMultiCurve_IsClosed(bool async)
        => Task.CompletedTask;

    public override Task IsEmpty(bool async)
        => Task.CompletedTask;

    public override Task IsEmpty_equal_to_null(bool async)
        => Task.CompletedTask;

    public override Task IsEmpty_not_equal_to_null(bool async)
        => Task.CompletedTask;

    public override Task IsRing(bool async)
        => Task.CompletedTask;

    public override Task IsSimple(bool async)
        => Task.CompletedTask;

    public override Task IsValid(bool async)
        => Task.CompletedTask;

    public override Task Item(bool async)
        => Task.CompletedTask;

    public override Task InteriorPoint(bool async)
        => Task.CompletedTask;

    public override Task LineString_Count(bool async)
        => Task.CompletedTask;

    public override Task M(bool async)
        => Task.CompletedTask;

    public override Task Normalized(bool async)
        => Task.CompletedTask;

    public override Task NumGeometries(bool async)
        => Task.CompletedTask;

    public override Task NumInteriorRings(bool async)
        => Task.CompletedTask;

    public override Task NumPoints(bool async)
        => Task.CompletedTask;

    public override Task OgcGeometryType(bool async)
        => Task.CompletedTask;

    public override Task Overlaps(bool async)
        => Task.CompletedTask;

    public override Task PointOnSurface(bool async)
        => Task.CompletedTask;

    public override Task Relate(bool async)
        => Task.CompletedTask;

    public override Task Reverse(bool async)
        => Task.CompletedTask;

    public override Task StartPoint(bool async)
        => Task.CompletedTask;

    public override Task SymmetricDifference(bool async)
        => Task.CompletedTask;

    public override Task Touches(bool async)
        => Task.CompletedTask;

    public override Task Union(bool async)
        => Task.CompletedTask;

    public override Task Union_aggregate(bool async)
        => Task.CompletedTask;

    public override Task Union_void(bool async)
        => Task.CompletedTask;

    public override Task Within(bool async)
        => Task.CompletedTask;

    public override Task X(bool async)
        => Task.CompletedTask;

    public override Task Y(bool async)
        => Task.CompletedTask;

    public override Task XY_with_collection_join(bool async)
        => Task.CompletedTask;

    public override Task Z(bool async)
        => Task.CompletedTask;

    #endregion

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private sealed class GeographyPropertyContext(DbContextOptions<GeographyPropertyContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolygonEntity>().Property(e => e.Polygon).HasColumnType(" GEOGRAPHY (POLYGON, 4326) ");
            modelBuilder.Entity<PointEntity>().Property(e => e.Geometry).HasColumnType("geography");
            modelBuilder.Entity<PointEntity>().Property<GeometryCollection>("Collection").HasColumnType("geography");
        }
    }

    public class SpatialQueryNpgsqlGeographyFixture : SpatialQueryNpgsqlFixture
    {
        protected override string StoreName
            => "SpatialQueryGeographyTest";

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
