using System;

namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlTrgmDbFunctionsExtensions
    {
        /// <summary>
        /// show_trgm(text)
        /// 
        /// Returns an array of all the trigrams in the given <paramref name="text" />.
        /// (In practice this is seldom useful except for debugging.)
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static string[] Trigrams(this DbFunctions _, string text) =>
            throw new NotSupportedException();

        /// <summary>
        /// similarity(text, text)
        /// 
        /// Returns a number that indicates how similar the two arguments are.
        /// The range of the result is zero (indicating that the two strings are
        /// completely dissimilar) to one (indicating that the two strings are identical).
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static double Similarity(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// word_similarity(text, text)
        ///
        /// Returns a number that indicates the greatest similarity between the set of trigrams
        /// in the first string and any continuous extent of an ordered set of trigrams
        /// in the second string.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static double WordSimilarity(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// strict_word_similarity(text, text)
        ///
        /// Same as word_similarity(text, text), but forces extent boundaries to match word boundaries.
        /// Since we don't have cross-word trigrams, this function actually returns greatest similarity
        /// between first string and any continuous extent of words of the second string.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static double StrictWordSimilarity(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// % (similarity_op)
        ///
        /// Returns true if its arguments have a similarity that is greater than the current similarity
        /// threshold set by pg_trgm.similarity_threshold.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static bool Similar(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// &lt;% (word_similarity_op)
        ///
        /// Returns true if the similarity between the trigram set in the first argument and a continuous
        /// extent of an ordered trigram set in the second argument is greater than the current word similarity
        /// threshold set by pg_trgm.word_similarity_threshold parameter.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static bool WordSimilar(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// %&gt; (word_similarity_commutator_op)
        ///
        /// Commutator of the &lt;% operator.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static bool WordSimilarInverted(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// &lt;&lt;% (strict_word_similarity_op)
        ///
        /// Returns true if its second argument has a continuous extent of an ordered trigram set that
        /// matches word boundaries, and its similarity to the trigram set of the first argument is greater
        /// than the current strict word similarity threshold set by the pg_trgm.strict_word_similarity_threshold
        /// parameter.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static bool StrictWordSimilar(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// %&gt;&gt; (strict_word_similarity_commutator_op)
        ///
        /// Commutator of the &lt;&lt;% operator.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static bool StrictWordSimilarInverted(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// &lt;-&gt; (similarity_dist)
        ///
        /// Returns the "distance" between the arguments, that is one minus the similarity() value.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static double SimilarityDistance(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// &lt;&lt;-&gt; (word_similarity_dist_op)
        ///
        /// Returns the "distance" between the arguments, that is one minus the word_similarity() value.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static double WordSimilarityDistance(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// &lt;-&gt;&gt; (word_similarity_dist_commutator_op)
        ///
        /// Commutator of the &lt;&lt;-&gt; operator.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static double WordSimilarityDistanceInverted(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// &lt;&lt;&lt;-&gt; (strict_word_similarity_dist_commutator_op)
        ///
        /// Returns the "distance" between the arguments, that is one minus the strict_word_similarity() value.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static double StrictWordSimilarityDistance(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();

        /// <summary>
        /// &lt;-&gt;&gt;&gt; (strict_word_similarity_dist_op)
        ///
        /// Commutator of the &lt;&lt;&lt;-&gt; operator.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </remarks>
        public static double StrictWordSimilarityDistanceInverted(this DbFunctions _, string source, string target) =>
            throw new NotSupportedException();
    }
}