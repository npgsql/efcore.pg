using System.Collections;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ChangeTracking.Internal;

/// <summary>
///     An <see cref="IComparer" /> for <see cref="NpgsqlRange{T}" /> values, providing an arbitrary but stable
///     total ordering. This is needed because <see cref="NpgsqlRange{T}" /> does not implement
///     <see cref="IComparable{T}" /> (ranges are only partially ordered), but EF Core requires an ordering for key properties
///     in the update pipeline.
/// </summary>
public sealed class NpgsqlRangeCurrentValueComparer(Type rangeClrType) : IComparer
{
    private readonly PropertyInfo _isEmptyProperty = rangeClrType.GetProperty(nameof(NpgsqlRange<>.IsEmpty))
        ?? throw new ArgumentException($"Type '{rangeClrType}' does not have an 'IsEmpty' property.");
    private readonly PropertyInfo _lowerBoundProperty = rangeClrType.GetProperty(nameof(NpgsqlRange<>.LowerBound))
        ?? throw new ArgumentException($"Type '{rangeClrType}' does not have a 'LowerBound' property.");
    private readonly PropertyInfo _upperBoundProperty = rangeClrType.GetProperty(nameof(NpgsqlRange<>.UpperBound))
        ?? throw new ArgumentException($"Type '{rangeClrType}' does not have an 'UpperBound' property.");
    private readonly PropertyInfo _lowerBoundInfiniteProperty = rangeClrType.GetProperty(nameof(NpgsqlRange<>.LowerBoundInfinite))
        ?? throw new ArgumentException($"Type '{rangeClrType}' does not have a 'LowerBoundInfinite' property.");
    private readonly PropertyInfo _upperBoundInfiniteProperty = rangeClrType.GetProperty(nameof(NpgsqlRange<>.UpperBoundInfinite))
        ?? throw new ArgumentException($"Type '{rangeClrType}' does not have an 'UpperBoundInfinite' property.");
    private readonly PropertyInfo _lowerBoundIsInclusiveProperty = rangeClrType.GetProperty(nameof(NpgsqlRange<>.LowerBoundIsInclusive))
        ?? throw new ArgumentException($"Type '{rangeClrType}' does not have a 'LowerBoundIsInclusive' property.");
    private readonly PropertyInfo _upperBoundIsInclusiveProperty = rangeClrType.GetProperty(nameof(NpgsqlRange<>.UpperBoundIsInclusive))
        ?? throw new ArgumentException($"Type '{rangeClrType}' does not have an 'UpperBoundIsInclusive' property.");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Compare(object? x, object? y)
    {
        if (x is null)
        {
            return y is null ? 0 : -1;
        }

        if (y is null)
        {
            return 1;
        }

        var xIsEmpty = (bool)_isEmptyProperty.GetValue(x)!;
        var yIsEmpty = (bool)_isEmptyProperty.GetValue(y)!;

        if (xIsEmpty && yIsEmpty)
        {
            return 0;
        }

        if (xIsEmpty)
        {
            return -1;
        }

        if (yIsEmpty)
        {
            return 1;
        }

        // Compare lower bounds
        var cmp = CompareBound(
            _lowerBoundProperty.GetValue(x),
            (bool)_lowerBoundInfiniteProperty.GetValue(x)!,
            _lowerBoundProperty.GetValue(y),
            (bool)_lowerBoundInfiniteProperty.GetValue(y)!,
            isLower: true);

        if (cmp != 0)
        {
            return cmp;
        }

        // Compare lower bound inclusivity
        cmp = ((bool)_lowerBoundIsInclusiveProperty.GetValue(x)!).CompareTo(
            (bool)_lowerBoundIsInclusiveProperty.GetValue(y)!);

        if (cmp != 0)
        {
            return cmp;
        }

        // Compare upper bounds
        cmp = CompareBound(
            _upperBoundProperty.GetValue(x),
            (bool)_upperBoundInfiniteProperty.GetValue(x)!,
            _upperBoundProperty.GetValue(y),
            (bool)_upperBoundInfiniteProperty.GetValue(y)!,
            isLower: false);

        if (cmp != 0)
        {
            return cmp;
        }

        // Compare upper bound inclusivity
        return ((bool)_upperBoundIsInclusiveProperty.GetValue(x)!).CompareTo(
            (bool)_upperBoundIsInclusiveProperty.GetValue(y)!);
    }

    private static int CompareBound(object? x, bool xInfinite, object? y, bool yInfinite, bool isLower) =>
        (xInfinite, yInfinite) switch
        {
            (true, true) => 0,
            (true, false) => isLower ? -1 : 1,
            (false, true) => isLower ? 1 : -1,
            (false, false) => Comparer.Default.Compare(x, y),
        };
}
