// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for <see cref="NpgsqlRange{T}"/> supporting PostgreSQL translation.
/// </summary>
public static class NpgsqlRangeDbFunctionsExtensions
{
    /// <summary>
    /// Determines whether a range contains a specified value.
    /// </summary>
    /// <param name="range">The range in which to locate the value.</param>
    /// <param name="value">The value to locate in the range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="range"/>.</typeparam>
    /// <returns>
    /// <value>true</value> if the range contains the specified value; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Contains{T}(NpgsqlRange{T}, T)" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool Contains<T>(this NpgsqlRange<T> range, T value)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    /// Determines whether a range contains a specified range.
    /// </summary>
    /// <param name="a">The range in which to locate the specified range.</param>
    /// <param name="b">The specified range to locate in the range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>
    /// <value>true</value> if the range contains the specified range; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Contains{T}(NpgsqlRange{T}, NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an EF Core LINQ
    /// query.
    /// </exception>
    public static bool Contains<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    /// Determines whether a range is contained by a specified range.
    /// </summary>
    /// <param name="a">The specified range to locate in the range.</param>
    /// <param name="b">The range in which to locate the specified range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>
    /// <value>true</value> if the range contains the specified range; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="ContainedBy{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool ContainedBy<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainedBy)));

    /// <summary>
    /// Determines whether a range overlaps another range.
    /// </summary>
    /// <param name="a">The first range.</param>
    /// <param name="b">The second range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>
    /// <value>true</value> if the ranges overlap (share points in common); otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Overlaps{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool Overlaps<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Overlaps)));

    /// <summary>
    /// Determines whether a range is strictly to the left of another range.
    /// </summary>
    /// <param name="a">The first range.</param>
    /// <param name="b">The second range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>
    /// <value>true</value> if the first range is strictly to the left of the second; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsStrictlyLeftOf{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool IsStrictlyLeftOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsStrictlyLeftOf)));

    /// <summary>
    /// Determines whether a range is strictly to the right of another range.
    /// </summary>
    /// <param name="a">The first range.</param>
    /// <param name="b">The second range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>
    /// <value>true</value> if the first range is strictly to the right of the second; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsStrictlyRightOf{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool IsStrictlyRightOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsStrictlyRightOf)));

    /// <summary>
    /// Determines whether a range does not extend to the left of another range.
    /// </summary>
    /// <param name="a">The first range.</param>
    /// <param name="b">The second range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>
    /// <value>true</value> if the first range does not extend to the left of the second; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DoesNotExtendLeftOf{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool DoesNotExtendLeftOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DoesNotExtendLeftOf)));

    /// <summary>
    /// Determines whether a range does not extend to the right of another range.
    /// </summary>
    /// <param name="a">The first range.</param>
    /// <param name="b">The second range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>
    /// <value>true</value> if the first range does not extend to the right of the second; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DoesNotExtendRightOf{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool DoesNotExtendRightOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DoesNotExtendRightOf)));

    /// <summary>
    /// Determines whether a range is adjacent to another range.
    /// </summary>
    /// <param name="a">The first range.</param>
    /// <param name="b">The second range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns><value>true</value> if the ranges are adjacent; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsAdjacentTo{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool IsAdjacentTo<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsAdjacentTo)));

    /// <summary>
    /// Returns the set union, which means unique elements that appear in either of two ranges.
    /// </summary>
    /// <param name="a">The first range.</param>
    /// <param name="b">The second range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>A range containing the unique elements that appear in either range.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Union{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlRange<T> Union<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Union)));

    /// <summary>
    /// Returns the set intersection, which means elements that appear in each of two ranges.
    /// </summary>
    /// <param name="a">The first range.</param>
    /// <param name="b">The second range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>A range containing the elements that appear in both ranges.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Intersect{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlRange<T> Intersect<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Intersect)));

    /// <summary>
    /// Returns the set difference, which means the elements of one range that do not appear in a second range.
    /// </summary>
    /// <param name="a">The first range.</param>
    /// <param name="b">The second range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>
    /// The elements that appear in the first range, but not the second range.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Except{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlRange<T> Except<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Except)));

    /// <summary>
    /// Returns the smallest range which includes both of the given ranges.
    /// </summary>
    /// <param name="a">The first range.</param>
    /// <param name="b">The second range.</param>
    /// <typeparam name="T">The type of the elements of <paramref name="a"/>.</typeparam>
    /// <returns>
    /// The smallest range which includes both of the given ranges.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Merge{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlRange<T> Merge<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Merge)));

    /// <summary>
    /// Computes the union of the non-null input ranges. Corresponds to the PostgreSQL <c>range_agg</c> aggregate function.
    /// </summary>
    /// <param name="input">The ranges to be aggregated via union into a multirange.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    /// <exception cref="NotSupportedException">
    /// <see cref="RangeAgg{T}" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlRange<T>[] RangeAgg<T>(this IEnumerable<NpgsqlRange<T>> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RangeAgg)));

    /// <summary>
    /// Computes the intersection of the non-null input ranges. Corresponds to the PostgreSQL <c>range_intersect_agg</c> aggregate function.
    /// </summary>
    /// <param name="input">The ranges on which to perform the intersection operation.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    /// <exception cref="NotSupportedException">
    /// <see cref="RangeIntersectAgg{T}(IEnumerable{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static NpgsqlRange<T> RangeIntersectAgg<T>(this IEnumerable<NpgsqlRange<T>> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RangeIntersectAgg)));

    /// <summary>
    /// Computes the intersection of the non-null input multiranges.
    /// Corresponds to the PostgreSQL <c>range_intersect_agg</c> aggregate function.
    /// </summary>
    /// <param name="input">The multiranges on which to perform the intersection operation.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    /// <exception cref="NotSupportedException">
    /// <see cref="RangeIntersectAgg{T}(IEnumerable{NpgsqlRange{T}[]})" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static NpgsqlRange<T>[] RangeIntersectAgg<T>(this IEnumerable<NpgsqlRange<T>[]> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(RangeIntersectAgg)));
}
