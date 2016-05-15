using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IPostgresExtension
    {
        IModel Model { get; }
        string Name { get; }
        string Schema { get; }
        string Version { get; }
    }
}
