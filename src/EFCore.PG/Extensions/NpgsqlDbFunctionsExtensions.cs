using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides Npgsql-specific extension methods on <see cref="DbFunctions"/>.
    /// </summary>
    public static class NpgsqlDbFunctionsExtensions
    {
        #region ILike

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
            => ILikeCore(matchExpression, pattern, null);

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
            => ILikeCore(matchExpression, pattern, escapeCharacter);

        /// <remarks>
        /// Regex special chars defined here:
        /// https://msdn.microsoft.com/en-us/library/4edbef7e(v=vs.110).aspx
        /// </remarks>
        static readonly char[] RegexSpecialChars =
            { '.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?', '\\' };

        static readonly string DefaultEscapeRegexCharsPattern =
            BuildEscapeRegexCharsPattern(RegexSpecialChars);

        static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(value: 1000.0);

        static string BuildEscapeRegexCharsPattern(IEnumerable<char> regexSpecialChars)
            => string.Join("|", regexSpecialChars.Select(c => @"\" + c));

        // ReSharper disable once InconsistentNaming
        static bool ILikeCore(string matchExpression, string pattern, string escapeCharacter)
        {
            //TODO: this fixes https://github.com/aspnet/EntityFramework/issues/8656 by insisting that
            // the "escape character" is a string but just using the first character of that string,
            // but we may later want to allow the complete string as the "escape character"
            // in which case we need to change the way we construct the regex below.
            var singleEscapeCharacter =
                string.IsNullOrEmpty(escapeCharacter)
                    ? (char?)null
                    : escapeCharacter.First();

            if (matchExpression == null || pattern == null)
                return false;

            if (matchExpression.Equals(pattern))
                return true;

            if (matchExpression.Length == 0 || pattern.Length == 0)
                return false;

            var escapeRegexCharsPattern =
                singleEscapeCharacter == null
                    ? DefaultEscapeRegexCharsPattern
                    : BuildEscapeRegexCharsPattern(RegexSpecialChars.Where(c => c != singleEscapeCharacter));

            var regexPattern =
                Regex.Replace(
                    pattern,
                    escapeRegexCharsPattern,
                    c => @"\" + c,
                    default,
                    RegexTimeout);

            var stringBuilder = new StringBuilder();

            for (var i = 0; i < regexPattern.Length; i++)
            {
                var c = regexPattern[i];
                var escaped = i > 0 && regexPattern[i - 1] == singleEscapeCharacter;

                switch (c)
                {
                case '_':
                {
                    stringBuilder.Append(escaped ? '_' : '.');
                    break;
                }
                case '%':
                {
                    stringBuilder.Append(escaped ? "%" : ".*");
                    break;
                }
                default:
                {
                    if (c != singleEscapeCharacter)
                    {
                        stringBuilder.Append(c);
                    }

                    break;
                }
                }
            }

            regexPattern = stringBuilder.ToString();

            return Regex.IsMatch(
                matchExpression,
                @"\A" + regexPattern + @"\s*\z",
                RegexOptions.IgnoreCase | RegexOptions.Singleline,
                RegexTimeout);
        }

        #endregion

        #region NullIf

        /// <summary>
        /// Returns null if <paramref name="a"/> equals <paramref name="b"/>; otherwise, <paramref name="a"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="a">The first item.</param>
        /// <param name="b">The second item.</param>
        /// <returns>
        /// Null if <paramref name="a"/> equals <paramref name="b"/>; otherwise, <paramref name="a"/>.
        /// </returns>
        [CanBeNull]
        public static T NullIf<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] T a,
            [CanBeNull] T b)
            where T : class
            => Equals(a, b) ? default : a;

        /// <summary>
        /// Returns null if <paramref name="a"/> equals <paramref name="b"/>; otherwise, <paramref name="a"/>.
        /// </summary>
        /// <param name="_">The <see cref="DbFunctions"/> instance.</param>
        /// <param name="a">The first item.</param>
        /// <param name="b">The second item.</param>
        /// <returns>
        /// Null if <paramref name="a"/> equals <paramref name="b"/>; otherwise, <paramref name="a"/>.
        /// </returns>
        [CanBeNull]
        public static T? NullIf<T>(
            [CanBeNull] this DbFunctions _,
            [CanBeNull] T? a,
            [CanBeNull] T? b)
            where T : struct
            => Equals(a, b) ? default : a;

        #endregion
    }
}
