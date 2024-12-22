namespace Microsoft.EntityFrameworkCore;

public class BadDataJsonDeserializationSqlServerTest : BadDataJsonDeserializationTestBase
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => base.OnConfiguring(optionsBuilder.UseNpgsql(b => b.UseNetTopologySuite()));
}
