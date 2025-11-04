// ReSharper disable once CheckNamespace

using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides extension methods for <see cref="NpgsqlCube" /> supporting PostgreSQL translation.
/// </summary>
/// <remarks>
///     See <see href="https://www.postgresql.org/docs/current/cube.html">PostgreSQL documentation for the cube extension</see>.
/// </remarks>
public static class NpgsqlCubeDbFunctionsExtensions
{
    /// <summary>
    ///     Determines whether two cubes overlap (have points in common).
    /// </summary>
    /// <param name="cube">The first cube.</param>
    /// <param name="other">The second cube.</param>
    /// <returns>
    ///     <value>true</value> if the cubes overlap; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Overlaps" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool Overlaps(this NpgsqlCube cube, NpgsqlCube other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Overlaps)));

    /// <summary>
    ///     Determines whether a cube contains another cube.
    /// </summary>
    /// <param name="cube">The cube to check.</param>
    /// <param name="other">The cube that may be contained.</param>
    /// <returns>
    ///     <value>true</value> if <paramref name="cube" /> contains <paramref name="other" />; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Contains" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool Contains(this NpgsqlCube cube, NpgsqlCube other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    ///     Determines whether a cube is contained by another cube.
    /// </summary>
    /// <param name="cube">The cube to check.</param>
    /// <param name="other">The cube that may contain it.</param>
    /// <returns>
    ///     <value>true</value> if <paramref name="cube" /> is contained by <paramref name="other" />; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="ContainedBy" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool ContainedBy(this NpgsqlCube cube, NpgsqlCube other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainedBy)));

    /// <summary>
    ///     Extracts the n-th coordinate of the cube (uses zero-based indexing).
    /// </summary>
    /// <param name="cube">The cube.</param>
    /// <param name="index">The zero-based coordinate index to extract.</param>
    /// <returns>The coordinate value at the specified index.</returns>
    /// <remarks>
    ///     This method uses zero-based indexing (C# convention), which is translated to PostgreSQL's one-based indexing.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="NthCoordinate" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double NthCoordinate(this NpgsqlCube cube, int index)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NthCoordinate)));

