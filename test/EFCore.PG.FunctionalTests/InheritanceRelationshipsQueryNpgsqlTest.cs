// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class InheritanceRelationshipsQueryNpgsqlTest : InheritanceRelationshipsQueryTestBase<NpgsqlTestStore, InheritanceRelationshipsQueryNpgsqlFixture>
    {
        public InheritanceRelationshipsQueryNpgsqlTest(InheritanceRelationshipsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override void ClearLog() {}
    }
}
