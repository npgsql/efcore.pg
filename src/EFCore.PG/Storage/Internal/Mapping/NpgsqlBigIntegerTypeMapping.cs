using System.Numerics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlBigIntegerTypeMapping : NpgsqlTypeMapping
{
    public NpgsqlBigIntegerTypeMapping() : base("numeric", typeof(BigInteger), NpgsqlDbType.Numeric) {}

    protected NpgsqlBigIntegerTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Numeric)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlBigIntegerTypeMapping(parameters);

    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null
            ? storeType
            : parameters.Scale is null
                ? $"numeric({parameters.Precision})"
                : $"numeric({parameters.Precision},{parameters.Scale})";
}