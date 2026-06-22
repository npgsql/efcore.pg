// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Represents a PostgreSQL ltree type. This type is implicitly convertible to a .NET <see cref="string" />.
/// </summary>
/// <remarks>See https://www.postgresql.org/docs/current/ltree.html</remarks>
public readonly struct LTree : IEquatable<LTree>
{
    private readonly string _value;

    /// <summary>
    ///     Constructs a new instance of <see cref="LTree" />.
    /// </summary>
    /// <param name="value">The string value for the ltree.</param>
    public LTree(string value)
    {
        _value = value;
    }

    /// <summary>
    ///     Returns whether this ltree is an ancestor of <paramref name="other" /> (or equal).
    /// </summary>
    /// <remarks>
    ///     <p>The method call is translated to <c>left @&gt; right</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public bool IsAncestorOf(LTree other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsAncestorOf)));

    /// <summary>
    ///     Returns whether this ltree is a descendant of <paramref name="other" /> (or equal).
    /// </summary>
    /// <remarks>
    ///     <p>The method call is translated to <c>left &lt;@ right</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public bool IsDescendantOf(LTree other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsDescendantOf)));

    /// <summary>
    ///     Returns whether this ltree matches <paramref name="lquery" />.
    /// </summary>
    /// <remarks>
    ///     <p>The method call is translated to <c>left ~ right</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public bool MatchesLQuery(string lquery)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchesLQuery)));

    /// <summary>
    ///     Returns whether this ltree matches <paramref name="ltxtquery" />.
    /// </summary>
    /// <remarks>
    ///     <p>The method call is translated to <c>left @ right</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public bool MatchesLTxtQuery(string ltxtquery)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MatchesLTxtQuery)));

    /// <summary>
    ///     Returns subpath of this ltree from position <paramref name="start" /> to position <paramref name="end" />-1
    ///     (counting from 0).
    /// </summary>
    /// <remarks>
    ///     <p>The method call is translated to <c>subltree(ltree, start, end)</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public LTree Subtree(int start, int end)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Subtree)));

    /// <summary>
    ///     Returns subpath of this ltree starting at position <paramref name="offset" />, with length <paramref name="len" />.
    ///     If <paramref name="offset" /> is negative, subpath starts that far from the end of the path.
    ///     If <paramref name="len" /> is negative, leaves that many labels off the end of the path.
    /// </summary>
    /// <remarks>
    ///     <p>The method call is translated to <c>subpath(ltree, offset, len)</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public LTree Subpath(int offset, int len)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Subpath)));

    /// <summary>
    ///     Returns subpath of ltree starting at position <paramref name="offset" />, extending to end of path.
    ///     If <paramref name="offset" /> is negative, subpath starts that far from the end of the path.
    /// </summary>
    /// <remarks>
    ///     <p>The method call is translated to <c>subpath(ltree, offset)</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public LTree Subpath(int offset)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Subpath)));

    /// <summary>
    ///     Returns number of labels in path.
    /// </summary>
    /// <remarks>
    ///     <p>The property is translated to <c>nlevel(ltree)</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public int NLevel
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(NLevel)));

    /// <summary>
    ///     Returns position of first occurrence of <paramref name="other" /> in this ltree, or -1 if not found.
    /// </summary>
    /// <remarks>
    ///     <p>The method call is translated to <c>index(ltree1, ltree2)</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public int Index(LTree other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Index)));

    /// <summary>
    ///     Returns position of first occurrence of <paramref name="other" /> in this ltree, or -1 if not found.
    ///     The search starts at position <paramref name="offset" />; negative offset means start -offset labels from the end of the path.
    /// </summary>
    /// <remarks>
    ///     <p>The method call is translated to <c>index(ltree1, ltree2, offset)</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public int Index(LTree other, int offset)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Index)));

    /// <summary>
    ///     Computes longest common ancestor of paths.
    /// </summary>
    /// <remarks>
    ///     <p>The method call is translated to <c>lca(others)</c>.</p>
    ///     <p>See https://www.postgresql.org/docs/current/ltree.html</p>
    /// </remarks>
    public static LTree LongestCommonAncestor(params LTree[] others)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LongestCommonAncestor)));

    /// <summary>
    ///     Converts an <see cref="LTree" /> type to a string.
    /// </summary>
    public static implicit operator LTree(string value)
        => new(value);

    /// <summary>
    ///     Converts a string to an <see cref="LTree" /> type.
    /// </summary>
    public static implicit operator string(LTree ltree)
        => ltree._value;

    /// <summary>
    ///     Compares two <see cref="LTree" /> instances for equality.
    /// </summary>
    public static bool operator ==(LTree x, LTree y)
        => x._value == y._value;

    /// <summary>
    ///     Compares two <see cref="LTree" /> instances for inequality.
    /// </summary>
    public static bool operator !=(LTree x, LTree y)
        => x._value != y._value;

    /// <inheritdocs />
    public bool Equals(LTree other)
        => _value == other._value;

    /// <inheritdocs />
    public override bool Equals(object? obj)
        => obj is LTree other && Equals(other);

    /// <inheritdocs />
    public override int GetHashCode()
        => _value is not null ? _value.GetHashCode() : 0;

    /// <inheritdocs />
    public override string ToString()
        => _value;
}
