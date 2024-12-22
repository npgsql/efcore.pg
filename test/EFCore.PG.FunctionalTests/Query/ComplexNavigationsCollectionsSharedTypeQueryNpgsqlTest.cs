namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsCollectionsSharedTypeQueryNpgsqlTest : ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<
    ComplexNavigationsSharedTypeQueryNpgsqlFixture>
{
    public ComplexNavigationsCollectionsSharedTypeQueryNpgsqlTest(
        ComplexNavigationsSharedTypeQueryNpgsqlFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
}
