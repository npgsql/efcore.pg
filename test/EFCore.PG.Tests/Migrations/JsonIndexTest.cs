using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

public class JsonIndexTest
{
    [Fact]
    public void Member_index_migration_scaffolding_fails_on_rc2()
    {
        using var context = new JsonIndexContext(b => b.HasIndex(e => e.Details.Slug));
        var model = context.GetService<IDesignTimeModel>().Model;
        var operations = context.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel());
        using var services = CreateDesignTimeServices(context);

        // Replace with successful scaffolding after https://github.com/dotnet/efcore/issues/39064.
        // Fixed by https://github.com/dotnet/efcore/pull/39089.
        Assert.Equal(
            "Cannot scaffold C# literals of type 'Microsoft.EntityFrameworkCore.Metadata.RelationalJsonIndex'. "
            + "The provider should implement CoreTypeMapping.GenerateCodeLiteral to support using it at design time.",
            Assert.Throws<InvalidOperationException>(
                () => services.GetRequiredService<IMigrationsCodeGenerator>()
                    .GenerateMigration("JsonIndexSnapshot", "Migration", operations, [])).Message);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Mixed_composite_directions_fail_on_rc2(bool first, bool second)
    {
        using var context = new JsonIndexContext(
            b => b.HasIndex(e => new { e.Details.Slug, e.Details.Rating }).IsDescending(first, second));
        var model = context.GetService<IDesignTimeModel>().Model;

        // Replace with successful index generation after https://github.com/dotnet/efcore/issues/39064.
        // Fixed by https://github.com/dotnet/efcore/pull/39089.
        Assert.Equal(
            "2 values were provided in CreateIndexOperations.IsDescending, but the operation has 1 columns.",
            Assert.Throws<ArgumentException>(
                () => context.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel())).Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Uniform_composite_directions_work(bool descending)
    {
        using var context = new JsonIndexContext(
            b => b.HasIndex(e => new { e.Details.Slug, e.Details.Rating }).IsDescending(descending, descending));
        var model = context.GetService<IDesignTimeModel>().Model;
        var operation = Assert.Single(
            context.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel())
                .OfType<CreateIndexOperation>());
        var direction = descending ? " DESC" : "";

        Assert.Equal(
            $"""CREATE INDEX "IX_Blogs_Details_Slug_Details_Rating" ON "Blogs" (("Details" ->> 'Slug'){direction}, (CAST("Details" ->> 'Rating' AS integer)){direction});""",
            Assert.Single(context.GetService<IMigrationsSqlGenerator>().Generate([operation], model)).CommandText.TrimEnd());
    }

    [Theory]
    [InlineData("Details")]
    [InlineData("RootItems")]
    public void Whole_document_index_migration_still_scaffolds(string property)
    {
        using var context = new JsonIndexContext(
            b => b.HasIndex(property).HasMethod("gin").HasOperators("jsonb_path_ops")
                .HasFilter("\"Id\" > 0").IsCreatedConcurrently());
        var model = context.GetService<IDesignTimeModel>().Model;
        var operation = Assert.Single(
            context.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel())
                .OfType<CreateIndexOperation>());
        Assert.Null(operation[RelationalAnnotationNames.JsonIndex]);
        Assert.Null(operation[NpgsqlAnnotationNames.JsonIndexTypes]);
        using var services = CreateDesignTimeServices(context);

        var source = services.GetRequiredService<IMigrationsCodeGenerator>()
            .GenerateMigration("JsonIndexSnapshot", "Migration", [operation], []);

