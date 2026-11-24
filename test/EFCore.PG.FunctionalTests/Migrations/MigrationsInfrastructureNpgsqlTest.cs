using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;
#nullable disable

public class MigrationsInfrastructureNpgsqlTest(MigrationsInfrastructureNpgsqlTest.MigrationsInfrastructureNpgsqlFixture fixture)
    : MigrationsInfrastructureTestBase<MigrationsInfrastructureNpgsqlTest.MigrationsInfrastructureNpgsqlFixture>(fixture)
{
    public override void Can_get_active_provider()
    {
        base.Can_get_active_provider();

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", ActiveProvider);
    }

    // This tests uses Fixture.CreateEmptyContext(), which does not go through MigrationsInfrastructureNpgsqlFixture.CreateContext()
    // and therefore does not set the PostgresVersion in the context options. As a result, we try to drop the database with
    // WITH (FORCE), which is only supported starting with PG 13.
    [MinimumPostgresVersion(13, 0)]
    public override Task Can_generate_no_migration_script()
        => base.Can_generate_no_migration_script();

    public override void Can_apply_all_migrations()
        => base.Can_apply_all_migrations();

    public override void Can_apply_range_of_migrations()
        => base.Can_apply_range_of_migrations();

    public override void Can_revert_all_migrations()
        => base.Can_revert_all_migrations();

    public override void Can_revert_one_migrations()
        => base.Can_revert_one_migrations();

    public override Task Can_apply_all_migrations_async()
        => base.Can_apply_all_migrations_async();

    [ConditionalFact]
    public async Task Empty_Migration_Creates_Database()
    {
        await using var context = new BloggingContext(
            Fixture.TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder().EnableServiceProviderCaching(false))
                .ConfigureWarnings(e => e.Log(RelationalEventId.PendingModelChangesWarning)).Options);

        var creator = (NpgsqlDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();
        creator.RetryTimeout = TimeSpan.FromMinutes(10);

        await context.Database.MigrateAsync();

        Assert.True(creator.Exists());
    }

    private class BloggingContext(DbContextOptions options) : DbContext(options)
    {
        // ReSharper disable once UnusedMember.Local
        public DbSet<Blog> Blogs { get; set; }

        // ReSharper disable once ClassNeverInstantiated.Local
        public class Blog
        {
            // ReSharper disable UnusedMember.Local
            public int Id { get; set; }

            public string Name { get; set; }
            // ReSharper restore UnusedMember.Local
        }
    }

    [DbContext(typeof(BloggingContext))]
    [Migration("00000000000000_Empty")]
    public class EmptyMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Can_apply_several_migrations_in_parallel(bool async)
    {
        // Each instance plans its whole run from the history it reads under the first lock. The lock must therefore be held
        // until the run ends; a lock released with each migration's transaction lets another instance apply the next migration
        // in between, and the first instance then applies it again (#4162).
        await using var store = NpgsqlTestStore.Create("MigrationsInParallel");
        await using (var db = CreateParallelContext(store))
        {
            await db.Database.EnsureDeletedAsync();
            await db.GetService<IRelationalDatabaseCreator>().CreateAsync();
        }

        await Task.WhenAll(
            Enumerable.Range(0, 4).Select(
                _ => Task.Run(
                    async () =>
                    {
                        await using var context = CreateParallelContext(store);
                        if (async)
                        {
                            await context.Database.MigrateAsync();
                        }
                        else
                        {
                            context.Database.Migrate();
                        }
                    })));

        await using var check = CreateParallelContext(store);
        Assert.Equal(
            new[] { "00000000000001_People", "00000000000002_SlowEmail", "00000000000003_Phone" },
            (await check.GetService<IHistoryRepository>().GetAppliedMigrationsAsync()).Select(r => r.MigrationId));
    }

    private static ParallelMigrationsContext CreateParallelContext(NpgsqlTestStore store)
        => new(
            new DbContextOptionsBuilder<ParallelMigrationsContext>()
                .UseNpgsql(store.ConnectionString)
                .ConfigureWarnings(e => e.Log(RelationalEventId.PendingModelChangesWarning))
                .Options);

    private class ParallelMigrationsContext(DbContextOptions options) : DbContext(options);

    [DbContext(typeof(ParallelMigrationsContext))]
    [Migration("00000000000001_People")]
    private class ParallelMigration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
            => migrationBuilder.CreateTable(
                "People",
                c => new { Id = c.Column<int>() },
                constraints: t => t.PrimaryKey("PK_People", x => x.Id));
    }

    [DbContext(typeof(ParallelMigrationsContext))]
    [Migration("00000000000002_SlowEmail")]
    private class ParallelMigration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A slow migration (a data migration, an index build) keeps the other instances waiting on the lock.
            migrationBuilder.Sql("SELECT pg_sleep(1)");
            migrationBuilder.AddColumn<string>("Email", "People", nullable: true);
        }
    }

    [DbContext(typeof(ParallelMigrationsContext))]
    [Migration("00000000000003_Phone")]
    private class ParallelMigration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
            => migrationBuilder.AddColumn<string>("Phone", "People", nullable: true);
    }

    public override void Can_diff_against_2_2_model()
    {
        using var context = new Migrations.BloggingContext();
        DiffSnapshot(new BloggingContextModelSnapshot22(), context);
    }

    public class BloggingContextModelSnapshot22 : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity(
                "ModelSnapshot22.Blog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Blogs");
                });

            modelBuilder.Entity(
                "ModelSnapshot22.Post", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("BlogId");

                    b.Property<string>("Content");

                    b.Property<DateTime>("EditDate");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("BlogId");

                    b.ToTable("Post");
                });

            modelBuilder.Entity(
                "ModelSnapshot22.Post", b =>
                {
                    b.HasOne("ModelSnapshot22.Blog", "Blog")
                        .WithMany("Posts")
                        .HasForeignKey("BlogId");
                });
#pragma warning restore 612, 618
        }
    }

    public override void Can_diff_against_3_0_ASP_NET_Identity_model()
    {
        // TODO: Implement
    }

    public override void Can_diff_against_2_2_ASP_NET_Identity_model()
    {
        // TODO: Implement
    }

    public override void Can_diff_against_2_1_ASP_NET_Identity_model()
    {
        // TODO: Implement
    }

    protected override Task ExecuteSqlAsync(string value)
        => ((NpgsqlTestStore)Fixture.TestStore).ExecuteNonQueryAsync(value);

    public class MigrationsInfrastructureNpgsqlFixture : MigrationsInfrastructureFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public override MigrationsContext CreateContext()
        {
            var options = AddOptions(
                    TestStore.AddProviderOptions(new DbContextOptionsBuilder())
                        .UseNpgsql(
                            TestStore.ConnectionString, b => b.ApplyConfiguration()
                                .SetPostgresVersion(TestEnvironment.PostgresVersion)))
                .UseInternalServiceProvider(ServiceProvider)
                .Options;
            return new MigrationsContext(options);
        }
    }
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Post> Posts { get; set; }
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime EditDate { get; set; }

    public Blog Blog { get; set; }
}

public class BloggingContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(TestEnvironment.ConnectionString);

    public DbSet<Blog> Blogs { get; set; }
}
