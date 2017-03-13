using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IPostgresExtension
    {
        IAnnotatable Annotatable { get; }
        string Name { get; }
        string Schema { get; }
        string Version { get; }
    }
}
