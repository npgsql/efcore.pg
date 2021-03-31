// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class FieldsOnlyLoadNpgsqlTest : FieldsOnlyLoadTestBase<FieldsOnlyLoadNpgsqlTest.FieldsOnlyLoadNpgsqlFixture>
    {
        public FieldsOnlyLoadNpgsqlTest(FieldsOnlyLoadNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class FieldsOnlyLoadNpgsqlFixture : FieldsOnlyLoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => NpgsqlTestStoreFactory.Instance;
        }
    }
}
