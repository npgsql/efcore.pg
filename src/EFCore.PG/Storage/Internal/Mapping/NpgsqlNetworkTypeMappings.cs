using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class NpgsqlMacaddrTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlMacaddrTypeMapping() : base("macaddr", typeof(PhysicalAddress), NpgsqlDbType.MacAddr) {}
    }

    public class NpgsqlInetTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlInetTypeMapping() : base("inet", typeof(IPAddress), NpgsqlDbType.Inet) {}
    }
}
