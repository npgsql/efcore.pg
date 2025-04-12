// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public class OwnedJsonTypeRelationshipsNpgsqlFixture : OwnedJsonRelationshipsRelationalFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName => "JsonTypeRelationshipsQueryTest";

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRootEntity>().OwnsOne(x => x.RequiredReferenceTrunk).HasColumnType("json");
        modelBuilder.Entity<RelationshipsRootEntity>().OwnsOne(x => x.OptionalReferenceTrunk).HasColumnType("json");
        modelBuilder.Entity<RelationshipsRootEntity>().OwnsMany(x => x.CollectionTrunk).HasColumnType("json");
    }
}
