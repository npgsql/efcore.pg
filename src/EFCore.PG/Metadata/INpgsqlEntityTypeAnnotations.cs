using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface INpgsqlEntityTypeAnnotations : IRelationalEntityTypeAnnotations
    {
        bool SetStorageParameter(string parameterName, object parameterValue);
        Dictionary<string, object> GetStorageParameters();
        string Comment { get; }
        CockroachDbInterleaveInParent CockroachDbInterleaveInParent { get; }
    }
}
