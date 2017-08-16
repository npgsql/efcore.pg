using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    enum PostgresTypeType
    {
        Base,
        Array,
        Range,
        Enum,
        Composite
    }
}
