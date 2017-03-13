using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.FunctionalTests.ReverseEngineering
{
    public class NpgsqlE2EFixture : IDisposable
    {
        readonly NpgsqlTestStore _testStore;

        public NpgsqlE2EFixture()
        {
            _testStore = NpgsqlTestStore.GetOrCreateShared(
                "NpgsqlReverseEngineerTestE2E",
                () => NpgsqlTestStore.ExecuteScript("NpgsqlReverseEngineerTestE2E", "ReverseEngineering/E2E.sql"));
        }

        public void Dispose() => _testStore.Dispose();
    }
}
