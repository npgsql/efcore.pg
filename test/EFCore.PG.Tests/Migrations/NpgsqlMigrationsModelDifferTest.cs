namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

public class NpgsqlMigrationsModelDifferTest
{
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
