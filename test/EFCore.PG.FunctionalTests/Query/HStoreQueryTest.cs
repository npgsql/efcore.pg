using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Query
{
    public class HStoreQueryTest : IClassFixture<HStoreFixture>
    {
        [Fact]
        public void HStore_key_value_selector()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.Tags["kind"] == "big").ToList();

                Assert.Equal(2, actual.Count);
                AssertContainsInSql(@"WHERE ""e"".""Tags"" -> 'kind' = 'big'");
            }
        }

        [Fact]
        public void HStore_projection()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities
                    .Select(e => new
                    {
                        Kind = e.Tags["kind"]
                    }).ToList();

                AssertContainsInSql(@"SELECT ""e"".""Tags"" -> 'kind' AS ""Kind""");
            }
        }

        [Fact]
        public void HStore_add_value()
        {
            //EF does not detect changes on single entries in dictionary
            using (var ctx = CreateContext())
            {
                var entity = ctx.SomeEntities.First(e => e.Id == 1);
                entity.Tags.Add("m", "d");
                var numberOfSavedEntities = ctx.SaveChanges();

                ctx.Entry(entity).State = EntityState.Detached;

                entity = ctx.SomeEntities.First(e => e.Id == 1);

                Assert.Equal(1, numberOfSavedEntities);
                Assert.True(entity.Tags.ContainsKey("m"));
                Assert.Equal("d", entity.Tags["m"]);
            }
        }

        [Fact]
        public void HStore_update_value()
        {
            //EF does not detect changes on single entries in dictionary
            using (var ctx = CreateContext())
            {
                var entity = ctx.SomeEntities.First(e => e.Id == 1);
                entity.Tags["kind"] = "thick";
                var numberOfSavedEntities = ctx.SaveChanges();

                ctx.Entry(entity).State = EntityState.Detached;

                entity = ctx.SomeEntities.First(e => e.Id == 1);

                Assert.Equal(1, numberOfSavedEntities);
                Assert.Equal("thick", entity.Tags["kind"]);
            }
        }

        [Fact]
        public void HStore_fetch_keys()
        {
            using (var ctx = CreateContext())
            {
                ctx.SomeEntities.Select(e => new
                {
                    TagNames = EF.Functions.HStoreKeys(e.Tags)
                }).ToList();

                AssertContainsInSql(@" akeys(""e"".""Tags"") ");
            }
        }

        [Fact]
        public void HStore_fetch_values()
        {
            using (var ctx = CreateContext())
            {
                ctx.SomeEntities.Select(e => new
                {
                    TagValues = EF.Functions.HStoreValues(e.Tags)
                }).ToList();

                AssertContainsInSql(@" avals(""e"".""Tags"") ");
            }
        }

        [Fact]
        public void HStore_contains_key()
        {
            using (var ctx = CreateContext())
            {
                var selected = ctx.SomeEntities.Where(e => e.Tags.ContainsKey("type")).ToList();

                Assert.Equal(1, selected.Count);
                AssertContainsInSql(@" ""e"".""Tags"" ? 'type' ");
            }
        }

        [Fact]
        public void HStore_key_contains_all_values_from_collection()
        {
            using (var ctx = CreateContext())
            {
                string[] values = new string[] { "big", "small" };
                ctx.SomeEntities.Where(e => values.All(v => e.Tags.Keys.Contains(v))).ToList();

                AssertContainsInSql(@"WHERE ""e"".""Tags"" ?& ARRAY [ 'big', 'small' ");
            }
        }

        [Fact]
        public void HStore_key_contains_any_value_from_collection()
        {
            using (var ctx = CreateContext())
            {
                string[] values = new string[] { "big", "small" };
                ctx.SomeEntities.Where(e => values.Any(v => e.Tags.Keys.Contains(v))).ToList();

                AssertContainsInSql(@"WHERE ""e"".""Tags"" ?| ARRAY [ 'big', 'small' ");
            }
        }

        #region Support

        HStoreFixture Fixture { get; }

        public HStoreQueryTest(HStoreFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        HStoreContext CreateContext() => Fixture.CreateContext();

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        #endregion Support
    }

    public class HStoreContext : DbContext
    {
        public DbSet<SomeEntity> SomeEntities { get; set; }
        public HStoreContext(DbContextOptions options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("hstore");
        }
    }

    public class SomeEntity
    {
        public int Id { get; set; }
        public Dictionary<string, string> Tags { get; set; }
    }

    public class HStoreFixture : IDisposable
    {
        readonly DbContextOptions _options;
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public HStoreFixture()
        {
            _testStore = NpgsqlTestStore.CreateScratch();
            _options = new DbContextOptionsBuilder()
                .UseNpgsql(_testStore.Connection, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkNpgsql()
                        .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                        .BuildServiceProvider())
                .Options;

            using (var ctx = CreateContext())
            {
                ctx.Database.EnsureCreated();
                ctx.SomeEntities.Add(new SomeEntity
                {
                    Id = 1,
                    Tags = new Dictionary<string, string>(ImmutableDictionary<string, string>.Empty.AddRange(new KeyValuePair<string, string>[] 
                    {
                        new KeyValuePair<string, string>("kind", "big"), 
                        new KeyValuePair<string, string>("type", "car")
                    }))
                });
                ctx.SomeEntities.Add(new SomeEntity
                {
                    Id = 2,
                    Tags = new Dictionary<string, string>(ImmutableDictionary<string, string>.Empty.AddRange(new KeyValuePair<string, string>[]
                    {
                        new KeyValuePair<string, string>("kind", "big")
                    }))
                });
                ctx.SomeEntities.Add(new SomeEntity
                {
                    Id = 3,
                    Tags = new Dictionary<string, string>(ImmutableDictionary<string, string>.Empty.AddRange(new KeyValuePair<string, string>[]
                    {
                        new KeyValuePair<string, string>("kind", "small")
                    }))
                });
                ctx.SaveChanges();
            }
        }

        readonly NpgsqlTestStore _testStore;
        public HStoreContext CreateContext() => new HStoreContext(_options);
        public void Dispose() => _testStore.Dispose();
    }
}
