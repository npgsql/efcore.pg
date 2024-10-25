namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides extension methods supporting array function translation for PostgreSQL.
/// </summary>
public static class NpgsqlArrayDbFunctionsExtensions
{
    /// <summary>
    ///     Removes all elements equal to the given value from the PostgreSQL array, so it is possible to remove NULLs.
    ///     Corresponds to the PostgreSQL <c>array_remove</c> array function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="input">The input an array to searched.</param>
    /// <param name="value">The value must be removed from array.</param>
    /// <returns>The new array without value.</returns>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-array.html">PostgreSQL documentation for array functions.</seealso>
    public static T[] ArrayRemove<T>(this DbFunctions _, IEnumerable<T> input, T value)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ArrayRemove)));
}