using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore.Storage;
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

        public override Expression GenerateCodeLiteral(object value)
            => Expression.Call(
                typeof(PhysicalAddress).GetMethod("Parse", new[] {typeof(string)}),
                Expression.Constant(((PhysicalAddress)value).ToString()));
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

        public override Expression GenerateCodeLiteral(object value)
            => Expression.Call(
                typeof(PhysicalAddress).GetMethod("Parse", new[] {typeof(string)}),
                Expression.Constant(((PhysicalAddress)value).ToString()));
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

        public override Expression GenerateCodeLiteral(object value)
            => Expression.Call(
                typeof(IPAddress).GetMethod("Parse", new[] {typeof(string)}),
                Expression.Constant(((IPAddress)value).ToString()));
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

        public override Expression GenerateCodeLiteral(object value)
        {
            var cidr = ((IPAddress Address, int Subnet))value;
            return Expression.New(
                typeof((IPAddress, int)).GetConstructor(new[] { typeof(IPAddress), typeof(int) }),
                Expression.Call(
                    typeof(IPAddress).GetMethod("Parse", new[] {typeof(string)}),
                    Expression.Constant(cidr.Address.ToString())),
                Expression.Constant(cidr.Subnet));
        }
    }
}
