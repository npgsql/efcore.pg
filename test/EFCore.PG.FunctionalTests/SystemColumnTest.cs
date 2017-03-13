using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class SystemColumnTest : IDisposable
    {
        [Fact]
        public void xmin()
        {
            using (var context = CreateContext())
            {
                var e = new SomeEntity { Name = "Bart" };
                context.Add(e);
                context.SaveChanges();
                var firstVersion = e.xmin;

                e.Name = "Lisa";
                context.SaveChanges();
                var secondVersion = e.xmin;

                Assert.NotEqual(firstVersion, secondVersion);
            }
        }

        class SystemColumnContext : DbContext
        {
            internal SystemColumnContext(DbContextOptions options) : base(options) {}

            public DbSet<SomeEntity> SomeEntity { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<SomeEntity>().Property(e => e.xmin).ValueGeneratedOnAddOrUpdate();
            }
        }

        public class SomeEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public uint xmin { get; set; }
        }

        public SystemColumnTest()
        {
            //_testStore = NpgsqlTestStore.GetOrCreateShared("Crap",
            //    () => NpgsqlTestStore.CreateDatabase("Crap"));
            _testStore = NpgsqlTestStore.CreateScratch();

            _options = new DbContextOptionsBuilder()
                .UseNpgsql(_testStore.Connection)
                .Options;

            using (var context = CreateContext())
                context.Database.EnsureCreated();
        }

        SystemColumnContext CreateContext()
        {
            var context = new SystemColumnContext(_options);
            context.Database.UseTransaction(_testStore.Transaction);
            return context;
        }

        readonly DbContextOptions _options;
        readonly NpgsqlTestStore _testStore;

        public void Dispose() => _testStore.Dispose();
    }
}
