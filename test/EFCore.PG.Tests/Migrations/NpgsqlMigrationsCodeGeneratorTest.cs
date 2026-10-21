using Microsoft.EntityFrameworkCore.Migrations.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

public class NpgsqlMigrationsCodeGeneratorTest
{
    [Fact]
    public void Hstore_seed_data()
    {
        using var context = new HstoreSeedContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var operations = context.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel());

        var services = new ServiceCollection();
        services.AddDbContextDesignTimeServices(context);
        new NpgsqlDesignTimeServices().ConfigureDesignTimeServices(services);
        services.AddEntityFrameworkDesignTimeServices();
        using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<IMigrationsCodeGenerator>();

        var migration = generator.GenerateMigration("HstoreSeed", "Migration", operations, []);
        var snapshot = generator.GenerateSnapshot("HstoreSeed", typeof(HstoreSeedContext), "Snapshot", model);

        Assert.Contains("migrationBuilder.InsertData(", migration);
        Assert.Contains(".HasData(", snapshot);

        string[] literals =
        [
            """new Dictionary<string, string> { ["k1"] = "v1", ["k2"] = "v2" }""",
            "new Dictionary<string, string>()",
            """new Dictionary<string, string> { ["nullable"] = null }""",
            """new Dictionary<string, string> { ["\"\\"] = "line1\nline2" }"""
        ];

        foreach (var literal in literals)
        {
            Assert.Contains(literal, migration);
            Assert.Contains(literal, snapshot);
        }
    }

    public class HstoreSeedContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity(
                "Blog", b =>
                {
                    b.Property<int>("Id");
                    b.Property<Dictionary<string, string>>("Properties").HasColumnType("hstore");
                    b.HasData(
                        new { Id = 1, Properties = new Dictionary<string, string> { ["k1"] = "v1", ["k2"] = "v2" } },
                        new { Id = 2, Properties = new Dictionary<string, string>() },
                        new { Id = 3, Properties = new Dictionary<string, string> { ["nullable"] = null } },
                        new { Id = 4, Properties = new Dictionary<string, string> { ["\"\\"] = "line1\nline2" } });
                });
    }
}
