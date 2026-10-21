using Microsoft.EntityFrameworkCore.Migrations.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

public class NpgsqlMigrationsModelDifferTest
{
    [Theory]
    [InlineData(null, null, "numeric(9)")]
    [InlineData(0, null, "numeric(9,0)")]
    [InlineData(2, null, "numeric(9,2)")]
    [InlineData(null, "numeric", "numeric(9)")]
    [InlineData(null, "numeric(9)", "numeric(9,0)")]
    [InlineData(null, "numeric(9,2)", "numeric(9,2)")]
    public void Decimal_precision_snapshot_round_trips(int? scale, string storeType, string expectedStoreType)
    {
        using var context = new PrecisionContext(scale, storeType);
        var model = context.GetService<IDesignTimeModel>().Model;
        var property = model.FindEntityType(typeof(PrecisionEntity)).FindProperty(nameof(PrecisionEntity.ScalingFactor));
        Assert.Equal(expectedStoreType, property.GetRelationalTypeMapping().StoreType);

        var services = new ServiceCollection();
        services.AddDbContextDesignTimeServices(context);
        new NpgsqlDesignTimeServices().ConfigureDesignTimeServices(services);
        services.AddEntityFrameworkDesignTimeServices();
        using var serviceProvider = services.BuildServiceProvider();
        var source = serviceProvider.GetRequiredService<IMigrationsCodeGenerator>()
            .GenerateSnapshot("PrecisionSnapshot", typeof(PrecisionContext), "Snapshot", model);

        if (scale is null && storeType is null)
        {
            Assert.Contains(".HasPrecision(9)", source);
            Assert.DoesNotContain(".HasColumnType(\"numeric(9)\")", source);
        }
        else
        {
            Assert.Contains($".HasColumnType(\"{storeType ?? expectedStoreType}\")", source);
        }

        var build = new BuildSource
        {
            Sources = { ["Snapshot.cs"] = source },
            References =
            {
                BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
                BuildReference.ByName("Npgsql.EntityFrameworkCore.PostgreSQL"),
                BuildReference.ByName("Npgsql"),
                BuildReference.ByName(typeof(NpgsqlMigrationsModelDifferTest).Assembly.GetName().Name)
            }
        };
        var snapshot = (ModelSnapshot)Activator.CreateInstance(build.BuildInMemory().GetType("PrecisionSnapshot.Snapshot"));
        var snapshotModel = context.GetService<IModelRuntimeInitializer>().Initialize(snapshot.Model, designTime: true);

        Assert.Empty(
            context.GetService<IMigrationsModelDiffer>()
                .GetDifferences(snapshotModel.GetRelationalModel(), model.GetRelationalModel()));
    }

    public class PrecisionContext(int? scale, string storeType) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql().EnableServiceProviderCaching(false);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<PrecisionEntity>();
            entity.ToTable("Parameter");
            entity.Property(e => e.Id).ValueGeneratedNever();
            if (scale is { } s)
            {
                entity.Property(e => e.ScalingFactor).HasPrecision(9, s);
            }

            if (storeType is not null)
            {
                entity.Property(e => e.ScalingFactor).HasColumnType(storeType);
            }
        }
    }

    private class PrecisionEntity
    {
        public int Id { get; set; }

        [Precision(9)]
        public decimal? ScalingFactor { get; set; }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Seed_data_with_collection(bool array, bool changed)
    {
        using var source = new SeedContext(array, changed: false);
        using var target = new SeedContext(array, changed);
        var operations = target.GetService<IMigrationsModelDiffer>().GetDifferences(
            source.GetService<IDesignTimeModel>().Model.GetRelationalModel(),
            target.GetService<IDesignTimeModel>().Model.GetRelationalModel());

        if (changed)
        {
            var operation = Assert.IsType<UpdateDataOperation>(Assert.Single(operations));
            Assert.Equal("MyEntity", operation.Table);
            Assert.Equal(["Id"], operation.KeyColumns);
            Assert.Equal(1, operation.KeyValues[0, 0]);
            Assert.Equal(["Tags"], operation.Columns);
            Assert.Equal(["A", "B", "D"], Assert.IsAssignableFrom<IEnumerable<string>>(operation.Values[0, 0]));
        }
        else
        {
            Assert.Empty(operations);
        }
    }

    private class SeedContext(bool array, bool changed) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql().EnableServiceProviderCaching(false);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            string[] tags = ["A", "B", changed ? "D" : "C"];

            modelBuilder.Entity(
                "MyEntity", b =>
                {
                    b.Property<int>("Id");
                    b.Property(array ? typeof(string[]) : typeof(List<string>), "Tags");
                    b.HasData(new { Id = 1, Tags = array ? (object)tags : tags.ToList() });
                });
        }
    }
}
