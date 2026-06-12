namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class NpgsqlDatabaseFacadeExtensions
{
    public static void EnsureClean(this DatabaseFacade databaseFacade, bool createTables = true)
        => databaseFacade.CreateExecutionStrategy()
            .Execute(databaseFacade, database => new NpgsqlDatabaseCleaner().Clean(database, createTables));
}
