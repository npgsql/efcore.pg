namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class ModelBuilding101NpgsqlTest : ModelBuilding101RelationalTestBase
{
    protected override DbContextOptionsBuilder ConfigureContext(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql();
}
