namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocAdvancedMappingsQueryNpgsqlTest(NonSharedFixture fixture)
    : AdHocAdvancedMappingsQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    // Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported.
    public override Task Query_generates_correct_datetime2_parameter_definition(int? fractionalSeconds, string postfix)
        => Assert.ThrowsAsync<ArgumentException>(
            () => base.Query_generates_correct_datetime2_parameter_definition(fractionalSeconds, postfix));

    // Cannot write DateTimeOffset with Offset=10:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.
    public override Task Query_generates_correct_datetimeoffset_parameter_definition(int? fractionalSeconds, string postfix)
        => Assert.ThrowsAsync<ArgumentException>(
            () => base.Query_generates_correct_datetime2_parameter_definition(fractionalSeconds, postfix));

    // Cannot write DateTimeOffset with Offset=10:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.
    public override Task Projecting_one_of_two_similar_complex_types_picks_the_correct_one()
        => Assert.ThrowsAsync<DbUpdateException>(
            () => base.Projecting_one_of_two_similar_complex_types_picks_the_correct_one());

    // Cannot write DateTimeOffset with Offset=10:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.
    public override Task Projecting_expression_with_converter_with_closure(bool async)
        => Assert.ThrowsAsync<DbUpdateException>(
            () => base.Projecting_one_of_two_similar_complex_types_picks_the_correct_one());

    // Cannot write DateTimeOffset with Offset=10:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.
    public override Task Projecting_property_with_converter_with_closure(bool async)
        => Assert.ThrowsAsync<DbUpdateException>(
            () => base.Projecting_one_of_two_similar_complex_types_picks_the_correct_one());

    // Cannot write DateTimeOffset with Offset=10:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.
    public override Task Projecting_property_with_converter_without_closure(bool async)
        => Assert.ThrowsAsync<DbUpdateException>(
            () => base.Projecting_one_of_two_similar_complex_types_picks_the_correct_one());
}
