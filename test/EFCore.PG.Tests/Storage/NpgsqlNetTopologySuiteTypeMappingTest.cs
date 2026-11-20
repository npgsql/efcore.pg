using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage;

public class NpgsqlNetTopologySuiteTypeMappingTest
{
    [Fact]
    public void GenerateSqlLiteral_returns_point_literal()
        => Assert.Equal(
            "GEOMETRY 'POINT (1 2)'",
            new NpgsqlGeometryTypeMapping<Point>("geometry", isGeography: false).GenerateSqlLiteral(new Point(1, 2)));

    [Fact]
    public void GenerateSqlLiteral_returns_point_literal_with_Z()
        => Assert.Equal(
            "GEOMETRY 'POINT Z(1 2 3)'",
            new NpgsqlGeometryTypeMapping<Point>("geometry", isGeography: false).GenerateSqlLiteral(new Point(new CoordinateZ(1, 2, 3))));

    [Fact] // #4093
    public void GenerateSqlLiteral_returns_point_literal_with_M()
        => Assert.Equal(
            "GEOMETRY 'POINT M(1 2 4)'",
            new NpgsqlGeometryTypeMapping<Point>("geometry", isGeography: false).GenerateSqlLiteral(new Point(new CoordinateM(1, 2, 4))));

    [Fact] // #4093
    public void GenerateSqlLiteral_returns_point_literal_with_Z_and_M()
        => Assert.Equal(
            "GEOGRAPHY 'SRID=4326;POINT ZM(1 2 3 4)'",
            new NpgsqlGeometryTypeMapping<Point>("geography", isGeography: true).GenerateSqlLiteral(
                new Point(new CoordinateZM(1, 2, 3, 4)) { SRID = 4326 }));
}
