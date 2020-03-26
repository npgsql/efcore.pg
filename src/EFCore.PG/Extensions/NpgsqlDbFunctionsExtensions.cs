using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="matchExpression">The string that is to be matched.</param>
        /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
        /// <returns>true if there is a match.</returns>
        public static bool ILike(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string matchExpression,
            [CanBeNull] string pattern)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ILike)));

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// An implementation of the PostgreSQL ILIKE operation, which is an insensitive LIKE.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="matchExpression">The string that is to be matched.</param>
        /// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
        /// <param name="escapeCharacter">
        /// The escape character (as a single character string) to use in front of %,_,[,],^
        /// if they are not used as wildcards.
        /// </param>
        /// <returns>true if there is a match.</returns>
        public static bool ILike(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] string matchExpression,
            [CanBeNull] string pattern,
            [CanBeNull] string escapeCharacter)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ILike)));

        /// <summary>
        /// <p>
        /// Explicitly specifies a collection to be used in a LINQ query. Can be used to generate fragments such as
        /// <code>WHERE customer.name COLLATE 'de_DE' = 'John Doe'</code>
        /// </p>
        /// <p>
        /// The collation must already exist in the database (see
        /// <see cref="NpgsqlModelBuilderExtensions.HasCollation(Microsoft.EntityFrameworkCore.ModelBuilder,string,string,string,System.Nullable{bool})" />).
        /// </p>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="operand">The operand on which to apply the collation.</param>
        /// <param name="collation">The name of the collection, which must already exist in the database.</param>
        public static T ApplyCollation<T>(
            [NotNull] this DbFunctions _,
            [NotNull] T operand,
            [NotNull] string collation)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ApplyCollation)));
    }
}
