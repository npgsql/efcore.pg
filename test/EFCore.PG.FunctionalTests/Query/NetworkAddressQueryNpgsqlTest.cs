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

        #region BugTests

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

        #endregion

        #region OperatorTests

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

        #region FunctionTests

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Abbreviate(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Abbreviate()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                string[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Abbreviate(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT abbrev(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Abbreviate(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Abbrebiate()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                string[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Abbreviate(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT abbrev(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Broadcast(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Broadcast()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Broadcast(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT broadcast(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Broadcast(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Broadcast()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Broadcast(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT broadcast(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Family(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Family()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                int[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Family(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT family(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Family(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Family()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                int[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Family(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT family(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Host(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Host()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                string[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Host(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT host(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Host(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Host()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                string[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Host(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT host(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.HostMask(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_HostMask()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.HostMask(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT hostmask(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.HostMask(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_HostMask()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.HostMask(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT hostmask(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.SubnetLength(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_SubnetLength()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                int[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SubnetLength(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT masklen(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.SubnetLength(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_SubnetLength()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                int[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SubnetLength(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT masklen(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.SubnetMask(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_SubnetMask()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SubnetMask(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT netmask(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.SubnetMask(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_SubnetMask()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SubnetMask(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT netmask(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Network(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Network()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Network(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT network(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Network(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Network()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Network(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT network(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.SetSubnetLength(DbFunctions,IPAddress,int)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_SetSubnetLength()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SetSubnetLength(x.Inet, 0))
                           .ToArray();

                AssertContainsSql("SELECT set_masklen(x.\"Inet\", 0)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.SetSubnetLength(DbFunctions,ValueTuple{IPAddress,int},int)"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_SetSubnetLength()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SetSubnetLength(x.Cidr, 0))
                           .ToArray();

                AssertContainsSql("SELECT set_masklen(x.\"Cidr\", 0)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Text(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Text()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                string[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Text(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT text(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Text(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Text()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                string[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Text(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT text(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.SameFamily(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_SameFamily()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                bool[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SameFamily(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("SELECT inet_same_family(x.\"Inet\", @__inet_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.SameFamily(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_SameFamily()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                bool[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SameFamily(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("SELECT inet_same_family(x.\"Cidr\", @__cidr_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Merge(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Merge()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Merge(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("SELECT inet_merge(x.\"Inet\", @__inet_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Merge(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Merge()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Merge(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("SELECT inet_merge(x.\"Cidr\", @__cidr_1)");
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
            public (IPAddress Address, int Subnet) Cidr { get; set; }
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
