using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class NpgsqlValueGenerationScenariosTest
    {
        static readonly string DatabaseName = "NpgsqlValueGenerationScenariosTest";

        [Fact]
        public void Insert_with_sequence_id()
        {
            using (var testStore = NpgsqlTestStore.Create(DatabaseName))
            {
                using (var context = new BlogContextSequence(testStore.Name))
                {
                    context.Database.EnsureCreated();

                    context.AddRange(new Blog { Name = "One Unicorn" }, new Blog { Name = "Two Unicorns" });

                    context.SaveChanges();
                }

                using (var context = new BlogContextSequence(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(1, blogs[0].Id);
                    Assert.Equal(2, blogs[1].Id);
                }
            }
        }

        public class BlogContextSequence : ContextBase
        {
            public BlogContextSequence(string databaseName) : base(databaseName) { }
        }

        [Fact]
        public void Insert_with_explicit_non_default_keys()
        {
            using (var testStore = NpgsqlTestStore.Create(DatabaseName))
            {
                using (var context = new BlogContextNoKeyGeneration(testStore.Name))
                {
                    context.Database.EnsureCreated();

                    context.AddRange(new Blog { Id = 66, Name = "One Unicorn" }, new Blog { Id = 67, Name = "Two Unicorns" });

                    context.SaveChanges();
                }

                using (var context = new BlogContextNoKeyGeneration(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(66, blogs[0].Id);
                    Assert.Equal(67, blogs[1].Id);
                }
            }
        }

        public class BlogContextNoKeyGeneration : ContextBase
        {
            public BlogContextNoKeyGeneration(string databaseName) : base(databaseName) {}

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Blog>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();
            }
        }

        [Fact]
        public void Insert_with_sequence_non_id()
        {
            using (var testStore = NpgsqlTestStore.Create(DatabaseName))
            {
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
        }

        public class BlogContextSequenceNonId : ContextBase
        {
            public BlogContextSequenceNonId(string databaseName) : base(databaseName) { }

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
        public void Insert_with_client_generated_GUID_key()
        {
            using (var testStore = NpgsqlTestStore.Create(DatabaseName))
            {
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
        }

        public class BlogContext : ContextBase
        {
            public BlogContext(string databaseName) : base(databaseName) {}
        }

        [Fact]
        public void Insert_with_server_generated_GUID_key()
        {
            using (var testStore = NpgsqlTestStore.Create(DatabaseName))
            {
                Guid afterSave;
                using (var context = new BlogContextServerGuidKey(testStore.Name))
                {
                    context.Database.EnsureCreated();

                    var blog = context.Add(new GuidBlog { Name = "One Unicorn" }).Entity;

                    var beforeSave = blog.Id;

                    context.SaveChanges();

                    afterSave = blog.Id;

                    Assert.NotEqual(beforeSave, afterSave);
                }

                using (var context = new BlogContextServerGuidKey(testStore.Name))
                {
                    Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
                }
            }
        }

        public class BlogContextServerGuidKey : ContextBase
        {
            public BlogContextServerGuidKey(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<GuidBlog>()
                    .Property(e => e.Id)
                    .HasDefaultValueSql("uuid_generate_v4()");
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
        }

        public class ConcurrentBlog
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public byte[] Timestamp { get; set; }
        }

        public abstract class ContextBase : DbContext
        {
            readonly string _databaseName;

            protected ContextBase(string databaseName)
            {
                _databaseName = databaseName;
            }

            public DbSet<Blog> Blogs { get; set; }
            public DbSet<NullableKeyBlog> NullableKeyBlogs { get; set; }
            public DbSet<FullNameBlog> FullNameBlogs { get; set; }
            public DbSet<GuidBlog> GuidBlogs { get; set; }
            public DbSet<ConcurrentBlog> ConcurrentBlogs { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(NpgsqlTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.HasPostgresExtension("uuid-ossp");
            }
        }

        public class TestBase<TContext>
            where TContext : ContextBase, new()
        {
            public TestBase()
            {
                using (var context = new TContext())
                {
                    context.Database.EnsureDeleted();
                }
            }
        }
    }
}
