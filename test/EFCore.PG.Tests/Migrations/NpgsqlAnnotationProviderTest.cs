using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

public class NpgsqlAnnotationProviderTest
{
    [Fact]
    public void ForColumn_resolves_tsvector_properties_over_json_navigation()
    {
        var modelBuilder = NpgsqlTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Blog>(
            eb =>
            {
                eb.OwnsOne(b => b.Owned).ToJson();
                eb.Property(b => b.SearchVector)
                    .IsGeneratedTsVectorColumn("english", nameof(Blog.Owned));
            });

        CheckTsVectorProperties(modelBuilder, ["Owned"]);
    }

    [Fact]
    public void ForColumn_resolves_tsvector_properties_over_renamed_json_navigation()
    {
        var modelBuilder = NpgsqlTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Blog>(
            eb =>
            {
                eb.OwnsOne(b => b.Owned).ToJson("owned_renamed");
                eb.Property(b => b.SearchVector)
                    .IsGeneratedTsVectorColumn("english", nameof(Blog.Owned));
            });

        CheckTsVectorProperties(modelBuilder, ["owned_renamed"]);
    }

    [Fact]
    public void ForColumn_resolves_tsvector_properties_over_json_complex_property()
    {
        var modelBuilder = NpgsqlTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Blog>().ComplexProperty(b => b.Owned).ToJson();
        modelBuilder.Entity<Blog>().Property(b => b.SearchVector).IsGeneratedTsVectorColumn("english", nameof(Blog.Owned));

        CheckTsVectorProperties(modelBuilder, ["Owned"]);
    }

    [Fact]
    public void ForColumn_resolves_tsvector_properties_over_renamed_json_complex_property()
    {
        var modelBuilder = NpgsqlTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Blog>().ComplexProperty(b => b.Owned).ToJson("owned_renamed");
        modelBuilder.Entity<Blog>().Property(b => b.SearchVector).IsGeneratedTsVectorColumn("english", nameof(Blog.Owned));

        CheckTsVectorProperties(modelBuilder, ["owned_renamed"]);
    }

    [Fact]
    public void ForColumn_resolves_tsvector_properties_over_mixed_text_jsonb_and_json_navigation()
    {
        var modelBuilder = NpgsqlTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity(
            "Blogs", eb =>
            {
                eb.Property<int>("Id");
                eb.Property<string>("Text");
                eb.Property<string>("Jsonb").HasColumnType("jsonb");
                eb.OwnsOne("Owned", "Owned").ToJson();
                eb.Property<NpgsqlTsVector>("SearchVector")
                    .IsGeneratedTsVectorColumn("english", "Text", "Jsonb", "Owned");
            });

        CheckTsVectorProperties(modelBuilder, ["Text", "Jsonb", "Owned"]);
    }

    [Fact]
    public void ForColumn_resolves_tsvector_properties_when_declared_on_complex_type()
    {
        var modelBuilder = NpgsqlTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<BlogWithComplexProperty>(
            eb =>
            {
                eb.ComplexProperty(b => b.ComplexProperty, cb =>
                {
                    cb.Property(cp => cp.Something).IsRequired();
                    // using [nameof(ComplexProperty.Something)] leads to an exception, see #3892
                    cb.Property(cp => cp.SearchVector).Metadata.SetTsVectorProperties(new string[] { nameof(ComplexProperty.Something) });
                });
            });

        CheckTsVectorProperties(modelBuilder, ["ComplexProperty_Something"], searchVectorName: "ComplexProperty_SearchVector");
    }

    [Fact]
    public void ForColumn_throws_when_included_navigation_is_not_mapped_to_json()
    {
        var modelBuilder = NpgsqlTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity(
            "Blogs", eb =>
            {
                eb.Property<int>("Id");
                eb.OwnsOne("Owned", "Owned");
                eb.Property<NpgsqlTsVector>("SearchVector")
                    .IsGeneratedTsVectorColumn("english", "Owned");
            });

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var model = modelBuilder.FinalizeModel(designTime: true);
            model.GetRelationalModel();
        });
        Assert.Equal(
            "Could not find property, navigation or complex property 'Owned' on entity type 'Blogs (Dictionary<string, object>)' for generated tsvector column",
            exception.Message);
    }

    [Fact]
    public void ForColumn_throws_when_included_name_does_not_resolve()
    {
        var modelBuilder = NpgsqlTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity(
            "Blogs", eb =>
            {
                eb.Property<int>("Id");
                eb.OwnsOne("Owned", "Owned");
                eb.Property<NpgsqlTsVector>("SearchVector")
                    .IsGeneratedTsVectorColumn("english", "Owne");
            });

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var model = modelBuilder.FinalizeModel(designTime: true);
            model.GetRelationalModel();
        });
        Assert.Equal(
            "Could not find property, navigation or complex property 'Owne' on entity type 'Blogs (Dictionary<string, object>)' for generated tsvector column",
            exception.Message);
    }

    private class Blog
    {
        public int Id { get; set; }
        public Owned Owned { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
    }

    private class Owned { public string Something { get; set; } }

    private class BlogWithComplexProperty
    {
        public int Id { get; set; }
        public ComplexProperty ComplexProperty { get; set; }
    }

    private class ComplexProperty
    {
        public string Something { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
    }

    private void CheckTsVectorProperties(TestHelpers.TestModelBuilder modelBuilder, IReadOnlyCollection<string> expectedProperties,
        string searchVectorName = "SearchVector")
    {
        var model = modelBuilder.FinalizeModel(designTime: true);
        var relationalModel = model.GetRelationalModel();

        var column = Assert.Single(relationalModel.Tables.Single().Columns, c => c.Name == searchVectorName);
        var tsVectorProperties = (IReadOnlyCollection<string>)column.FindAnnotation(NpgsqlAnnotationNames.TsVectorProperties)!.Value!;
        Assert.Equal(expectedProperties, tsVectorProperties);
    }
}
