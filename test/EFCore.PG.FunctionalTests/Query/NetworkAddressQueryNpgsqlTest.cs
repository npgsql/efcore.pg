using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    /// <summary>
    /// Provides unit tests for network address operator translations.
    /// </summary>
    public class NetworkAddressQueryNpgsqlTest : IClassFixture<NetworkAddressQueryNpgsqlTest.NetworkAddressQueryNpgsqlFixture>
    {
        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        NetworkAddressQueryNpgsqlFixture Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture"> The fixture of resources for testing. </param>
        public NetworkAddressQueryNpgsqlTest(NetworkAddressQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        #region Tests

        /// <summary>
        /// Demonstrates parameter duplication.
        /// </summary>
        [Fact(Skip = nameof(NetworkAddressQueryNpgsqlTest))]
        public void Demonstrate_ValueTypeParametersAreDuplicated()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                bool[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrEqual(x.Cidr, cidr))
                           .Select(x => x.Cidr.Equals(cidr))
                           .ToArray();

                AssertContainsSql("SELECT x.\"Cidr\" = @__cidr_1 = TRUE");
                AssertContainsSql("WHERE x.\"Cidr\" >>= @__cidr_1 = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Contains_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.Contains(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Inet\" >> @__inet_1 = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Contains_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.Contains(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Cidr\" >> @__cidr_1 = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_DoesNotContain_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !EF.Functions.Contains(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Inet\" >> @__inet_1 = TRUE)");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_DoesNotContain_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !EF.Functions.Contains(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Cidr\" >> @__cidr_1 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainsOrEquals_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrEqual(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Inet\" >>= @__inet_1 = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_ContainsOrEquals_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrEqual(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Cidr\" >>= @__cidr_1 = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_DoesNotContainOrEqual_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !EF.Functions.ContainsOrEqual(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Inet\" >>= @__inet_1 = TRUE)");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_DoesNotContainOrEqual_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !EF.Functions.ContainsOrEqual(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Cidr\" >>= @__cidr_1 = TRUE)");
            }
        }

        #endregion

        #region Fixtures

        /// <summary>
        /// Represents a fixture suitable for testing network address operators.
        /// </summary>
        public class NetworkAddressQueryNpgsqlFixture : IDisposable
        {
            /// <summary>
            /// The <see cref="NpgsqlTestStore"/> used for testing.
            /// </summary>
            private readonly NpgsqlTestStore _testStore;

            /// <summary>
            /// The <see cref="DbContextOptions"/> used for testing.
            /// </summary>
            private readonly DbContextOptions _options;

            /// <summary>
            /// The logger factory used for testing.
            /// </summary>
            public TestSqlLoggerFactory TestSqlLoggerFactory { get; }

            /// <summary>
            /// Initializes a <see cref="NetworkAddressQueryNpgsqlFixture"/>.
            /// </summary>
            // ReSharper disable once UnusedMember.Global
            public NetworkAddressQueryNpgsqlFixture()
            {
                TestSqlLoggerFactory = new TestSqlLoggerFactory();

                _testStore = NpgsqlTestStore.CreateScratch();

                _options =
                    new DbContextOptionsBuilder()
                        .UseNpgsql(_testStore.ConnectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(
                            new ServiceCollection()
                                .AddEntityFrameworkNpgsql()
                                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                                .BuildServiceProvider())
                        .Options;

                using (NetContext context = CreateContext())
                {
                    context.Database.EnsureCreated();

// BUG: This throws for some reason
//                    for (int i = 0; i < 10; i++)
//                    {
//                        context.NetTestEntities
//                               .Add(
//                                   new NetTestEntity
//                                   {
//                                       Id = i,
//                                       Inet = new IPAddress(i),
//                                       Cidr = (IPAddress: new IPAddress(i), Subnet: i)
//                                   });
//                    }

                    context.SaveChanges();
                }
            }

            /// <summary>
            /// Creates a new <see cref="NetContext"/>.
            /// </summary>
            /// <returns>
            /// A <see cref="NetContext"/> for testing.
            /// </returns>
            public NetContext CreateContext()
            {
                return new NetContext(_options);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _testStore.Dispose();
            }
        }

        /// <summary>
        /// Represents an entity suitable for testing network address operators.
        /// </summary>
        public class NetTestEntity
        {
            /// <summary>
            /// The primary key.
            /// </summary>
            [Key]
            public int Id { get; set; }

            /// <summary>
            /// The network address.
            /// </summary>
            public IPAddress Inet { get; set; }

            /// <summary>
            /// The network address.
            /// </summary>
            public (IPAddress IPAddress, int Subnet) Cidr { get; set; }
        }

        /// <summary>
        /// Represents a database suitable for testing network address operators.
        /// </summary>
        public class NetContext : DbContext
        {
            /// <summary>
            /// Represents a set of entities with <see cref="IPAddress"/> properties.
            /// </summary>
            public DbSet<NetTestEntity> NetTestEntities { get; set; }

            /// <summary>
            /// Initializes a <see cref="NetContext"/>.
            /// </summary>
            /// <param name="options">
            /// The options to be used for configuration.
            /// </param>
            public NetContext(DbContextOptions options) : base(options) { }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        public void AssertContainsSql(string sql)
        {
            Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);
        }

        #endregion
    }
}
