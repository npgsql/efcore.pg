using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Metadata
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
