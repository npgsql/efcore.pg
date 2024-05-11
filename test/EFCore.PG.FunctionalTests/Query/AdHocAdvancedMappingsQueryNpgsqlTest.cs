using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class AdHocAdvancedMappingsQueryNpgsqlTest : AdHocAdvancedMappingsQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    // Cannot write DateTimeOffset with Offset=10:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.
    public override Task Projecting_one_of_two_similar_complex_types_picks_the_correct_one()
        => Assert.ThrowsAsync<DbUpdateException>(
            () => base.Projecting_one_of_two_similar_complex_types_picks_the_correct_one());
}
