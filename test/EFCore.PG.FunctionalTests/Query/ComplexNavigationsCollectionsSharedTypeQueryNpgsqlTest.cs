namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class ComplexNavigationsCollectionsSharedTypeQueryNpgsqlTest : ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<
    ComplexNavigationsSharedTypeQueryNpgsqlFixture>
{
    public ComplexNavigationsCollectionsSharedTypeQueryNpgsqlTest(
        ComplexNavigationsSharedTypeQueryNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(bool async)
        // Nested collection with ToList. Issue #23303.
        => await Assert.ThrowsAsync<ArgumentNullException>(
            () => base.Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(async));
}
