using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Npgsql specific extension methods for <see cref="IndexBuilder" />.
    /// </summary>
    public static class NpgsqlIndexBuilderExtensions
    {
        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="method"> The name of the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlHasMethod([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string method)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(method, nameof(method));

            indexBuilder.Metadata.Npgsql().Method = method;

            return indexBuilder;
        }

        /// <summary>
        /// The PostgreSQL index operators to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="operators"> The operators to use for each column. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForNpgsqlHasOperators([NotNull] this IndexBuilder indexBuilder, [CanBeNull] params string[] operators)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(operators, nameof(operators));

            indexBuilder.Metadata.Npgsql().Operators = operators;

            return indexBuilder;
        }
    }
}
