using Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;

namespace Microsoft.EntityFrameworkCore.TestModels.NodaTime;

public class NodaTimeContext(DbContextOptions<NodaTimeContext> options) : PoolableDbContext(options)
{
    // ReSharper disable once MemberHidesStaticFromOuterClass
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public DbSet<NodaTimeTypes> NodaTimeTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("btree_gist");
    }

    public static async Task SeedAsync(NodaTimeContext context)
    {
        context.AddRange(NodaTimeData.CreateNodaTimeTypes());
        await context.SaveChangesAsync();
    }
}
