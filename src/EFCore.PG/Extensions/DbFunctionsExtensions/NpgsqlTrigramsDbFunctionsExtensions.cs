// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
///     The methods on this class are accessed via <see cref="EF.Functions" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>.
/// </remarks>
public static class NpgsqlTrigramsDbFunctionsExtensions
{
    /// <summary>
    /// Returns an array of all the trigrams in the given <paramref name="text" />.
    /// (In practice this is seldom useful except for debugging.)
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>show_trgm(text)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static string[] TrigramsShow(this DbFunctions _, string text)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsShow)));

    /// <summary>
    /// Returns a number that indicates how similar the two arguments are.
    /// The range of the result is zero (indicating that the two strings are
    /// completely dissimilar) to one (indicating that the two strings are identical).
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>similarity(source, target)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static double TrigramsSimilarity(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsSimilarity)));

    /// <summary>
    /// Returns a number that indicates the greatest similarity between the set of trigrams
    /// in the first string and any continuous extent of an ordered set of trigrams
    /// in the second string.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>word_similarity(source, target)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static double TrigramsWordSimilarity(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsWordSimilarity)));

    /// <summary>
    /// Same as word_similarity(text, text), but forces extent boundaries to match word boundaries.
    /// Since we don't have cross-word trigrams, this function actually returns greatest similarity
    /// between first string and any continuous extent of words of the second string.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>strict_word_similarity(source, target)</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static double TrigramsStrictWordSimilarity(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsStrictWordSimilarity)));

    /// <summary>
    /// Returns true if its arguments have a similarity that is greater than the current similarity
    /// threshold set by pg_trgm.similarity_threshold.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>source % target</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static bool TrigramsAreSimilar(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsAreSimilar)));

    /// <summary>
    /// Returns true if the similarity between the trigram set in the first argument and a continuous
    /// extent of an ordered trigram set in the second argument is greater than the current word similarity
    /// threshold set by pg_trgm.word_similarity_threshold parameter.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>source &lt;% target</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static bool TrigramsAreWordSimilar(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsAreWordSimilar)));

    /// <summary>
    /// Commutator of the &lt;% operator.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>source %&gt; target</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static bool TrigramsAreNotWordSimilar(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsAreNotWordSimilar)));

    /// <summary>
    /// Returns true if its second argument has a continuous extent of an ordered trigram set that
    /// matches word boundaries, and its similarity to the trigram set of the first argument is greater
    /// than the current strict word similarity threshold set by the pg_trgm.strict_word_similarity_threshold
    /// parameter.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>source &lt;&lt;% target</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static bool TrigramsAreStrictWordSimilar(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsAreStrictWordSimilar)));

    /// <summary>
    /// Commutator of the &lt;&lt;% operator.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>source %&gt;&gt; target</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static bool TrigramsAreNotStrictWordSimilar(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsAreNotStrictWordSimilar)));

    /// <summary>
    /// Returns the "distance" between the arguments, that is one minus the similarity() value.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>source &lt;-&gt; target</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static double TrigramsSimilarityDistance(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsSimilarityDistance)));

    /// <summary>
    /// Returns the "distance" between the arguments, that is one minus the word_similarity() value.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>source &lt;&lt;-&gt; target</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static double TrigramsWordSimilarityDistance(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsWordSimilarityDistance)));

    /// <summary>
    /// Commutator of the &lt;&lt;-&gt; operator.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>source &lt;-&gt;&gt; target</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static double TrigramsWordSimilarityDistanceInverted(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsWordSimilarityDistanceInverted)));

    /// <summary>
    /// Returns the "distance" between the arguments, that is one minus the strict_word_similarity() value.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>source &lt;&lt;&lt;-&gt; target</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static double TrigramsStrictWordSimilarityDistance(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsStrictWordSimilarityDistance)));

    /// <summary>
    /// Commutator of the &lt;&lt;&lt;-&gt; operator.
    /// </summary>
    /// <remarks>
    /// The method call is translated to <c>source &lt;-&gt;&gt;&gt; target</c>.
    ///
    /// See https://www.postgresql.org/docs/current/pgtrgm.html.
    /// </remarks>
    public static double TrigramsStrictWordSimilarityDistanceInverted(this DbFunctions _, string source, string target)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(TrigramsStrictWordSimilarityDistanceInverted)));
}
