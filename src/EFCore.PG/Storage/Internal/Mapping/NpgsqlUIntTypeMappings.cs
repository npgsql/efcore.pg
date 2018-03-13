using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlXidTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlXidTypeMapping() : base("xid", typeof(uint), NpgsqlDbType.Xid) {}
    }

    public class NpgsqlOidTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlOidTypeMapping() : base("oid", typeof(uint), NpgsqlDbType.Oid) {}
    }

    public class NpgsqlCidTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCidTypeMapping() : base("cid", typeof(uint), NpgsqlDbType.Cid) {}
    }

    public class NpgsqlRegtypeTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlRegtypeTypeMapping() : base("regtype", typeof(uint), NpgsqlDbType.Regtype) {}
    }
}
