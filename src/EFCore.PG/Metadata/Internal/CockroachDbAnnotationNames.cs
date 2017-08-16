using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class CockroachDbAnnotationNames
    {
        public const string Prefix = NpgsqlAnnotationNames.Prefix + "CockroachDB:";

        public const string InterleaveInParent = Prefix + "InterleaveInParent";
    }
}
