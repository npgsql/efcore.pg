using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides Npgsql-specific extension methods on <see cref="DbFunctions" />.
/// </summary>
public static class NpgsqlDbFunctionsExtensions
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    ///     An implementation of the PostgreSQL ILIKE operation, which is an insensitive LIKE.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="matchExpression">The string that is to be matched.</param>
    /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
    /// <returns><see langword="true" /> if there is a match.</returns>
    public static bool ILike(this DbFunctions _, string matchExpression, string pattern)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ILike)));

    // ReSharper disable once InconsistentNaming
    /// <summary>
    ///     An implementation of the PostgreSQL ILIKE operation, which is an insensitive LIKE.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="matchExpression">The string that is to be matched.</param>
    /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
    /// <param name="escapeCharacter">
    ///     The escape character (as a single character string) to use in front of %,_,[,],^
    ///     if they are not used as wildcards.
    /// </param>
    /// <returns><see langword="true" /> if there is a match.</returns>
    public static bool ILike(this DbFunctions _, string matchExpression, string pattern, string escapeCharacter)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ILike)));

    /// <summary>
    ///     Splits <paramref name="value" /> at occurrences of delimiter and forms the resulting fields into a text array.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="value">The string to be split.</param>
    /// <param name="delimiter">
    ///     If <c>null</c>, each character in the string will become a separate element in the array.
    ///     If an empty string, the string is treated as a single field.
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
    public static string[] StringToArray(this DbFunctions _, string value, string delimiter)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StringToArray)));

    /// <summary>
    ///     Splits <paramref name="value" /> at occurrences of delimiter and forms the resulting fields into a text array.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="value">The string to be split.</param>
    /// <param name="delimiter">
    ///     If <c>null</c>, each character in the string will become a separate element in the array.
    ///     If an empty string, the string is treated as a single field.
    /// </param>
    /// <param name="nullString">Fields matching this value string are replaced by <c>null</c>.</param>
    public static string[] StringToArray(this DbFunctions _, string value, string delimiter, string nullString)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(StringToArray)));

    /// <summary>
    ///     Reverses a string by calling PostgreSQL <c>reverse()</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="value">The string that is to be reversed.</param>
    /// <returns>The reversed string.</returns>
    public static string Reverse(this DbFunctions _, string value)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Reverse)));

    /// <summary>
    ///     Returns whether the row value represented by <paramref name="a" /> is greater than the row value represented by
    ///     <paramref name="b" />.
    /// </summary>
    /// <remarks>
    ///     For more information on row value comparisons, see
    ///     <see href="https://www.postgresql.org/docs/current/functions-comparisons.html#ROW-WISE-COMPARISON">
    ///         the PostgreSQL documentation.
    ///     </see>
    /// </remarks>
    public static bool GreaterThan(this DbFunctions _, ITuple a, ITuple b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GreaterThan)));

    /// <summary>
    ///     Returns whether the row value represented by <paramref name="a" /> is less than the row value represented by <paramref name="b" />.
    /// </summary>
    /// <remarks>
    ///     For more information on row value comparisons, see
    ///     <see href="https://www.postgresql.org/docs/current/functions-comparisons.html#ROW-WISE-COMPARISON">
    ///         the PostgreSQL documentation.
    ///     </see>
    /// </remarks>
    public static bool LessThan(this DbFunctions _, ITuple a, ITuple b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LessThan)));

    /// <summary>
    ///     Returns whether the row value represented by <paramref name="a" /> is greater than or equal to the row value represented by
    ///     <paramref name="b" />.
    /// </summary>
    /// <remarks>
    ///     For more information on row value comparisons, see
    ///     <see href="https://www.postgresql.org/docs/current/functions-comparisons.html#ROW-WISE-COMPARISON">
    ///         the PostgreSQL documentation.
    ///     </see>
    /// </remarks>
    public static bool GreaterThanOrEqual(this DbFunctions _, ITuple a, ITuple b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GreaterThanOrEqual)));

    /// <summary>
    ///     Returns whether the row value represented by <paramref name="a" /> is less than or equal to the row value represented by
    ///     <paramref name="b" />.
    /// </summary>
    /// <remarks>
    ///     For more information on row value comparisons, see
    ///     <see href="https://www.postgresql.org/docs/current/functions-comparisons.html#ROW-WISE-COMPARISON">
    ///         the PostgreSQL documentation.
    ///     </see>
    /// </remarks>
    public static bool LessThanOrEqual(this DbFunctions _, ITuple a, ITuple b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LessThanOrEqual)));

    /// <summary>
    ///     Returns the distance between two dates as a number of days, particularly suitable for sorting where the appropriate index is
    ///     defined.
    /// </summary>
    /// <remarks>
    ///     This requires the <c>btree_gist</c> built-in PostgreSQL extension, see
    ///     <see href="https://www.postgresql.org/docs/current/btree-gist.html" />.
    /// </remarks>
    public static int Distance(this DbFunctions _, DateOnly a, DateOnly b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Distance)));

    /// <summary>
    ///     Returns the distance between two timestamps as a PostgreSQL <c>interval</c>, particularly suitable for sorting where the appropriate
    ///     index is defined.
    /// </summary>
    /// <remarks>
    ///     This requires the <c>btree_gist</c> built-in PostgreSQL extension, see
    ///     <see href="https://www.postgresql.org/docs/current/btree-gist.html" />.
    /// </remarks>
    public static TimeSpan Distance(this DbFunctions _, DateTime a, DateTime b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Distance)));

    /// <summary>
    ///     Converts string to date according to the given format.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="value">The string to be converted.</param>
    /// <param name="format">The format of the input date.</param>
    /// <see href="https://www.postgresql.org/docs/current/functions-formatting.html" />
    public static DateOnly ToDate(this DbFunctions _, string value, string format)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToDate)));

    /// <summary>
    ///     Converts string to time stamp according to the given format.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="value">The string to be converted</param>
    /// <param name="format">The format of the input date</param>
    /// <see href="https://www.postgresql.org/docs/current/functions-formatting.html" />
    public static DateTime ToTimestamp(this DbFunctions _, string value, string format)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ToTimestamp)));
}
