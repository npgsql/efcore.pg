using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// Type mapping for the PostgreSQL 'character' data type. Handles both CLR strings and chars.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/datatype-character.html
/// </remarks>
public class NpgsqlCharacterCharTypeMapping : CharTypeMapping, INpgsqlTypeMapping
{
    /// <inheritdoc />
    public virtual NpgsqlDbType NpgsqlDbType
        => NpgsqlDbType.Char;

    public NpgsqlCharacterCharTypeMapping(string storeType)
        : this(new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(typeof(char)),
            storeType,
            StoreTypePostfix.Size,
            System.Data.DbType.StringFixedLength,
            unicode: false,
            size: 1,
            fixedLength: true)) {}

    protected NpgsqlCharacterCharTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlCharacterCharTypeMapping(parameters);

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