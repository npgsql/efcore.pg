using NpgsqlTypes;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query.Translations;

/// <summary>
/// Tests operations on the PostgreSQL cube type.
/// </summary>
[MinimumPostgresVersion(14, 0)] // Binary conversion for cube type was added in PostgreSQL 14
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

SELECT c."Id", c."Cube", c."IndexArray"
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

SELECT c."Id", c."Cube", c."IndexArray"
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

SELECT c."Id", c."Cube", c."IndexArray"
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

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE c."Cube" <@ @cube
LIMIT 2
""");
    }

    [ConditionalFact]
    public void Overlaps()
    {
        using var context = CreateContext();
        var overlappingCube = new NpgsqlCube([0.0, 1.0, 2.0], [5.0, 6.0, 7.0]);
        var result = context.CubeTestEntities.Single(x => x.Cube.Overlaps(overlappingCube));
        Assert.Equal(1, result.Id);

        AssertSql(
            """
@overlappingCube='(0, 1, 2),(5, 6, 7)' (DbType = Object)

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE c."Cube" && @overlappingCube
LIMIT 2
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

SELECT c."Id", c."Cube", c."IndexArray"
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

SELECT c."Id", c."Cube", c."IndexArray"
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

SELECT c."Id", c."Cube", c."IndexArray"
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
SELECT c."Id", c."Cube", c."IndexArray"
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
SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE c."Cube" ~> 2 = 4.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void LowerLeft_indexer_different_dimension()
    {
        using var context = CreateContext();
        // Zero-based index 1 should translate to PostgreSQL index 2
        // LowerLeft[1] accesses the second lower-left coordinate = 2.0 for cube (1,2,3),(4,5,6)
        var result = context.CubeTestEntities.Where(x => x.Cube.LowerLeft[1] == 2.0).Single();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_ll_coord(c."Cube", 2) = 2.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void UpperRight_indexer_different_dimension()
    {
        using var context = CreateContext();
        // Zero-based index 1 should translate to PostgreSQL index 2
        // UpperRight[1] accesses the second upper-right coordinate = 5.0 for cube (1,2,3),(4,5,6)
        var result = context.CubeTestEntities.Where(x => x.Cube.UpperRight[1] == 5.0).Single();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_ur_coord(c."Cube", 2) = 5.0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void LowerLeft_and_UpperRight_indexers_combined()
    {
        using var context = CreateContext();
        // Combine both LowerLeft and UpperRight indexers in the same query
        // For cube (1,2,3),(4,5,6): LowerLeft[0]=1.0, UpperRight[0]=4.0
        var result = context.CubeTestEntities.Where(x => x.Cube.LowerLeft[0] == 1.0 && x.Cube.UpperRight[0] == 4.0).Single();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT c."Id", c."Cube", c."IndexArray"
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

SELECT c."Id", c."Cube", c."IndexArray"
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

SELECT c."Id", c."Cube", c."IndexArray"
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
SELECT c."Id", c."Cube", c."IndexArray"
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
SELECT c."Id", c."Cube", c."IndexArray"
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
    public void ToSubset_extract()
    {
        using var context = CreateContext();
        // Extract dimension 0 (PostgreSQL index 1) - expected result is (1),(4)
        var subset = new NpgsqlCube([1.0], [4.0]);
        var result = context.CubeTestEntities.Where(x => x.Cube.ToSubset(0) == subset).ToList();

        AssertSql(
            """
@subset='(1),(4)' (DbType = Object)

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_subset(c."Cube", ARRAY[1]::integer[]) = @subset
""");
    }

    [ConditionalFact]
    public void ToSubset_reorder()
    {
        using var context = CreateContext();
        // Reorder dimensions: [2, 1, 0] (PostgreSQL: [3, 2, 1]) - expected result is (3,2,1),(6,5,4)
        var reordered = new NpgsqlCube([3.0, 2.0, 1.0], [6.0, 5.0, 4.0]);
        // Filter by dimension to avoid errors on cubes with different dimensionality
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.ToSubset(2, 1, 0) == reordered)
            .ToList();

        AssertSql(
            """
@reordered='(3, 2, 1),(6, 5, 4)' (DbType = Object)

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_subset(c."Cube", ARRAY[3,2,1]::integer[]) = @reordered
""");
    }

    [ConditionalFact]
    public void ToSubset_duplicate()
    {
        using var context = CreateContext();
        // Duplicate dimension 0: [0, 0, 1] (PostgreSQL: [1, 1, 2]) - expected result is (1,1,2),(4,4,5)
        var duplicated = new NpgsqlCube([1.0, 1.0, 2.0], [4.0, 4.0, 5.0]);
        // Filter by dimension to avoid errors on cubes with different dimensionality
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.ToSubset(0, 0, 1) == duplicated)
            .ToList();

        AssertSql(
            """
@duplicated='(1, 1, 2),(4, 4, 5)' (DbType = Object)

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_subset(c."Cube", ARRAY[1,1,2]::integer[]) = @duplicated
""");
    }

    [ConditionalFact]
    public void ToSubset_with_parameter_array_single_index()
    {
        using var context = CreateContext();
        // Test parameter array conversion: zero-based [0] should become one-based [1] in SQL
        var indexes = new[] { 0 };
        var subset = new NpgsqlCube([1.0], [4.0]);
        var result = context.CubeTestEntities.Where(x => x.Cube.ToSubset(indexes) == subset).ToList();

        AssertSql(
            """
@indexes={ '0' } (DbType = Object)
@subset='(1),(4)' (DbType = Object)

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_subset(c."Cube", (
    SELECT array_agg(u.x + 1)
    FROM unnest(@indexes) AS u(x))) = @subset
""");
    }

    [ConditionalFact]
    public void ToSubset_with_parameter_array_multiple_indexes()
    {
        using var context = CreateContext();
        // Test parameter array conversion with reordering: [2, 1, 0] should become [3, 2, 1] in SQL
        var indexes = new[] { 2, 1, 0 };
        var reordered = new NpgsqlCube([3.0, 2.0, 1.0], [6.0, 5.0, 4.0]);
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.ToSubset(indexes) == reordered)
            .ToList();

        AssertSql(
            """
@indexes={ '2'
'1'
'0' } (DbType = Object)
@reordered='(3, 2, 1),(6, 5, 4)' (DbType = Object)

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_subset(c."Cube", (
    SELECT array_agg(u.x + 1)
    FROM unnest(@indexes) AS u(x))) = @reordered
""");
    }

    [ConditionalFact]
    public void ToSubset_with_inline_array_literal()
    {
        using var context = CreateContext();
        // Test inline array literal: new[] { 0, 1 } should be converted at translation time
        var extracted = new NpgsqlCube([1.0, 2.0], [4.0, 5.0]);
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.ToSubset(new[] { 0, 1 }) == extracted)
            .ToList();

        AssertSql(
            """
@extracted='(1, 2),(4, 5)' (DbType = Object)

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_subset(c."Cube", ARRAY[1,2]::integer[]) = @extracted
""");
    }

    [ConditionalFact]
    public void ToSubset_with_empty_array_parameter()
    {
        using var context = CreateContext();
        // Test empty array parameter - PostgreSQL should handle this gracefully
        var indexes = Array.Empty<int>();
        var result = context.CubeTestEntities
            .Where(x => x.Cube.ToSubset(indexes).Dimensions == 0)
            .ToList();

        AssertSql(
            """
@indexes={  } (DbType = Object)

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_dim(cube_subset(c."Cube", (
    SELECT array_agg(u.x + 1)
    FROM unnest(@indexes) AS u(x)))) = 0
""");
    }

    [ConditionalFact]
    public void ToSubset_with_repeated_indexes_in_parameter()
    {
        using var context = CreateContext();
        // Test parameter array with repeated indices - duplicates should be converted correctly
        var indexes = new[] { 0, 1, 0, 2, 1 };
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.ToSubset(indexes).Dimensions == 5)
            .ToList();

        AssertSql(
            """
@indexes={ '0'
'1'
'0'
'2'
'1' } (DbType = Object)

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_dim(cube_subset(c."Cube", (
    SELECT array_agg(u.x + 1)
    FROM unnest(@indexes) AS u(x)))) = 5
""");
    }

    [ConditionalFact]
    public void ToSubset_with_column_array()
    {
        using var context = CreateContext();
        // Test column-based array: should generate runtime subquery conversion
        var result = context.CubeTestEntities
            .Where(x => x.Id == 1 && x.Cube.ToSubset(x.IndexArray!).Dimensions == 2)
            .ToList();

        AssertSql(
            """
SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE c."Id" = 1 AND cube_dim(cube_subset(c."Cube", (
    SELECT array_agg(u.x + 1)
    FROM unnest(c."IndexArray") AS u(x)))) = 2
""");
    }

    [ConditionalFact]
    public void ToSubset_with_column_array_multiple_rows()
    {
        using var context = CreateContext();
        // Test column-based array with multiple rows
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.ToSubset(x.IndexArray!).Dimensions == 2)
            .ToList();

        AssertSql(
            """
SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_dim(cube_subset(c."Cube", (
    SELECT array_agg(u.x + 1)
    FROM unnest(c."IndexArray") AS u(x)))) = 2
""");
    }

    [ConditionalFact]
    public void ToSubset_with_mixed_constant_and_variable_indexes()
    {
        using var context = CreateContext();
        var variableIndex = 1;
        var extracted = new NpgsqlCube([1.0, 2.0, 3.0], [4.0, 5.0, 6.0]);
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.ToSubset(new[] { 0, variableIndex, 2 }) == extracted)
            .ToList();

        AssertSql(
            """
@variableIndex='1'
@extracted='(1, 2, 3),(4, 5, 6)' (DbType = Object)

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_subset(c."Cube", ARRAY[1,@variableIndex + 1,3]::integer[]) = @extracted
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
SELECT c."Id", c."Cube", c."IndexArray"
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
SELECT c."Id", c."Cube", c."IndexArray"
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
SELECT c."Id", c."Cube", c."IndexArray"
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
SELECT c."Id", c."Cube", c."IndexArray"
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
        Assert.Contains("indexer syntax", exception.Message);
    }

    [ConditionalFact]
    public void LowerLeft_index_with_constant()
    {
        using var context = CreateContext();
        var result = context.CubeTestEntities
            .Where(x => x.Cube.LowerLeft[0] == 1.0)
            .ToList();

        AssertSql(
            """
SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_ll_coord(c."Cube", 1) = 1.0
""");
    }

    [ConditionalFact]
    public void UpperRight_Count_throws_helpful_error()
    {
        using var context = CreateContext();
        var exception = Assert.Throws<InvalidOperationException>(
            () => context.CubeTestEntities.Where(x => x.Cube.UpperRight.Count > 0).ToList());

        Assert.Contains("UpperRight", exception.Message);
        Assert.Contains("cannot be translated to SQL", exception.Message);
        Assert.Contains("indexer syntax", exception.Message);
    }

    [ConditionalFact]
    public void LowerLeft_in_projection_throws_helpful_error()
    {
        using var context = CreateContext();
        var exception = Assert.Throws<InvalidOperationException>(
            () => context.CubeTestEntities
                .Select(x => new { x.Id, LowerLeft = x.Cube.LowerLeft })
                .ToList());

        Assert.Contains("LowerLeft", exception.Message);
        Assert.Contains("cannot be translated to SQL", exception.Message);
        Assert.Contains("indexer syntax", exception.Message);
    }

    [ConditionalFact]
    public void UpperRight_in_projection_throws_helpful_error()
    {
        using var context = CreateContext();
        var exception = Assert.Throws<InvalidOperationException>(
            () => context.CubeTestEntities
                .Select(x => new { x.Id, UpperRight = x.Cube.UpperRight })
                .ToList());

        Assert.Contains("UpperRight", exception.Message);
        Assert.Contains("cannot be translated to SQL", exception.Message);
        Assert.Contains("indexer syntax", exception.Message);
    }

    [ConditionalFact]
    public void LowerLeft_FirstOrDefault_throws_helpful_error()
    {
        using var context = CreateContext();
        var exception = Assert.Throws<InvalidOperationException>(
            () => context.CubeTestEntities
                .Select(x => x.Cube.LowerLeft.FirstOrDefault())
                .ToList());

        Assert.Contains("LowerLeft", exception.Message);
        Assert.Contains("cannot be translated to SQL", exception.Message);
        Assert.Contains("indexer syntax", exception.Message);
    }

    [ConditionalFact]
    public void UpperRight_Any_throws_helpful_error()
    {
        using var context = CreateContext();
        var exception = Assert.Throws<InvalidOperationException>(
            () => context.CubeTestEntities
                .Where(x => x.Cube.UpperRight.Any(coord => coord > 5.0))
                .ToList());

        Assert.Contains("UpperRight", exception.Message);
        Assert.Contains("cannot be translated to SQL", exception.Message);
        Assert.Contains("indexer syntax", exception.Message);
    }

    [ConditionalFact]
    public void UpperRight_index_with_constant()
    {
        using var context = CreateContext();
        var result = context.CubeTestEntities
            .Where(x => x.Cube.UpperRight[2] > 5.0)
            .ToList();

        AssertSql(
            """
SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_ur_coord(c."Cube", 3) > 5.0
""");
    }

    [ConditionalFact]
    public void LowerLeft_index_with_parameter()
    {
        using var context = CreateContext();
        var index = 1;
        var result = context.CubeTestEntities
            .Where(x => x.Cube.LowerLeft[index] < 3.0)
            .ToList();

        AssertSql(
            """
@index='1'

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_ll_coord(c."Cube", @index + 1) < 3.0
""");
    }

    [ConditionalFact]
    public void UpperRight_index_with_parameter()
    {
        using var context = CreateContext();
        var index = 2;
        var result = context.CubeTestEntities
            .Where(x => x.Cube.UpperRight[index] > 5.0)
            .ToList();

        AssertSql(
            """
@index='2'

SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_ur_coord(c."Cube", @index + 1) > 5.0
""");
    }

    [ConditionalFact]
    public void LowerLeft_index_with_column()
    {
        using var context = CreateContext();
        // Use Id % 3 to get a valid index (0, 1, or 2) for the 3D cubes in test data
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.LowerLeft[x.Id % 3] > 0.0)
            .ToList();

        AssertSql(
            """
SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_ll_coord(c."Cube", c."Id" % 3 + 1) > 0.0
""");
    }

    [ConditionalFact]
    public void UpperRight_index_with_column()
    {
        using var context = CreateContext();
        // Use Id % 3 to get a valid index (0, 1, or 2) for the 3D cubes in test data
        var result = context.CubeTestEntities
            .Where(x => x.Cube.Dimensions == 3 && x.Cube.UpperRight[x.Id % 3] < 10.0)
            .ToList();

        AssertSql(
            """
SELECT c."Id", c."Cube", c."IndexArray"
FROM "CubeTestEntities" AS c
WHERE cube_dim(c."Cube") = 3 AND cube_ur_coord(c."Cube", c."Id" % 3 + 1) < 10.0
""");
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

    #region Constructor NULL handling

    [ConditionalFact]
    public void Constructor_single_coordinate_null()
    {
        using var context = CreateContext();
        double? nullCoord = null;

        // cube(NULL) should return NULL (STRICT function)
        var result = context.CubeConstructorTestEntities
            .Select(x => new { x.Id, Cube = nullCoord.HasValue ? new NpgsqlCube(nullCoord.Value) : (NpgsqlCube?)null })
            .OrderBy(x => x.Id)
            .First();

        Assert.Null(result.Cube);

        AssertSql(
            """
SELECT c."Id", NULL AS "Cube"
FROM "CubeConstructorTestEntities" AS c
ORDER BY c."Id" NULLS FIRST
LIMIT 1
""");
    }

    [ConditionalFact]
    public void Constructor_two_coordinates_first_null()
    {
        using var context = CreateContext();
        double? nullCoord = null;
        double secondCoord = 20.0;

        // cube(NULL, 20.0) should return NULL (STRICT function)
        var result = context.CubeConstructorTestEntities
            .Select(x => new {
                x.Id,
                Cube = nullCoord.HasValue ? new NpgsqlCube(nullCoord.Value, secondCoord) : (NpgsqlCube?)null
            })
            .OrderBy(x => x.Id)
            .First();

        Assert.Null(result.Cube);

        AssertSql(
            """
SELECT c."Id", NULL AS "Cube"
FROM "CubeConstructorTestEntities" AS c
ORDER BY c."Id" NULLS FIRST
LIMIT 1
""");
    }

    [ConditionalFact]
    public void Constructor_two_coordinates_second_null()
    {
        using var context = CreateContext();
        double firstCoord = 10.0;
        double? nullCoord = null;

        // cube(10.0, NULL) should return NULL (STRICT function)
        var result = context.CubeConstructorTestEntities
            .Select(x => new {
                x.Id,
                Cube = nullCoord.HasValue ? new NpgsqlCube(firstCoord, nullCoord.Value) : (NpgsqlCube?)null
            })
            .OrderBy(x => x.Id)
            .First();

        Assert.Null(result.Cube);

        AssertSql(
            """
SELECT c."Id", NULL AS "Cube"
FROM "CubeConstructorTestEntities" AS c
ORDER BY c."Id" NULLS FIRST
LIMIT 1
""");
    }

    [ConditionalFact]
    public void Constructor_array_null()
    {
        using var context = CreateContext();
        double[]? nullArray = null;

        var result = context.CubeConstructorTestEntities
            .Select(x => new {
                x.Id,
                Cube = nullArray != null ? new NpgsqlCube(nullArray) : (NpgsqlCube?)null
            })
            .OrderBy(x => x.Id)
            .First();

        Assert.Null(result.Cube);

        AssertSql(
            """
SELECT c."Id", NULL AS "Cube"
FROM "CubeConstructorTestEntities" AS c
ORDER BY c."Id" NULLS FIRST
LIMIT 1
""");
    }

    [ConditionalFact]
    public void Constructor_two_arrays_first_null()
    {
        using var context = CreateContext();
        double[]? nullArray = null;
        double[] secondArray = [4.0, 5.0, 6.0];

        var result = context.CubeConstructorTestEntities
            .Select(x => new {
                x.Id,
                Cube = nullArray != null ? new NpgsqlCube(nullArray, secondArray) : (NpgsqlCube?)null
            })
            .OrderBy(x => x.Id)
            .First();

        Assert.Null(result.Cube);

        AssertSql(
            """
SELECT c."Id", NULL AS "Cube"
FROM "CubeConstructorTestEntities" AS c
ORDER BY c."Id" NULLS FIRST
LIMIT 1
""");
    }

    [ConditionalFact]
    public void Constructor_extend_cube_null_cube()
    {
        using var context = CreateContext();
        NpgsqlCube? nullCube = null;
        const double coord = 7.0;

        var result = context.CubeConstructorTestEntities
            .Select(x => new {
                x.Id,
                Cube = nullCube.HasValue ? new NpgsqlCube(nullCube.Value, coord) : (NpgsqlCube?)null
            })
            .OrderBy(x => x.Id)
            .First();

        Assert.Null(result.Cube);

        AssertSql(
            """
SELECT c."Id", NULL AS "Cube"
FROM "CubeConstructorTestEntities" AS c
ORDER BY c."Id" NULLS FIRST
LIMIT 1
""");
    }

    [ConditionalFact]
    public void Constructor_extend_cube_null_coord()
    {
        using var context = CreateContext();
        var cube = new NpgsqlCube([1.0, 2.0, 3.0], [4.0, 5.0, 6.0]);
        double? nullCoord = null;

        var result = context.CubeConstructorTestEntities
            .Select(x => new {
                x.Id,
                Cube = nullCoord.HasValue ? new NpgsqlCube(cube, nullCoord.Value) : (NpgsqlCube?)null
            })
            .OrderBy(x => x.Id)
            .First();

        Assert.Null(result.Cube);

        AssertSql(
            """
SELECT c."Id", NULL AS "Cube"
FROM "CubeConstructorTestEntities" AS c
ORDER BY c."Id" NULLS FIRST
LIMIT 1
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
        public int[]? IndexArray { get; set; }
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
                    ),  // (1,2,3),(4,5,6)
                    IndexArray = [0, 1]  // Extract first two dimensions
                },
                new CubeTestEntity
                {
                    Id = 2,
                    // 3D point cube with all different dimensions
                    Cube = new NpgsqlCube(
                        [7.0, 8.0, 9.0]
                    ),  // (7,8,9)
                    IndexArray = [2, 1, 0]  // Reverse order
                },
                new CubeTestEntity
                {
                    Id = 3,
                    // Non-overlapping cube with all different dimensions (far from others)
                    Cube = new NpgsqlCube(
                        [20.0, 30.0, 40.0],
                        [25.0, 35.0, 45.0]
                    ),  // (20,30,40),(25,35,45)
                    IndexArray = [1, 2]  // Extract middle and last dimensions
                },
                new CubeTestEntity
                {
                    Id = 4,
                    // Far away cube for distance tests, all different dimensions
                    Cube = new NpgsqlCube(
                        [100.0, 200.0, 300.0],
                        [400.0, 500.0, 600.0]
                    ),  // (100,200,300),(400,500,600)
                    IndexArray = [0, 0, 1]  // Duplicate indices
                },
                new CubeTestEntity
                {
                    Id = 5,
                    // 1D cube (single point)
                    Cube = new NpgsqlCube(2.5),  // (2.5)
                    IndexArray = [0]  // Single index
                },
                new CubeTestEntity
                {
                    Id = 6,
                    // Cube with negative coordinates for testing
                    Cube = new NpgsqlCube(
                        [-5.0, -10.0, -15.0],
                        [-2.0, -7.0, -12.0]
                    ),  // (-5,-10,-15),(-2,-7,-12)
                    IndexArray = []  // Empty array
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
