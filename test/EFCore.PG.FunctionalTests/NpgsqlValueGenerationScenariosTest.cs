using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class NpgsqlValueGenerationScenariosTest
{
    private static readonly string DatabaseName = "NpgsqlValueGenerationScenariosTest";

    [Fact]
    public async Task Insert_with_sequence_id()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        using (var context = new BlogContextSequence(testStore.Name))
        {
            context.Database.EnsureCreated();
            context.AddRange(
                new Blog { Name = "One Unicorn" },
                new Blog { Name = "Two Unicorns" });
            context.SaveChanges();
        }

        using (var context = new BlogContextSequence(testStore.Name))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, blogs[0].Id);
            Assert.Equal(2, blogs[1].Id);
        }
    }

    public class BlogContextSequence(string databaseName) : ContextBase(databaseName);

    [Fact]
    public async Task Insert_with_sequence_HiLo()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        using (var context = new BlogContextHiLo(testStore.Name))
        {
            context.Database.EnsureCreated();
            context.AddRange(
                new Blog { Name = "One Unicorn" },
                new Blog { Name = "Two Unicorns" });
            context.SaveChanges();
        }

        using (var context = new BlogContextHiLo(testStore.Name))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, blogs[0].Id);
            Assert.Equal(2, blogs[1].Id);
        }
    }

    public class BlogContextHiLo(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.UseHiLo();
        }
    }

    [Fact]
    public async Task Insert_with_default_value_from_sequence()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        using (var context = new BlogContextDefaultValue(testStore.Name))
        {
            context.Database.EnsureCreated();
            context.AddRange(
                new Blog { Name = "One Unicorn" },
                new Blog { Name = "Two Unicorns" });
            context.SaveChanges();
        }

        using (var context = new BlogContextDefaultValue(testStore.Name))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(0, blogs[0].Id);
            Assert.Equal(1, blogs[1].Id);
        }

        using (var context = new BlogContextDefaultValueNoMigrations(testStore.Name))
        {
            context.AddRange(
                new Blog { Name = "One Unicorn" },
                new Blog { Name = "Two Unicorns" });
            context.SaveChanges();
        }

        using (var context = new BlogContextDefaultValueNoMigrations(testStore.Name))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(0, blogs[0].Id);
            Assert.Equal(1, blogs[1].Id);
            Assert.Equal(2, blogs[2].Id);
            Assert.Equal(3, blogs[3].Id);
        }
    }

    public class BlogContextDefaultValue(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .HasSequence("MySequence")
                .HasMin(0)
                .StartsAt(0);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"MySequence\"')");
        }
    }

    public class BlogContextDefaultValueNoMigrations(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .HasDefaultValue();
        }
    }

    public class BlogWithStringKey
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    [Fact]
    public async Task Insert_with_key_default_value_from_sequence()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        using (var context = new BlogContextKeyColumnWithDefaultValue(testStore.Name))
        {
            context.Database.EnsureCreated();
            context.AddRange(
                new Blog { Name = "One Unicorn" },
                new Blog { Name = "Two Unicorns" });
            context.SaveChanges();
        }

        using (var context = new BlogContextKeyColumnWithDefaultValue(testStore.Name))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(77, blogs[0].Id);
            Assert.Equal(78, blogs[1].Id);
        }
    }

    public class BlogContextKeyColumnWithDefaultValue(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .HasSequence("MySequence")
                .StartsAt(77);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"MySequence\"')")
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
        }
    }

    [ConditionalFact]
    public async Task Insert_uint_to_Identity_column_using_value_converter()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextUIntToIdentityUsingValueConverter(testStore.Name))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new BlogWithUIntKey { Name = "One Unicorn" }, new BlogWithUIntKey { Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextUIntToIdentityUsingValueConverter(testStore.Name))
        {
            var blogs = context.UnsignedBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal((uint)1, blogs[0].Id);
            Assert.Equal((uint)2, blogs[1].Id);
        }
    }

    public class BlogContextUIntToIdentityUsingValueConverter(string databaseName) : ContextBase(databaseName)
    {
        public DbSet<BlogWithUIntKey> UnsignedBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<BlogWithUIntKey>()
                .Property(e => e.Id)
                .HasConversion<int>();
        }
    }

    public class BlogWithUIntKey
    {
        public uint Id { get; set; }
        public string Name { get; set; }
    }

    [ConditionalFact]
    public async Task Insert_string_to_Identity_column_using_value_converter()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextStringToIdentityUsingValueConverter(testStore.Name))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new BlogWithStringKey { Name = "One Unicorn" }, new BlogWithStringKey { Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextStringToIdentityUsingValueConverter(testStore.Name))
        {
            var blogs = context.StringyBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal("1", blogs[0].Id);
            Assert.Equal("2", blogs[1].Id);
        }
    }

    public class BlogContextStringToIdentityUsingValueConverter(string databaseName) : ContextBase(databaseName)
    {
        public DbSet<BlogWithStringKey> StringyBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Guid guid;
            modelBuilder
                .Entity<BlogWithStringKey>()
                .Property(e => e.Id)
                .HasValueGenerator<TemporaryStringValueGenerator>()
                .HasConversion(
                    v => Guid.TryParse(v, out guid)
                        ? default
                        : int.Parse(v),
                    v => v.ToString())
                .ValueGeneratedOnAdd();
        }
    }

    [Fact]
    public async Task  Insert_with_explicit_non_default_keys()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        using (var context = new BlogContextNoKeyGeneration(testStore.Name))
        {
            context.Database.EnsureCreated();
            context.AddRange(
                new Blog { Id = 66, Name = "One Unicorn" },
                new Blog { Id = 67, Name = "Two Unicorns" });
            context.SaveChanges();
        }

        using (var context = new BlogContextNoKeyGeneration(testStore.Name))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(66, blogs[0].Id);
            Assert.Equal(67, blogs[1].Id);
        }
    }

    public class BlogContextNoKeyGeneration(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .ValueGeneratedNever();
        }
    }

    [Fact]
    public async Task  Insert_with_explicit_with_default_keys()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        using (var context = new BlogContextNoKeyGenerationNullableKey(testStore.Name))
        {
            context.Database.EnsureCreated();
            context.AddRange(
                new NullableKeyBlog { Id = 0, Name = "One Unicorn" },
                new NullableKeyBlog { Id = 1, Name = "Two Unicorns" });
            context.SaveChanges();
        }

        using (var context = new BlogContextNoKeyGenerationNullableKey(testStore.Name))
        {
            var blogs = context.NullableKeyBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(0, blogs[0].Id);
            Assert.Equal(1, blogs[1].Id);
        }
    }

    public class BlogContextNoKeyGenerationNullableKey(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<NullableKeyBlog>()
                .Property(e => e.Id)
                .ValueGeneratedNever();
        }
    }

    [Fact]
    public async Task  Insert_with_non_key_default_value()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        using (var context = new BlogContextNonKeyDefaultValue(testStore.Name))
        {
            context.Database.EnsureCreated();
            var blogs = new List<Blog>
            {
                new() { Name = "One Unicorn" }, new() { Name = "Two Unicorns", CreatedOn = new DateTime(1969, 8, 3, 0, 10, 0) }
            };
            context.AddRange(blogs);
            context.SaveChanges();

            Assert.NotEqual(new DateTime(), blogs[0].CreatedOn);
            Assert.NotEqual(new DateTime(), blogs[1].CreatedOn);
        }

        using (var context = new BlogContextNonKeyDefaultValue(testStore.Name))
        {
            var blogs = context.Blogs.OrderBy(e => e.Name).ToList();

            Assert.NotEqual(new DateTime(), blogs[0].CreatedOn);
            Assert.Equal(new DateTime(1969, 8, 3, 0, 10, 0), blogs[1].CreatedOn);

            blogs[0].CreatedOn = new DateTime(1973, 9, 3, 0, 10, 0);
            blogs[1].Name = "Zwo Unicorns";
            context.SaveChanges();
        }

        using (var context = new BlogContextNonKeyDefaultValue(testStore.Name))
        {
            var blogs = context.Blogs.OrderBy(e => e.Name).ToList();

            Assert.Equal(new DateTime(1969, 8, 3, 0, 10, 0), blogs[1].CreatedOn);
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), blogs[0].CreatedOn);
        }
    }

    public class BlogContextNonKeyDefaultValue(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>()
                .Property(e => e.CreatedOn)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("now()");
        }
    }

    [Fact]
    public async Task  Insert_with_non_key_default_value_readonly()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        using (var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name))
        {
            context.Database.EnsureCreated();
            context.AddRange(
                new Blog { Name = "One Unicorn" },
                new Blog { Name = "Two Unicorns" });
            context.SaveChanges();
            Assert.NotEqual(new DateTime(), context.Blogs.ToList()[0].CreatedOn);
        }

        DateTime dateTime0;

        using (var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            dateTime0 = blogs[0].CreatedOn;

            Assert.NotEqual(new DateTime(), dateTime0);
            Assert.NotEqual(new DateTime(), blogs[1].CreatedOn);

            blogs[0].Name = "One Pegasus";
            blogs[1].CreatedOn = new DateTime(1973, 9, 3, 0, 10, 0);

            context.SaveChanges();
        }

        using (var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(dateTime0, blogs[0].CreatedOn);
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), blogs[1].CreatedOn);
        }
    }

    public class BlogContextNonKeyReadOnlyDefaultValue(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>()
                .Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
        }
    }

    [Fact]
    public async Task  Insert_with_serial_non_id()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        int afterSave;

        using (var context = new BlogContextSequenceNonId(testStore.Name))
        {
            context.Database.EnsureCreated();

            var blog = context.Add(new Blog { Name = "One Unicorn" }).Entity;
            var beforeSave = blog.OtherId;
            context.SaveChanges();
            afterSave = blog.OtherId;

            Assert.NotEqual(beforeSave, afterSave);
        }

        using (var context = new BlogContextSequenceNonId(testStore.Name))
        {
            Assert.Equal(afterSave, context.Blogs.Single().OtherId);
        }
    }

    public class BlogContextSequenceNonId(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.OtherId)
                .ValueGeneratedOnAdd();
        }
    }

    [Fact]
    public async Task  Insert_with_client_generated_GUID_key()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        Guid afterSave;
        using (var context = new BlogContext(testStore.Name))
        {
            context.Database.EnsureCreated();
            var blog = context.Add(new GuidBlog { Name = "One Unicorn" }).Entity;
            var beforeSave = blog.Id;
            context.SaveChanges();
            afterSave = blog.Id;

            Assert.Equal(beforeSave, afterSave);
        }

        using (var context = new BlogContext(testStore.Name))
        {
            Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
        }
    }

    public class BlogContext(string databaseName) : ContextBase(databaseName);

    [Fact]
    public async Task  Insert_with_server_generated_GUID_key()
    {
        await using var testStore = await NpgsqlTestStore.CreateInitializedAsync(DatabaseName);

        Guid afterSave;
        using (var context = new BlogContextServerGuidKey(testStore.Name))
        {
            context.Database.EnsureCreated();

            var blog = context.Add(
                new GuidBlog { Name = "One Unicorn" }).Entity;
            var beforeSave = blog.Id;
            var beforeSaveNotId = blog.NotId;

            Assert.Equal(default, beforeSave);
            Assert.Equal(default, beforeSaveNotId);

            context.SaveChanges();

            afterSave = blog.Id;
            var afterSaveNotId = blog.NotId;

            Assert.NotEqual(default, afterSave);
            Assert.NotEqual(default, afterSaveNotId);
            Assert.NotEqual(beforeSave, afterSave);
            Assert.NotEqual(beforeSaveNotId, afterSaveNotId);
        }

        using (var context = new BlogContextServerGuidKey(testStore.Name))
        {
            Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
        }
    }

    public class BlogContextServerGuidKey(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");
            modelBuilder
                .Entity<GuidBlog>(
                    eb =>
                    {
                        eb.Property(e => e.Id)
                            .HasDefaultValueSql("uuid_generate_v4()");
                        eb.Property(e => e.NotId)
                            .HasDefaultValueSql("uuid_generate_v4()");
                    });
        }
    }

    public class Blog
    {
        public int Id { get; set; }
        public int OtherId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class NullableKeyBlog
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class FullNameBlog
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
    }

    public class GuidBlog
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid NotId { get; set; }
    }

    public class ConcurrentBlog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Timestamp { get; set; }
    }

    public abstract class ContextBase(string databaseName) : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<NullableKeyBlog> NullableKeyBlogs { get; set; }
        public DbSet<FullNameBlog> FullNameBlogs { get; set; }
        public DbSet<GuidBlog> GuidBlogs { get; set; }
        public DbSet<ConcurrentBlog> ConcurrentBlogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .EnableServiceProviderCaching(false)
                .UseNpgsql(
                    NpgsqlTestStore.CreateConnectionString(databaseName),
                    b => b.ApplyConfiguration());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
            // supported.
            modelBuilder.Entity<Blog>().Property(b => b.CreatedOn).HasColumnType("timestamp without time zone");
            modelBuilder.Entity<NullableKeyBlog>().Property(b => b.CreatedOn).HasColumnType("timestamp without time zone");
        }
    }
}
