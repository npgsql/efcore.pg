#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for PostgreSQL network address (cidr, inet, macaddr, macaddr8) operators and functions.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-net.html
    /// </remarks>
    public class NpgsqlNetworkAddressTranslator : IMethodCallTranslator
    {
        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType != typeof(NpgsqlNetworkAddressExtensions))
                return null;

            switch (expression.Method.Name)
            {
            case nameof(NpgsqlNetworkAddressExtensions.LessThan):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.LessThanOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<=", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.Equal):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "=", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.GreaterThanOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">=", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.GreaterThan):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.NotEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<>", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.ContainedBy):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<<", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.ContainedByOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<<=", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.Contains):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">>", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.ContainsOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">>=", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.ContainsOrContainedBy):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "&&", typeof(bool));

            case nameof(NpgsqlNetworkAddressExtensions.Not):
                return new CustomUnaryExpression(expression.Arguments[1], "~", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkAddressExtensions.And):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "&", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkAddressExtensions.Or):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "|", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkAddressExtensions.Add):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "+", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkAddressExtensions.Subtract):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "-", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkAddressExtensions.Abbreviate):
                return new PgFunctionExpression("abbrev", typeof(string), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkAddressExtensions.Broadcast):
                return new PgFunctionExpression("broadcast", typeof(IPAddress), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkAddressExtensions.Family):
                return new PgFunctionExpression("family", typeof(int), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkAddressExtensions.Host):
                return new PgFunctionExpression("host", typeof(string), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkAddressExtensions.HostMask):
                return new PgFunctionExpression("hostmask", typeof(IPAddress), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkAddressExtensions.SubnetLength):
                return new PgFunctionExpression("masklen", typeof(int), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkAddressExtensions.SubnetMask):
                return new PgFunctionExpression("netmask", typeof(IPAddress), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkAddressExtensions.Network):
                return new PgFunctionExpression("network", typeof((IPAddress Address, int Subnet)), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkAddressExtensions.SetSubnetLength):
                return new PgFunctionExpression("set_masklen", expression.Arguments[1].Type, new[] { expression.Arguments[1], expression.Arguments[2] });

            case nameof(NpgsqlNetworkAddressExtensions.Text):
                return new PgFunctionExpression("text", typeof(string), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkAddressExtensions.SameFamily):
                return new PgFunctionExpression("inet_same_family", typeof(bool), new[] { expression.Arguments[1], expression.Arguments[2] });

            case nameof(NpgsqlNetworkAddressExtensions.Merge):
                return new PgFunctionExpression("inet_merge", typeof((IPAddress Address, int Subnet)), new[] { expression.Arguments[1], expression.Arguments[2] });

            case nameof(NpgsqlNetworkAddressExtensions.Truncate):
                return new PgFunctionExpression("trunc", typeof(PhysicalAddress), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkAddressExtensions.Set7BitMac8):
                return new PgFunctionExpression("macaddr8_set7bit", typeof(PhysicalAddress), new[] { expression.Arguments[1] });

            default:
                return null;
            }
        }
    }
}
