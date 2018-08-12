using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NetTopologySuiteTest : IClassFixture<NetTopologySuiteTest.NetTopologySuiteFixture>
    {
        public NetTopologySuiteTest(NetTopologySuiteFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        NetTopologySuiteFixture Fixture { get; }

        #region Method/Member translation

        [Fact]
        public void Area()
        {
            AssertQuery(st => st.Where(s => s.Polygon.Area == 16 && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Area(s.""Polygon"")", Sql);
        }

        [Fact]
        public void AsText()
        {
            // Note: PostgreSQL returns 'POINT (3 4)' while NetTopologySuite returns 'POINT(3 4)' (without the space)
            // So we don't use AssertQuery
            using (var ctx = CreateContext())
            {
                Assert.Equal(1, ctx.SpatialTypes.Single(s => s.Point.AsText() == "POINT(3 4)" && s.Id == 1).Id);
                Assert.Contains(@"ST_AsText(s.""Point"")", Sql);
            }
        }

        [Fact]
        public void Boundary()
        {
            var boundary = (MultiPoint)Reader.Read("MULTIPOINT(0 0,1 1)");
            AssertQuery(st => st.Where(s => s.LineString.Boundary.EqualsExact(boundary) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Boundary(s.""LineString"") = @__boundary_0", Sql);
        }

        [Fact]
        public void Contains()
        {
            AssertQuery(st => st.Where(s => s.Polygon.Contains(new Point(0, 0)) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"WHERE (ST_Contains(s.""Polygon"", GEOMETRY 'POINT (0 0)') = TRUE)", Sql);
        }

        [Fact]
        public void Covers()
        {
            AssertQuery(st => st.Where(s =>
                    new LineString(new[] { new Coordinate(1, 2), new Coordinate(3, 4) }).Covers(s.Point) && s.Id == 1),
                entryCount: 1);
            Assert.Contains(@"ST_Covers(GEOMETRY 'LINESTRING (1 2, 3 4)', s.""Point"")", Sql);
        }

        [Fact]
        public void CoveredBy()
        {
            AssertQuery(st => st.Where(s =>
                s.Point.CoveredBy(new LineString(new[] { new Coordinate(1, 2), new Coordinate(3, 4) })) && s.Id == 1),
                entryCount: 1);
            Assert.Contains(@"ST_CoveredBy(s.""Point"", GEOMETRY 'LINESTRING (1 2, 3 4)')", Sql);
        }

        [Fact]
        public void Crosses()
        {
            var doesCross = new LineString(new[] { new Coordinate(0, 1), new Coordinate(1, 0) });
            var doesNotCross = new LineString(new[] { new Coordinate(2, 2), new Coordinate(3, 3) });

            AssertQuery(st => st.Where(s => s.LineString.Crosses(doesCross) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Crosses(s.""LineString"", @__doesCross_0)", Sql);
            AssertQuery(st => st.Where(s => s.LineString.Crosses(doesNotCross) && s.Id == 1), entryCount: 0);
        }
        [Fact]
        public void Difference()
        {
            var polygon = (Polygon)Reader.Read("POLYGON((-1 -1,-1 3,3 3,3 -1,-1 -1))");
            var difference = (Polygon)Reader.Read("POLYGON((-2 -2,-2 2,-1 2,-1 -1,2 -1,2 -2,-2 -2))");
            AssertQuery(st => st.Where(s => s.Polygon.Difference(polygon).EqualsExact(difference) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Difference(s.""Polygon"", @__polygon_0)", Sql);
        }

        [Fact]
        public void Disjoint()
        {
            AssertQuery(st => st.Where(s => s.LineString.Disjoint(new Point(2, 2)) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Disjoint(s.""LineString"", GEOMETRY 'POINT (2 2)')", Sql);
            AssertQuery(st => st.Where(s => s.LineString.Disjoint(new Point(0.5, 0.5)) && s.Id == 1), entryCount: 0);
        }

        [Fact]
        public void Distance()
        {
            AssertQuery(st => st.Where(s => s.Point.Distance(new Point(4, 4)) == 1 && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Distance(s.""Point"", GEOMETRY 'POINT (4 4)')", Sql);
        }

        [Fact]
        public void Equals()
        {
            AssertQuery(st => st.Where(s => s.LineString.Equals(new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1) })) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"s.""LineString"" = GEOMETRY 'LINESTRING (0 0, 1 1)'", Sql);
            AssertQuery(st => st.Where(s => s.LineString.EqualsExact(new LineString(new[] { new Coordinate(1, 1), new Coordinate(0, 0) })) && s.Id == 1), entryCount: 0);
        }

        [Fact]
        public void EqualsExact()
        {
            AssertQuery(st => st.Where(s => s.LineString.EqualsExact(new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1) })) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"s.""LineString"" = GEOMETRY 'LINESTRING (0 0, 1 1)'", Sql);
            AssertQuery(st => st.Where(s => s.LineString.EqualsExact(new LineString(new[] { new Coordinate(1, 1), new Coordinate(0, 0) })) && s.Id == 1), entryCount: 0);
        }

        [Fact]
        public void EqualsTopologically()
        {
            AssertQuery(st => st.Where(s => s.LineString.EqualsTopologically(new LineString(new[] { new Coordinate(1, 1), new Coordinate(0, 0) })) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Equals(s.""LineString"", GEOMETRY 'LINESTRING (1 1, 0 0)')", Sql);
            AssertQuery(st => st.Where(s => s.LineString.EqualsTopologically(new LineString(new[] { new Coordinate(2, 2), new Coordinate(0, 0) })) && s.Id == 1), entryCount: 0);
        }

        [Fact]
        public void GeometryType()
        {
            // It would have been nice to be able to do this with the C# is operators :)

            // Note that NTS returns Point whereas PostGIS returns POINT, hence ToUpper() here
            AssertQuery(st => st.Where(s => s.Geometry.GeometryType.ToUpper() == "POINT" && s.Id == 1), entryCount: 1);
            Assert.Contains(@"GeometryType(s.""Geometry"")", Sql);
        }

        [Fact]
        public void GetGeometryN()
        {
            // Constant index
            AssertQuery(st => st.Where(s => s.Id == 2 && s.Collection.GetGeometryN(0).EqualsExact(new Point(3, 4))), entryCount: 1);
            Assert.Contains(@"ST_GeometryN(s.""Collection"", 1)", Sql);
            // Parameter index
            var i = 0;
            AssertQuery(st => st.Where(s => s.Id == 2 && s.Collection.GetGeometryN(i).EqualsExact(new Point(3, 4))), entryCount: 1);
            Assert.Contains(@"ST_GeometryN(s.""Collection"", @__i_0 + 1)", Sql);

            AssertQuery(st => st.Where(s => s.Id == 2 && ((Point)s.Collection.GetGeometryN(0)).X == 3), entryCount: 1);
        }

        [Fact]
        public void Intersection()
        {
            var polygon = (Polygon)Reader.Read("POLYGON((-1 -1,-1 3,3 3,3 -1,-1 -1))");
            var intersection = (Polygon)Reader.Read("POLYGON((-1 2,2 2,2 -1,-1 -1,-1 2))");
            AssertQuery(st => st.Where(s => s.Polygon.Intersection(polygon).EqualsExact(intersection) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Intersection(s.""Polygon"", @__polygon_0)", Sql);
        }

        [Fact]
        public void Intersects()
        {
            AssertQuery(st => st.Where(s => s.LineString.Intersects(new Point(0.5, 0.5)) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Intersects(s.""LineString"", GEOMETRY 'POINT (0.5 0.5)')", Sql);
            AssertQuery(st => st.Where(s => s.LineString.Intersects(new Point(2, 2)) && s.Id == 1), entryCount: 0);
        }

        [Fact]
        public void IsClosed()
        {
            AssertQuery(st => st.Where(s => s.LineString.IsClosed && s.Id == 2), entryCount: 1);
            Assert.Contains(@"ST_IsClosed(s.""LineString"")", Sql);
            AssertQuery(st => st.Where(s => !s.LineString.IsClosed && s.Id == 1), entryCount: 1);
        }

        [Fact]
        public void IsEmpty()
        {
            AssertQuery(st => st.Where(s => s.Collection.IsEmpty && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_IsEmpty(s.""Collection"")", Sql);
            AssertQuery(st => st.Where(s => s.Collection.IsEmpty && s.Id == 2), entryCount: 0);
        }

        [Fact]
        public void IsSimple()
        {
            AssertQuery(st => st.Where(s => s.LineString.IsSimple && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_IsSimple(s.""LineString"")", Sql);
        }

        [Fact]
        public void IsValid()
        {
            AssertQuery(st => st.Where(s => s.LineString.IsValid && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_IsValid(s.""LineString"")", Sql);
        }

        [Fact]
        public void Length()
        {
            AssertQuery(st => st.Where(s => s.LineString.Length - 1.4142135623731 < 0.01 && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Length(s.""LineString"")", Sql);
        }

        [Fact(Skip="No support currently in Npgsql.NetTopologySuite")]
        public void M() {}

        [Fact]
        public void NumGeometries()
        {
            AssertQuery(st => st.Where(s => s.Collection.NumGeometries == 0 && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_NumGeometries(s.""Collection"")", Sql);
            AssertQuery(st => st.Where(s => s.Collection.NumGeometries == 2 && s.Id == 2), entryCount: 1);
        }

        [Fact]
        public void NumPoints()
        {
            AssertQuery(st => st.Where(s => s.LineString.NumPoints == 2 && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_NumPoints(s.""LineString"") = 2", Sql);
            AssertQuery(st => st.Where(s => s.LineString.NumPoints == 4 && s.Id == 2), entryCount: 1);
        }

        [Fact]
        public void Overlaps()
        {
            var polygon = (Polygon)Reader.Read("POLYGON((-1 -1,-1 3,3 3,3 -1,-1 -1))");
            AssertQuery(st => st.Where(s => s.Id == 1 && s.Polygon.Overlaps(polygon)), entryCount: 1);
            Assert.Contains(@"ST_Overlaps(s.""Polygon"", @__polygon_0)", Sql);
        }

        [Fact]
        public void Relate()
        {
            AssertQuery(st => st.Where(s => s.Point.Relate(new Point(3, 4), "0FFFFFFF2") && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Relate(s.""Point"", GEOMETRY 'POINT (3 4)', '0FFFFFFF2')", Sql);
        }

        [Fact]
        public void Reverse()
        {
            AssertQuery(st => st.Where(s => s.LineString.Reverse().EqualsExact(new LineString(new[] { new Coordinate(1, 1), new Coordinate(0, 0) })) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Reverse(s.""LineString"")", Sql);
        }

        [Fact]
        public void SymmetricDifference()
        {
            var polygon = (Polygon)Reader.Read("POLYGON((-1 -1,-1 3,3 3,3 -1,-1 -1))");
            var symDifference = (MultiPolygon)Reader.Read("MULTIPOLYGON(((-2 -2,-2 2,-1 2,-1 -1,2 -1,2 -2,-2 -2)),((2 -1,2 2,-1 2,-1 3,3 3,3 -1,2 -1)))");
            AssertQuery(st => st.Where(s => s.Polygon.SymmetricDifference(polygon).EqualsExact(symDifference) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_SymDifference(s.""Polygon"", @__polygon_0)", Sql);
        }

        [Fact]
        public void Touches()
        {
            AssertQuery(st => st.Where(s => s.LineString.Touches(new Point(1, 1)) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Touches(s.""LineString"", GEOMETRY 'POINT (1 1)')", Sql);
            AssertQuery(st => st.Where(s => s.LineString.Touches(new Point(2, 2)) && s.Id == 1), entryCount: 0);
        }

        [Fact]
        public void ToText()
        {
            // Note: PostgreSQL returns 'POINT (3 4)' while NetTopologySuite returns 'POINT(3 4)' (without the space)
            // So we don't use AssertQuery
            using (var ctx = CreateContext())
            {
                Assert.Equal(1, ctx.SpatialTypes.Single(s => s.Point.ToText() == "POINT(3 4)" && s.Id == 1).Id);
                Assert.Contains(@"ST_AsText(s.""Point"")", Sql);
            }
        }

        [Fact]
        public void Union()
        {
            var polygon = (Polygon)Reader.Read("POLYGON((-1 -1,-1 3,3 3,3 -1,-1 -1))");
            var union = (Polygon)Reader.Read("POLYGON((-2 -2,-2 2,-1 2,-1 3,3 3,3 -1,2 -1,2 -2,-2 -2))");
            AssertQuery(st => st.Where(s => s.Polygon.Union(polygon).EqualsExact(union) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Union(s.""Polygon"", @__polygon_0)", Sql);
        }

        [Fact]
        public void Within()
        {
            var polygon = (Polygon)Reader.Read("POLYGON((-2 -2,-2 2,2 2,2 -2,-2 -2))");

            AssertQuery(st => st.Where(s => s.LineString.Within(polygon) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Within(s.""LineString"", @__polygon_0)", Sql);
            AssertQuery(st => st.Where(s => s.LineString.Within(polygon) && s.Id == 2), entryCount: 0);
        }

        [Fact]
        public void X()
        {
            AssertQuery(st => st.Where(s => s.Point.X == 3 && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_X(s.""Point"")", Sql);
        }

        [Fact]
        public void Y()
        {
            AssertQuery(st => st.Where(s => s.Point.Y == 4 && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Y(s.""Point"")", Sql);
        }

        [Fact(Skip="https://github.com/npgsql/npgsql/issues/1906")]
        public void Z()
        {
            AssertQuery(st => st.Where(s => s.Point.Z == 5 && s.Id == 2), entryCount: 1);
            Assert.Contains(@"ST_Z(s.""Point"")", Sql);
        }

        #endregion Method/Member translation

        [Fact]
        public void Polymorphism()
        {
            AssertQuery(st => st.Where(s => s.Geometry.EqualsExact(new Point(3,4)) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"WHERE (s.""Geometry"" = GEOMETRY 'POINT (3 4)')", Sql);
        }

        [Fact]
        public void Geography()
        {
            // Makes sure that when NTS types are mapped to PostGIS geography, the proper overload of ST_Distance() gets called
            // See http://workshops.boundlessgeo.com/postgis-intro/geography.html
            using (var ctx = CreateContext())
            {
                var paris = new Point(2.5559, 49.0083);
                Assert.Equal(1, ctx.SpatialTypes.Single(s => s.Geography.Distance(paris) - 9124665.26917268 < 1 && s.Id == 1).Id);
                Assert.Contains(@"ST_Distance(s.""Geography"", @__paris_0)", Sql);
            }
        }

        [Fact]
        public void IGeometry()
        {
            var igeometry = (IGeometry)new Point(0.5, 0.5);
            AssertQuery(st => st.Where(s => s.LineString.Intersects(igeometry) && s.Id == 1), entryCount: 1);
            Assert.Contains(@"ST_Intersects(s.""LineString"", @__igeometry_0)", Sql);
            AssertQuery(st => st.Where(s => s.IGeometry.Intersects(new Point(6, 6)) && s.Id == 1), entryCount: 0);
            Assert.Contains(@"ST_Intersects(s.""IGeometry"", GEOMETRY 'POINT (6 6)')", Sql);
        }

        #region Support

        static WKTReader Reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));

        NetTopologySuiteContext CreateContext() => Fixture.CreateContext();

        public void AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem1 : class
            => Fixture.QueryAsserter.AssertQuery(query, query, elementSorter, elementAsserter, assertOrder, entryCount).GetAwaiter().GetResult();

        public void AssertQuery(
            Func<IQueryable<SpatialTypes>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool assertOrder = false,
            int entryCount = 0)
            => AssertQuery<SpatialTypes>(query, elementSorter, elementAsserter, assertOrder, entryCount);

        string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class NetTopologySuiteFixture : SharedStoreFixtureBase<NetTopologySuiteContext>, IQueryFixtureBase
        {
            public NetTopologySuiteFixture()
            {
                var entitySorters = new Dictionary<Type, Func<dynamic, object>>
                {
                    { typeof(SpatialTypes), e => e?.Id }
                };

                var entityAsserters = new Dictionary<Type, Action<dynamic, dynamic>>
                {
                    {
                        typeof(SpatialTypes),
                        (e, a) =>
                            {
                                Assert.Equal(e == null, a == null);

                                if (a != null)
                                {
                                    Assert.Equal(e.Id, a.Id);
                                }
                            }
                    }
                };

                QueryAsserter = new QueryAsserter<NetTopologySuiteContext>(
                    CreateContext,
                    new SpatialData(),
                    entitySorters,
                    entityAsserters);
            }

            protected override string StoreName { get; } = "NetTopologySuiteTest";

            public QueryAsserterBase QueryAsserter { get; set; }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var npgsqlBuilder = new NpgsqlDbContextOptionsBuilder(builder).UseNetTopologySuite();
                return builder;
            }

            protected override void Seed(NetTopologySuiteContext context) => SpatialData.Seed(context);

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }

        public class NetTopologySuiteContext : DbContext
        {
            public NetTopologySuiteContext(DbContextOptions<NetTopologySuiteContext> options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.HasPostgresExtension("postgis");
            }

            public DbSet<SpatialTypes> SpatialTypes { get; set; }
        }

        public class SpatialTypes
        {
            public int Id { get; set; }
            public Point Point { get; set; }
            public LineString LineString { get; set; }
            public Polygon Polygon { get; set; }
            public GeometryCollection Collection { get; set; }
            public Geometry Geometry { get; set; }
            [Column(TypeName="geography")]
            public Geometry Geography { get; set; }
            public IGeometry IGeometry { get; set; }
        }

        public class SpatialData : IExpectedData
        {
            readonly SpatialTypes[] _spatialTypes = CreateSpatialTypes();

            static SpatialTypes[] CreateSpatialTypes()
            {
                return new[]
                {
                    new SpatialTypes
                    {
                        Id = 1,
                        Point = new Point(3, 4),
                        LineString = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1) }), // Open
                        Polygon = (Polygon)Reader.Read("POLYGON((-2 -2,-2 2,2 2,2 -2,-2 -2))"),
                        Collection = new GeometryCollection(new IGeometry[0]),
                        Geometry = new Point(3, 4),
                        Geography = new Point(-118.4079, 33.9434),   // Los Angeles
                        IGeometry = new Point(3, 4)
                    },
                    new SpatialTypes
                    {
                        Id = 2,
                        Point = new Point(3, 4, 5),
                        LineString = (LineString)Reader.Read("LINESTRING(0 0,0 3,3 3,0 0)"),  // Closed
                        Polygon = (Polygon)Reader.Read("POLYGON((-2 -2,-2 2,2 2,2 -2,-2 -2))"),
                        Collection = new GeometryCollection(new IGeometry[] { new Point(3, 4), new Point(4, 5)  }),
                        Geometry = (LineString)Reader.Read("LINESTRING(0 0,0 3,3 3,0 0)"),
                        Geography = (LineString)Reader.Read("LINESTRING(0 0,0 3,3 3,0 0)"),
                        IGeometry = new Point(5, 6)
                    }
                };
            }

            public IQueryable<TEntity> Set<TEntity>() where TEntity : class
            {
                if (typeof(TEntity) == typeof(SpatialTypes))
                {
                    return (IQueryable<TEntity>)_spatialTypes.AsQueryable();
                }

                throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
            }

            public static void Seed(NetTopologySuiteContext context)
            {
                context.SpatialTypes.AddRange(CreateSpatialTypes());
                context.SaveChanges();
            }
        }

        #endregion Support
    }
}
