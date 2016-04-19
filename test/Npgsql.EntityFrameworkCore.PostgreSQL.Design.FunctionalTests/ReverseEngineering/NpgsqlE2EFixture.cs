using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.FunctionalTests.ReverseEngineering
{
    public class NpgsqlE2EFixture
    {
        public NpgsqlE2EFixture()
        {
            NpgsqlTestStore.CreateDatabase(
                "NpgsqlReverseEngineerTestE2E", "ReverseEngineering/E2E.sql", true);
        }
    }
}
