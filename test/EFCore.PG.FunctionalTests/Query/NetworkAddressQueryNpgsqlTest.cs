using System.Linq;
using System.Net;
using NpgsqlTypes;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    /// <summary>
    /// Provides unit tests for network address operator translations.
    /// </summary>
    public class NetworkAddressQueryNpgsqlTest : IClassFixture<NetworkAddressQueryNpgsqlFixture>
    {
        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        NetworkAddressQueryNpgsqlFixture Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture">
        /// The fixture of resources for testing.
        /// </param>
        public NetworkAddressQueryNpgsqlTest(NetworkAddressQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        public void AssertContainsSql(string sql)
        {
            Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddressContainsIPAddress()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress address = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => x.InetMappedToIPAddress.Contains(address))
                           .ToArray();

                AssertContainsSql("WHERE x.\"InetMappedToIPAddress\" >> @__address_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(NpgsqlInet,NpgsqlInet)"/>.
        /// </summary>
        [Fact]
        public void NpgsqlInetContainsNpgsqlInet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NpgsqlInet npgsqlInet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => x.CidrMappedToNpgsqlInet.Contains(npgsqlInet))
                           .ToArray();

                AssertContainsSql("WHERE x.\"CidrMappedToNpgsqlInet\" >> @__npgsqlInet_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddressDoesNotContainsIPAddress()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress address = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !x.InetMappedToIPAddress.Contains(address))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"InetMappedToIPAddress\" >> @__address_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(NpgsqlInet,NpgsqlInet)"/>.
        /// </summary>
        [Fact]
        public void NpgsqlInetDoesNotContainsNpgsqlInet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NpgsqlInet npgsqlInet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !x.CidrMappedToNpgsqlInet.Contains(npgsqlInet))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"CidrMappedToNpgsqlInet\" >> @__npgsqlInet_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddressContainsOrEqualsIPAddress()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress address = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => x.InetMappedToIPAddress.ContainsOrEquals(address))
                           .ToArray();

                AssertContainsSql("WHERE x.\"InetMappedToIPAddress\" >>= @__address_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(NpgsqlInet,NpgsqlInet)"/>.
        /// </summary>
        [Fact]
        public void NpgsqlInetContainsOrEqualsNpgsqlInet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NpgsqlInet npgsqlInet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => x.CidrMappedToNpgsqlInet.ContainsOrEquals(npgsqlInet))
                           .ToArray();

                AssertContainsSql("WHERE x.\"CidrMappedToNpgsqlInet\" >>= @__npgsqlInet_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddressDoesNotContainOrEqualIPAddress()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress address = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !x.InetMappedToIPAddress.ContainsOrEquals(address))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"InetMappedToIPAddress\" >>= @__address_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Contains(NpgsqlInet,NpgsqlInet)"/>.
        /// </summary>
        [Fact]
        public void NpgsqlInetDoesNotContainOrEqualNpgsqlInet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NpgsqlInet npgsqlInet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => !x.CidrMappedToNpgsqlInet.ContainsOrEquals(npgsqlInet))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"CidrMappedToNpgsqlInet\" >>= @__npgsqlInet_0 = TRUE)");
            }
        }
    }
}
