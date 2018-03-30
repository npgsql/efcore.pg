using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlXidTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlXidTypeMapping() : base("xid", typeof(uint), NpgsqlDbType.Xid) {}

        protected NpgsqlXidTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlXidTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlXidTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);
    }

    public class NpgsqlOidTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlOidTypeMapping() : base("oid", typeof(uint), NpgsqlDbType.Oid) {}

        protected NpgsqlOidTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlOidTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlOidTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);
    }

    public class NpgsqlCidTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCidTypeMapping() : base("cid", typeof(uint), NpgsqlDbType.Cid) {}

        protected NpgsqlCidTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlCidTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlCidTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);
    }

    public class NpgsqlRegtypeTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlRegtypeTypeMapping() : base("regtype", typeof(uint), NpgsqlDbType.Regtype) {}

        protected NpgsqlRegtypeTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters, npgsqlDbType) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlRegtypeTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size), NpgsqlDbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlRegtypeTypeMapping(Parameters.WithComposedConverter(converter), NpgsqlDbType);
    }
}
