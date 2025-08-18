// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedJson;

public class OwnedJsonTypeNpgsqlFixture : OwnedJsonRelationalFixtureBase
{
    protected override string StoreName
        => "OwnedJsonTypeRelationshipsQueryTest";

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    // protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    // {
    //     base.OnModelCreating(modelBuilder, context);

    //     modelBuilder.Entity<RootEntity>().OwnsOne(x => x.RequiredTrunk).HasColumnType("json");
    //     modelBuilder.Entity<RootEntity>().OwnsOne(x => x.OptionalTrunk).HasColumnType("json");
    //     modelBuilder.Entity<RootEntity>().OwnsMany(x => x.CollectionTrunk).HasColumnType("json");
    // }
}
