// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public class OwnedJsonMiscellaneousNpgsqlTest(
    OwnedJsonNpgsqlFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedJsonMiscellaneousRelationalTestBase<OwnedJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (CAST(r."RequiredAssociate" ->> 'Int' AS integer)) = 8
""");
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (CAST(r."OptionalAssociate" ->> 'Int' AS integer)) = 8
""");
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        AssertSql(
            """
SELECT r."Id", r."Name", r."AssociateCollection", r."OptionalAssociate", r."RequiredAssociate"
FROM "RootEntity" AS r
WHERE (CAST(r."RequiredAssociate" #>> '{RequiredNestedAssociate,Int}' AS integer)) = 8
""");
    }

    #endregion Simple filters


    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT m."Id", m."Name", m."AssociateCollection", m."OptionalAssociate", m."RequiredAssociate"
FROM (
    SELECT * FROM "RootEntity"
) AS m
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
