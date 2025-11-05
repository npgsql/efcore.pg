// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Represents zero-based indices that are automatically converted to PostgreSQL's one-based indexing.
/// </summary>
/// <remarks>
///     This is an internal infrastructure type used by EFCore.PG translators to handle the conversion
///     between C#'s zero-based indexing convention and PostgreSQL's one-based indexing convention.
///     Users should not need to use this type directly - the conversion happens transparently when
///     passing int[] arrays to PostgreSQL functions.
/// </remarks>
internal readonly struct PgIndexes : IEquatable<PgIndexes>
{
    private readonly int[] _zeroBasedIndexes;

    /// <summary>
    ///     Creates a <see cref="PgIndexes"/> instance from zero-based indices.
    /// </summary>
    /// <param name="zeroBasedIndexes">The zero-based indices (C# convention).</param>
    internal PgIndexes(int[] zeroBasedIndexes)
    {
        _zeroBasedIndexes = zeroBasedIndexes ?? [];
    }

    /// <summary>
    ///     Gets the number of indices.
    /// </summary>
    public int Count => _zeroBasedIndexes.Length;

    /// <summary>
    ///     Gets the zero-based indices as an array.
    /// </summary>
    /// <returns>The original zero-based indices.</returns>
    internal int[] GetZeroBasedIndexes() => _zeroBasedIndexes;

    /// <summary>
    ///     Converts the zero-based indices to one-based indices (PostgreSQL convention).
    /// </summary>
    /// <returns>An array of one-based indices.</returns>
    internal int[] ToOneBased() => _zeroBasedIndexes.Select(i => i + 1).ToArray();

    /// <inheritdoc />
    public bool Equals(PgIndexes other) =>
        _zeroBasedIndexes.SequenceEqual(other._zeroBasedIndexes);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is PgIndexes other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var index in _zeroBasedIndexes)
        {
            hash.Add(index);
        }
        return hash.ToHashCode();
    }

    /// <summary>
    ///     Compares two <see cref="PgIndexes"/> instances for equality.
    /// </summary>
    public static bool operator ==(PgIndexes left, PgIndexes right) => left.Equals(right);

    /// <summary>
    ///     Compares two <see cref="PgIndexes"/> instances for inequality.
    /// </summary>
    public static bool operator !=(PgIndexes left, PgIndexes right) => !left.Equals(right);

    /// <inheritdoc />
    public override string ToString() => $"PgIndexes[{string.Join(", ", _zeroBasedIndexes)}]";
}
