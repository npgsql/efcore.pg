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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlMacaddrTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlMacaddrTypeMapping() : base("macaddr", typeof(PhysicalAddress), NpgsqlDbType.MacAddr) {}

        protected NpgsqlMacaddrTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.MacAddr) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlMacaddrTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"MACADDR '{(PhysicalAddress)value}'";
    }

    public class NpgsqlMacaddr8TypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlMacaddr8TypeMapping() : base("macaddr8", typeof(PhysicalAddress), NpgsqlDbType.MacAddr8) {}

        protected NpgsqlMacaddr8TypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.MacAddr8) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlMacaddr8TypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"MACADDR8 '{(PhysicalAddress)value}'";
    }

    public class NpgsqlInetTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlInetTypeMapping() : base("inet", typeof(IPAddress), NpgsqlDbType.Inet) {}

        protected NpgsqlInetTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Inet) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlInetTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"INET '{(IPAddress)value}'";
    }

    public class NpgsqlCidrTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCidrTypeMapping() : base("cidr", typeof((IPAddress, int)), NpgsqlDbType.Cidr) {}

        protected NpgsqlCidrTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlTypes.NpgsqlDbType.Cidr) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlCidrTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var cidr = ((IPAddress Address, int Subnet))value;
            return $"CIDR '{cidr.Address}/{cidr.Subnet}'";
        }
    }
}
