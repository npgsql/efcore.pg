using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlTimeTypeMapping : NpgsqlTypeMapping
{
    public NpgsqlTimeTypeMapping(Type clrType) : base("time without time zone", clrType, NpgsqlDbType.Time) {}

    protected NpgsqlTimeTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Time) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlTimeTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIME '{GenerateEmbeddedNonNullSqlLiteral(value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => value switch
        {
            TimeSpan ts => ts.Ticks % 10000000 == 0
                ? ts.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
                : ts.ToString(@"hh\:mm\:ss\.FFFFFF", CultureInfo.InvariantCulture),
            TimeOnly t => t.Ticks % 10000000 == 0
                ? t.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture)
                : t.ToString(@"HH\:mm\:ss\.FFFFFF", CultureInfo.InvariantCulture),
            _ => throw new InvalidCastException($"Can't generate a time SQL literal for CLR type {value.GetType()}")
        };
}