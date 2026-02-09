// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonMiscellaneousNpgsqlTest(ComplexJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonMiscellaneousRelationalTestBase<ComplexJsonNpgsqlFixture>(fixture, testOutputHelper)
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

    #region Value types

    public override async Task Where_property_on_non_nullable_value_type()
    {
        await base.Where_property_on_non_nullable_value_type();

        AssertSql(
            """
SELECT v."Id", v."Name", v."AssociateCollection", v."OptionalAssociate", v."RequiredAssociate"
FROM "ValueRootEntity" AS v
WHERE (CAST(v."RequiredAssociate" ->> 'Int' AS integer)) = 8
""");
    }

    public override async Task Where_property_on_nullable_value_type_Value()
    {
        await base.Where_property_on_nullable_value_type_Value();

        AssertSql(
            """
SELECT v."Id", v."Name", v."AssociateCollection", v."OptionalAssociate", v."RequiredAssociate"
FROM "ValueRootEntity" AS v
WHERE (CAST(v."OptionalAssociate" ->> 'Int' AS integer)) = 8
""");
    }

    public override async Task Where_HasValue_on_nullable_value_type()
    {
        await base.Where_HasValue_on_nullable_value_type();

        AssertSql(
            """
SELECT v."Id", v."Name", v."AssociateCollection", v."OptionalAssociate", v."RequiredAssociate"
FROM "ValueRootEntity" AS v
WHERE (v."OptionalAssociate") IS NOT NULL
""");
    }

    #endregion Value types


    [ConditionalFact(Skip = "EF Core bug - NullReferenceException in GenerateComplexJsonShaper")]
    public override async Task FromSql_on_root()
        => await base.FromSql_on_root();

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
