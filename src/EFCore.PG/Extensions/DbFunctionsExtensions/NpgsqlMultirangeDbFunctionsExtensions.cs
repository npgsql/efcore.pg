// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for multiranges supporting PostgreSQL translation.
/// </summary>
public static class NpgsqlMultirangeDbFunctionsExtensions
{
    #region Contains

    /// <summary>
    /// Determines whether a multirange contains a specified value.
    /// </summary>
    /// <param name="multirange">The multirange in which to locate the value.</param>
    /// <param name="value">The value to locate in the range.</param>
    /// <returns><value>true</value> if the multirange contains the specified value; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Contains{T}(NpgsqlRange{T}[], T)" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool Contains<T>(this NpgsqlRange<T>[] multirange, T value)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    /// Determines whether a multirange contains a specified value.
    /// </summary>
    /// <param name="multirange">The multirange in which to locate the value.</param>
    /// <param name="value">The value to locate in the range.</param>
    /// <returns><value>true</value> if the multirange contains the specified value; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Contains{T}(List{NpgsqlRange{T}}, T)" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool Contains<T>(this List<NpgsqlRange<T>> multirange, T value)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    /// Determines whether a multirange contains a specified multirange.
    /// </summary>
    /// <param name="multirange1">The multirange in which to locate the specified multirange.</param>
    /// <param name="multirange2">The specified multirange to locate in the multirange.</param>
    /// <returns><value>true</value> if the multirange contains the specified multirange; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Contains{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static bool Contains<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    /// Determines whether a multirange contains a specified multirange.
    /// </summary>
    /// <param name="multirange1">The multirange in which to locate the specified multirange.</param>
    /// <param name="multirange2">The specified multirange to locate in the multirange.</param>
    /// <returns><value>true</value> if the multirange contains the specified multirange; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Contains{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part of an EF
    /// Core LINQ query.
    /// </exception>
    public static bool Contains<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    /// Determines whether a multirange contains a specified range.
    /// </summary>
    /// <param name="multirange1">The multirange in which to locate the specified range.</param>
    /// <param name="multirange2">The specified range to locate in the multirange.</param>
    /// <returns><value>true</value> if the multirange contains the specified range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Contains{T}(NpgsqlRange{T}[], NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an EF Core LINQ
    /// query.
    /// </exception>
    public static bool Contains<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    /// Determines whether a multirange contains a specified range.
    /// </summary>
    /// <param name="multirange1">The multirange in which to locate the specified range.</param>
    /// <param name="multirange2">The specified range to locate in the multirange.</param>
    /// <returns><value>true</value> if the multirange contains the specified range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Contains{T}(List{NpgsqlRange{T}}, NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static bool Contains<T>(this List<NpgsqlRange<T>> multirange1, NpgsqlRange<T> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    #endregion Contains

    #region ContainedBy

    /// <summary>
    /// Determines whether a multirange is contained by a specified multirange.
    /// </summary>
    /// <param name="multirange1">The specified multirange to locate in the multirange.</param>
    /// <param name="multirange2">The multirange in which to locate the specified multirange.</param>
    /// <returns><value>true</value> if the multirange contains the specified multirange; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="ContainedBy{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static bool ContainedBy<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainedBy)));

