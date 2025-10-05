﻿using System.Net;
using System.Net.NetworkInformation;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides extension methods supporting operator translation for PostgreSQL network types.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/static/functions-net.html
/// </remarks>
public static class NpgsqlNetworkDbFunctionsExtensions
{
    #region RelationalOperators

    /// <summary>
    ///     Determines whether an <see cref="NpgsqlInet" /> is less than another <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The left-hand inet.</param>
    /// <param name="other">The right-hand inet.</param>
    /// <returns>
    ///     True if the <see cref="NpgsqlInet" /> is less than the other <see cref="NpgsqlInet" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool LessThan(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LessThan)));

    /// <summary>
    ///     Determines whether an <see cref="PhysicalAddress" /> is less than another <see cref="PhysicalAddress" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="macaddr">The left-hand macaddr.</param>
    /// <param name="other">The right-hand macaddr.</param>
    /// <returns>
    ///     True if the <see cref="PhysicalAddress" /> is less than the other <see cref="PhysicalAddress" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool LessThan(this DbFunctions _, PhysicalAddress macaddr, PhysicalAddress other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LessThan)));

    /// <summary>
    ///     Determines whether an <see cref="NpgsqlInet" /> is less than or equal to another <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The left-hand inet.</param>
    /// <param name="other">The right-hand inet.</param>
    /// <returns>
    ///     True if the <see cref="NpgsqlInet" /> is less than or equal to the other <see cref="NpgsqlInet" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool LessThanOrEqual(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LessThanOrEqual)));

    /// <summary>
    ///     Determines whether an <see cref="PhysicalAddress" /> is less than or equal to another <see cref="PhysicalAddress" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="macaddr">The left-hand macaddr.</param>
    /// <param name="other">The right-hand macaddr.</param>
    /// <returns>
    ///     True if the <see cref="PhysicalAddress" /> is less than or equal to the other <see cref="PhysicalAddress" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool LessThanOrEqual(this DbFunctions _, PhysicalAddress macaddr, PhysicalAddress other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LessThanOrEqual)));

    /// <summary>
    ///     Determines whether an <see cref="NpgsqlInet" /> is greater than or equal to another <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The left-hand inet.</param>
    /// <param name="other">The right-hand inet.</param>
    /// <returns>
    ///     True if the <see cref="NpgsqlInet" /> is greater than or equal to the other <see cref="NpgsqlInet" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool GreaterThanOrEqual(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GreaterThanOrEqual)));

    /// <summary>
    ///     Determines whether an <see cref="PhysicalAddress" /> is greater than or equal to another <see cref="PhysicalAddress" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="macaddr">The left-hand macaddr.</param>
    /// <param name="other">The right-hand macaddr.</param>
    /// <returns>
    ///     True if the <see cref="PhysicalAddress" /> is greater than or equal to the other <see cref="PhysicalAddress" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool GreaterThanOrEqual(this DbFunctions _, PhysicalAddress macaddr, PhysicalAddress other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GreaterThanOrEqual)));

    /// <summary>
    ///     Determines whether an <see cref="NpgsqlInet" /> is greater than another <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The left-hand inet.</param>
    /// <param name="other">The right-hand inet.</param>
    /// <returns>
    ///     True if the <see cref="NpgsqlInet" /> is greater than the other <see cref="NpgsqlInet" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool GreaterThan(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GreaterThan)));

    /// <summary>
    ///     Determines whether an <see cref="PhysicalAddress" /> is greater than another <see cref="PhysicalAddress" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="macaddr">The left-hand macaddr.</param>
    /// <param name="other">The right-hand macaddr.</param>
    /// <returns>
    ///     True if the <see cref="PhysicalAddress" /> is greater than the other <see cref="PhysicalAddress" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool GreaterThan(this DbFunctions _, PhysicalAddress macaddr, PhysicalAddress other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GreaterThan)));

    #endregion

    #region ContainmentOperators

    /// <summary>
    ///     Determines whether an <see cref="NpgsqlInet" /> is contained within another <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet to locate.</param>
    /// <param name="other">The inet to search.</param>
    /// <returns>
    ///     True if the <see cref="NpgsqlInet" /> is contained within the other <see cref="NpgsqlInet" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool ContainedBy(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainedBy)));

