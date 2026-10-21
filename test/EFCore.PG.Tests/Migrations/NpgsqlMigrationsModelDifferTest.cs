namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

public class NpgsqlMigrationsModelDifferTest
{
    [Theory]
    [InlineData(null, "numeric(9)")]
    [InlineData(0, "numeric(9,0)")]
    [InlineData(2, "numeric(9,2)")]
    public void Decimal_precision_snapshot_round_trips(int? scale, string storeType)
    {
        using var context = new PrecisionContext(scale);
        var model = context.GetService<IDesignTimeModel>().Model;
        var snapshot = new PrecisionModelSnapshot(scale, storeType);
        var snapshotModel = context.GetService<IModelRuntimeInitializer>().Initialize(snapshot.Model, designTime: true);

        Assert.Empty(
            context.GetService<IMigrationsModelDiffer>()
                .GetDifferences(snapshotModel.GetRelationalModel(), model.GetRelationalModel()));
    }

    private class PrecisionContext(int? scale) : DbContext
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
        }
    }

    private class PrecisionEntity
    {
        public int Id { get; set; }

        [Precision(9)]
        public decimal? ScalingFactor { get; set; }
    }

    private class PrecisionModelSnapshot(int? scale, string storeType) : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 63);
            modelBuilder.UseIdentityByDefaultColumns();
            modelBuilder.Entity(
                typeof(PrecisionEntity).FullName!, b =>
                {
                    b.Property<int>("Id").HasColumnType("integer");
                    var property = b.Property<decimal?>("ScalingFactor").HasColumnType(storeType);
                    if (scale is { } s)
                    {
                        property.HasPrecision(9, s);
                    }
                    else
                    {
                        property.HasPrecision(9);
                    }

                    b.HasKey("Id");
                    b.ToTable("Parameter");
                });
        }
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