        Assert.Contains("migrationBuilder.CreateIndex(", source);
        Assert.DoesNotContain("Relational:JsonIndex", source);
        Assert.Equal(
            $"""CREATE INDEX CONCURRENTLY "IX_Blogs_{property}" ON "Blogs" USING gin ("{property}" jsonb_path_ops) WHERE "Id" > 0;""",
            Assert.Single(context.GetService<IMigrationsSqlGenerator>().Generate([operation], model)).CommandText.TrimEnd());
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(JsonDocument))]
    [InlineData(typeof(LegacyJson))]
    public void Legacy_document_index_migration_still_scaffolds(Type clrType)
    {
        using var context = new JsonIndexContext(
            b =>
            {
                b.Property(clrType, "LegacyJson").HasColumnType("jsonb");
                b.HasIndex("LegacyJson").HasMethod("gin").HasOperators("jsonb_path_ops")
                    .HasFilter("\"Id\" > 0").IsCreatedConcurrently();
            });
        var model = context.GetService<IDesignTimeModel>().Model;
        var operation = Assert.Single(
            context.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel())
                .OfType<CreateIndexOperation>());
        Assert.Null(operation[RelationalAnnotationNames.JsonIndex]);
        Assert.Null(operation[NpgsqlAnnotationNames.JsonIndexTypes]);
        using var services = CreateDesignTimeServices(context);

        Assert.Contains(
            "migrationBuilder.CreateIndex(",
            services.GetRequiredService<IMigrationsCodeGenerator>()
                .GenerateMigration("JsonIndexSnapshot", "Migration", [operation], []));
        Assert.Equal(
            """CREATE INDEX CONCURRENTLY "IX_Blogs_LegacyJson" ON "Blogs" USING gin ("LegacyJson" jsonb_path_ops) WHERE "Id" > 0;""",
            Assert.Single(context.GetService<IMigrationsSqlGenerator>().Generate([operation], model)).CommandText.TrimEnd());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Member_index_snapshot_round_trips(bool composite)
    {
        using var context = new JsonIndexContext(
            b =>
            {
                if (composite)
                {
                    b.HasIndex(e => new { e.Details.Slug, e.Details.Rating });
                }
                else
                {
                    b.HasIndex(e => e.Details.Slug).IsUnique();
                }
            });
        var model = context.GetService<IDesignTimeModel>().Model;
        using var services = CreateDesignTimeServices(context);
        var source = services.GetRequiredService<IMigrationsCodeGenerator>()
            .GenerateSnapshot("JsonIndexSnapshot", typeof(JsonIndexContext), "Snapshot", model);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator)
            .Append(typeof(JsonIndexTest).Assembly.Location)
            .Distinct()
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            "JsonIndexSnapshot",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        stream.Position = 0;
        var loadContext = new AssemblyLoadContext("JsonIndexSnapshot", isCollectible: true);
        try
        {
            var snapshot = (ModelSnapshot)Activator.CreateInstance(
                loadContext.LoadFromStream(stream).GetType("JsonIndexSnapshot.Snapshot")!);
            var snapshotModel = context.GetService<IModelRuntimeInitializer>().Initialize(snapshot.Model, designTime: true);
            Assert.Empty(
                context.GetService<IMigrationsModelDiffer>()
                    .GetDifferences(model.GetRelationalModel(), snapshotModel.GetRelationalModel()));
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [Fact]
    public void Member_type_change_recreates_index()
    {
        using var source = new JsonIndexContext(b => b.HasIndex(e => e.Details.Rating));
        using var target = new JsonIndexContext(
            b =>
            {
                b.ComplexProperty(e => e.Details).Property(e => e.Rating).HasConversion<long>();
                b.HasIndex(e => e.Details.Rating);
            });
        var sourceModel = source.GetService<IDesignTimeModel>().Model.GetRelationalModel();
        var targetModel = target.GetService<IDesignTimeModel>().Model;
        var operations = target.GetService<IMigrationsModelDiffer>().GetDifferences(sourceModel, targetModel.GetRelationalModel());

        Assert.Collection(
            operations,
            o => Assert.IsType<DropIndexOperation>(o),
            o => Assert.IsType<CreateIndexOperation>(o));
        var sql = string.Concat(target.GetService<IMigrationsSqlGenerator>().Generate(operations, targetModel).Select(c => c.CommandText));
        Assert.Contains("""DROP INDEX "IX_Blogs_Details_Rating";""", sql);
        Assert.Contains("""((CAST("Details" ->> 'Rating' AS bigint)))""", sql);
    }

    [Theory]
    [InlineData("Details.Slug", "Details.Rating")]
    [InlineData("Details.Items[0].Value", "Details.Items[1].Value")]
    public void Changing_member_path_or_position_recreates_index(string sourcePath, string targetPath)
    {
        using var source = new JsonIndexContext(b => b.HasIndex(sourcePath).HasDatabaseName("IX_Json"));
        using var target = new JsonIndexContext(b => b.HasIndex(targetPath).HasDatabaseName("IX_Json"));
        var operations = target.GetService<IMigrationsModelDiffer>().GetDifferences(
            source.GetService<IDesignTimeModel>().Model.GetRelationalModel(),
            target.GetService<IDesignTimeModel>().Model.GetRelationalModel());

        Assert.Collection(
            operations,
            o => Assert.IsType<DropIndexOperation>(o),
            o => Assert.IsType<CreateIndexOperation>(o));
    }

    [Fact]
    public void Wildcard_collection_index_is_rejected()
    {
        using var context = new JsonIndexContext(b => b.HasIndex("Details.Items[].Value"));

        Assert.Equal(
            NpgsqlStrings.JsonIndexAllElementsNotSupported,
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [Fact]
    public void Member_tsvector_expression_index_is_rejected()
    {
        using var context = new JsonIndexContext(
            b => b.HasIndex(e => e.Details.Slug).IsTsVectorExpressionIndex("english"));

        Assert.Equal(
            NpgsqlStrings.JsonIndexTsVectorNotSupported,
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    private static ServiceProvider CreateDesignTimeServices(DbContext context)
    {
        var services = new ServiceCollection();
        services.AddDbContextDesignTimeServices(context);
        new NpgsqlDesignTimeServices().ConfigureDesignTimeServices(services);
        services.AddEntityFrameworkDesignTimeServices();
        return services.BuildServiceProvider();
    }

    public class JsonIndexContext(Action<EntityTypeBuilder<JsonIndexBlog>> configureIndex) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql().EnableServiceProviderCaching(false);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var blog = modelBuilder.Entity<JsonIndexBlog>();
            blog.ToTable("Blogs");
            blog.ComplexProperty(
                e => e.Details, d =>
                {
                    d.ToJson();
                    d.ComplexCollection(e => e.Items);
                });
            blog.ComplexCollection(e => e.RootItems).ToJson();
            configureIndex(blog);
        }
    }

    public class JsonIndexBlog
    {
        public int Id { get; set; }
        public JsonIndexDetails Details { get; set; } = new();
        public List<JsonIndexItem> RootItems { get; set; } = [];
    }

    public class JsonIndexDetails
    {
        public string Slug { get; set; }
        public int Rating { get; set; }
        public List<JsonIndexItem> Items { get; set; } = [];
    }

    public class JsonIndexItem
    {
        public string Value { get; set; }
    }

    public class LegacyJson
    {
        public string Value { get; set; }
    }
}
