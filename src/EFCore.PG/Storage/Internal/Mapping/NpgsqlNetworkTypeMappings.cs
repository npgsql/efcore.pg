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

using System.Net;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The type mapping for the PostgreSQL macaddr type.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/datatype-net-types.html#DATATYPE-MACADDR
    /// </remarks>
    public class NpgsqlMacaddrTypeMapping : NpgsqlTypeMapping
    {
        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlMacaddrTypeMapping"/> class.
        /// </summary>
        public NpgsqlMacaddrTypeMapping() : base("macaddr", typeof(PhysicalAddress), NpgsqlDbType.MacAddr) {}

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlMacaddrTypeMapping"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        protected NpgsqlMacaddrTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.MacAddr) {}

        /// <inheritdoc />
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlMacaddrTypeMapping(parameters);

        /// <inheritdoc />
        protected override string GenerateNonNullSqlLiteral(object value)
            => $"MACADDR '{(PhysicalAddress)value}'";
    }

    /// <summary>
    /// The type mapping for the PostgreSQL macaddr8 type.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/datatype-net-types.html#DATATYPE-MACADDR8
    /// </remarks>
    public class NpgsqlMacaddr8TypeMapping : NpgsqlTypeMapping
    {
        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlMacaddr8TypeMapping"/> class.
        /// </summary>
        public NpgsqlMacaddr8TypeMapping() : base("macaddr8", typeof(PhysicalAddress), NpgsqlDbType.MacAddr8) {}

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlMacaddr8TypeMapping"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        protected NpgsqlMacaddr8TypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.MacAddr8) {}

        /// <inheritdoc />
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlMacaddr8TypeMapping(parameters);

        /// <inheritdoc />
        protected override string GenerateNonNullSqlLiteral(object value)
            => $"MACADDR8 '{(PhysicalAddress)value}'";
    }

    /// <summary>
    /// The type mapping for the PostgreSQL inet type.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/datatype-net-types.html#DATATYPE-INET
    /// </remarks>
    public class NpgsqlInetTypeMapping : NpgsqlTypeMapping
    {
        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlInetTypeMapping"/> class.
        /// </summary>
        public NpgsqlInetTypeMapping() : base("inet", typeof(IPAddress), NpgsqlDbType.Inet) {}

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlInetTypeMapping"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        protected NpgsqlInetTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Inet) {}

        /// <inheritdoc />
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlInetTypeMapping(parameters);

        /// <inheritdoc />
        protected override string GenerateNonNullSqlLiteral(object value)
            => $"INET '{(IPAddress)value}'";
    }

    /// <summary>
    /// The type mapping for the PostgreSQL cidr type.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/datatype-net-types.html#DATATYPE-CIDR
    /// </remarks>
    public class NpgsqlCidrTypeMapping : NpgsqlTypeMapping
    {
        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlCidrTypeMapping"/> class.
        /// </summary>
        public NpgsqlCidrTypeMapping() : base("cidr", typeof((IPAddress, int)), NpgsqlDbType.Cidr) {}

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlCidrTypeMapping"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        protected NpgsqlCidrTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Cidr) {}

        /// <inheritdoc />
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlCidrTypeMapping(parameters);

        /// <inheritdoc />
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var cidr = ((IPAddress Address, int Subnet))value;
            return $"CIDR '{cidr.Address}/{cidr.Subnet}'";
        }
    }
}
