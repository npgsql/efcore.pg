using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.NetworkInformation;

// ReSharper disable ConvertToConstant.Local

namespace Microsoft.EntityFrameworkCore.Query.Translations;

/// <summary>
///     Provides unit tests for network address operator and function translations.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/static/functions-net.html
/// </remarks>
public class NetworkTranslationsNpgsqlTest : IClassFixture<NetworkTranslationsNpgsqlTest.NetworkAddressQueryNpgsqlFixture>
{
    private NetworkAddressQueryNpgsqlFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public NetworkTranslationsNpgsqlTest(NetworkAddressQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region BugTests

    [Fact]
    public void Demonstrate_ValueTypeParametersAreDuplicated()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);

        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrEqual(x.IPNetwork, cidr))
            .Select(x => x.IPNetwork.Equals(cidr))
            .ToArray();

        AssertSql(
            """
@cidr='0.0.0.0/0' (DbType = Object)
@p='0.0.0.0/0' (DbType = Object)

SELECT n."IPNetwork" = @cidr
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" >>= @p
""");
    }

    #endregion

    #region ParseTests

    [Fact]
    public void IPAddress_inet_parse_column()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => x.Inet.Equals(IPAddress.Parse(x.TextInet)));

        Assert.Equal(9, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Inet" = n."TextInet"::inet
""");
    }

    [Fact]
    public void PhysicalAddress_macaddr_parse_column()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => x.Macaddr.Equals(PhysicalAddress.Parse(x.TextMacaddr)));

        Assert.Equal(9, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Macaddr" = n."TextMacaddr"::macaddr
""");
    }

    [Fact]
    public void IPAddress_inet_parse_literal()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => x.Inet.Equals(IPAddress.Parse("192.168.1.2")));

        Assert.Equal(1, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Inet" = INET '192.168.1.2'
""");
    }

    [Fact]
    public void PhysicalAddress_macaddr_parse_literal()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => x.Macaddr.Equals(PhysicalAddress.Parse("12-34-56-00-00-02")));

        Assert.Equal(1, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Macaddr" = MACADDR '123456000002'
""");
    }

    [Fact]
    public void IPAddress_inet_parse_parameter()
    {
        using var context = CreateContext();
        var inet = "192.168.1.2";
        var count = context.NetTestEntities.Count(x => x.Inet.Equals(IPAddress.Parse(inet)));

        Assert.Equal(1, count);
    }

    [Fact]
    public void PhysicalAddress_macaddr_parse_parameter()
    {
        using var context = CreateContext();
        var macaddr = "12-34-56-00-00-01";
        var count = context.NetTestEntities.Count(x => x.Macaddr.Equals(PhysicalAddress.Parse(macaddr)));

        Assert.Equal(1, count);
    }

    #endregion

    #region RelationalOperatorTests

    [Fact]
    public void LessThan_IPAddress()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => EF.Functions.LessThan(x.Inet, IPAddress.Parse("192.168.1.7")));

        Assert.Equal(6, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Inet" < INET '192.168.1.7'
""");
    }

    [Fact]
    public void LessThan_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.LessThan(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" < @p
""");
    }

    [Fact]
    public void LessThan_PhysicalAddress()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => EF.Functions.LessThan(x.Macaddr, PhysicalAddress.Parse("12-34-56-00-00-07")));

        Assert.Equal(6, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Macaddr" < MACADDR '123456000007'
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(10, 0)]
    public void LessThan_PhysicalAddress_macaddr8()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => EF.Functions.LessThan(x.Macaddr8, PhysicalAddress.Parse("08-00-2B-01-02-03-04-07")));

        Assert.Equal(6, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Macaddr8" < MACADDR8 '08002B0102030407'
""");
    }

    [Fact]
    public void LessThanOrEqual_IPAddress()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => EF.Functions.LessThanOrEqual(x.Inet, IPAddress.Parse("192.168.1.7")));

        Assert.Equal(7, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Inet" <= INET '192.168.1.7'
""");
    }

    [Fact]
    public void LessThanOrEqual_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.LessThanOrEqual(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" <= @p
""");
    }

    [Fact]
    public void LessThanOrEqual_PhysicalAddress()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => EF.Functions.LessThanOrEqual(x.Macaddr, PhysicalAddress.Parse("12-34-56-00-00-07")));

        Assert.Equal(7, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Macaddr" <= MACADDR '123456000007'
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(10, 0)]
    public void LessThanOrEqual_PhysicalAddress_macaddr8()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(
            x => EF.Functions.LessThanOrEqual(x.Macaddr8, PhysicalAddress.Parse("08-00-2B-01-02-03-04-07")));

        Assert.Equal(7, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Macaddr8" <= MACADDR8 '08002B0102030407'
""");
    }

    [Fact]
    public void GreaterThanOrEqual_IPAddress()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => EF.Functions.GreaterThanOrEqual(x.Inet, IPAddress.Parse("192.168.1.7")));

        Assert.Equal(3, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Inet" >= INET '192.168.1.7'
""");
    }

    [Fact]
    public void GreaterThanOrEqual_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.GreaterThanOrEqual(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" >= @p
""");
    }

    [Fact]
    public void GreaterThanOrEqual_PhysicalAddress()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(
            x => EF.Functions.GreaterThanOrEqual(x.Macaddr, PhysicalAddress.Parse("12-34-56-00-00-07")));

        Assert.Equal(3, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Macaddr" >= MACADDR '123456000007'
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(10, 0)]
    public void GreaterThanOrEqual_PhysicalAddress_macaddr8()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(
            x => EF.Functions.GreaterThanOrEqual(x.Macaddr8, PhysicalAddress.Parse("08-00-2B-01-02-03-04-07")));

        Assert.Equal(3, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Macaddr8" >= MACADDR8 '08002B0102030407'
""");
    }

    [Fact]
    public void GreaterThan_IPAddress()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => EF.Functions.GreaterThan(x.Inet, IPAddress.Parse("192.168.1.7")));

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Inet" > INET '192.168.1.7'
""");
    }

    [Fact]
    public void GreaterThan_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.GreaterThan(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" > @p
""");
    }

    [Fact]
    public void GreaterThan_PhysicalAddress()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(x => EF.Functions.GreaterThan(x.Macaddr, PhysicalAddress.Parse("12-34-56-00-00-07")));

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Macaddr" > MACADDR '123456000007'
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(10, 0)]
    public void GreaterThan_PhysicalAddress_macaddr8()
    {
        using var context = CreateContext();
        var count = context.NetTestEntities.Count(
            x => EF.Functions.GreaterThan(x.Macaddr8, PhysicalAddress.Parse("08-00-2B-01-02-03-04-07")));

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Macaddr8" > MACADDR8 '08002B0102030407'
""");
    }

    #endregion

    #region ContainmentOperatorTests

    [Fact]
    public void ContainedBy_IPAddress_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainedBy(x.Inet, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" << @p
""");
    }

    [Fact]
    public void ContainedBy_IPAddress_and_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainedBy(x.Inet, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" << @p
""");
    }

    [Fact]
    public void ContainedBy_IPNetwork_and_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainedBy(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" << @p
""");
    }

    [Fact]
    public void ContainedByOrEqual_IPAddress_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainedByOrEqual(x.Inet, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" <<= @p
""");
    }

    [Fact]
    public void ContainedByOrEqual_IPAddress_and_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainedByOrEqual(x.Inet, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" <<= @p
""");
    }

    [Fact]
    public void ContainedByOrEqual_IPNetwork_and_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainedByOrEqual(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" <<= @p
""");
    }

    [Fact]
    public void Contains_IPAddress_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.Contains(x.Inet, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" >> @p
""");
    }

    [Fact]
    public void Contains_IPNetwork_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.Contains(x.IPNetwork, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" >> @p
""");
    }

    [Fact]
    public void Contains_IPNetwork_and_IPNetworks()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.Contains(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" >> @p
""");
    }

    [Fact]
    public void ContainsOrEqual_IPAddress_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrEqual(x.Inet, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" >>= @p
""");
    }

    [Fact]
    public void ContainsOrEqual_IPNetwork_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrEqual(x.IPNetwork, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" >>= @p
""");
    }

    [Fact]
    public void ContainsOrEqual_IPNetwork_and_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrEqual(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" >>= @p
""");
    }

    [Fact]
    public void ContainsOrContainedBy_IPAddress_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrContainedBy(x.Inet, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" && @p
""");
    }

    [Fact]
    public void ContainsOrContainedBy_IPAddress_and_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrContainedBy(x.Inet, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" && @p
""");
    }

    [Fact]
    public void ContainsOrContainedBy_IPNetwork_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrContainedBy(x.IPNetwork, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" && @p
""");
    }

    [Fact]
    public void ContainsOrContainedBy_IPNetwork_and_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrContainedBy(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."IPNetwork" && @p
""");
    }

    #endregion

    #region BitwiseOperatorTests

    [Fact]
    public void BitwiseNot_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseNot(x.Inet))
            .ToArray();

        AssertSql(
            """
SELECT ~n."Inet"
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void BitwiseNot_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseNot(x.IPNetwork))
            .ToArray();

        AssertSql(
            """
SELECT ~n."IPNetwork"
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void BitwiseNot_PhysicalAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseNot(x.Macaddr))
            .ToArray();

        AssertSql(
            """
SELECT ~n."Macaddr"
FROM "NetTestEntities" AS n
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(10, 0)]
    public void BitwiseNot_PhysicalAddress_macaddr8()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseNot(x.Macaddr8))
            .ToArray();

        AssertSql(
            """
SELECT ~n."Macaddr8"
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void BitwiseAnd_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        var count = context.NetTestEntities.Count(x => x.Inet == EF.Functions.BitwiseAnd(x.Inet, inet));

        Assert.Equal(0, count);
        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT count(*)::int
FROM "NetTestEntities" AS n
WHERE n."Inet" = n."Inet" & @p
""");
    }

    [Fact]
    public void BitwiseAnd_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseAnd(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."IPNetwork" & @p
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void BitwiseAnd_PhysicalAddress()
    {
        using var context = CreateContext();
        var macaddr = new PhysicalAddress(new byte[6]);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseAnd(x.Macaddr, macaddr))
            .ToArray();

        AssertSql(
            """
@macaddr='000000000000' (DbType = Object)

SELECT n."Macaddr" & @macaddr
FROM "NetTestEntities" AS n
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(10, 0)]
    public void BitwiseAnd_PhysicalAddress_macaddr8()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseAnd(x.Macaddr8, x.Macaddr8))
            .ToArray();

        AssertSql(
            """
SELECT n."Macaddr8" & n."Macaddr8"
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void BitwiseOr_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseOr(x.Inet, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Inet" | @p
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void BitwiseOr_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseOr(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."IPNetwork" | @p
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void BitwiseOr_PhysicalAddress()
    {
        using var context = CreateContext();
        var macaddr = new PhysicalAddress(new byte[6]);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseOr(x.Macaddr, macaddr))
            .ToArray();

        AssertSql(
            """
@macaddr='000000000000' (DbType = Object)

SELECT n."Macaddr" | @macaddr
FROM "NetTestEntities" AS n
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(10, 0)]
    public void BitwiseOr_PhysicalAddress_macaddr8()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseOr(x.Macaddr8, x.Macaddr8))
            .ToArray();

        AssertSql(
            """
SELECT n."Macaddr8" | n."Macaddr8"
FROM "NetTestEntities" AS n
""");
    }

    #endregion

    #region ArithmeticOperatorTests

    [Fact]
    public void Add_IPAddress_and_int()
    {
        using var context = CreateContext();
        var actual = context.NetTestEntities.Single(x => EF.Functions.Add(x.Inet, 1) == IPAddress.Parse("192.168.1.2")).Inet;

        Assert.Equal(actual, IPAddress.Parse("192.168.1.1"));
        AssertSql(
            """
SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" + 1 = INET '192.168.1.2'
LIMIT 2
""");
    }

    [Fact]
    public void Add_IPNetwork_and_int()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Add(x.IPNetwork, 1))
            .ToArray();

        AssertSql(
            """
SELECT n."IPNetwork" + 1
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Subtract_IPAddress_and_int()
    {
        using var context = CreateContext();
        var actual = context.NetTestEntities.Single(x => EF.Functions.Subtract(x.Inet, 1) == IPAddress.Parse("192.168.1.1")).Inet;

        Assert.Equal(actual, IPAddress.Parse("192.168.1.2"));
        AssertSql(
            """
SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" - 1 = INET '192.168.1.1'
LIMIT 2
""");
    }

    [Fact]
    public void Subtract_IPNetwork_and_int()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Subtract(x.IPNetwork, 1))
            .ToArray();

        AssertSql(
            """
SELECT n."IPNetwork" - 1
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Subtract_IPAddress_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Subtract(x.Inet, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Inet" - @p
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Subtract_IPNetwork_and_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Subtract(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."IPNetwork" - @p
FROM "NetTestEntities" AS n
""");
    }

    #endregion

    #region FunctionTests

    [Fact]
    public void Abbreviate_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Abbreviate(x.Inet))
            .ToArray();

        AssertSql(
            """
SELECT abbrev(n."Inet")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Abbreviate_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Abbreviate(x.IPNetwork))
            .ToArray();

        AssertSql(
            """
SELECT abbrev(n."IPNetwork")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Broadcast_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Broadcast(x.Inet))
            .ToArray();

        AssertSql(
            """
SELECT broadcast(n."Inet")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Broadcast_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Broadcast(x.IPNetwork))
            .ToArray();

        AssertSql(
            """
SELECT broadcast(n."IPNetwork")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Family_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Family(x.Inet))
            .ToArray();

        AssertSql(
            """
SELECT family(n."Inet")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Family_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Family(x.IPNetwork))
            .ToArray();

        AssertSql(
            """
SELECT family(n."IPNetwork")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Host_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Host(x.Inet))
            .ToArray();

        AssertSql(
            """
SELECT host(n."Inet")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Host_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Host(x.IPNetwork))
            .ToArray();

        AssertSql(
            """
SELECT host(n."IPNetwork")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void HostMask_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.HostMask(x.Inet))
            .ToArray();

        AssertSql(
            """
SELECT hostmask(n."Inet")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void HostMask_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.HostMask(x.IPNetwork))
            .ToArray();

        AssertSql(
            """
SELECT hostmask(n."IPNetwork")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void MaskLength_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.MaskLength(x.Inet))
            .ToArray();

        AssertSql(
            """
SELECT masklen(n."Inet")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void MaskLength_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.MaskLength(x.IPNetwork))
            .ToArray();

        AssertSql(
            """
SELECT masklen(n."IPNetwork")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Netmask_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Netmask(x.Inet))
            .ToArray();

        AssertSql(
            """
SELECT netmask(n."Inet")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Netmask_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Netmask(x.IPNetwork))
            .ToArray();

        AssertSql(
            """
SELECT netmask(n."IPNetwork")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Network_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Network(x.Inet))
            .ToArray();

        AssertSql(
            """
SELECT network(n."Inet")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Network_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Network(x.IPNetwork))
            .ToArray();

        AssertSql(
            """
SELECT network(n."IPNetwork")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void SetMaskLength_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.SetMaskLength(x.Inet, default))
            .ToArray();

        AssertSql(
            """
SELECT set_masklen(n."Inet", 0)
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void SetMaskLength_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.SetMaskLength(x.IPNetwork, default))
            .ToArray();

        AssertSql(
            """
SELECT set_masklen(n."IPNetwork", 0)
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Text_IPAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Text(x.Inet))
            .ToArray();

        AssertSql(
            """
SELECT text(n."Inet")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Text_IPNetwork()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Text(x.IPNetwork))
            .ToArray();

        AssertSql(
            """
SELECT text(n."IPNetwork")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void SameFamily_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Select(x => EF.Functions.SameFamily(x.Inet, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT inet_same_family(n."Inet", @p)
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void SameFamily_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.SameFamily(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT inet_same_family(n."IPNetwork", @p)
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Merge_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Merge(x.Inet, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT inet_merge(n."Inet", @p)
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Merge_IPNetwork()
    {
        using var context = CreateContext();
        var cidr = new IPNetwork(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Merge(x.IPNetwork, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT inet_merge(n."IPNetwork", @p)
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Truncate_PhysicalAddress()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Truncate(x.Macaddr))
            .ToArray();

        AssertSql(
            """
SELECT trunc(n."Macaddr")
FROM "NetTestEntities" AS n
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(10, 0)]
    public void Truncate_PhysicalAddress_macaddr8()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Truncate(x.Macaddr8))
            .ToArray();

        AssertSql(
            """
SELECT trunc(n."Macaddr8")
FROM "NetTestEntities" AS n
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(10, 0)]
    public void Set7BitMac8_PhysicalAddress_macaddr8()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Set7BitMac8(x.Macaddr8))
            .ToArray();

        AssertSql(
            """
SELECT macaddr8_set7bit(n."Macaddr8")
FROM "NetTestEntities" AS n
""");
    }

    #endregion

    #region Obsolete (NpgsqlCidr)

#pragma warning disable CS0618 // NpgsqlCidr is obsolete, replaced by .NET IPNetwork

    [Fact]
    public void LessThan_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.LessThan(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" < @p
""");
    }

    [Fact]
    public void LessThanOrEqual_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.LessThanOrEqual(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" <= @p
""");
    }

    [Fact]
    public void GreaterThanOrEqual_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.GreaterThanOrEqual(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" >= @p
""");
    }

    [Fact]
    public void GreaterThan_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.GreaterThan(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" > @p
""");
    }

    [Fact]
    public void ContainedBy_IPAddress_and_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainedBy(x.Inet, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" << @p
""");
    }

    [Fact]
    public void ContainedBy_NpgsqlCidr_and_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainedBy(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" << @p
""");
    }

    [Fact]
    public void ContainedByOrEqual_IPAddress_and_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainedByOrEqual(x.Inet, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" <<= @p
""");
    }

    [Fact]
    public void ContainedByOrEqual_NpgsqlCidr_and_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainedByOrEqual(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" <<= @p
""");
    }

    [Fact]
    public void Contains_NpgsqlCidr_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.Contains(x.Cidr, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" >> @p
""");
    }

    [Fact]
    public void Contains_NpgsqlCidr_and_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.Contains(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" >> @p
""");
    }

    [Fact]
    public void ContainsOrEqual_NpgsqlCidr_and_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrEqual(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" >>= @p
""");
    }

    [Fact]
    public void ContainsOrEqual_NpgsqlCidr_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrEqual(x.Cidr, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" >>= @p
""");
    }

    [Fact]
    public void ContainsOrContainedBy_IPAddress_and_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrContainedBy(x.Inet, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Inet" && @p
""");
    }

    [Fact]
    public void ContainsOrContainedBy_NpgsqlCidr_and_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrContainedBy(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" && @p
""");
    }

    [Fact]
    public void ContainsOrContainedBy_NpgsqlCidr_and_IPAddress()
    {
        using var context = CreateContext();
        var inet = IPAddress.Any;
        _ = context.NetTestEntities
            .Where(x => EF.Functions.ContainsOrContainedBy(x.Cidr, inet))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0' (DbType = Object)

SELECT n."Id", n."Cidr", n."IPNetwork", n."Inet", n."Macaddr", n."Macaddr8", n."TextInet", n."TextMacaddr"
FROM "NetTestEntities" AS n
WHERE n."Cidr" && @p
""");
    }

    [Fact]
    public void BitwiseAnd_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseAnd(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Cidr" & @p
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void BitwiseOr_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseOr(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Cidr" | @p
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Subtract_NpgsqlCidr_and_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Subtract(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT n."Cidr" - @p
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void SameFamily_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.SameFamily(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT inet_same_family(n."Cidr", @p)
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Merge_NpgsqlCidr()
    {
        using var context = CreateContext();
        var cidr = new NpgsqlCidr(IPAddress.Any, default);
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Merge(x.Cidr, cidr))
            .ToArray();

        AssertSql(
            """
@p='0.0.0.0/0' (DbType = Object)

SELECT inet_merge(n."Cidr", @p)
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void BitwiseNot_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.BitwiseNot(x.Cidr))
            .ToArray();

        AssertSql(
            """
SELECT ~n."Cidr"
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Add_NpgsqlCidr_and_int()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Add(x.Cidr, 1))
            .ToArray();

        AssertSql(
            """
SELECT n."Cidr" + 1
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Subtract_NpgsqlCidr_and_int()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Subtract(x.Cidr, 1))
            .ToArray();

        AssertSql(
            """
SELECT n."Cidr" - 1
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Abbreviate_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Abbreviate(x.Cidr))
            .ToArray();

        AssertSql(
            """
SELECT abbrev(n."Cidr")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Broadcast_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Broadcast(x.Cidr))
            .ToArray();

        AssertSql(
            """
SELECT broadcast(n."Cidr")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Family_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Family(x.Cidr))
            .ToArray();

        AssertSql(
            """
SELECT family(n."Cidr")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Host_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Host(x.Cidr))
            .ToArray();

        AssertSql(
            """
SELECT host(n."Cidr")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void HostMask_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.HostMask(x.Cidr))
            .ToArray();

        AssertSql(
            """
SELECT hostmask(n."Cidr")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void MaskLength_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.MaskLength(x.Cidr))
            .ToArray();

        AssertSql(
            """
SELECT masklen(n."Cidr")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Netmask_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Netmask(x.Cidr))
            .ToArray();

        AssertSql(
            """
SELECT netmask(n."Cidr")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Network_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Network(x.Cidr))
            .ToArray();

        AssertSql(
            """
SELECT network(n."Cidr")
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void SetMaskLength_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.SetMaskLength(x.Cidr, default))
            .ToArray();

        AssertSql(
            """
SELECT set_masklen(n."Cidr", 0)
FROM "NetTestEntities" AS n
""");
    }

    [Fact]
    public void Text_NpgsqlCidr()
    {
        using var context = CreateContext();
        _ = context.NetTestEntities
            .Select(x => EF.Functions.Text(x.Cidr))
            .ToArray();

        AssertSql(
            """
SELECT text(n."Cidr")
FROM "NetTestEntities" AS n
""");
    }

#pragma warning restore CS0618

    #endregion Obsolete (NpgsqlCidr)

    #region Fixtures

    /// <summary>
    ///     Represents a fixture suitable for testing network address operators.
    /// </summary>
    public class NetworkAddressQueryNpgsqlFixture : SharedStoreFixtureBase<NetContext>
    {
        protected override string StoreName
            => "NetworkQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(NetContext context)
            => NetContext.SeedAsync(context);
    }

    /// <summary>
    ///     Represents an entity suitable for testing network address operators.
    /// </summary>
    public class NetTestEntity
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        ///     The primary key.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        ///     The network address.
        /// </summary>
        public IPAddress Inet { get; set; } = null!;

        /// <summary>
        ///     The network address.
        /// </summary>
        public IPNetwork IPNetwork { get; set; }

        /// <summary>
        ///     The network address.
        /// </summary>
        [Obsolete("NpgsqlCidr is obsolete, replaced by .NET IPNetwork")]
        public NpgsqlCidr Cidr { get; set; }

        /// <summary>
        ///     The MAC address.
        /// </summary>
        public PhysicalAddress Macaddr { get; set; } = null!;

        /// <summary>
        ///     The MAC address.
        /// </summary>
        [Column(TypeName = "macaddr8")]
        public PhysicalAddress Macaddr8 { get; set; } = null!;

        /// <summary>
        ///     The text form of <see cref="Inet" />.
        /// </summary>
        public string TextInet { get; set; } = null!;

        /// <summary>
        ///     The text form of <see cref="Macaddr" />.
        /// </summary>
        public string TextMacaddr { get; set; } = null!;
    }

    /// <summary>
    ///     Represents a database suitable for testing network address operators.
    /// </summary>
    public class NetContext : PoolableDbContext
    {
        /// <summary>
        ///     Represents a set of entities with <see cref="IPAddress" /> properties.
        /// </summary>
        public DbSet<NetTestEntity> NetTestEntities { get; set; }

        /// <summary>
        ///     Initializes a <see cref="NetContext" />.
        /// </summary>
        /// <param name="options">
        ///     The options to be used for configuration.
        /// </param>
        public NetContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (TestEnvironment.PostgresVersion < new Version(10, 0))
            {
                modelBuilder.Entity<NetTestEntity>().Ignore(x => x.Macaddr8);
            }

            base.OnModelCreating(modelBuilder);
        }

        public static async Task SeedAsync(NetContext context)
        {
            for (var i = 1; i <= 9; i++)
            {
                var ip = IPAddress.Parse("192.168.1." + i);
                var macaddr = PhysicalAddress.Parse("12-34-56-00-00-0" + i);
                var macaddr8 = PhysicalAddress.Parse("08-00-2B-01-02-03-04-0" + i);
                context.NetTestEntities.Add(
                    new NetTestEntity
                    {
                        Id = i,
                        Inet = ip,
#pragma warning disable CS0618 // NpgsqlCidr is obsolete, replaced by .NET IPNetwork
                        Cidr = new NpgsqlCidr(IPAddress.Parse("192.168.1.0"), 24),
#pragma warning restore CS0618
                        IPNetwork = new IPNetwork(IPAddress.Parse("192.168.1.0"), 24),
                        Macaddr = macaddr,
                        Macaddr8 = macaddr8,
                        TextInet = ip.ToString(),
                        TextMacaddr = macaddr.ToString()
                    });
            }

            await context.SaveChangesAsync();
        }
    }

    #endregion

    #region Helpers

    protected NetContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    #endregion
}
