using System;
using System.Linq;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
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

        [Fact]
        public void IsClosed()
        {
            using (var ctx = CreateContext())
            {
                Assert.Equal(1, ctx.SpatialTypes.Single(b => b.LineString.IsClosed && b.Id == 1).Id);
                throw new Exception(Fixture.TestSqlLoggerFactory.Sql);
                //AssertContainsSql();
            }
        }

        [Fact]
        public void Covers()
        {
            using (var ctx = CreateContext())
            {
                var lineString = new LineString(new[] { new Coordinate(1, 2), new Coordinate(3, 4) });
                /*
                var polygon = Polygon.Empty;
                new Polygon(new LinearRing(new[] {
                    new Coordinate(1d, 1d),
                    new Coordinate(10d, 1d),
                    new Coordinate(10d, 10d),
                    new Coordinate(1d, 10d),
                    new Coordinate(1d, 1d)
                }));*/
                var spatialType = ctx.SpatialTypes.Where(b => lineString.Covers(b.Point)).ToList();
                throw new Exception(Fixture.TestSqlLoggerFactory.Sql);
                //AssertContainsSql();
            }
        }

        #region Support

        NetTopologySuiteContext CreateContext() => Fixture.CreateContext();

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

        public class NetTopologySuiteFixture : SharedStoreFixtureBase<NetTopologySuiteContext>
        {
            protected override string StoreName { get; } = "NetTopologySuiteTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var npgsqlBuilder = new NpgsqlDbContextOptionsBuilder(builder).UseNetTopologySuite();
                return builder;
            }

            protected override void Seed(NetTopologySuiteContext context)
            {
                context.Add(new SpatialTypes
                {
                    Id = 1,
                    Point = new Point(3, 4),
                    LineString = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1)})  // Open
                });
                context.Add(new SpatialTypes
                {
                    Id = 2,
                    Point = new Point(3, 4),
                    LineString = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(0, 1),
                        new Coordinate(1, 1),
                        new Coordinate(0, 0)
                    })  // Closed
                });
                context.SaveChanges();
            }

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
        }

        #endregion Support
    }
}
