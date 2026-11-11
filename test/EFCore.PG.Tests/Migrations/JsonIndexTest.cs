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
    public void Member_index_migration_scaffolds_and_generates_sql()
    {
        using var context = new JsonIndexContext(b => b.HasIndex(e => e.Details.Slug).IsUnique());
        var model = context.GetService<IDesignTimeModel>().Model;
        var operation = Assert.Single(
            context.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel())
                .OfType<CreateIndexOperation>());
        using var services = CreateDesignTimeServices(context);
        var generator = services.GetRequiredService<IMigrationsCodeGenerator>();
        var source = generator.GenerateMigration("JsonIndexSnapshot", "AddJsonIndex", [operation], []);
        var metadata = generator.GenerateMetadata(
            "JsonIndexSnapshot", typeof(JsonIndexContext), "AddJsonIndex", "20261111000000_AddJsonIndex", model);

        Assert.DoesNotContain(RelationalAnnotationNames.JsonIndex, source);

        CompileAndAssert(
            [source, metadata], assembly =>
            {
                var migration = (Migration)Activator.CreateInstance(assembly.GetType("JsonIndexSnapshot.AddJsonIndex"));
                Assert.Null(Assert.Single(migration.UpOperations)[RelationalAnnotationNames.JsonIndex]);
                var targetModel = context.GetService<IModelRuntimeInitializer>().Initialize(migration.TargetModel, designTime: true);
                Assert.Equal(
                    """CREATE UNIQUE INDEX "IX_Blogs_Details_Slug" ON "Blogs" (("Details" ->> 'Slug'));""",
                    Assert.Single(context.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations, targetModel))
                        .CommandText.TrimEnd());
            });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Composite_sort_directions(bool first, bool second)
    {
        using var context = new JsonIndexContext(
            b => b.HasIndex(e => new { e.Details.Slug, e.Details.Rating }).IsDescending(first, second));
        var model = context.GetService<IDesignTimeModel>().Model;
        var operation = Assert.Single(
            context.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel())
                .OfType<CreateIndexOperation>());
        var firstDirection = first ? " DESC" : "";
        var secondDirection = second ? " DESC" : "";

        Assert.Equal(
            $"""CREATE INDEX "IX_Blogs_Details_Slug_Details_Rating" ON "Blogs" (("Details" ->> 'Slug'){firstDirection}, (CAST("Details" ->> 'Rating' AS integer)){secondDirection});""",
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
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Member_index_snapshot_round_trips(bool composite, bool first, bool second)
    {
        using var context = new JsonIndexContext(
            b =>
            {
                if (composite)
                {
                    b.HasIndex(e => new { e.Details.Slug, e.Details.Rating }).IsDescending(first, second);
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
        CompileAndAssert(
            [source], assembly =>
            {
                var snapshot = (ModelSnapshot)Activator.CreateInstance(assembly.GetType("JsonIndexSnapshot.Snapshot"));
                var snapshotModel = context.GetService<IModelRuntimeInitializer>().Initialize(snapshot.Model, designTime: true);
                Assert.Empty(
                    context.GetService<IMigrationsModelDiffer>()
                        .GetDifferences(model.GetRelationalModel(), snapshotModel.GetRelationalModel()));
            });
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

    private static void CompileAndAssert(string[] sources, Action<Assembly> assert)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator)
            .Append(typeof(JsonIndexTest).Assembly.Location)
            .Distinct()
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            "JsonIndexSnapshot",
            sources.Select(source => CSharpSyntaxTree.ParseText(source)),
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        stream.Position = 0;
        var loadContext = new AssemblyLoadContext("JsonIndexSnapshot", isCollectible: true);
        try
        {
            assert(loadContext.LoadFromStream(stream));
        }
        finally
        {
            loadContext.Unload();
        }
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
