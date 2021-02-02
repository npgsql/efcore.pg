using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public interface INpgsqlTypeMapping
    {
        /// <summary>
        /// The database type used by Npgsql.
        /// </summary>
        NpgsqlDbType NpgsqlDbType { get; }
    }
}
