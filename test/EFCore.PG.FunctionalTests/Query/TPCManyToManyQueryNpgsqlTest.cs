// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class TPCManyToManyQueryNpgsqlTest : TPCManyToManyQueryRelationalTestBase<TPCManyToManyQueryNpgsqlFixture>
{
    public TPCManyToManyQueryNpgsqlTest(TPCManyToManyQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
