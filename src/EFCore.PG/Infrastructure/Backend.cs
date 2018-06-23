using JetBrains.Annotations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure
{
    /// <summary>
    /// Describes the backend process that uses the PostgreSQL wire protocol.
    /// </summary>
    [PublicAPI]
    public enum Backend
    {
        /// <summary>
        /// The backend process is PostgreSQL.
        /// </summary>
        PostgreSQL = 0,

        /// <summary>
        /// The backend process is Amazon Redshift.
        /// </summary>
        Redshift,

        /// <summary>
        /// The backend process is CockroachDB.
        /// </summary>
        CockroachDB,

        /// <summary>
        /// The backend process is CrateDB.
        /// </summary>
        CrateDB
    }
}
