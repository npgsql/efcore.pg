using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    /// <summary>
    /// Provides unit tests for network address operator and function translations.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-net.html
    /// </remarks>
    public class NetworkQueryNpgsqlTest : IClassFixture<NetworkQueryNpgsqlTest.NetworkAddressQueryNpgsqlFixture>
    {
        #region Setup

        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        NetworkAddressQueryNpgsqlFixture Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture"> The fixture of resources for testing. </param>
        public NetworkQueryNpgsqlTest(NetworkAddressQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        #endregion

        #region BugTests

        /// <summary>
        /// Demonstrates parameter duplication.
        /// </summary>
        [Fact(Skip = nameof(NetworkQueryNpgsqlTest))]
        public void Demonstrate_ValueTypeParametersAreDuplicated()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

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

        #region ParseTests

        /// <summary>
        /// Tests translation for <see cref="IPAddress.Parse(string)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_parse_column()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => x.Inet.Equals(IPAddress.Parse(x.TextInet)))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Inet\" = CAST(x.\"TextInet\" AS inet)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="IPAddress.Parse(string)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_parse_column()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => x.Macaddr.Equals(PhysicalAddress.Parse(x.TextMacaddr)))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Macaddr\" = CAST(x.\"TextMacaddr\" AS macaddr)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="IPAddress.Parse(string)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_parse_literal()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => x.Inet.Equals(IPAddress.Parse("127.0.0.1")))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Inet\" = @__Parse_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="IPAddress.Parse(string)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_parse_literal()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => x.Macaddr.Equals(PhysicalAddress.Parse("12-34-56-00-00-00")))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Macaddr\" = @__Parse_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="IPAddress.Parse(string)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_parse_parameter()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var inet = "127.0.0.1";

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => x.Inet.Equals(IPAddress.Parse(inet)))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Inet\" = @__Parse_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="IPAddress.Parse(string)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_parse_parameter()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var macaddr = "12-34-56-00-00-00";

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => x.Macaddr.Equals(PhysicalAddress.Parse(macaddr)))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Macaddr\" = @__Parse_0");
            }
        }

        #endregion

        #region RelationalOperatorTests

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.LessThan(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_LessThan_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThan(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" < @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.LessThan(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_LessThan_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThan(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" < @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.LessThan(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_LessThan_macaddr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress macaddr = new PhysicalAddress(new byte[6]);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThan(x.Macaddr, macaddr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Macaddr\" < @__macaddr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.LessThan(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr8_LessThan_macaddr8()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThan(x.Macaddr8, x.Macaddr8))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Macaddr8\" < x.\"Macaddr8\") = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.LessThanOrEqual(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_LessThanOrEqual_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThanOrEqual(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" <= @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.LessThanOrEqual(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_LessThanOrEqual_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThanOrEqual(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" <= @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.LessThanOrEqual(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_LessThanOrEqual_macaddr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress macaddr = new PhysicalAddress(new byte[6]);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThanOrEqual(x.Macaddr, macaddr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Macaddr\" <= @__macaddr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.LessThanOrEqual(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr8_LessThanOrEqual_macaddr8()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThanOrEqual(x.Macaddr8, x.Macaddr8))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Macaddr8\" <= x.\"Macaddr8\") = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.GreaterThanOrEqual(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_GreaterThanOrEqual_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThanOrEqual(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" >= @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.GreaterThanOrEqual(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_GreaterThanOrEqual_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThanOrEqual(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" >= @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.GreaterThanOrEqual(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_GreaterThanOrEqual_macaddr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress macaddr = new PhysicalAddress(new byte[6]);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThanOrEqual(x.Macaddr, macaddr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Macaddr\" >= @__macaddr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.GreaterThanOrEqual(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr8_GreaterThanOrEqual_macaddr8()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThanOrEqual(x.Macaddr8, x.Macaddr8))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Macaddr8\" >= x.\"Macaddr8\") = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.GreaterThan(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_GreaterThan_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThan(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" > @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.GreaterThan(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_GreaterThan_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThan(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" > @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.GreaterThan(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_GreaterThan_macaddr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress macaddr = new PhysicalAddress(new byte[6]);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThan(x.Macaddr, macaddr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Macaddr\" > @__macaddr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.GreaterThan(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr8_GreaterThan_macaddr8()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThan(x.Macaddr8, x.Macaddr8))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Macaddr8\" > x.\"Macaddr8\") = TRUE");
            }
        }

        #endregion

        #region ContainmentOperatorTests

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainedBy(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainedBy_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainedBy(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" << @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainedBy(DbFunctions,IPAddress,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainedBy_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainedBy(x.Inet, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" << @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainedBy(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_ContainedBy_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainedBy(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" << @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainedByOrEqual(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainedByOrEqual_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainedByOrEqual(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" <<= @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainedByOrEqual(DbFunctions,IPAddress,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainedByOrEqual_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainedByOrEqual(x.Inet, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" <<= @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainedByOrEqual(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_ContainedByOrEqual_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainedByOrEqual(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" <<= @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Contains(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Contains_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.Contains(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" >> @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Contains(DbFunctions,ValueTuple{IPAddress,int},IPAddress)"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Contains_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.Contains(x.Cidr, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" >> @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Contains(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Contains_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.Contains(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" >> @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainsOrEqual(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainsOrEqual_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrEqual(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" >>= @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainsOrEqual(DbFunctions,ValueTuple{IPAddress,int},IPAddress)"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_ContainsOrEqual_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrEqual(x.Cidr, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" >>= @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainsOrEqual(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_ContainsOrEqual_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrEqual(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" >>= @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainsOrContainedBy(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainsOrContainedBy_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrContainedBy(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" && @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainsOrContainedBy(DbFunctions,IPAddress,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainsOrContainedBy_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrContainedBy(x.Inet, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" && @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainsOrContainedBy(DbFunctions,ValueTuple{IPAddress,int},IPAddress)"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_ContainsOrContainedBy_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrContainedBy(x.Cidr, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" && @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.ContainsOrContainedBy(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_ContainsOrContainedBy_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrContainedBy(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" && @__cidr_1) = TRUE");
            }
        }

        #endregion

        #region BitwiseOperatorTests

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseNot(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_BitwiseNot()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseNot(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT ~x.\"Inet\"");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseNot(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_BitwiseNot()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseNot(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT ~x.\"Cidr\"");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseNot(DbFunctions,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_BitwiseNot()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseNot(x.Macaddr))
                           .ToArray();

                AssertContainsSql("SELECT ~x.\"Macaddr\"");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseNot(DbFunctions,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr8_BitwiseNot()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseNot(x.Macaddr8))
                           .ToArray();

                AssertContainsSql("SELECT ~x.\"Macaddr8\"");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseAnd(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_BitwiseAnd_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseAnd(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Inet\" & @__inet_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseAnd(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_BitwiseAnd_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseAnd(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Cidr\" & @__cidr_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseAnd(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_BitwiseAnd_macaddr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress macaddr = new PhysicalAddress(new byte[6]);

                PhysicalAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseAnd(x.Macaddr, macaddr))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Macaddr\" & @__macaddr_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseAnd(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr8_BitwiseAnd_macaddr8()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseAnd(x.Macaddr8, x.Macaddr8))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Macaddr8\" & x.\"Macaddr8\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseOr(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_BitwiseOr_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseOr(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Inet\" | @__inet_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseOr(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_BitwiseOr_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseOr(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Cidr\" | @__cidr_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseOr(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_BitwiseOr_macaddr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress macaddr = new PhysicalAddress(new byte[6]);

                PhysicalAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseOr(x.Macaddr, macaddr))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Macaddr\" | @__macaddr_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.BitwiseOr(DbFunctions,PhysicalAddress,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr8_BitwiseOr_macaddr8()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.BitwiseOr(x.Macaddr8, x.Macaddr8))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Macaddr8\" | x.\"Macaddr8\")");
            }
        }

        #endregion

        #region ArithmeticOperatorTests

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Add(DbFunctions,IPAddress,int)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Add_int()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Add(x.Inet, 1))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Inet\" + 1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Add(DbFunctions,ValueTuple{IPAddress,int},int)"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Add_int()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Add(x.Cidr, 1))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Cidr\" + 1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Subtract(DbFunctions,IPAddress,int)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Subtract_int()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Subtract(x.Inet, 1))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Inet\" - 1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Subtract(DbFunctions,ValueTuple{IPAddress,int},int)"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Subtract_int()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Subtract(x.Cidr, 1))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Cidr\" - 1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Subtract(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Subtract_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Subtract(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Inet\" - @__inet_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Subtract(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Subtract_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Subtract(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"Cidr\" - @__cidr_1)");
            }
        }

        #endregion

        #region FunctionTests

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Abbreviate(DbFunctions,IPAddress)"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Abbreviate(DbFunctions,ValueTuple{IPAddress,int})"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Broadcast(DbFunctions,IPAddress)"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Broadcast(DbFunctions,ValueTuple{IPAddress,int})"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Family(DbFunctions,IPAddress)"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Family(DbFunctions,ValueTuple{IPAddress,int})"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Host(DbFunctions,IPAddress)"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Host(DbFunctions,ValueTuple{IPAddress,int})"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.HostMask(DbFunctions,IPAddress)"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.HostMask(DbFunctions,ValueTuple{IPAddress,int})"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.MaskLength(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_MaskLength()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                int[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.MaskLength(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT masklen(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.MaskLength(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_MaskLength()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                int[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.MaskLength(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT masklen(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Netmask(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Netmask()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Netmask(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT netmask(x.\"Inet\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Netmask(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Netmask()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Netmask(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT netmask(x.\"Cidr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Network(DbFunctions,IPAddress)"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Network(DbFunctions,ValueTuple{IPAddress,int})"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.SetMaskLength(DbFunctions,IPAddress,int)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_SetMaskLength()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SetMaskLength(x.Inet, default))
                           .ToArray();

                AssertContainsSql("SELECT set_masklen(x.\"Inet\", 0)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.SetMaskLength(DbFunctions,ValueTuple{IPAddress,int},int)"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_SetMaskLength()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SetMaskLength(x.Cidr, default))
                           .ToArray();

                AssertContainsSql("SELECT set_masklen(x.\"Cidr\", 0)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Text(DbFunctions,IPAddress)"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Text(DbFunctions,ValueTuple{IPAddress,int})"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.SameFamily(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_SameFamily()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                bool[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SameFamily(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("SELECT inet_same_family(x.\"Inet\", @__inet_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.SameFamily(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_SameFamily()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                bool[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.SameFamily(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("SELECT inet_same_family(x.\"Cidr\", @__cidr_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Merge(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Merge()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = IPAddress.Any;

                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Merge(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("SELECT inet_merge(x.\"Inet\", @__inet_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Merge(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Merge()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (IPAddress.Any, default);

                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Merge(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("SELECT inet_merge(x.\"Cidr\", @__cidr_1)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Truncate(DbFunctions,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_Truncate()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                var _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Truncate(x.Macaddr))
                           .ToArray();

                AssertContainsSql("SELECT trunc(x.\"Macaddr\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Truncate(DbFunctions,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr8_Truncate()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                var _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Truncate(x.Macaddr8))
                           .ToArray();

                AssertContainsSql("SELECT trunc(x.\"Macaddr8\")");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkExtensions.Set7BitMac8(DbFunctions,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr8_Set7BitMac8()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                PhysicalAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Set7BitMac8(x.Macaddr8))
                           .ToArray();

                AssertContainsSql("SELECT macaddr8_set7bit(x.\"Macaddr8\")");
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
            public NetContext CreateContext() => new NetContext(_options);

            /// <inheritdoc />
            public void Dispose() => _testStore.Dispose();
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

            /// <summary>
            /// The MAC address.
            /// </summary>
            public PhysicalAddress Macaddr { get; set; }

            /// <summary>
            /// The MAC address.
            /// </summary>
            [Column(TypeName = "macaddr8")]
            public PhysicalAddress Macaddr8 { get; set; }

            /// <summary>
            /// The text form of <see cref="Inet"/>.
            /// </summary>
            public string TextInet { get; set; }

            /// <summary>
            /// The text form of <see cref="Macaddr"/>.
            /// </summary>
            public string TextMacaddr { get; set; }
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
            public NetContext(DbContextOptions options) : base(options) {}
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        public void AssertContainsSql(string sql) => Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);

        #endregion
    }
}
