// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides extension methods supporting NodaTime function translation for PostgreSQL.
/// </summary>
public static class NpgsqlNodaTimeDbFunctionsExtensions
{
    /// <summary>
    /// Computes the sum of the non-null input intervals. Corresponds to the PostgreSQL <c>sum</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input values to be summed.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static Period? Sum(this DbFunctions _, IEnumerable<Period> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Sum)));

    /// <summary>
    /// Computes the sum of the non-null input intervals. Corresponds to the PostgreSQL <c>sum</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input values to be summed.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static Duration? Sum(this DbFunctions _, IEnumerable<Duration> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Sum)));

    /// <summary>
    /// Computes the average (arithmetic mean) of the non-null input intervals. Corresponds to the PostgreSQL <c>avg</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input values to be computed into an average.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static Period? Average(this DbFunctions _, IEnumerable<Period> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Average)));

    /// <summary>
    /// Computes the average (arithmetic mean) of the non-null input intervals. Corresponds to the PostgreSQL <c>avg</c> aggregate function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input values to be computed into an average.</param>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-aggregate.html">PostgreSQL documentation for aggregate functions.</seealso>
    public static Duration? Average(this DbFunctions _, IEnumerable<Duration> input)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Average)));
}
