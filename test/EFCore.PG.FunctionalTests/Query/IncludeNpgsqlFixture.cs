namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class IncludeNpgsqlFixture : NorthwindQueryNpgsqlFixture<NoopModelCustomizer>
{
    protected override bool ShouldLogCategory(string logCategory)
        => base.ShouldLogCategory(logCategory) || logCategory == DbLoggerCategory.Query.Name;
}
