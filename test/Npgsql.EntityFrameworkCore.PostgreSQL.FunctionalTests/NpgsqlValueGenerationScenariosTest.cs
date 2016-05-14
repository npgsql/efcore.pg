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
        public class SequenceId : TestBase<SequenceId.BlogContext>
        {
            [Fact]
            public void Insert_with_sequence_id()
            {
                int afterSave;

                using (var context = new BlogContext())
                {
                    var blog = context.Add(new Blog { Name = "One Unicorn" }).Entity;
                    var beforeSave = blog.Id;
                    context.SaveChanges();
                    afterSave = blog.Id;
                    Assert.NotEqual(beforeSave, afterSave);
                }

                using (var context = new BlogContext())
                {
                    Assert.Equal(afterSave, context.Blogs.Single().Id);
                }
            }

            public class BlogContext : ContextBase {}
        }

        public class NonSequenceId : TestBase<NonSequenceId.BlogContext>
        {
            [Fact]
            public void Insert_with_non_sequence_id()
            {
                int afterSave;

                using (var context = new BlogContext())
                {
                    var blog = context.Add(new Blog { Name = "One Unicorn" }).Entity;
                    var beforeSave = blog.Id;
                    context.SaveChanges();
                    afterSave = blog.Id;
                    Assert.Equal(beforeSave, afterSave);
                }

                using (var context = new BlogContext())
                {
                    Assert.Equal(afterSave, context.Blogs.Single().Id);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder
                        .Entity<Blog>()
                        .Property(e => e.Id)
                        .ValueGeneratedNever();
                }
            }
        }

        public class SequenceNonId : TestBase<SequenceNonId.BlogContext>
        {
            [Fact]
            public void Insert_with_sequence_non_id()
            {
                int afterSave;

                using (var context = new BlogContext())
                {
                    var blog = context.Add(new Blog { Name = "One Unicorn" }).Entity;
                    var beforeSave = blog.OtherId;
                    context.SaveChanges();
                    afterSave = blog.OtherId;
                    Assert.NotEqual(beforeSave, afterSave);
                }

                using (var context = new BlogContext())
                {
                    Assert.Equal(afterSave, context.Blogs.Single().OtherId);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder
                        .Entity<Blog>()
                        .Property(e => e.OtherId)
                        .ValueGeneratedOnAdd();
                }
            }
        }

        public class ServerGuidKey : TestBase<ServerGuidKey.BlogContext>
        {
            [Fact]
            public void Insert_with_server_generated_GUID_key()
            {
                Guid afterSave;

                using (var context = new BlogContext())
                {
                    var blog = context.Add(new GuidBlog { Name = "One Unicorn" }).Entity;
                    var beforeSave = blog.Id;
                    context.SaveChanges();
                    afterSave = blog.Id;
                    Assert.NotEqual(beforeSave, afterSave);
                }

                using (var context = new BlogContext())
                {
                    Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.HasPostgresExtension("uuid-ossp");

                    modelBuilder
                        .Entity<GuidBlog>()
                        .Property(e => e.Id)
                        .HasDefaultValueSql("uuid_generate_v4()");
                }
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
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<NullableKeyBlog> NullableKeyBlogs { get; set; }
            public DbSet<FullNameBlog> FullNameBlogs { get; set; }
            public DbSet<GuidBlog> GuidBlogs { get; set; }
            public DbSet<ConcurrentBlog> ConcurrentBlogs { get; set; }

            protected ContextBase()
            {
                Database.EnsureCreated();
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                var name = GetType().FullName.Substring((GetType().Namespace + nameof(NpgsqlValueGenerationScenariosTest)).Length + 2);
                optionsBuilder.UseNpgsql(NpgsqlTestStore.CreateConnectionString(name));
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
