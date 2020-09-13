using System;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlUnaccentDbFunctionsExtensions
    {
        /// <summary>
        /// Returns a new string that removes diacriatics from characters in the given <paramref name="text" />.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>unaccent(regdictionary, text)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/unaccent.html.
        /// </remarks>
        public static string Unaccent(this DbFunctions _, string regdictionary, string text)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Unaccent)));

        /// <summary>
        /// Returns a new string that removes diacriatics from characters in the given <paramref name="text" />.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>unaccent(regdictionary, text)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/unaccent.html.
        /// </remarks>
        public static string Unaccent(this DbFunctions _, string text)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Unaccent)));
    }
}
