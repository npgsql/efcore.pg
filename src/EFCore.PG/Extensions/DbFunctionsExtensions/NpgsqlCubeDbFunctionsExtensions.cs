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
}
