using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlDecimalTypeMapping : NpgsqlTypeMapping
{
    private const string DecimalFormatConst = "{0:0.0###########################}";

    public NpgsqlDecimalTypeMapping(Type? clrType = null) : base("numeric", clrType ?? typeof(decimal), NpgsqlTypes.NpgsqlDbType.Numeric) {}

    protected NpgsqlDecimalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlTypes.NpgsqlDbType.Numeric)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlDecimalTypeMapping(parameters);

    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null
            ? storeType
            : parameters.Scale is null
                ? $"numeric({parameters.Precision})"
                : $"numeric({parameters.Precision},{parameters.Scale})";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string SqlLiteralFormatString
        => DecimalFormatConst;
}