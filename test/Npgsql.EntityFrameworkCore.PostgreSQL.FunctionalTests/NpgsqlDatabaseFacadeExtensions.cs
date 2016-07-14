using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public static class NpgsqlDatabaseFacadeExtensions
    {
        public static void EnsureClean(this DatabaseFacade databaseFacade)
            => new NpgsqlDatabaseCleaner().Clean(databaseFacade);
    }
}
