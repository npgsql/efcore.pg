using System;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlTimeTzTypeMapping : NpgsqlTypeMapping
{
    public NpgsqlTimeTzTypeMapping() : base("time with time zone", typeof(DateTimeOffset), NpgsqlDbType.TimeTz) {}

    protected NpgsqlTimeTzTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.TimeTz) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlTimeTzTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => FormattableString.Invariant($"TIMETZ '{(DateTimeOffset)value:HH:mm:ss.FFFFFFz}'");

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => FormattableString.Invariant(@$"""{(DateTimeOffset)value:HH:mm:ss.FFFFFFz}""");
}