    /// <summary>
    ///     Determines whether an <see cref="NpgsqlInet" /> is contained within or equal to another <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet to locate.</param>
    /// <param name="other">The inet to search.</param>
    /// <returns>
    ///     True if the <see cref="NpgsqlInet" /> is contained within or equal to the other <see cref="NpgsqlInet" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool ContainedByOrEqual(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainedByOrEqual)));

    /// <summary>
    ///     Determines whether an <see cref="NpgsqlInet" /> contains another <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The IP address to search.</param>
    /// <param name="other">The IP address to locate.</param>
    /// <returns>
    ///     True if the <see cref="NpgsqlInet" /> contains the other <see cref="NpgsqlInet" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool Contains(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Contains)));

    /// <summary>
    ///     Determines whether an <see cref="NpgsqlInet" /> contains or is equal to another <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The IP address to search.</param>
    /// <param name="other">The IP address to locate.</param>
    /// <returns>
    ///     True if the <see cref="NpgsqlInet" /> contains or is equal to the other <see cref="NpgsqlInet" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool ContainsOrEqual(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainsOrEqual)));

    /// <summary>
    ///     Determines whether an <see cref="NpgsqlInet" /> contains or is contained by another <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The IP address to search.</param>
    /// <param name="other">The IP address to locate.</param>
    /// <returns>
    ///     True if the <see cref="NpgsqlInet" /> contains or is contained by the other <see cref="NpgsqlInet" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool ContainsOrContainedBy(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(ContainsOrContainedBy)));

    #endregion

    #region BitwiseOperators

    /// <summary>
    ///     Computes the bitwise NOT operation on an <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet to negate.</param>
    /// <returns>
    ///     The result of the bitwise NOT operation.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlInet BitwiseNot(this DbFunctions _, NpgsqlInet inet)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(BitwiseNot)));

    /// <summary>
    ///     Computes the bitwise NOT operation on an <see cref="PhysicalAddress" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="macaddr">The macaddr to negate.</param>
    /// <returns>
    ///     The result of the bitwise NOT operation.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static PhysicalAddress BitwiseNot(this DbFunctions _, PhysicalAddress macaddr)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(BitwiseNot)));

    /// <summary>
    ///     Computes the bitwise AND of two <see cref="NpgsqlInet" /> instances.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The left-hand inet.</param>
    /// <param name="other">The right-hand inet.</param>
    /// <returns>
    ///     The result of the bitwise AND operation.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlInet BitwiseAnd(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(BitwiseAnd)));

    /// <summary>
    ///     Computes the bitwise AND of two <see cref="PhysicalAddress" /> instances.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="macaddr">The left-hand macaddr.</param>
    /// <param name="other">The right-hand macaddr.</param>
    /// <returns>
    ///     The result of the bitwise AND operation.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static PhysicalAddress BitwiseAnd(this DbFunctions _, PhysicalAddress macaddr, PhysicalAddress other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(BitwiseAnd)));

    /// <summary>
    ///     Computes the bitwise OR of two <see cref="NpgsqlInet" /> instances.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The left-hand inet.</param>
    /// <param name="other">The right-hand inet.</param>
    /// <returns>
    ///     The result of the bitwise OR operation.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlInet BitwiseOr(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(BitwiseOr)));

    /// <summary>
    ///     Computes the bitwise OR of two <see cref="PhysicalAddress" /> instances.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="macaddr">The left-hand macaddr.</param>
    /// <param name="other">The right-hand macaddr.</param>
    /// <returns>
    ///     The result of the bitwise OR operation.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static PhysicalAddress BitwiseOr(this DbFunctions _, PhysicalAddress macaddr, PhysicalAddress other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(BitwiseOr)));

    #endregion

    #region ArithmeticOperators

    /// <summary>
    ///     Adds the <paramref name="value" /> to the <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>
    ///     The <see cref="NpgsqlInet" /> augmented by the <paramref name="value" />.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlInet Add(this DbFunctions _, NpgsqlInet inet, int value)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Add)));

    /// <summary>
    ///     Subtracts the <paramref name="value" /> from the <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet.</param>
    /// <param name="value">The value to subtract.</param>
    /// <returns>
    ///     The <see cref="NpgsqlInet" /> augmented by the <paramref name="value" />.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlInet Subtract(this DbFunctions _, NpgsqlInet inet, long value)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Subtract)));

