namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public class OwnedJsonBulkUpdateNpgsqlTest(
    OwnedJsonNpgsqlFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedJsonBulkUpdateRelationalTestBase<OwnedJsonNpgsqlFixture>(fixture, testOutputHelper);
