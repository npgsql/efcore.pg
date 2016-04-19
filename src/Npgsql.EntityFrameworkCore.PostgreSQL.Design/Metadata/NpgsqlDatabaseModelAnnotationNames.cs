using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    class NpgsqlDatabaseModelAnnotationNames
    {
        public const string Prefix = "NpgsqlDatabaseModel:";
        public const string IsSerial = Prefix + "IsSerial";
        public const string Expression = Prefix + "Expression";
    }
}
