// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for <see cref="NpgsqlCube"/> supporting PostgreSQL translation.
/// </summary>
public static class NpgsqlCubeDbFunctionsExtensions
{
    /// <summary>
    /// Determines whether a cube overlaps with a specified cube.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>
    /// <value>true</value> if the cubes overlap; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Overlaps" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool Overlaps(this NpgsqlCube a, NpgsqlCube b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Overlaps)));

    /// <summary>
    /// Determines whether a cube contains a specified cube.
    /// </summary>
    /// <param name="a">The cube in which to locate the specified cube.</param>
    /// <param name="b">The specified cube to locate in the cube.</param>
    /// <returns>
    /// <value>true</value> if the cube contains the specified cube; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Contains" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool Contains(this NpgsqlCube a, NpgsqlCube b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    /// Determines whether a cube is contained by a specified cube.
    /// </summary>
    /// <param name="a">The cube to locate in the specified cube.</param>
    /// <param name="b">The specified cube in which to locate the cube.</param>
    /// <returns>
    /// <value>true</value> if the cube is contained by the specified cube; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="ContainedBy" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool ContainedBy(this NpgsqlCube a, NpgsqlCube b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainedBy)));

    /// <summary>
    /// Extracts the n-th coordinate of the cube (counting from 1).
    /// </summary>
    /// <param name="cube">The cube from which to extract the specified coordinate.</param>
    /// <param name="n">The specified coordinate to extract from the cube.</param>
    /// <returns>
    /// The n-th coordinate of the cube (counting from 1).
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="NthCoordinate" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double NthCoordinate(this NpgsqlCube cube, int n)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NthCoordinate)));

    /// <summary>
    /// Extracts the n-th coordinate of the cube, counting in the following way: n = 2 * k - 1 means lower bound
    /// of k-th dimension, n = 2 * k means upper bound of k-th dimension. Negative n denotes the inverse value
    /// of the corresponding positive coordinate. This operator is designed for KNN-GiST support.
    /// </summary>
    /// <param name="cube">The cube from which to extract the specified coordinate.</param>
    /// <param name="n">The specified coordinate to extract from the cube.</param>
    /// <returns>
    /// The n-th coordinate of the cube, counting in the following way: n = 2 * k - 1.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="NthCoordinate2" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double NthCoordinate2(this NpgsqlCube cube, int n)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NthCoordinate2)));

    /// <summary>
    /// Computes the Euclidean distance between two cubes.
    /// </summary>
    /// <param name="a">The first cube.</param>
    /// <param name="b">The second cube.</param>
    /// <returns>
    /// The Euclidean distance between the specified cubes.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Distance" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double Distance(this NpgsqlCube a, NpgsqlCube b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Distance)));

    /// <summary>
    /// Computes the taxicab (L-1 metric) distance between two cubes.
    /// </summary>
    /// <param name="a">The first cube.</param>
    /// <param name="b">The second cube.</param>
    /// <returns>
    /// The taxicab (L-1 metric) distance between the two cubes.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DistanceTaxicab" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double DistanceTaxicab(this NpgsqlCube a, NpgsqlCube b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DistanceTaxicab)));

    /// <summary>
    /// Computes the Chebyshev (L-inf metric) distance between two cubes.
    /// </summary>
    /// <param name="a">The first cube.</param>
    /// <param name="b">The second cube.</param>
    /// <returns>
    /// The Chebyshev (L-inf metric) distance between the two cubes.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DistanceChebyshev" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double DistanceChebyshev(this NpgsqlCube a, NpgsqlCube b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DistanceChebyshev)));

    /// <summary>
    /// Produces the union of two cubes.
    /// </summary>
    /// <param name="a">The first cube.</param>
    /// <param name="b">The second cube.</param>
    /// <returns>
    /// The union of the two cubes.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Union" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlCube Union(this NpgsqlCube a, NpgsqlCube b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Union)));

    /// <summary>
    /// Produces the intersection of two cubes.
    /// </summary>
    /// <param name="a">The first cube.</param>
    /// <param name="b">The second cube.</param>
    /// <returns>
    /// The intersection of the two cubes.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Intersect" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlCube Intersect(this NpgsqlCube a, NpgsqlCube b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Intersect)));

    /// <summary>
    /// Produces a cube enlarged by the specified radius r in at least n dimensions. If the radius is negative
    /// the cube is shrunk instead. All defined dimensions are changed by the radius r. Lower-left coordinates are
    /// decreased by r and upper-right coordinates are increased by r. If a lower-left coordinate is increased to more
    /// than the corresponding upper-right coordinate (this can only happen when r &lt; 0) than both coordinates are set
    /// to their average. If n is greater than the number of defined dimensions and the cube is being enlarged (r &gt; 0),
    /// then extra dimensions are added to make n altogether; 0 is used as the initial value for the extra coordinates.
    /// This function is useful for creating bounding boxes around a point for searching for nearby points.
    /// </summary>
    /// <param name="cube">The cube to enlarge.</param>
    /// <param name="r">The radius by which to increase the size of the cube.</param>
    /// <param name="n">The number of dimensions in which to increase the size of the cube.</param>
    /// <returns>
    /// A cube enlarged by the specified radius in at least the specified number of dimensions.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Enlarge" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlCube Enlarge(this NpgsqlCube cube, double r, int n)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Enlarge)));
}
