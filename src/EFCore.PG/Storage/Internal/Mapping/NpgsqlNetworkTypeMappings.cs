using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlMacaddrTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlMacaddrTypeMapping() : base("macaddr", typeof(PhysicalAddress), NpgsqlDbType.MacAddr) {}

        protected NpgsqlMacaddrTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlMacaddrTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlMacaddrTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"MACADDR '{(PhysicalAddress)value}'";
    }

    public class NpgsqlMacaddr8TypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlMacaddr8TypeMapping() : base("macaddr8", typeof(PhysicalAddress), NpgsqlDbType.MacAddr) {}

        protected NpgsqlMacaddr8TypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlMacaddr8TypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlMacaddr8TypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"MACADDR8 '{(PhysicalAddress)value}'";
    }

    public class NpgsqlInetTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlInetTypeMapping() : base("inet", typeof(IPAddress), NpgsqlDbType.Inet) {}

        protected NpgsqlInetTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlInetTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlInetTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"INET '{(IPAddress)value}'";
    }

    public class NpgsqlCidrTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCidrTypeMapping() : base("cidr", typeof(NpgsqlInet), NpgsqlDbType.Cidr) {}

        protected NpgsqlCidrTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlCidrTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlCidrTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"CIDR '{(NpgsqlInet)value}'";
    }
}
