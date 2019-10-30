using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
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

        protected NpgsqlMacaddrTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.MacAddr) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlMacaddrTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"MACADDR '{(PhysicalAddress)value}'";

        public override Expression GenerateCodeLiteral(object value)
            => Expression.Call(ParseMethod, Expression.Constant(((PhysicalAddress)value).ToString()));

        static readonly MethodInfo ParseMethod = typeof(PhysicalAddress).GetMethod("Parse", new[] { typeof(string) });
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

        protected NpgsqlMacaddr8TypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.MacAddr8) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlMacaddr8TypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"MACADDR8 '{(PhysicalAddress)value}'";

        public override Expression GenerateCodeLiteral(object value)
            => Expression.Call(ParseMethod, Expression.Constant(((PhysicalAddress)value).ToString()));

        static readonly MethodInfo ParseMethod = typeof(PhysicalAddress).GetMethod("Parse", new[] { typeof(string) });
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

        protected NpgsqlInetTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Inet) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlInetTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"INET '{(IPAddress)value}'";

        public override Expression GenerateCodeLiteral(object value)
            => Expression.Call(ParseMethod, Expression.Constant(((IPAddress)value).ToString()));

        static readonly MethodInfo ParseMethod = typeof(IPAddress).GetMethod("Parse", new[] { typeof(string) });
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

        protected NpgsqlCidrTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Cidr) {}

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
                Constructor,
                Expression.Call(ParseMethod, Expression.Constant(cidr.Address.ToString())),
                Expression.Constant(cidr.Subnet));
        }

        static readonly MethodInfo ParseMethod = typeof(IPAddress).GetMethod("Parse", new[] { typeof(string) });

        static readonly ConstructorInfo Constructor =
            typeof((IPAddress, int)).GetConstructor(new[] { typeof(IPAddress), typeof(int) });
    }
}