    /// <summary>
    /// Determines whether a multirange is contained by a specified multirange.
    /// </summary>
    /// <param name="multirange1">The specified multirange to locate in the multirange.</param>
    /// <param name="multirange2">The multirange in which to locate the specified multirange.</param>
    /// <returns><value>true</value> if the multirange contains the specified multirange; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="ContainedBy{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part of an
    /// EF Core LINQ query.
    /// </exception>
    public static bool ContainedBy<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainedBy)));

    /// <summary>
    /// Determines whether a range is contained by a specified multirange.
    /// </summary>
    /// <param name="range">The specified range to locate in the multirange.</param>
    /// <param name="multirange">The multirange in which to locate the specified range.</param>
    /// <returns><value>true</value> if the multirange contains the specified range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static bool ContainedBy<T>(this NpgsqlRange<T> range, NpgsqlRange<T>[] multirange)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainedBy)));

    /// <summary>
    /// Determines whether a range is contained by a specified multirange.
    /// </summary>
    /// <param name="range">The specified range to locate in the multirange.</param>
    /// <param name="multirange">The multirange in which to locate the specified range.</param>
    /// <returns><value>true</value> if the multirange contains the specified range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="ContainedBy{T}(NpgsqlRange{T}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part of an EF
    /// Core LINQ query.
    /// </exception>
    public static bool ContainedBy<T>(this NpgsqlRange<T> range, List<NpgsqlRange<T>> multirange)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainedBy)));

    #endregion ContainedBy

    #region Overlaps

    /// <summary>
    /// Determines whether a multirange overlaps another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns><value>true</value> if the multiranges overlap (share points in common); otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Overlaps{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static bool Overlaps<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Overlaps)));

    /// <summary>
    /// Determines whether a multirange overlaps another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns><value>true</value> if the multiranges overlap (share points in common); otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Overlaps{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part of an
    /// EF Core LINQ query.
    /// </exception>
    public static bool Overlaps<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Overlaps)));

    /// <summary>
    /// Determines whether a multirange overlaps another range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The range.</param>
    /// <returns>
    /// <value>true</value> if the multirange and range overlap (share points in common); otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Overlaps{T}(NpgsqlRange{T}[], NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an EF Core LINQ
    /// query.
    /// </exception>
    public static bool Overlaps<T>(this NpgsqlRange<T>[] multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Overlaps)));

    /// <summary>
    /// Determines whether a multirange overlaps another range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The range.</param>
    /// <returns>
    /// <value>true</value> if the multirange and range overlap (share points in common); otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Overlaps{T}(List{NpgsqlRange{T}}, NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static bool Overlaps<T>(this List<NpgsqlRange<T>> multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Overlaps)));

    #endregion Overlaps

    #region IsStrictlyLeftOf

    /// <summary>
    /// Determines whether a multirange is strictly to the left of another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>
    /// <value>true</value> if the first multirange is strictly to the left of the second multirange; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsStrictlyLeftOf{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an EF
    /// Core LINQ query.
    /// </exception>
    public static bool IsStrictlyLeftOf<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsStrictlyLeftOf)));

    /// <summary>
    /// Determines whether a multirange is strictly to the left of another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>
    /// <value>true</value> if the first multirange is strictly to the left of the second multirange; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsStrictlyLeftOf{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part
    /// of an EF Core LINQ query.
    /// </exception>
    public static bool IsStrictlyLeftOf<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsStrictlyLeftOf)));

    /// <summary>
    /// Determines whether a multirange is strictly to the left of a range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The range.</param>
    /// <returns><value>true</value> if the multirange is strictly to the left of the range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsStrictlyLeftOf{T}(NpgsqlRange{T}[], NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an EF
    /// Core LINQ query.
    /// </exception>
    public static bool IsStrictlyLeftOf<T>(this NpgsqlRange<T>[] multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsStrictlyLeftOf)));

    /// <summary>
    /// Determines whether a multirange is strictly to the left of a range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The range.</param>
    /// <returns><value>true</value> if the multirange is strictly to the left of the range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsStrictlyLeftOf{T}(List{NpgsqlRange{T}}, NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an
    /// EF Core LINQ query.
    /// </exception>
    public static bool IsStrictlyLeftOf<T>(this List<NpgsqlRange<T>> multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsStrictlyLeftOf)));

    #endregion IsStrictlyLeftOf

    #region IsStrictlyRightOf

    /// <summary>
    /// Determines whether a multirange is strictly to the right of another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>
    /// <value>true</value> if the first multirange is strictly to the right of the second multirange; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsStrictlyRightOf{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an
    /// EF Core LINQ query.
    /// </exception>
    public static bool IsStrictlyRightOf<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsStrictlyRightOf)));

    /// <summary>
    /// Determines whether a multirange is strictly to the right of another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>
    /// <value>true</value> if the first multirange is strictly to the right of the second multirange; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsStrictlyRightOf{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part
    /// of an EF Core LINQ query.
    /// </exception>
    public static bool IsStrictlyRightOf<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsStrictlyRightOf)));

    /// <summary>
    /// Determines whether a multirange is strictly to the right of a range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The range.</param>
    /// <returns><value>true</value> if the multirange is strictly to the right of the range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsStrictlyRightOf{T}(NpgsqlRange{T}[], NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an EF
    /// Core LINQ query.
    /// </exception>
    public static bool IsStrictlyRightOf<T>(this NpgsqlRange<T>[] multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsStrictlyRightOf)));

    /// <summary>
    /// Determines whether a multirange is strictly to the right of a range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The range.</param>
    /// <returns><value>true</value> if the multirange is strictly to the right of the range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsStrictlyRightOf{T}(List{NpgsqlRange{T}}, NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an
    /// EF Core LINQ query.
    /// </exception>
    public static bool IsStrictlyRightOf<T>(this List<NpgsqlRange<T>> multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsStrictlyRightOf)));

    #endregion IsStrictlyRightOf

    #region DoesNotExtendLeftOf

    /// <summary>
    /// Determines whether a multirange does not extend to the left of another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>
    /// <value>true</value> if the first multirange does not extend to the left of the multirange; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DoesNotExtendLeftOf{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an
    /// EF Core LINQ query.
    /// </exception>
    public static bool DoesNotExtendLeftOf<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DoesNotExtendLeftOf)));

    /// <summary>
    /// Determines whether a multirange does not extend to the left of another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>
    /// <value>true</value> if the first multirange does not extend to the left of the multirange; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DoesNotExtendLeftOf{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as
    /// part of an EF Core LINQ query.
    /// </exception>
    public static bool DoesNotExtendLeftOf<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DoesNotExtendLeftOf)));

    /// <summary>
    /// Determines whether a multirange does not extend to the left of a range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The multirange.</param>
    /// <returns><value>true</value> if the multirange does not extend to the left of the range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DoesNotExtendLeftOf{T}(NpgsqlRange{T}[], NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an
    /// EF Core LINQ query.
    /// </exception>
    public static bool DoesNotExtendLeftOf<T>(this NpgsqlRange<T>[] multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DoesNotExtendLeftOf)));

    /// <summary>
    /// Determines whether a multirange does not extend to the left of a range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The multirange.</param>
    /// <returns><value>true</value> if the multirange does not extend to the left of the range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DoesNotExtendLeftOf{T}(List{NpgsqlRange{T}}, NpgsqlRange{T})" /> is only intended for use via SQL translation as part of
    /// an EF Core LINQ query.
    /// </exception>
    public static bool DoesNotExtendLeftOf<T>(this List<NpgsqlRange<T>> multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DoesNotExtendLeftOf)));

    #endregion DoesNotExtendLeftOf

    #region DoesNotExtendRightOf

    /// <summary>
    /// Determines whether a multirange does not extend to the right of another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>
    /// <value>true</value> if the first multirange does not extend to the right of the multirange; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DoesNotExtendRightOf{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of
    /// an EF Core LINQ query.
    /// </exception>
    public static bool DoesNotExtendRightOf<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DoesNotExtendRightOf)));

    /// <summary>
    /// Determines whether a multirange does not extend to the right of another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>
    /// <value>true</value> if the first multirange does not extend to the right of the multirange; otherwise, <value>false</value>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DoesNotExtendRightOf{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as
    /// part of an EF Core LINQ query.
    /// </exception>
    public static bool DoesNotExtendRightOf<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DoesNotExtendRightOf)));

    /// <summary>
    /// Determines whether a multirange does not extend to the right of a range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The multirange.</param>
    /// <returns><value>true</value> if the multirange does not extend to the right of the range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DoesNotExtendRightOf{T}(NpgsqlRange{T}[], NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an
    /// EF Core LINQ query.
    /// </exception>
    public static bool DoesNotExtendRightOf<T>(this NpgsqlRange<T>[] multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DoesNotExtendRightOf)));

    /// <summary>
    /// Determines whether a multirange does not extend to the right of a range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The multirange.</param>
    /// <returns><value>true</value> if the multirange does not extend to the right of the range; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="DoesNotExtendRightOf{T}(List{NpgsqlRange{T}}, NpgsqlRange{T})" /> is only intended for use via SQL translation as part of
    /// an EF Core LINQ query.
    /// </exception>
    public static bool DoesNotExtendRightOf<T>(this List<NpgsqlRange<T>> multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DoesNotExtendRightOf)));

    #endregion DoesNotExtendRightOf

    #region IsAdjacentTo

    /// <summary>
    /// Determines whether a multirange is adjacent to another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns><value>true</value> if the multiranges are adjacent; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsAdjacentTo{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static bool IsAdjacentTo<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsAdjacentTo)));

    /// <summary>
    /// Determines whether a multirange is adjacent to another multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns><value>true</value> if the multiranges are adjacent; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsAdjacentTo{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part of
    /// an EF Core LINQ query.
    /// </exception>
    public static bool IsAdjacentTo<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsAdjacentTo)));

    /// <summary>
    /// Determines whether a multirange is adjacent to a range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The range.</param>
    /// <returns><value>true</value> if the multirange and range are adjacent; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsAdjacentTo{T}(NpgsqlRange{T}[], NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static bool IsAdjacentTo<T>(this NpgsqlRange<T>[] multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsAdjacentTo)));

    /// <summary>
    /// Determines whether a multirange is adjacent to a range.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <param name="range">The range.</param>
    /// <returns><value>true</value> if the multirange and range are adjacent; otherwise, <value>false</value>.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="IsAdjacentTo{T}(List{NpgsqlRange{T}}, NpgsqlRange{T})" /> is only intended for use via SQL translation as part of an EF
    /// Core LINQ query.
    /// </exception>
    public static bool IsAdjacentTo<T>(this List<NpgsqlRange<T>> multirange, NpgsqlRange<T> range)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsAdjacentTo)));

    #endregion IsAdjacentTo

    #region Union

    /// <summary>
    /// Returns the set union, which means unique elements that appear in either of two multiranges.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>A multirange containing the unique elements that appear in either multirange.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Union{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an EF Core LINQ
    /// query.
    /// </exception>
    public static NpgsqlRange<T>[] Union<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Union)));

    /// <summary>
    /// Returns the set union, which means unique elements that appear in either of two multiranges.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>A multirange containing the unique elements that appear in either multirange.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Union{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part of an EF
    /// Core LINQ query.
    /// </exception>
    public static List<NpgsqlRange<T>> Union<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Union)));

    #endregion Union

    #region Intersect

    /// <summary>
    /// Returns the set intersection, which means elements that appear in each of two multiranges.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>A multirange containing the elements that appear in both ranges.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Intersect{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an EF Core
    /// LINQ query.
    /// </exception>
    public static NpgsqlRange<T>[] Intersect<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Intersect)));

    /// <summary>
    /// Returns the set intersection, which means elements that appear in each of two multiranges.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>A multirange containing the elements that appear in both ranges.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Intersect{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part of an
    /// EF Core LINQ query.
    /// </exception>
    public static List<NpgsqlRange<T>> Intersect<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Intersect)));

    #endregion Intersect

    #region Except

    /// <summary>
    /// Returns the set difference, which means the elements of one multirange that do not appear in a second multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>A multirange containing the elements that appear in the first range, but not the second range.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Except{T}(NpgsqlRange{T}[], NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an EF Core LINQ
    /// query.
    /// </exception>
    public static NpgsqlRange<T>[] Except<T>(this NpgsqlRange<T>[] multirange1, NpgsqlRange<T>[] multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Except)));

    /// <summary>
    /// Returns the set difference, which means the elements of one multirange that do not appear in a second multirange.
    /// </summary>
    /// <param name="multirange1">The first multirange.</param>
    /// <param name="multirange2">The second multirange.</param>
    /// <returns>A multirange containing the elements that appear in the first range, but not the second range.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Except{T}(List{NpgsqlRange{T}}, List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part of an EF
    /// Core LINQ query.
    /// </exception>
    public static List<NpgsqlRange<T>> Except<T>(this List<NpgsqlRange<T>> multirange1, List<NpgsqlRange<T>> multirange2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Except)));

    #endregion Except

    #region Merge

    /// <summary>
    /// Computes the smallest range that includes the entire multirange.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <returns>The smallest range that includes the entire multirange.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Merge{T}(NpgsqlRange{T}[])" /> is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlRange<T> Merge<T>(this NpgsqlRange<T>[] multirange)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Merge)));

    /// <summary>
    /// Computes the smallest range that includes the entire multirange.
    /// </summary>
    /// <param name="multirange">The multirange.</param>
    /// <returns>The smallest range that includes the entire multirange.</returns>
    /// <exception cref="NotSupportedException">
    /// <see cref="Merge{T}(List{NpgsqlRange{T}})" /> is only intended for use via SQL translation as part of an EF
    /// Core LINQ query.
    /// </exception>
    public static NpgsqlRange<T> Merge<T>(this List<NpgsqlRange<T>> multirange)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Merge)));

    #endregion Merge
}