    /// <summary>
    ///     Subtracts one <see cref="NpgsqlInet" /> from another <see cref="NpgsqlInet" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet from which to subtract.</param>
    /// <param name="other">The inet to subtract.</param>
    /// <returns>
    ///     The numeric difference between the two given addresses.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static int Subtract(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Subtract)));

    #endregion

    #region Functions

    /// <summary>
    ///     Returns the abbreviated display format as text.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet to abbreviate.</param>
    /// <returns>
    ///     The abbreviated display format as text.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static string Abbreviate(this DbFunctions _, NpgsqlInet inet)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Abbreviate)));

    /// <summary>
    ///     Returns the abbreviated display format as text.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to abbreviate.</param>
    /// <returns>
    ///     The abbreviated display format as text.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static string Abbreviate(this DbFunctions _, IPNetwork cidr)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Abbreviate)));

#pragma warning disable CS0618 // NpgsqlCidr is obsolete, replaced by .NET IPNetwork
    /// <summary>
    ///     Returns the abbreviated display format as text.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to abbreviate.</param>
    /// <returns>
    ///     The abbreviated display format as text.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static string Abbreviate(this DbFunctions _, NpgsqlCidr cidr)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Abbreviate)));
#pragma warning restore CS0618

    /// <summary>
    ///     Returns the broadcast address for a network.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet used to derive the broadcast address.</param>
    /// <returns>
    ///     The broadcast address for a network.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlInet Broadcast(this DbFunctions _, NpgsqlInet inet)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Broadcast)));

    /// <summary>
    ///     Extracts the family of an address; 4 for IPv4, 6 for IPv6.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet used to derive the family.</param>
    /// <returns>
    ///     The family of an address; 4 for IPv4, 6 for IPv6.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static int Family(this DbFunctions _, NpgsqlInet inet)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Family)));

    /// <summary>
    ///     Extracts the host (i.e. the IP address) as text.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet from which to extract the host.</param>
    /// <returns>
    ///     The host (i.e. the IP address) as text.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static string Host(this DbFunctions _, NpgsqlInet inet)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Host)));

    /// <summary>
    ///     Constructs the host mask for the network.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet used to construct the host mask.</param>
    /// <returns>
    ///     The constructed host mask.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlInet HostMask(this DbFunctions _, NpgsqlInet inet)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(HostMask)));

    /// <summary>
    ///     Extracts the length of the subnet mask.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet used to extract the subnet length.</param>
    /// <returns>
    ///     The length of the subnet mask.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static int MaskLength(this DbFunctions _, NpgsqlInet inet)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(MaskLength)));

    /// <summary>
    ///     Constructs the subnet mask for the network.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet used to construct the subnet mask.</param>
    /// <returns>
    ///     The subnet mask for the network.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlInet Netmask(this DbFunctions _, NpgsqlInet inet)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Netmask)));

    /// <summary>
    ///     Extracts the network part of the address.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet used to extract the network.</param>
    /// <returns>
    ///     The network part of the address.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static IPNetwork Network(this DbFunctions _, NpgsqlInet inet)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Network)));

    /// <summary>
    ///     Sets the length of the subnet mask.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet to modify.</param>
    /// <param name="length">The subnet mask length to set.</param>
    /// <returns>
    ///     The network with a subnet mask of the specified length.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlInet SetMaskLength(this DbFunctions _, NpgsqlInet inet, int length)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(SetMaskLength)));

    /// <summary>
    ///     Sets the length of the subnet mask.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to modify.</param>
    /// <param name="length">The subnet mask length to set.</param>
    /// <returns>
    ///     The network with a subnet mask of the specified length.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static IPNetwork SetMaskLength(this DbFunctions _, IPNetwork cidr, int length)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(SetMaskLength)));

#pragma warning disable CS0618 // NpgsqlCidr is obsolete, replaced by .NET IPNetwork
    /// <summary>
    ///     Sets the length of the subnet mask.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to modify.</param>
    /// <param name="length">The subnet mask length to set.</param>
    /// <returns>
    ///     The network with a subnet mask of the specified length.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static NpgsqlCidr SetMaskLength(this DbFunctions _, NpgsqlCidr cidr, int length)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(SetMaskLength)));
#pragma warning restore CS0618

