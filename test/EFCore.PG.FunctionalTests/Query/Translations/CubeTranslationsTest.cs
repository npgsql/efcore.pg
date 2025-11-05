using NpgsqlTypes;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query.Translations;

/// <summary>
/// Tests operations on the PostgreSQL cube type.
/// </summary>
public class CubeTranslationsTest : IClassFixture<CubeTranslationsTest.CubeQueryNpgsqlFixture>
{
    private CubeQueryNpgsqlFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public CubeTranslationsTest(CubeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Operators

    [ConditionalFact]
    public void Contains_cube()
    {
        using var context = CreateContext();
        var smallCube = new NpgsqlCube([2.0, 3.0, 4.0], [3.0, 4.0, 5.0]);
        var result = context.CubeTestEntities.Single(x => x.Cube.Contains(smallCube));
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@smallCube='(2, 3, 4),(3, 4, 5)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" @> @smallCube
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Contains_point()
    {
        using var context = CreateContext();
        var point = new NpgsqlCube([2.0, 3.0, 4.0]);
        var result = context.CubeTestEntities.Single(x => x.Cube.Contains(point));
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@point='(2, 3, 4)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" @> @point
LIMIT 2
""");
    }

    [ConditionalFact]
    public void ContainedBy_cube()
    {
        using var context = CreateContext();
        // Cube that contains Id=1 (1,2,3),(4,5,6)
        // Filter by dimension to exclude 1D cube (Id=5)
        var largeCube = new NpgsqlCube([0.5, 1.5, 2.5], [4.5, 5.5, 6.5]);
        var result = context.CubeTestEntities
            .Single(x => x.Cube.ContainedBy(largeCube) && x.Cube.Dimensions == 3);
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@largeCube='(0.5, 1.5, 2.5),(4.5, 5.5, 6.5)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" <@ @largeCube AND cube_dim(c."Cube") = 3
LIMIT 2
""");
    }

    [ConditionalFact]
    public void ContainedBy_point()
    {
        using var context = CreateContext();
        var cube = new NpgsqlCube([6.0, 7.0, 8.0], [8.0, 9.0, 10.0]);
        var result = context.CubeTestEntities.Single(x => x.Cube.ContainedBy(cube));
        Assert.Equal(2, result.Id);

        AssertSql(
            """
@cube='(6, 7, 8),(8, 9, 10)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" <@ @cube
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Overlaps_true()
    {
        using var context = CreateContext();
        var overlappingCube = new NpgsqlCube([0.0, 1.0, 2.0], [5.0, 6.0, 7.0]);
        var result = context.CubeTestEntities.Single(x => x.Cube.Overlaps(overlappingCube));
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@overlappingCube='(0, 1, 2),(5, 6, 7)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" && @overlappingCube
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Overlaps_false()
    {
        using var context = CreateContext();
        var nonOverlappingCube = new NpgsqlCube([50.0, 60.0, 70.0], [80.0, 90.0, 100.0]);
        var result = context.CubeTestEntities.Where(x => x.Cube.Overlaps(nonOverlappingCube)).ToList();
        Assert.Empty(result);

        AssertSql(
            """
@nonOverlappingCube='(50, 60, 70),(80, 90, 100)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" && @nonOverlappingCube
""");
    }

    [ConditionalFact]
    public void Distance()
    {
        using var context = CreateContext();
        var targetCube = new NpgsqlCube([5.0, 6.0, 7.0]);
        var result = context.CubeTestEntities.OrderBy(x => x.Cube.Distance(targetCube)).First();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@targetCube='(5, 6, 7)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
ORDER BY c."Cube" <-> @targetCube NULLS FIRST
LIMIT 1
""");
    }

    [ConditionalFact]
    public void DistanceTaxicab()
    {
        using var context = CreateContext();
        var targetCube = new NpgsqlCube([5.0, 6.0, 7.0]);
        var result = context.CubeTestEntities.OrderBy(x => x.Cube.DistanceTaxicab(targetCube)).First();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@targetCube='(5, 6, 7)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
ORDER BY c."Cube" <#> @targetCube NULLS FIRST
LIMIT 1
""");
    }

    [ConditionalFact]
    public void DistanceChebyshev()
    {
        using var context = CreateContext();
        var targetCube = new NpgsqlCube([5.0, 6.0, 7.0]);
        var result = context.CubeTestEntities.OrderBy(x => x.Cube.DistanceChebyshev(targetCube)).First();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@targetCube='(5, 6, 7)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
ORDER BY c."Cube" <=> @targetCube NULLS FIRST
LIMIT 1
""");
    }

