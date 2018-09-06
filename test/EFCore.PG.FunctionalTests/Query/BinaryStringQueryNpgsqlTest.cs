using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class BinaryStringQueryNpgsqlTest : IClassFixture<BinaryStringQueryNpgsqlTest.BinaryStringFixture>
    {
        #region Tests

        [Fact]
        public void Md5_cryptography_v8_0()
        {
            using (var ctx = Fixture.CreateContext(new Version(8, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => MD5.Create().ComputeHash(e.Data)).ToArray();

                AssertDoesNotContainInSql("md5");
            }
        }

        [Fact]
        public void Md5_cryptography_v8_1()
        {
            using (var ctx = Fixture.CreateContext(new Version(8, 1)))
            {
                var _ = ctx.SomeEntities.Select(e => MD5.Create().ComputeHash(e.Data)).ToArray();

                AssertContainsInSql("SELECT decode(md5(e.\"Data\"), 'hex')");
            }
        }

        [Fact]
        public void Md5_extension_v8_0()
        {
            using (var ctx = Fixture.CreateContext(new Version(8, 0)))
            {
                Assert.Throws<NotSupportedException>(() => ctx.SomeEntities.Select(e => EF.Functions.MD5(e.Text)).ToArray());

                AssertDoesNotContainInSql("md5");
            }
        }

        [Fact]
        public void Md5_extension_v8_1()
        {
            using (var ctx = Fixture.CreateContext(new Version(8, 1)))
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.MD5(e.Text)).ToArray();

                AssertContainsInSql("SELECT md5(e.\"Text\")");
            }
        }

        [Fact]
        public void Sha224_extension_v10()
        {
            using (var ctx = Fixture.CreateContext(new Version(10, 4)))
            {
                Assert.Throws<NotSupportedException>(() => ctx.SomeEntities.Select(e => EF.Functions.SHA224(e.Data)).ToArray());

                AssertDoesNotContainInSql("sha224");
            }
        }

        [Fact]
        public void Sha224_extension_v11()
        {
            using (var ctx = Fixture.CreateContext(new Version(11, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.SHA224(e.Data)).ToArray();

                AssertContainsInSql("SELECT sha224(e.\"Data\")");
            }
        }

        [Fact]
        public void Sha256_cryptography_v10()
        {
            using (var ctx = Fixture.CreateContext(new Version(10, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => SHA256.Create().ComputeHash(e.Data)).ToArray();

                AssertDoesNotContainInSql("sha256");
            }
        }

        [Fact]
        public void Sha256_cryptography_v11()
        {
            using (var ctx = Fixture.CreateContext(new Version(11, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => SHA256.Create().ComputeHash(e.Data)).ToArray();

                AssertContainsInSql("SELECT sha256(e.\"Data\")");
            }
        }

        [Fact]
        public void Sha256_extension_v10()
        {
            using (var ctx = Fixture.CreateContext(new Version(10, 4)))
            {
                Assert.Throws<NotSupportedException>(() => ctx.SomeEntities.Select(e => EF.Functions.SHA256(e.Data)).ToArray());

                AssertDoesNotContainInSql("sha256");
            }
        }

        [Fact]
        public void Sha256_extension_v11()
        {
            using (var ctx = Fixture.CreateContext(new Version(11, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.SHA256(e.Data)).ToArray();

                AssertContainsInSql("SELECT sha256(e.\"Data\")");
            }
        }

        [Fact]
        public void Sha384_cryptography_v10()
        {
            using (var ctx = Fixture.CreateContext(new Version(10, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => SHA384.Create().ComputeHash(e.Data)).ToArray();

                AssertDoesNotContainInSql("sha384");
            }
        }

        [Fact]
        public void Sha384_cryptography_v11()
        {
            using (var ctx = Fixture.CreateContext(new Version(11, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => SHA384.Create().ComputeHash(e.Data)).ToArray();

                AssertContainsInSql("SELECT sha384(e.\"Data\")");
            }
        }

        [Fact]
        public void Sha384_extension_v10()
        {
            using (var ctx = Fixture.CreateContext(new Version(10, 4)))
            {
                Assert.Throws<NotSupportedException>(() => ctx.SomeEntities.Select(e => EF.Functions.SHA384(e.Data)).ToArray());

                AssertDoesNotContainInSql("sha384");
            }
        }

        [Fact]
        public void Sha384_extension_v11()
        {
            using (var ctx = Fixture.CreateContext(new Version(11, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.SHA384(e.Data)).ToArray();

                AssertContainsInSql("SELECT sha384(e.\"Data\")");
            }
        }

        [Fact]
        public void Sha512_extension_v10()
        {
            using (var ctx = Fixture.CreateContext(new Version(10, 4)))
            {
                Assert.Throws<NotSupportedException>(() => ctx.SomeEntities.Select(e => EF.Functions.SHA512(e.Data)).ToArray());

                AssertDoesNotContainInSql("sha512");
            }
        }

        [Fact]
        public void Sha512_extension_v11()
        {
            using (var ctx = Fixture.CreateContext(new Version(11, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.SHA512(e.Data)).ToArray();

                AssertContainsInSql("SELECT sha512(e.\"Data\")");
            }
        }

        [Fact]
        public void Sha512_cryptography_v10()
        {
            using (var ctx = Fixture.CreateContext(new Version(10, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => SHA512.Create().ComputeHash(e.Data)).ToArray();

                AssertDoesNotContainInSql("sha512");
            }
        }

        [Fact]
        public void Sha512_cryptography_v11()
        {
            using (var ctx = Fixture.CreateContext(new Version(11, 0)))
            {
                var _ = ctx.SomeEntities.Select(e => SHA512.Create().ComputeHash(e.Data)).ToArray();

                AssertContainsInSql("SELECT sha512(e.\"Data\")");
            }
        }

        #endregion

        #region Support

        BinaryStringFixture Fixture { get; }

        public BinaryStringQueryNpgsqlTest(BinaryStringFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        public class BinaryStringContext : DbContext
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public DbSet<SomeEnumEntity> SomeEntities { get; set; }

            public BinaryStringContext(DbContextOptions options) : base(options) {}
        }

        public class SomeEnumEntity
        {
            public int Id { get; set; }

            public byte[] Data { get; set; }

            public string Text { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        public class BinaryStringFixture : IDisposable
        {
            readonly NpgsqlTestStore _testStore;
            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public BinaryStringFixture()
            {
                _testStore = NpgsqlTestStore.CreateScratch();

                using (var ctx = CreateContext())
                {
                    ctx.Database.EnsureCreated();

                    // TODO: remove once test servers have PG 11.
                    ctx.Database.ExecuteSqlCommand("CREATE FUNCTION sha224(bytea) RETURNS bytea AS $$ SELECT $1; $$ LANGUAGE SQL;");
                    ctx.Database.ExecuteSqlCommand("CREATE FUNCTION sha256(bytea) RETURNS bytea AS $$ SELECT $1; $$ LANGUAGE SQL;");
                    ctx.Database.ExecuteSqlCommand("CREATE FUNCTION sha384(bytea) RETURNS bytea AS $$ SELECT $1; $$ LANGUAGE SQL;");
                    ctx.Database.ExecuteSqlCommand("CREATE FUNCTION sha512(bytea) RETURNS bytea AS $$ SELECT $1; $$ LANGUAGE SQL;");

                    ctx.SomeEntities
                       .Add(new SomeEnumEntity { Id = 1, Data = Encoding.UTF8.GetBytes("testing"), Text = "testing" });

                    ctx.SaveChanges();
                }
            }

            internal BinaryStringContext CreateContext(Version postgresVersion = default)
                => new BinaryStringContext(CreateOptions(postgresVersion));

            public void Dispose() => _testStore.Dispose();

            DbContextOptions CreateOptions(Version postgresVersion = null)
                => new DbContextOptionsBuilder()
                   .UseNpgsql(_testStore.ConnectionString, b => b.ApplyConfiguration().SetPostgresVersion(postgresVersion))
                   .UseInternalServiceProvider(
                       new ServiceCollection()
                           .AddEntityFrameworkNpgsql()
                           .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                           .BuildServiceProvider())
                   .Options;
        }

        #endregion
    }
}