    /// <summary>
    ///     Extracts the IP address and subnet mask as text.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet to extract as text.</param>
    /// <returns>
    ///     The IP address and subnet mask as text.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static string Text(this DbFunctions _, NpgsqlInet inet)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Text)));

    /// <summary>
    ///     Tests if the addresses are in the same family.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The primary inet.</param>
    /// <param name="other">The other inet.</param>
    /// <returns>
    ///     True if the addresses are in the same family; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static bool SameFamily(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(SameFamily)));

    /// <summary>
    ///     Constructs the smallest network which includes both of the given networks.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The first inet.</param>
    /// <param name="other">The second inet.</param>
    /// <returns>
    ///     The smallest network which includes both of the given networks.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static IPNetwork Merge(this DbFunctions _, NpgsqlInet inet, NpgsqlInet other)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Merge)));

    /// <summary>
    ///     Sets the last 3 bytes of the MAC address to zero. For macaddr8, the last 5 bytes are set to zero.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="macAddress">The MAC address to truncate.</param>
    /// <returns>
    ///     The MAC address with the last 3 bytes set to zero. For macaddr8, the last 5 bytes are set to zero.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static PhysicalAddress Truncate(this DbFunctions _, PhysicalAddress macAddress)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Truncate)));

    /// <summary>
    ///     Sets the 7th bit to one, also known as modified EUI-64, for inclusion in an IPv6 address.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="macAddress">The MAC address to modify.</param>
    /// <returns>
    ///     The MAC address with the 7th bit set to one.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    public static PhysicalAddress Set7BitMac8(this DbFunctions _, PhysicalAddress macAddress)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Set7BitMac8)));

    #endregion

    #region Obsolete

