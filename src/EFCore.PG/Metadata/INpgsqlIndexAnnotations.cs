using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public interface INpgsqlIndexAnnotations : IRelationalIndexAnnotations
    {
        /// <summary>
        /// The method to be used, or <c>null</c> if it hasn't been specified. <c>null</c> selects the default (currently <c>btree</c>).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        string Method { get; }

        /// <summary>
        /// The column operators to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        IReadOnlyList<string> Operators { get; }

        /// <summary>
        /// The column collations to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-collations.html
        /// </remarks>
        IReadOnlyList<string> Collation { get; }

        /// <summary>
        /// The column sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        IReadOnlyList<SortOrder> SortOrder { get; }

        /// <summary>
        /// The column NULL sort orders to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-ordering.html
        /// </remarks>
        IReadOnlyList<NullSortOrder> NullSortOrder { get; }

        /// <summary>
        /// The included property names, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        IReadOnlyList<string> IncludeProperties { get; }
    }
}
