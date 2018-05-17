using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;
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
                NpgsqlInet npgsqlInet = new IPAddress(0);

                bool[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrEquals(x.CidrMappedToNpgsqlInet, npgsqlInet))
                           .Select(x => x.CidrMappedToNpgsqlInet.Equals(npgsqlInet))
                           .ToArray();

                AssertContainsSql("SELECT x.\"CidrMappedToNpgsqlInet\" = @__npgsqlInet_0");
                AssertContainsSql("WHERE x.\"CidrMappedToNpgsqlInet\" >>= @__npgsqlInet_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddressContainsIPAddress()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress address = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.Contains(x.InetMappedToIPAddress, address))
                           .ToArray();

                AssertContainsSql("WHERE x.\"InetMappedToIPAddress\" >> @__address_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,NpgsqlInet,NpgsqlInet)"/>.
        /// </summary>
        [Fact]
        public void NpgsqlInetContainsNpgsqlInet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NpgsqlInet npgsqlInet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.Contains(x.CidrMappedToNpgsqlInet, npgsqlInet))
                           .ToArray();

                AssertContainsSql("WHERE x.\"CidrMappedToNpgsqlInet\" >> @__npgsqlInet_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddressDoesNotContainsIPAddress()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress address = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !EF.Functions.Contains(x.InetMappedToIPAddress, address))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"InetMappedToIPAddress\" >> @__address_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,NpgsqlInet,NpgsqlInet)"/>.
        /// </summary>
        [Fact]
        public void NpgsqlInetDoesNotContainNpgsqlInet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NpgsqlInet npgsqlInet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !EF.Functions.Contains(x.CidrMappedToNpgsqlInet, npgsqlInet))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"CidrMappedToNpgsqlInet\" >> @__npgsqlInet_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddressContainOrEqualIPAddress()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress address = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrEquals(x.InetMappedToIPAddress, address))
                           .ToArray();

                AssertContainsSql("WHERE x.\"InetMappedToIPAddress\" >>= @__address_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,NpgsqlInet,NpgsqlInet)"/>.
        /// </summary>
        [Fact]
        public void NpgsqlInetContainsOrEqualsNpgsqlInet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NpgsqlInet npgsqlInet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrEquals(x.CidrMappedToNpgsqlInet, npgsqlInet))
                           .ToArray();

                AssertContainsSql("WHERE x.\"CidrMappedToNpgsqlInet\" >>= @__npgsqlInet_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddressDoesNotContainOrEqualIPAddress()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress address = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !EF.Functions.ContainsOrEquals(x.InetMappedToIPAddress, address))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"InetMappedToIPAddress\" >>= @__address_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(DbFunctions,NpgsqlInet,NpgsqlInet)"/>.
        /// </summary>
        [Fact]
        public void NpgsqlInetDoesNotContainOrEqualNpgsqlInet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NpgsqlInet npgsqlInet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !EF.Functions.ContainsOrEquals(x.CidrMappedToNpgsqlInet, npgsqlInet))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"CidrMappedToNpgsqlInet\" >>= @__npgsqlInet_0 = TRUE)");
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

                    context.NetTestEntities
                           .AddRange(
                               new NetTestEntity { Id = 1, InetMappedToIPAddress = new IPAddress(1), CidrMappedToNpgsqlInet = new IPAddress(1) },
                               new NetTestEntity { Id = 2, InetMappedToIPAddress = new IPAddress(2), CidrMappedToNpgsqlInet = new IPAddress(2) },
                               new NetTestEntity { Id = 3, InetMappedToIPAddress = new IPAddress(3), CidrMappedToNpgsqlInet = new IPAddress(3) },
                               new NetTestEntity { Id = 4, InetMappedToIPAddress = new IPAddress(4), CidrMappedToNpgsqlInet = new IPAddress(4) },
                               new NetTestEntity { Id = 5, InetMappedToIPAddress = new IPAddress(5), CidrMappedToNpgsqlInet = new IPAddress(5) },
                               new NetTestEntity { Id = 6, InetMappedToIPAddress = new IPAddress(6), CidrMappedToNpgsqlInet = new IPAddress(6) },
                               new NetTestEntity { Id = 7, InetMappedToIPAddress = new IPAddress(7), CidrMappedToNpgsqlInet = new IPAddress(7) },
                               new NetTestEntity { Id = 8, InetMappedToIPAddress = new IPAddress(8), CidrMappedToNpgsqlInet = new IPAddress(8) },
                               new NetTestEntity { Id = 9, InetMappedToIPAddress = new IPAddress(9), CidrMappedToNpgsqlInet = new IPAddress(9) },
                               new NetTestEntity { Id = 10, InetMappedToIPAddress = new IPAddress(10), CidrMappedToNpgsqlInet = new IPAddress(10) });

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
            [Column(TypeName = "inet")]
            public IPAddress InetMappedToIPAddress { get; set; }

            /// <summary>
            /// The network address.
            /// </summary>
            [Column(TypeName = "cidr")]
            public NpgsqlInet CidrMappedToNpgsqlInet { get; set; }
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