    /// <summary>
    ///     Determines whether an (IPAddress Address, int Subnet) is less than another (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The left-hand cidr.</param>
    /// <param name="other">The right-hand cidr.</param>
    /// <returns>
    ///     True if the (IPAddress Address, int Subnet) is less than the other (IPAddress Address, int Subnet); otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool LessThan(this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an (IPAddress Address, int Subnet) is less than or equal to another (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The left-hand cidr.</param>
    /// <param name="other">The right-hand cidr.</param>
    /// <returns>
    ///     True if the (IPAddress Address, int Subnet) is less than or equal to the other (IPAddress Address, int Subnet); otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool LessThanOrEqual(this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an (IPAddress Address, int Subnet) is greater than or equal to another (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The left-hand cidr.</param>
    /// <param name="other">The right-hand cidr.</param>
    /// <returns>
    ///     True if the (IPAddress Address, int Subnet) is greater than or equal to the other (IPAddress Address, int Subnet); otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool GreaterThanOrEqual(this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an (IPAddress Address, int Subnet) is greater than another (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The left-hand cidr.</param>
    /// <param name="other">The right-hand cidr.</param>
    /// <returns>
    ///     True if the (IPAddress Address, int Subnet) is greater than the other (IPAddress Address, int Subnet); otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool GreaterThan(this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an <see cref="IPAddress" /> is contained within a network.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet to locate.</param>
    /// <param name="other">The cidr to search.</param>
    /// <returns>
    ///     True if the <see cref="IPAddress" /> is contained within the network; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool ContainedBy(this DbFunctions _, IPAddress inet, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether a network contains or is equal to another <see cref="IPAddress" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The network to search.</param>
    /// <param name="other">The IP address to locate.</param>
    /// <returns>
    ///     True if the network contains or is equal to the other <see cref="IPAddress" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool ContainsOrEqual(this DbFunctions _, (IPAddress Address, int Subnet) cidr, IPAddress other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an (IPAddress Address, int Subnet) is contained within another (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to locate.</param>
    /// <param name="other">The cidr to search.</param>
    /// <returns>
    ///     True if the (IPAddress Address, int Subnet) is contained within the other (IPAddress Address, int Subnet); otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool ContainedBy(this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an <see cref="IPAddress" /> is contained within or equal to a network.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The inet to locate.</param>
    /// <param name="other">The cidr to search.</param>
    /// <returns>
    ///     True if the <see cref="IPAddress" /> is contained within or equal to the network; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool ContainedByOrEqual(this DbFunctions _, IPAddress inet, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an (IPAddress Address, int Subnet) is contained within or equal to another (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to locate.</param>
    /// <param name="other">The cidr to search.</param>
    /// <returns>
    ///     True if the (IPAddress Address, int Subnet) is contained within or equal to the other (IPAddress Address, int Subnet); otherwise,
    ///     false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool ContainedByOrEqual(this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether a network contains another <see cref="IPAddress" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The network to search.</param>
    /// <param name="other">The IP address to locate.</param>
    /// <returns>
    ///     True if the network contains the other <see cref="IPAddress" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool Contains(
        this DbFunctions _,
        (IPAddress Address, int Subnet) cidr,
        IPAddress other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an (IPAddress Address, int Subnet) contains another (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to search.</param>
    /// <param name="other">The cidr to locate.</param>
    /// <returns>
    ///     True if the (IPAddress Address, int Subnet) contains the other (IPAddress Address, int Subnet); otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool Contains(this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an (IPAddress Address, int Subnet) contains or is equal to another (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to search.</param>
    /// <param name="other">The cidr to locate.</param>
    /// <returns>
    ///     True if the (IPAddress Address, int Subnet) contains or is equal to the other (IPAddress Address, int Subnet); otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool ContainsOrEqual(this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether a network contains or is contained by an <see cref="IPAddress" />.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The network to search.</param>
    /// <param name="other">The IP address to locate.</param>
    /// <returns>
    ///     True if the network contains or is contained by the <see cref="IPAddress" />; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool ContainsOrContainedBy(this DbFunctions _, (IPAddress Address, int Subnet) cidr, IPAddress other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an <see cref="IPAddress" /> contains or is contained by a network.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="inet">The IP address to search.</param>
    /// <param name="other">The network to locate.</param>
    /// <returns>
    ///     True if the <see cref="IPAddress" /> contains or is contained by the network; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool ContainsOrContainedBy(this DbFunctions _, IPAddress inet, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Determines whether an (IPAddress Address, int Subnet) contains or is contained by another (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to search.</param>
    /// <param name="other">The cidr to locate.</param>
    /// <returns>
    ///     True if the (IPAddress Address, int Subnet) contains or is contained by the other (IPAddress Address, int Subnet); otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool ContainsOrContainedBy(
        this DbFunctions _,
        (IPAddress Address, int Subnet) cidr,
        (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Computes the bitwise NOT operation on an (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to negate.</param>
    /// <returns>
    ///     The result of the bitwise NOT operation.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static (IPAddress Address, int Subnet) BitwiseNot(this DbFunctions _, (IPAddress Address, int Subnet) cidr)
        => throw new NotSupportedException();

    /// <summary>
    ///     Computes the bitwise AND of two (IPAddress Address, int Subnet) instances.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The left-hand cidr.</param>
    /// <param name="other">The right-hand cidr.</param>
    /// <returns>
    ///     The result of the bitwise AND operation.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static (IPAddress Address, int Subnet) BitwiseAnd(
        this DbFunctions _,
        (IPAddress Address, int Subnet) cidr,
        (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Computes the bitwise OR of two (IPAddress Address, int Subnet) instances.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The left-hand cidr.</param>
    /// <param name="other">The right-hand cidr.</param>
    /// <returns>
    ///     The result of the bitwise OR operation.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static (IPAddress Address, int Subnet) BitwiseOr(
        this DbFunctions _,
        (IPAddress Address, int Subnet) cidr,
        (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Adds the <paramref name="value" /> to the (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>
    ///     The (IPAddress Address, int Subnet) augmented by the <paramref name="value" />.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static (IPAddress Address, int Subnet) Add(this DbFunctions _, (IPAddress Address, int Subnet) cidr, int value)
        => throw new NotSupportedException();

    /// <summary>
    ///     Subtracts the <paramref name="value" /> from the (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The inet.</param>
    /// <param name="value">The value to subtract.</param>
    /// <returns>
    ///     The (IPAddress Address, int Subnet) augmented by the <paramref name="value" />.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static (IPAddress Address, int Subnet) Subtract(this DbFunctions _, (IPAddress Address, int Subnet) cidr, int value)
        => throw new NotSupportedException();

    /// <summary>
    ///     Subtracts one (IPAddress Address, int Subnet) from another (IPAddress Address, int Subnet).
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr from which to subtract.</param>
    /// <param name="other">The cidr to subtract.</param>
    /// <returns>
    ///     The difference between the two addresses.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static int Subtract(
        this DbFunctions _,
        (IPAddress Address, int Subnet) cidr,
        (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Returns the abbreviated display format as text.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to abbreviate.</param>
    /// <returns>
    ///     The abbreviated display format as text.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static string Abbreviate(this DbFunctions _, (IPAddress Address, int Subnet) cidr)
        => throw new NotSupportedException();

    /// <summary>
    ///     Returns the broadcast address for a network.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr used to derive the broadcast address.</param>
    /// <returns>
    ///     The broadcast address for a network.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static IPAddress Broadcast(this DbFunctions _, (IPAddress Address, int Subnet) cidr)
        => throw new NotSupportedException();

    /// <summary>
    ///     Extracts the family of an address; 4 for IPv4, 6 for IPv6.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr used to derive the family.</param>
    /// <returns>
    ///     The family of an address; 4 for IPv4, 6 for IPv6.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static int Family(this DbFunctions _, (IPAddress Address, int Subnet) cidr)
        => throw new NotSupportedException();

    /// <summary>
    ///     Extracts the host (i.e. the IP address) as text.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr from which to extract the host.</param>
    /// <returns>
    ///     The host (i.e. the IP address) as text.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static string Host(this DbFunctions _, (IPAddress Address, int Subnet) cidr)
        => throw new NotSupportedException();

    /// <summary>
    ///     Constructs the host mask for the network.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr used to construct the host mask.</param>
    /// <returns>
    ///     The constructed host mask.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static IPAddress HostMask(this DbFunctions _, (IPAddress Address, int Subnet) cidr)
        => throw new NotSupportedException();

    /// <summary>
    ///     Extracts the length of the subnet mask.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr used to extract the subnet length.</param>
    /// <returns>
    ///     The length of the subnet mask.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static int MaskLength(this DbFunctions _, (IPAddress Address, int Subnet) cidr)
        => throw new NotSupportedException();

    /// <summary>
    ///     Constructs the subnet mask for the network.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr used to construct the subnet mask.</param>
    /// <returns>
    ///     The subnet mask for the network.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static IPAddress Netmask(this DbFunctions _, (IPAddress Address, int Subnet) cidr)
        => throw new NotSupportedException();

    /// <summary>
    ///     Extracts the network part of the address.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr used to extract the network.</param>
    /// <returns>
    ///     The network part of the address.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static (IPAddress Address, int Subnet) Network(this DbFunctions _, (IPAddress Address, int Subnet) cidr)
        => throw new NotSupportedException();

    /// <summary>
    ///     Sets the length of the subnet mask.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to modify.</param>
    /// <param name="length">The subnet mask length to set.</param>
    /// <returns>
    ///     The network with a subnet mask of the specified length.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static (IPAddress Address, int Subnet) SetMaskLength(this DbFunctions _, (IPAddress Address, int Subnet) cidr, int length)
        => throw new NotSupportedException();

    /// <summary>
    ///     Extracts the IP address and subnet mask as text.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The cidr to extract as text.</param>
    /// <returns>
    ///     The IP address and subnet mask as text.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static string Text(this DbFunctions _, (IPAddress Address, int Subnet) cidr)
        => throw new NotSupportedException();

    /// <summary>
    ///     Tests if the addresses are in the same family.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The primary cidr.</param>
    /// <param name="other">The other cidr.</param>
    /// <returns>
    ///     True if the addresses are in the same family; otherwise, false.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static bool SameFamily(this DbFunctions _, (IPAddress Address, int Subnet) cidr, (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    /// <summary>
    ///     Constructs the smallest network which includes both of the given networks.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="cidr">The first cidr.</param>
    /// <param name="other">The second cidr.</param>
    /// <returns>
    ///     The smallest network which includes both of the given networks.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     This method is only intended for use via SQL translation as part of an EF Core LINQ query.
    /// </exception>
    [Obsolete("Use the overload which accepts NpgsqlCidr", error: true)]
    public static (IPAddress Address, int Subnet) Merge(
        this DbFunctions _,
        (IPAddress Address, int Subnet) cidr,
        (IPAddress Address, int Subnet) other)
        => throw new NotSupportedException();

    #endregion Obsolete
}
