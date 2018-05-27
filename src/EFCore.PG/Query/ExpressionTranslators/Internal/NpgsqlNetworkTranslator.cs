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
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for operators and functions of PostgreSQL network typess (cidr, inet, macaddr, macaddr8).
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-net.html
    /// </remarks>
    public class NpgsqlNetworkTranslator : IMethodCallTranslator
    {
        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType != typeof(NpgsqlNetworkExtensions))
                return null;

            switch (expression.Method.Name)
            {
            case nameof(NpgsqlNetworkExtensions.LessThan):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.LessThanOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<=", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.Equal):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "=", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.GreaterThanOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">=", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.GreaterThan):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.NotEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<>", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.ContainedBy):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<<", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.ContainedByOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<<=", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.Contains):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">>", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.ContainsOrEqual):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], ">>=", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.ContainsOrContainedBy):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "&&", typeof(bool));

            case nameof(NpgsqlNetworkExtensions.BitwiseNot):
                return new CustomUnaryExpression(expression.Arguments[1], "~", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkExtensions.BitwiseAnd):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "&", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkExtensions.BitwiseOr):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "|", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkExtensions.Add):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "+", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkExtensions.Subtract):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "-", expression.Arguments[1].Type);

            case nameof(NpgsqlNetworkExtensions.Abbreviate):
                return new SqlFunctionExpression("abbrev", typeof(string), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkExtensions.Broadcast):
                return new SqlFunctionExpression("broadcast", typeof(IPAddress), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkExtensions.Family):
                return new SqlFunctionExpression("family", typeof(int), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkExtensions.Host):
                return new SqlFunctionExpression("host", typeof(string), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkExtensions.HostMask):
                return new SqlFunctionExpression("hostmask", typeof(IPAddress), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkExtensions.MaskLength):
                return new SqlFunctionExpression("masklen", typeof(int), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkExtensions.Netmask):
                return new SqlFunctionExpression("netmask", typeof(IPAddress), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkExtensions.Network):
                return new SqlFunctionExpression("network", typeof((IPAddress Address, int Subnet)), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkExtensions.SetMaskLength):
                return new SqlFunctionExpression("set_masklen", expression.Arguments[1].Type, new[] { expression.Arguments[1], expression.Arguments[2] });

            case nameof(NpgsqlNetworkExtensions.Text):
                return new SqlFunctionExpression("text", typeof(string), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkExtensions.SameFamily):
                return new SqlFunctionExpression("inet_same_family", typeof(bool), new[] { expression.Arguments[1], expression.Arguments[2] });

            case nameof(NpgsqlNetworkExtensions.Merge):
                return new SqlFunctionExpression("inet_merge", typeof((IPAddress Address, int Subnet)), new[] { expression.Arguments[1], expression.Arguments[2] });

            case nameof(NpgsqlNetworkExtensions.Truncate):
                return new SqlFunctionExpression("trunc", typeof(PhysicalAddress), new[] { expression.Arguments[1] });

            case nameof(NpgsqlNetworkExtensions.Set7BitMac8):
                return new SqlFunctionExpression("macaddr8_set7bit", typeof(PhysicalAddress), new[] { expression.Arguments[1] });

            default:
                return null;
            }
        }
    }
}
