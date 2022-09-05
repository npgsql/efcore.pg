using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides Npgsql-specific extension methods on <see cref="DbFunctions"/>.
/// </summary>
public static class NpgsqlDbFunctionsExtensions
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// An implementation of the PostgreSQL ILIKE operation, which is an insensitive LIKE.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="matchExpression">The string that is to be matched.</param>
    /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
    /// <returns><see langword="true" /> if there is a match.</returns>
    public static bool ILike(this DbFunctions _, string matchExpression, string pattern)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ILike)));

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// An implementation of the PostgreSQL ILIKE operation, which is an insensitive LIKE.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="matchExpression">The string that is to be matched.</param>
    /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
    /// <param name="escapeCharacter">
    /// The escape character (as a single character string) to use in front of %,_,[,],^
    /// if they are not used as wildcards.
    /// </param>
    /// <returns><see langword="true" /> if there is a match.</returns>
    public static bool ILike(this DbFunctions _, string matchExpression, string pattern, string escapeCharacter)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ILike)));

    /// <summary>
    /// Reverses a string by calling PostgreSQL <c>reverse()</c>.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
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
    ///     <see href="https://www.postgresql.org/docs/current/functions-comparisons.html#ROW-WISE-COMPARISON">the PostgreSQL documentation
    ///     </see>.
    /// </remarks>
    public static bool GreaterThan(this DbFunctions _, ITuple a, ITuple b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GreaterThan)));

    /// <summary>
    ///     Returns whether the row value represented by <paramref name="a" /> is less than the row value represented by
    ///     <paramref name="b" />.
    /// </summary>
    /// <remarks>
    ///     For more information on row value comparisons, see
    ///     <see href="https://www.postgresql.org/docs/current/functions-comparisons.html#ROW-WISE-COMPARISON">the PostgreSQL documentation
    ///     </see>.
    /// </remarks>
    public static bool LessThan(this DbFunctions _, ITuple a, ITuple b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LessThan)));

    /// <summary>
    ///     Returns whether the row value represented by <paramref name="a" /> is greater than or equal to the row value represented by
    ///     <paramref name="b" />.
    /// </summary>
    /// <remarks>
    ///     For more information on row value comparisons, see
    ///     <see href="https://www.postgresql.org/docs/current/functions-comparisons.html#ROW-WISE-COMPARISON">the PostgreSQL documentation
    ///     </see>.
    /// </remarks>
    public static bool GreaterThanOrEqual(this DbFunctions _, ITuple a, ITuple b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GreaterThanOrEqual)));

    /// <summary>
    ///     Returns whether the row value represented by <paramref name="a" /> is less than or equal to the row value represented by
    ///     <paramref name="b" />.
    /// </summary>
    /// <remarks>
    ///     For more information on row value comparisons, see
    ///     <see href="https://www.postgresql.org/docs/current/functions-comparisons.html#ROW-WISE-COMPARISON">the PostgreSQL documentation
    ///     </see>.
    /// </remarks>
    public static bool LessThanOrEqual(this DbFunctions _, ITuple a, ITuple b)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LessThanOrEqual)));
}
