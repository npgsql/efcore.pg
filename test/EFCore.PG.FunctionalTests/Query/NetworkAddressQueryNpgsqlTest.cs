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
    public class NetworkAddressQueryNpgsqlTest : IClassFixture<NetworkAddressQueryNpgsqlTest.NetworkAddressQueryNpgsqlFixture>
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
        public NetworkAddressQueryNpgsqlTest(NetworkAddressQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        #endregion

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
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.LessThan(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_LessThan_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThan(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" < @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.LessThan(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_LessThan_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThan(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" < @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.LessThanOrEqual(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_LessThanOrEqual_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThanOrEqual(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" <= @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.LessThanOrEqual(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_LessThanOrEqual_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.LessThanOrEqual(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" <= @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Equal(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Equal_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.Equal(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" = @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Equal(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Equal_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.Equal(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" = @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.GreaterThanOrEqual(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_GreaterThanOrEqual_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThanOrEqual(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" >= @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.GreaterThanOrEqual(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_GreaterThanOrEqual_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThanOrEqual(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" >= @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.GreaterThan(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_GreaterThan_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThan(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" > @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.GreaterThan(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_GreaterThan_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.GreaterThan(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" > @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.NotEqual(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_NotEqual_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.NotEqual(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Inet\" <> @__inet_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.NotEqual(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_NotEqual_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.NotEqual(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE (x.\"Cidr\" <> @__cidr_1) = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.ContainedBy(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainedBy_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainedBy(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Inet\" << @__inet_1 = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.ContainedBy(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_ContainedBy_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainedBy(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Cidr\" << @__cidr_1 = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.ContainedByOrEqual(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainedByOrEqual_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainedByOrEqual(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Inet\" <<= @__inet_1 = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.ContainedByOrEqual(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_ContainedByOrEqual_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainedByOrEqual(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Cidr\" <<= @__cidr_1 = TRUE");
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
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.ContainsOrEqual(DbFunctions,IPAddress,IPAddress)"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.ContainsOrEqual(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
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
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.ContainsOrContainedBy(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_ContainsOrContainedBy_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrContainedBy(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Inet\" && @__inet_1 = TRUE");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.ContainsOrContainedBy(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_ContainsOrContainedBy_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                NetTestEntity[] _ =
                    context.NetTestEntities
                           .Where(x => EF.Functions.ContainsOrContainedBy(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Cidr\" && @__cidr_1 = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Not(DbFunctions,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Not()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Not(x.Inet))
                           .ToArray();

                AssertContainsSql("SELECT ~x.\"Inet\"");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlNetworkAddressExtensions.Not(DbFunctions,ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Not()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Not(x.Cidr))
                           .ToArray();

                AssertContainsSql("SELECT ~x.\"Cidr\"");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.And(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_And_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.And(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("SELECT x.\"Inet\" & @__inet_1");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.And(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_And_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.And(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("SELECT x.\"Cidr\" & @__cidr_1");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Or(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Or_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Or(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("SELECT x.\"Inet\" | @__inet_1");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Or(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Or_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Or(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("SELECT x.\"Cidr\" | @__cidr_1");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Add(DbFunctions,IPAddress,int)"/>.
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

                AssertContainsSql("SELECT x.\"Inet\" + 1");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Add(DbFunctions,ValueTuple{IPAddress,int},int)"/>.
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

                AssertContainsSql("SELECT x.\"Cidr\" + 1");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Subtract(DbFunctions,IPAddress,int)"/>.
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

                AssertContainsSql("SELECT x.\"Inet\" - 1");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Subtract(DbFunctions,ValueTuple{IPAddress,int},int)"/>.
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

                AssertContainsSql("SELECT x.\"Cidr\" - 1");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Subtract(DbFunctions,IPAddress,IPAddress)"/>.
        /// </summary>
        [Fact]
        public void IPAddress_inet_Subtract_inet()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                IPAddress inet = new IPAddress(0);

                IPAddress[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Subtract(x.Inet, inet))
                           .ToArray();

                AssertContainsSql("SELECT x.\"Inet\" - @__inet_1");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Subtract(DbFunctions,ValueTuple{IPAddress,int},ValueTuple{IPAddress,int})"/>.
        /// </summary>
        [Fact]
        public void ValueTuple_cidr_Subtract_cidr()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                (IPAddress Address, int Subnet) cidr = (new IPAddress(0), 0);

                (IPAddress Address, int Subnet)[] _ =
                    context.NetTestEntities
                           .Select(x => EF.Functions.Subtract(x.Cidr, cidr))
                           .ToArray();

                AssertContainsSql("SELECT x.\"Cidr\" - @__cidr_1");
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

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Truncate(DbFunctions,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_Truncate()
        {
            using (NetContext context = Fixture.CreateContext())
            {
                var _ =
                    context.NetTestEntities
                           .Select(
                               x => new
                               {
                                   macaddr = EF.Functions.Truncate(x.Macaddr),
                                   macaddr8 = EF.Functions.Truncate(x.Macaddr8)
                               })
                           .ToArray();

                AssertContainsSql("SELECT trunc(x.\"Macaddr\") AS macaddr, trunc(x.\"Macaddr8\") AS macaddr8");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlNetworkAddressExtensions.Set7BitMac8(DbFunctions,PhysicalAddress)"/>.
        /// </summary>
        [Fact]
        public void PhysicalAddress_macaddr_Set7BitMac8()
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

            /// <summary>
            /// The MAC address.
            /// </summary>
            public PhysicalAddress Macaddr { get; set; }

            /// <summary>
            /// The MAC address.
            /// </summary>
            [Column(TypeName = "macaddr8")]
            public PhysicalAddress Macaddr8 { get; set; }
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
