using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <remarks>
    /// This mapping is only used in Npgsql 3.2 and below.
    /// Later versions use type plugins to set up mappings, and corresponding EF Core
    /// plugins need to be used.
    /// </remarks>
    public class NpgsqlLegacyPostgisTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlLegacyPostgisTypeMapping() : base("geometry", typeof(PostgisGeometry), NpgsqlDbType.Geometry) {}
    }
}
