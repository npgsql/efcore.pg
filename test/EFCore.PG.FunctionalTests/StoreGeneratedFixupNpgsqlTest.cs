using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class StoreGeneratedFixupNpgsqlTest : StoreGeneratedFixupRelationalTestBase<StoreGeneratedFixupNpgsqlTest.StoreGeneratedFixupNpgsqlFixture>
    {
        public StoreGeneratedFixupNpgsqlTest(StoreGeneratedFixupNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void Temp_values_are_replaced_on_save()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entry = context.Add(new TestTemp());

                    Assert.True(entry.Property(e => e.Id).IsTemporary);
                    Assert.False(entry.Property(e => e.NotId).IsTemporary);

                    var tempValue = entry.Property(e => e.Id).CurrentValue;

                    context.SaveChanges();

                    Assert.False(entry.Property(e => e.Id).IsTemporary);
                    Assert.NotEqual(tempValue, entry.Property(e => e.Id).CurrentValue);
                });
        }

        protected override void MarkIdsTemporary(DbContext context, object dependent, object principal)
        {
            var entry = context.Entry(dependent);
            entry.Property("Id1").IsTemporary = true;
            entry.Property("Id2").IsTemporary = true;

            entry = context.Entry(principal);
            entry.Property("Id1").IsTemporary = true;
            entry.Property("Id2").IsTemporary = true;
        }

        protected override void MarkIdsTemporary(DbContext context, object game, object level, object item)
        {
            var entry = context.Entry(game);
            entry.Property("Id").IsTemporary = true;

            entry = context.Entry(item);
            entry.Property("Id").IsTemporary = true;
        }

        protected override bool EnforcesFKs => true;

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class StoreGeneratedFixupNpgsqlFixture : StoreGeneratedFixupRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.HasPostgresExtension("uuid-ossp");

                modelBuilder.Entity<Parent>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<Child>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<ParentPN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<ChildPN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<ParentDN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<ChildDN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<ParentNN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<ChildNN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<CategoryDN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<ProductDN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<CategoryPN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<ProductPN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<CategoryNN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<ProductNN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<Category>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<Product>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()");
                        });

                modelBuilder.Entity<Item>(b => { b.Property(e => e.Id).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<Game>(b => { b.Property(e => e.Id).ValueGeneratedOnAdd().HasDefaultValueSql("uuid_generate_v4()"); });
            }
        }
    }
}
