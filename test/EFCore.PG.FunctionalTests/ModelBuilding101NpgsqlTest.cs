namespace Microsoft.EntityFrameworkCore;

public class ModelBuilding101NpgsqlTest : ModelBuilding101RelationalTestBase
{
    protected override DbContextOptionsBuilder ConfigureContext(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql();
}
