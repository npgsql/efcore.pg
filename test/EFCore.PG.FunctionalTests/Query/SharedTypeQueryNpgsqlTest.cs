// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class SharedTypeQueryNpgsqlTest : SharedTypeQueryRelationalTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

        public override Task Can_use_shared_type_entity_type_in_query_filter_with_from_sql(bool async)
            => Task.CompletedTask; // https://github.com/dotnet/efcore/issues/25661
    }
}
