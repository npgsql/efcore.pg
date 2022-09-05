// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
///     The methods on this class are accessed via <see cref="EF.Functions" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>.
/// </remarks>
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
    public static string FuzzyStringMatchSoundex(this DbFunctions _, string text)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FuzzyStringMatchSoundex)));

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
    public static int FuzzyStringMatchDifference(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FuzzyStringMatchDifference)));

    /// <summary>
    /// Returns the Levenshtein distance between two strings.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>levenshtein(source, target)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
    /// </remarks>
    public static int FuzzyStringMatchLevenshtein(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FuzzyStringMatchLevenshtein)));

    /// <summary>
    /// Returns the Levenshtein distance between two strings.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>levenshtein(source, target, insertionCost, deletionCost, substitutionCost)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
    /// </remarks>
    public static int FuzzyStringMatchLevenshtein(
        this DbFunctions _, string source, string target, int insertionCost, int deletionCost, int substitutionCost)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FuzzyStringMatchLevenshtein)));

    /// <summary>
    /// levenshtein_less_equal is an accelerated version of the Levenshtein function for use when only small distances are of interest.
    /// If the actual distance is less than or equal to maximum distance, then levenshtein_less_equal returns the correct distance;
    /// otherwise it returns some value greater than maximum distance. If maximum distance is negative then the behavior is the same as levenshtein.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>levenshtein_less_equal(source, target, maximumDistance)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
    /// </remarks>
    public static int FuzzyStringMatchLevenshteinLessEqual(this DbFunctions _, string source, string target, int maximumDistance)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FuzzyStringMatchLevenshteinLessEqual)));

    /// <summary>
    /// levenshtein_less_equal is an accelerated version of the Levenshtein function for use when only small distances are of interest.
    /// If the actual distance is less than or equal to maximum distance, then levenshtein_less_equal returns the correct distance;
    /// otherwise it returns some value greater than maximum distance. If maximum distance is negative then the behavior is the same as levenshtein.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>levenshtein_less_equal(source, target, insertionCost, deletionCost, substitutionCost, maximumDistance)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
    /// </remarks>
    public static int FuzzyStringMatchLevenshteinLessEqual(
        this DbFunctions _, string source, string target, int insertionCost, int deletionCost, int substitutionCost,
        int maximumDistance)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FuzzyStringMatchLevenshteinLessEqual)));

    /// <summary>
    /// The metaphone function converts a string to its Metaphone code.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>metaphone(text, maximumOutputLength)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
    /// </remarks>
    public static string FuzzyStringMatchMetaphone(this DbFunctions _, string text, int maximumOutputLength)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FuzzyStringMatchMetaphone)));

    /// <summary>
    /// The dmetaphone function converts a string to its primary Double Metaphone code.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>dmetaphone(text)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
    /// </remarks>
    public static string FuzzyStringMatchDoubleMetaphone(this DbFunctions _, string text)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FuzzyStringMatchDoubleMetaphone)));

    /// <summary>
    /// The dmetaphone_alt function converts a string to its alternate Double Metaphone code.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>dmetaphone_alt(text)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/fuzzystrmatch.html.
    /// </remarks>
    public static string FuzzyStringMatchDoubleMetaphoneAlt(this DbFunctions _, string text)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(FuzzyStringMatchDoubleMetaphoneAlt)));
}
