using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The base class for mapping Npgsql-specific string types. It configures parameters with the
/// <see cref="NpgsqlDbType"/> provider-specific type enum.
/// </summary>
public class NpgsqlStringTypeMapping : StringTypeMapping, INpgsqlTypeMapping
{
    /// <inheritdoc />
    public virtual NpgsqlDbType NpgsqlDbType { get; }

    // ReSharper disable once PublicConstructorInAbstractClass
    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlTypeMapping"/> class.
    /// </summary>
    /// <param name="storeType">The database type to map.</param>
    /// <param name="npgsqlDbType">The database type used by Npgsql.</param>
    public NpgsqlStringTypeMapping(string storeType, NpgsqlDbType npgsqlDbType)
        : base(storeType, System.Data.DbType.String)
        => NpgsqlDbType = npgsqlDbType;

    protected NpgsqlStringTypeMapping(
        RelationalTypeMappingParameters parameters,
        NpgsqlDbType npgsqlDbType)
        : base(parameters)
        => NpgsqlDbType = npgsqlDbType;

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlStringTypeMapping(parameters, NpgsqlDbType);

    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is not NpgsqlParameter npgsqlParameter)
        {
            throw new InvalidOperationException($"Npgsql-specific type mapping {GetType().Name} being used with non-Npgsql parameter type {parameter.GetType().Name}");
        }

        base.ConfigureParameter(parameter);
        npgsqlParameter.NpgsqlDbType = NpgsqlDbType;
    }
}