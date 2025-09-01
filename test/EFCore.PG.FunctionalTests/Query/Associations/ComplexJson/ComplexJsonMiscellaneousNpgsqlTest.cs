// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonMiscellaneousNpgsqlTest(ComplexJsonNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonMiscellaneousRelationalTestBase<ComplexJsonNpgsqlFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_related_property()
    {
        await base.Where_related_property();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (CAST(r."RequiredRelated" ->> 'Int' AS integer)) = 8
""");
    }

    public override async Task Where_optional_related_property()
    {
        await base.Where_optional_related_property();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (CAST(r."OptionalRelated" ->> 'Int' AS integer)) = 8
""");
    }

    public override async Task Where_nested_related_property()
    {
        await base.Where_nested_related_property();

        AssertSql(
            """
SELECT r."Id", r."Name", r."OptionalRelated", r."RelatedCollection", r."RequiredRelated"
FROM "RootEntity" AS r
WHERE (CAST(r."RequiredRelated" #>> '{RequiredNested,Int}' AS integer)) = 8
""");
    }

    #endregion Simple filters

    #region Value types

    public override async Task Where_property_on_non_nullable_value_type()
    {
        await base.Where_property_on_non_nullable_value_type();

        AssertSql(
            """
SELECT v."Id", v."Name", v."OptionalRelated", v."RelatedCollection", v."RequiredRelated"
FROM "ValueRootEntity" AS v
WHERE (CAST(v."RequiredRelated" ->> 'Int' AS integer)) = 8
""");
    }

    public override async Task Where_property_on_nullable_value_type_Value()
    {
        await base.Where_property_on_nullable_value_type_Value();

        AssertSql(
            """
SELECT v."Id", v."Name", v."OptionalRelated", v."RelatedCollection", v."RequiredRelated"
FROM "ValueRootEntity" AS v
WHERE (CAST(v."OptionalRelated" ->> 'Int' AS integer)) = 8
""");
    }

    public override async Task Where_HasValue_on_nullable_value_type()
    {
        await base.Where_HasValue_on_nullable_value_type();

        AssertSql(
            """
SELECT v."Id", v."Name", v."OptionalRelated", v."RelatedCollection", v."RequiredRelated"
FROM "ValueRootEntity" AS v
WHERE (v."OptionalRelated") IS NOT NULL
""");
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