    [ConditionalFact]
    public void NthCoordinate()
    {
        using var context = CreateContext();
        // Zero-based index 0 should translate to PostgreSQL index 1
        var result = context.CubeTestEntities.Where(x => x.Cube.NthCoordinate(0) == 1.0).Single();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" -> 1 = 1.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void NthCoordinateKnn()
    {
        using var context = CreateContext();
        // Zero-based index 1 should translate to PostgreSQL index 2
        // The ~> operator uses flattened representation: odd indices=lower-left, even=upper-right
        // Index 2 accesses upper-right coordinate of first dimension = 4.0 for cube (1,2,3),(4,5,6)
        var result = context.CubeTestEntities.Where(x => x.Cube.NthCoordinateKnn(1) == 4.0).Single();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" ~> 2 = 4.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void LlCoord()
    {
        using var context = CreateContext();
        // Zero-based index 0 should translate to PostgreSQL index 1
        // LlCoord accesses the lower-left coordinate = 1.0 for cube (1,2,3),(4,5,6)
        var result = context.CubeTestEntities.Where(x => x.Cube.LlCoord(0) == 1.0).Single();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_ll_coord(c."Cube", 1) = 1.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void LlCoord_different_dimension()
    {
        using var context = CreateContext();
        // Zero-based index 1 should translate to PostgreSQL index 2
        // LlCoord(1) accesses the second lower-left coordinate = 2.0 for cube (1,2,3),(4,5,6)
        var result = context.CubeTestEntities.Where(x => x.Cube.LlCoord(1) == 2.0).Single();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_ll_coord(c."Cube", 2) = 2.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void UrCoord()
    {
        using var context = CreateContext();
        // Zero-based index 0 should translate to PostgreSQL index 1
        // UrCoord accesses the upper-right coordinate = 4.0 for cube (1,2,3),(4,5,6)
        var result = context.CubeTestEntities.Where(x => x.Cube.UrCoord(0) == 4.0).Single();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_ur_coord(c."Cube", 1) = 4.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void UrCoord_different_dimension()
    {
        using var context = CreateContext();
        // Zero-based index 1 should translate to PostgreSQL index 2
        // UrCoord(1) accesses the second upper-right coordinate = 5.0 for cube (1,2,3),(4,5,6)
        var result = context.CubeTestEntities.Where(x => x.Cube.UrCoord(1) == 5.0).Single();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_ur_coord(c."Cube", 2) = 5.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void LlCoord_and_UrCoord_combined()
    {
        using var context = CreateContext();
        // Combine both LlCoord and UrCoord in the same query
        // For cube (1,2,3),(4,5,6): LlCoord(0)=1.0, UrCoord(0)=4.0
        var result = context.CubeTestEntities.Where(x => x.Cube.LlCoord(0) == 1.0 && x.Cube.UrCoord(0) == 4.0).Single();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_ll_coord(c."Cube", 1) = 1.0 AND cube_ur_coord(c."Cube", 1) = 4.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Equals_operator()
    {
        using var context = CreateContext();
        var cube = new NpgsqlCube([1.0, 2.0, 3.0], [4.0, 5.0, 6.0]);
        var result = context.CubeTestEntities.Single(x => x.Cube == cube);
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@cube='(1, 2, 3),(4, 5, 6)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" = @cube
LIMIT 2
""");
    }

    [ConditionalFact]
    public void NotEquals_operator()
    {
        using var context = CreateContext();
        var cube = new NpgsqlCube(new[] { 1.0, 2.0, 3.0 }, [4.0, 5.0, 6.0]);
        var results = context.CubeTestEntities.Where(x => x.Cube != cube).ToList();
        Assert.Equal(5, results.Count); // All except Id=1

        AssertSql(
            """
@cube='(1, 2, 3),(4, 5, 6)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" <> @cube
""");
    }

    #endregion

    #region Properties

    [ConditionalFact]
    public void Dimensions_property()
    {
        using var context = CreateContext();
        var results = context.CubeTestEntities.Where(x => x.Cube.Dimensions == 3).ToList();
        Assert.Equal(5, results.Count); // Id 1, 2, 3, 4, 6 all have 3 dimensions

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3
""");
    }

    [ConditionalFact]
    public void IsPoint_property()
    {
        using var context = CreateContext();
        var results = context.CubeTestEntities.Where(x => x.Cube.IsPoint).ToList();
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Id == 2);
        Assert.Contains(results, r => r.Id == 5);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_is_point(c."Cube")
""");
    }

    #endregion

    #region Functions

    [ConditionalFact]
    public void Union()
    {
        using var context = CreateContext();
        var cube2 = new NpgsqlCube([7.0, 8.0, 9.0]);
        // Call Union on database column (x.Cube) with a parameter to avoid client evaluation
        var result = context.CubeTestEntities
            .Where(x => x.Id == 1)
            .Select(x => x.Cube.Union(cube2))
            .First();

        AssertSql(
            """
@cube2='(7, 8, 9)' (DbType = Object)

SELECT cube_union(c."Cube", @cube2)
FROM "CubeTestEntities" AS c
WHERE c."Id" = 1
LIMIT 1
""");
    }

    [ConditionalFact]
    public void Intersect()
    {
        using var context = CreateContext();
        var cube2 = new NpgsqlCube([2.0, 3.0, 4.0], [8.0, 9.0, 10.0]);
        // Call Intersect on database column (x.Cube) with a parameter to avoid client evaluation
        var result = context.CubeTestEntities
            .Where(x => x.Id == 1)
            .Select(x => x.Cube.Intersect(cube2))
            .First();

        AssertSql(
            """
@cube2='(2, 3, 4),(8, 9, 10)' (DbType = Object)

SELECT cube_inter(c."Cube", @cube2)
FROM "CubeTestEntities" AS c
WHERE c."Id" = 1
LIMIT 1
""");
    }

    [ConditionalFact]
    public void Enlarge()
    {
        using var context = CreateContext();
        // Call Enlarge on database column (x.Cube) to avoid client evaluation
        var result = context.CubeTestEntities
            .Where(x => x.Id == 1)
            .Select(x => x.Cube.Enlarge(1.0, 3))
            .First();

        AssertSql(
            """
SELECT cube_enlarge(c."Cube", 1.0, 3)
FROM "CubeTestEntities" AS c
WHERE c."Id" = 1
LIMIT 1
""");
    }

    [ConditionalFact]
    public void Enlarge_negative()
    {
        using var context = CreateContext();
        // Call Enlarge on database column (x.Cube) with negative radius to shrink
        var result = context.CubeTestEntities
            .Where(x => x.Id == 1)
            .Select(x => x.Cube.Enlarge(-1.0, 3))
            .First();

        AssertSql(
            """
SELECT cube_enlarge(c."Cube", -1.0, 3)
FROM "CubeTestEntities" AS c
WHERE c."Id" = 1
LIMIT 1
""");
    }

    [ConditionalFact]
    public void Subset_extract()
    {
        using var context = CreateContext();
        // Extract dimension 0 (PostgreSQL index 1) - expected result is (1),(4)
        var subset = new NpgsqlCube([1.0], [4.0]);
        var result = context.CubeTestEntities.Where(x => x.Cube.Subset(0) == subset).ToList();

        AssertSql(
            """
@subset='(1),(4)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_subset(c."Cube", ARRAY[1]::integer[]) = @subset
""");
    }

    [ConditionalFact]
    public void Subset_reorder()
    {
        using var context = CreateContext();
        // Reorder dimensions: [2, 1, 0] (PostgreSQL: [3, 2, 1]) - expected result is (3,2,1),(6,5,4)
        var reordered = new NpgsqlCube([3.0, 2.0, 1.0], [6.0, 5.0, 4.0]);
        // Filter by dimension to avoid errors on cubes with different dimensionality
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.Subset(2, 1, 0) == reordered)
            .ToList();

        AssertSql(
            """
@reordered='(3, 2, 1),(6, 5, 4)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_subset(c."Cube", ARRAY[3,2,1]::integer[]) = @reordered
""");
    }

    [ConditionalFact]
    public void Subset_duplicate()
    {
        using var context = CreateContext();
        // Duplicate dimension 0: [0, 0, 1] (PostgreSQL: [1, 1, 2]) - expected result is (1,1,2),(4,4,5)
        var duplicated = new NpgsqlCube([1.0, 1.0, 2.0], [4.0, 4.0, 5.0]);
        // Filter by dimension to avoid errors on cubes with different dimensionality
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.Subset(0, 0, 1) == duplicated)
            .ToList();

        AssertSql(
            """
@duplicated='(1, 1, 2),(4, 4, 5)' (DbType = Object)

SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_subset(c."Cube", ARRAY[1,1,2]::integer[]) = @duplicated
""");
    }

    #endregion

    #region Edge Cases

    [ConditionalFact]
    public void Single_dimension_cube()
    {
        using var context = CreateContext();
        var result = context.CubeTestEntities.Where(x => x.Cube.Dimensions == 1).Single();
        Assert.Equal(5, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 1
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Negative_coordinates()
    {
        using var context = CreateContext();
        // Find cubes with any negative coordinate
        var result = context.CubeTestEntities.Where(x => x.Cube.NthCoordinate(0) < 0).Single();
        Assert.Equal(6, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" -> 1 < 0.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Zero_volume_cube_point()
    {
        using var context = CreateContext();
        var results = context.CubeTestEntities.Where(x => x.Cube.IsPoint).ToList();
        Assert.Equal(2, results.Count);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE cube_is_point(c."Cube")
""");
    }

    [ConditionalFact]
    public void Large_coordinates()
    {
        using var context = CreateContext();
        // Find the cube with large coordinates (Id = 4)
        var result = context.CubeTestEntities.Where(x => x.Cube.NthCoordinate(0) > 50.0).Single();
        Assert.Equal(4, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube"
FROM "CubeTestEntities" AS c
WHERE c."Cube" -> 1 > 50.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void LowerLeft_Count_throws_helpful_error()
    {
        using var context = CreateContext();
        var exception = Assert.Throws<InvalidOperationException>(
            () => context.CubeTestEntities.Where(x => x.Cube.LowerLeft.Count > 0).ToList());

        Assert.Contains("LowerLeft", exception.Message);
        Assert.Contains("cannot be translated to SQL", exception.Message);
        Assert.Contains("LlCoord()", exception.Message);
    }

    [ConditionalFact]
    public void LowerLeft_index_throws_helpful_error()
    {
        using var context = CreateContext();
        var exception = Assert.Throws<InvalidOperationException>(
            () => context.CubeTestEntities.Where(x => x.Cube.LowerLeft[0] > 0).ToList());

        Assert.Contains("LowerLeft", exception.Message);
        Assert.Contains("cannot be translated to SQL", exception.Message);
        Assert.Contains("LlCoord()", exception.Message);
    }

    [ConditionalFact]
    public void UpperRight_Count_throws_helpful_error()
    {
        using var context = CreateContext();
        var exception = Assert.Throws<InvalidOperationException>(
            () => context.CubeTestEntities.Where(x => x.Cube.UpperRight.Count > 0).ToList());

        Assert.Contains("UpperRight", exception.Message);
        Assert.Contains("cannot be translated to SQL", exception.Message);
        Assert.Contains("UrCoord()", exception.Message);
    }

    [ConditionalFact]
    public void UpperRight_index_throws_helpful_error()
    {
        using var context = CreateContext();
        var exception = Assert.Throws<InvalidOperationException>(
            () => context.CubeTestEntities.Where(x => x.Cube.UpperRight[0] > 0).ToList());

        Assert.Contains("UpperRight", exception.Message);
        Assert.Contains("cannot be translated to SQL", exception.Message);
        Assert.Contains("UrCoord()", exception.Message);
    }

    #endregion

    #region Constructors

    [ConditionalFact]
    public void Constructor_in_where_point()
    {
        using var context = CreateContext();
        // Use constructor in WHERE clause - this exercises the translation when comparing
        var targetCube = new NpgsqlCube(10.0);
        var result = context.CubeConstructorTestEntities
            .Where(x => new NpgsqlCube(x.Coord1) == targetCube)
            .Select(x => x.Id)
            .Single();

        Assert.Equal(1, result);

        AssertSql(
            """
@targetCube='(10)' (DbType = Object)

SELECT c."Id"
FROM "CubeConstructorTestEntities" AS c
WHERE cube(c."Coord1") = @targetCube
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Constructor_in_where_one_dimension()
    {
        using var context = CreateContext();
        var targetCube = new NpgsqlCube(10.0, 20.0);
        var result = context.CubeConstructorTestEntities
            .Where(x => new NpgsqlCube(x.Coord1, x.Coord2) == targetCube)
            .Select(x => x.Id)
            .Single();

        Assert.Equal(1, result);

        AssertSql(
            """
@targetCube='(10),(20)' (DbType = Object)

SELECT c."Id"
FROM "CubeConstructorTestEntities" AS c
WHERE cube(c."Coord1", c."Coord2") = @targetCube
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Constructor_in_where_zero_volume()
    {
        using var context = CreateContext();
        var targetCube = new NpgsqlCube([10.0, 20.0, 30.0]);
        var result = context.CubeConstructorTestEntities
            .Where(x => new NpgsqlCube(x.Coordinates) == targetCube)
            .Select(x => x.Id)
            .Single();

        Assert.Equal(1, result);

        AssertSql(
            """
@targetCube='(10, 20, 30)' (DbType = Object)

SELECT c."Id"
FROM "CubeConstructorTestEntities" AS c
WHERE cube(c."Coordinates") = @targetCube
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Constructor_in_where_lower_left_upper_right()
    {
        using var context = CreateContext();
        var targetCube = new NpgsqlCube([1.0, 2.0, 3.0], [4.0, 5.0, 6.0]);
        var result = context.CubeConstructorTestEntities
            .Where(x => new NpgsqlCube(x.LowerLeft, x.UpperRight) == targetCube)
            .Select(x => x.Id)
            .Single();

        Assert.Equal(1, result);

        AssertSql(
            """
@targetCube='(1, 2, 3),(4, 5, 6)' (DbType = Object)

SELECT c."Id"
FROM "CubeConstructorTestEntities" AS c
WHERE cube(c."LowerLeft", c."UpperRight") = @targetCube
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Constructor_extend_cube_one_coordinate()
    {
        using var context = CreateContext();
        var baseCube = new NpgsqlCube([1.0, 2.0, 3.0], [4.0, 5.0, 6.0]);
        var targetCube = new NpgsqlCube(baseCube, 40.0);
        var result = context.CubeConstructorTestEntities
            .Where(x => new NpgsqlCube(x.BaseCube, x.Coord1) == targetCube)
            .Select(x => x.Id)
            .Single();

        Assert.Equal(2, result);

        AssertSql(
            """
@targetCube='(1, 2, 3, 40),(4, 5, 6, 40)' (DbType = Object)

SELECT c."Id"
FROM "CubeConstructorTestEntities" AS c
WHERE cube(c."BaseCube", c."Coord1") = @targetCube
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Constructor_extend_cube_two_coordinates()
    {
        using var context = CreateContext();
        var baseCube = new NpgsqlCube([1.0, 2.0, 3.0], [4.0, 5.0, 6.0]);
        var targetCube = new NpgsqlCube(baseCube, 40.0, 50.0);
        var result = context.CubeConstructorTestEntities
            .Where(x => new NpgsqlCube(x.BaseCube, x.Coord1, x.Coord2) == targetCube)
            .Select(x => x.Id)
            .Single();

        Assert.Equal(2, result);

        AssertSql(
            """
@targetCube='(1, 2, 3, 40),(4, 5, 6, 50)' (DbType = Object)

SELECT c."Id"
FROM "CubeConstructorTestEntities" AS c
WHERE cube(c."BaseCube", c."Coord1", c."Coord2") = @targetCube
LIMIT 2
""");
    }

    #endregion

    #region Fixture

    public class CubeQueryNpgsqlFixture : SharedStoreFixtureBase<CubeContext>
    {
        protected override string StoreName
            => "CubeQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(CubeContext context)
            => CubeContext.SeedAsync(context);
    }

    public class CubeTestEntity
    {
        public int Id { get; set; }
        public NpgsqlCube Cube { get; set; }
    }

    public class CubeConstructorTestEntity
    {
        public int Id { get; set; }
        public double Coord1 { get; set; }
        public double Coord2 { get; set; }
        public double[] Coordinates { get; set; } = null!;
        public double[] LowerLeft { get; set; } = null!;
        public double[] UpperRight { get; set; } = null!;
        public NpgsqlCube BaseCube { get; set; }
    }

    public class CubeContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<CubeTestEntity> CubeTestEntities { get; set; }
        public DbSet<CubeConstructorTestEntity> CubeConstructorTestEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
            => builder.HasPostgresExtension("cube");

        public static async Task SeedAsync(CubeContext context)
        {
            context.CubeTestEntities.AddRange(
                new CubeTestEntity
                {
                    Id = 1,
                    // 3D non-point cube with all different dimensions
                    Cube = new NpgsqlCube(
                        [1.0, 2.0, 3.0],
                        [4.0, 5.0, 6.0]
                    )  // (1,2,3),(4,5,6)
                },
                new CubeTestEntity
                {
                    Id = 2,
                    // 3D point cube with all different dimensions
                    Cube = new NpgsqlCube(
                        [7.0, 8.0, 9.0]
                    )  // (7,8,9)
                },
                new CubeTestEntity
                {
                    Id = 3,
                    // Non-overlapping cube with all different dimensions (far from others)
                    Cube = new NpgsqlCube(
                        [20.0, 30.0, 40.0],
                        [25.0, 35.0, 45.0]
                    )  // (20,30,40),(25,35,45)
                },
                new CubeTestEntity
                {
                    Id = 4,
                    // Far away cube for distance tests, all different dimensions
                    Cube = new NpgsqlCube(
                        [100.0, 200.0, 300.0],
                        [400.0, 500.0, 600.0]
                    )  // (100,200,300),(400,500,600)
                },
                new CubeTestEntity
                {
                    Id = 5,
                    // 1D cube (single point)
                    Cube = new NpgsqlCube(2.5)  // (2.5)
                },
                new CubeTestEntity
                {
                    Id = 6,
                    // Cube with negative coordinates for testing
                    Cube = new NpgsqlCube(
                        [-5.0, -10.0, -15.0],
                        [-2.0, -7.0, -12.0]
                    )  // (-5,-10,-15),(-2,-7,-12)
                });

            context.CubeConstructorTestEntities.AddRange(
                new CubeConstructorTestEntity
                {
                    Id = 1,
                    Coord1 = 10.0,
                    Coord2 = 20.0,
                    Coordinates = [10.0, 20.0, 30.0],
                    LowerLeft = [1.0, 2.0, 3.0],
                    UpperRight = [4.0, 5.0, 6.0],
                    BaseCube = new NpgsqlCube([1.0, 2.0, 3.0], [4.0, 5.0, 6.0])
                },
                new CubeConstructorTestEntity
                {
                    Id = 2,
                    Coord1 = 40.0,
                    Coord2 = 50.0,
                    Coordinates = [40.0, 50.0, 60.0],
                    LowerLeft = [7.0, 8.0, 9.0],
                    UpperRight = [10.0, 11.0, 12.0],
                    BaseCube = new NpgsqlCube([1.0, 2.0, 3.0], [4.0, 5.0, 6.0])
                });

            await context.SaveChangesAsync();
        }
    }

    #endregion

    #region Helpers

    protected CubeContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    #endregion
}
