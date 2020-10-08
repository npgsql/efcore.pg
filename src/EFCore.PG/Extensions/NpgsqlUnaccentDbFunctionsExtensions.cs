using System;
using Microsoft.EntityFrameworkCore.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides Npgsql unaccent extension methods on <see cref="DbFunctions"/>.
    /// </summary>
    public static class NpgsqlUnaccentDbFunctionsExtensions
    {
        /// <summary>
        /// Returns a new string that removes diacritics from characters in the given <paramref name="text" />.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="regDictionary">A specific text search dictionary.</param>
        /// <param name="text">The text to remove the diacritics.</param>
        /// <remarks>
        /// <para>The method call is translated to <c>unaccent(regdictionary, text)</c>.</para>
        /// 
        /// See https://www.postgresql.org/docs/current/unaccent.html.
        /// </remarks>
        /// <returns>A string without diacritics.</returns>
        public static string Unaccent(this DbFunctions _, string regDictionary, string text)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Unaccent)));

        /// <summary>
        /// Returns a new string that removes diacritics from characters in the given <paramref name="text" />.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="text">The text to remove the diacritics.</param>
        /// <remarks>
        /// <para>The method call is translated to <c>unaccent(text)</c>.</para>
        ///
        /// See https://www.postgresql.org/docs/current/unaccent.html.
        /// </remarks>
        /// <returns>A string without diacritics.</returns>
        public static string Unaccent(this DbFunctions _, string text)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Unaccent)));
    }
}
