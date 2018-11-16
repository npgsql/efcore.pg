using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public interface INpgsqlIndexAnnotations : IRelationalIndexAnnotations
    {
        /// <summary>
        /// The PostgreSQL index method to be used. Null selects the default (currently btree).
        /// </summary>
        /// <remarks>
        /// http://www.postgresql.org/docs/current/static/sql-createindex.html
        /// </remarks>
        string Method { get; }

        /// <summary>
        /// The PostgreSQL index operators to be used, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        IReadOnlyList<string> Operators { get; }

        /// <summary>
        /// The PostgreSQL included property names, or <c>null</c> if they have not been specified.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/sql-createindex.html
        /// </remarks>
        IReadOnlyList<string> IncludeProperties { get; }
    }
}
