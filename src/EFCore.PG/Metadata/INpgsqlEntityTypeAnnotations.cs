using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public interface INpgsqlEntityTypeAnnotations : IRelationalEntityTypeAnnotations
    {
        bool SetStorageParameter(string parameterName, object parameterValue);
        Dictionary<string, object> GetStorageParameters();
        string Comment { get; }
        CockroachDbInterleaveInParent CockroachDbInterleaveInParent { get; }
    }
}