    /// <summary>
    ///     Extracts the n-th coordinate of the cube for K-nearest neighbor (KNN) indexing (uses zero-based indexing).
    /// </summary>
    /// <param name="cube">The cube.</param>
    /// <param name="index">The zero-based coordinate index to extract.</param>
    /// <returns>The coordinate value at the specified index.</returns>
    /// <remarks>
    ///     <para>
    ///         This method uses zero-based indexing (C# convention), which is translated to PostgreSQL's one-based indexing.
    ///     </para>
    ///     <para>
    ///         This is the same as <see cref="NthCoordinate" /> except it is marked "lossy" for GiST indexing purposes,
    ///         which is useful for K-nearest neighbor queries.
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="NthCoordinateKnn" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double NthCoordinateKnn(this NpgsqlCube cube, int index)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NthCoordinateKnn)));

    /// <summary>
    ///     Extracts the n-th coordinate of the lower-left corner of the cube (uses zero-based indexing).
    /// </summary>
    /// <param name="cube">The cube.</param>
    /// <param name="index">The zero-based coordinate index to extract.</param>
    /// <returns>The lower-left coordinate value at the specified index.</returns>
    /// <remarks>
    ///     <para>
    ///         This method uses zero-based indexing (C# convention), which is translated to PostgreSQL's one-based indexing.
    ///     </para>
    ///     <para>
    ///         This method is intended for use via SQL translation as part of an EF Core LINQ query,
    ///         to get the lower-left coordinate of a cube locally use <see cref="NpgsqlCube.LowerLeft"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="LlCoord" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double LlCoord(this NpgsqlCube cube, int index)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LlCoord)));

    /// <summary>
    ///     Extracts the n-th coordinate of the upper-right corner of the cube (uses zero-based indexing).
    /// </summary>
    /// <param name="cube">The cube.</param>
    /// <param name="index">The zero-based coordinate index to extract.</param>
    /// <returns>The upper-right coordinate value at the specified index.</returns>
    /// <remarks>
    ///     <para>
    ///         This method uses zero-based indexing (C# convention), which is translated to PostgreSQL's one-based indexing.
    ///     </para>
    ///     <para>
    ///         This method is intended for use via SQL translation as part of an EF Core LINQ query,
    ///         to get the upper-right coordinate of a cube locally use <see cref="NpgsqlCube.UpperRight"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="UrCoord" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double UrCoord(this NpgsqlCube cube, int index)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(UrCoord)));

    /// <summary>
    ///     Computes the Euclidean distance between two cubes.
    /// </summary>
    /// <param name="cube">The first cube.</param>
    /// <param name="other">The second cube.</param>
    /// <returns>The Euclidean distance between the two cubes.</returns>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Distance" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double Distance(this NpgsqlCube cube, NpgsqlCube other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Distance)));

    /// <summary>
    ///     Computes the taxicab (L-1 metric) distance between two cubes.
    /// </summary>
    /// <param name="cube">The first cube.</param>
    /// <param name="other">The second cube.</param>
    /// <returns>The taxicab distance between the two cubes.</returns>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="DistanceTaxicab" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double DistanceTaxicab(this NpgsqlCube cube, NpgsqlCube other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DistanceTaxicab)));

    /// <summary>
    ///     Computes the Chebyshev (L-inf metric) distance between two cubes.
    /// </summary>
    /// <param name="cube">The first cube.</param>
    /// <param name="other">The second cube.</param>
    /// <returns>The Chebyshev distance between the two cubes.</returns>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="DistanceChebyshev" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static double DistanceChebyshev(this NpgsqlCube cube, NpgsqlCube other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DistanceChebyshev)));

    /// <summary>
    ///     Computes the union of two cubes, producing the smallest cube that encloses both.
    /// </summary>
    /// <param name="cube">The first cube.</param>
    /// <param name="other">The second cube.</param>
    /// <returns>The smallest cube that encloses both input cubes.</returns>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Union" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlCube Union(this NpgsqlCube cube, NpgsqlCube other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Union)));

    /// <summary>
    ///     Computes the intersection of two cubes.
    /// </summary>
    /// <param name="cube">The first cube.</param>
    /// <param name="other">The second cube.</param>
    /// <returns>The intersection of the two cubes.</returns>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Intersect" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlCube Intersect(this NpgsqlCube cube, NpgsqlCube other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Intersect)));

    /// <summary>
    ///     Increases the size of a cube by a specified radius in at least the specified number of dimensions.
    /// </summary>
    /// <param name="cube">The cube to enlarge.</param>
    /// <param name="radius">The amount by which to enlarge the cube (can be negative to shrink).</param>
    /// <param name="dimensions">The number of dimensions to enlarge (optional, defaults to all dimensions).</param>
    /// <returns>The enlarged (or shrunk) cube.</returns>
    /// <remarks>
    ///     If the specified number of dimensions is greater than the cube's current dimensions,
    ///     the extra dimensions are added with the specified radius.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Enlarge" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlCube Enlarge(this NpgsqlCube cube, double radius, int dimensions)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Enlarge)));

    /// <summary>
    ///     Creates a new cube by extracting and optionally reordering specified dimensions (uses zero-based indexing).
    /// </summary>
    /// <param name="cube">The cube.</param>
    /// <param name="indexes">The zero-based dimension indexes to extract (in the desired order).</param>
    /// <returns>A new cube containing only the specified dimensions in the specified order.</returns>
    /// <remarks>
    ///     <para>
    ///         This method uses zero-based indexing (C# convention), which is translated to PostgreSQL's one-based indexing.
    ///     </para>
    ///     <para>
    ///         Can be used to drop dimensions, reorder them, or duplicate them as desired.
    ///     </para>
    ///     <para>
    ///         This method is only intended for use via SQL translation as part of an EF Core LINQ query,
    ///         to get the subset of a cube locally, use <see cref="NpgsqlCube.ToSubset"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="Subset" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlCube Subset(this NpgsqlCube cube, params int[] indexes)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Subset)));
}
