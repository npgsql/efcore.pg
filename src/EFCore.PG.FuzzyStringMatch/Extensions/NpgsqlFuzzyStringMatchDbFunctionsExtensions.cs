using System;

namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlFuzzyStringMatchDbFunctionsExtensions
    {
        /// <summary>
        /// The soundex function converts a string to its Soundex code.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>soundex(text)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
        /// </remarks>
        public static string FuzzyStringMatchSoundex(this DbFunctions _, string text) =>
            throw new NotSupportedException();

        /// <summary>
        /// The difference function converts two strings to their Soundex codes and
        /// then returns the number of matching code positions. Since Soundex codes
        /// have four characters, the result ranges from zero to four, with zero being
        /// no match and four being an exact match.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>difference(source, target)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
        /// </remarks>
        public static int FuzzyStringMatchDifference(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// Returns the Levenshtein distance between two strings.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>levenshtein(source, target)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
        /// </remarks>
        public static int FuzzyStringMatchLevenshtein(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// Returns the Levenshtein distance between two strings.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>levenshtein(source, target, ins_cost, del_cost, sub_cost)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
        /// </remarks>
        public static int FuzzyStringMatchLevenshtein(this DbFunctions _, string source, string target, int ins_cost, int del_cost, int sub_cost) =>
            throw new NotSupportedException();

        /// <summary>
        /// levenshtein_less_equal is an accelerated version of the Levenshtein function for use when only small distances are of interest.
        /// If the actual distance is less than or equal to max_d, then levenshtein_less_equal returns the correct distance;
        /// otherwise it returns some value greater than max_d. If max_d is negative then the behavior is the same as levenshtein.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>levenshtein_less_equal(source, target, max_d)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
        /// </remarks>
        public static int FuzzyStringMatchLevenshteinLessEqual(this DbFunctions _, string source, string target, int max_d) =>
            throw new NotSupportedException();

        /// <summary>
        /// levenshtein_less_equal is an accelerated version of the Levenshtein function for use when only small distances are of interest.
        /// If the actual distance is less than or equal to max_d, then levenshtein_less_equal returns the correct distance;
        /// otherwise it returns some value greater than max_d. If max_d is negative then the behavior is the same as levenshtein.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>levenshtein_less_equal(source, target, ins_cost, del_cost, sub_cost, max_d)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
        /// </remarks>
        public static int FuzzyStringMatchLevenshteinLessEqual(this DbFunctions _, string source, string target, int ins_cost, int del_cost, int sub_cost, int max_d) =>
            throw new NotSupportedException();

        /// <summary>
        /// The metaphone function converts a string to its Metaphone code.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>metaphone(text, max_output_length)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
        /// </remarks>
        public static string FuzzyStringMatchMetaphone(this DbFunctions _, string text, int max_output_length) =>
            throw new NotSupportedException();

        /// <summary>
        /// The dmetaphone function converts a string to its primary Double Metaphone code.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>dmetaphone(text)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
        /// </remarks>
        public static string FuzzyStringMatchDoubleMetaphone(this DbFunctions _, string text) =>
            throw new NotSupportedException();

        /// <summary>
        /// The dmetaphone_alt function converts a string to its alternate Double Metaphone code.
        /// </summary>
        /// <remarks>
        /// The method call is translated to <c>dmetaphone_alt(text)</c>.
        ///
        /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
        /// </remarks>
        public static string FuzzyStringMatchDoubleMetaphoneAlt(this DbFunctions _, string text) =>
            throw new NotSupportedException();
    }
}
