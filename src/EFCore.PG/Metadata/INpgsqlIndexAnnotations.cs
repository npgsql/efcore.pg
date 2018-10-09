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
        /// The PostgreSQL index operators to be used.
        /// </summary>
        /// <remarks>
        /// https://www.postgresql.org/docs/current/static/indexes-opclass.html
        /// </remarks>
        string[] Operators { get; }
    }
}
