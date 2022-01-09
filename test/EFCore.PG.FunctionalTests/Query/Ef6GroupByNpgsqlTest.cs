// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class Ef6GroupByNpgsqlTest : Ef6GroupByTestBase<Ef6GroupByNpgsqlTest.Ef6GroupByNpgsqlFixture>
{
    public Ef6GroupByNpgsqlTest(Ef6GroupByNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/27155")]
    public override Task Average_Grouped_from_LINQ_101(bool async)
        => base.Average_Grouped_from_LINQ_101(async);

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/27155")]
    public override Task Whats_new_2021_sample_10(bool async)
        => base.Whats_new_2021_sample_10(async);

    public class Ef6GroupByNpgsqlFixture : Ef6GroupByFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<OrderForLinq>().Property(o => o.OrderDate).HasColumnType("timestamp");
        }
    }
}
