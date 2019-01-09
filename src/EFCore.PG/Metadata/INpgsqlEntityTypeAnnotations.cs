using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public interface INpgsqlEntityTypeAnnotations : IRelationalEntityTypeAnnotations
    {
        bool SetStorageParameter(string parameterName, object parameterValue);
        Dictionary<string, object> GetStorageParameters();
        string Comment { get; }

        /// <summary>
        /// True to configure the entity to use an unlogged table; otherwise, false.
        /// </summary>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/sql-createtable.html
        /// </remarks>
        bool UnloggedTable { get; }

        CockroachDbInterleaveInParent CockroachDbInterleaveInParent { get; }
    }
}
