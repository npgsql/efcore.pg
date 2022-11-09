// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update;

public class NonSharedModelUpdatesNpgsqlTest : NonSharedModelUpdatesTestBase
{
    protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
}
