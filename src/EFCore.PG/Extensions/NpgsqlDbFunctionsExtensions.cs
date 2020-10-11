using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
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
        public static bool ILike(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string matchExpression,
            [CanBeNull] string pattern)
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
        public static bool ILike(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string matchExpression,
            [CanBeNull] string pattern,
            [CanBeNull] string escapeCharacter)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ILike)));

        /// <summary>
        /// Reverses a string by calling PostgreSQL <c>reverse()</c>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="value">The string that is to be reversed.</param>
        /// <returns>The reversed string.</returns>
        public static string Reverse([CanBeNull] this DbFunctions _, [CanBeNull] string value)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Reverse)));

        public static string StringAggregate<TSource, TResult>(
            [CanBeNull] this DbFunctions _,
            [NotNull] IEnumerable<TSource> source,
            [NotNull] Expression<Func<TSource, TResult>> selector)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Reverse)));
    }
}
