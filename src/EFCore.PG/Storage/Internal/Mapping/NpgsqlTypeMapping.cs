using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     The base class for mapping Npgsql-specific types. It configures parameters with the
///     <see cref="NpgsqlDbType" /> provider-specific type enum.
/// </summary>
public abstract class NpgsqlTypeMapping : RelationalTypeMapping, INpgsqlTypeMapping
{
    /// <inheritdoc />
    public virtual NpgsqlDbType NpgsqlDbType { get; }

    // ReSharper disable once PublicConstructorInAbstractClass
    /// <summary>
    ///     Constructs an instance of the <see cref="NpgsqlTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The database type to map.</param>
    /// <param name="clrType">The CLR type to map.</param>
    /// <param name="npgsqlDbType">The database type used by Npgsql.</param>
    /// <param name="jsonValueReaderWriter">Handles reading and writing JSON values for instances of the mapped type.</param>
    /// <param name="elementTypeMapping">If this type mapping represents a primitive collection, this holds the element's type mapping.</param>
    public NpgsqlTypeMapping(
        string storeType,
        Type clrType,
        NpgsqlDbType npgsqlDbType,
        JsonValueReaderWriter? jsonValueReaderWriter = null,
        CoreTypeMapping? elementTypeMapping = null)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    clrType,
                    jsonValueReaderWriter: jsonValueReaderWriter,
                    elementMapping: elementTypeMapping),
                storeType))
    {
        NpgsqlDbType = npgsqlDbType;
    }

    /// <summary>
    ///     Constructs an instance of the <see cref="NpgsqlTypeMapping" /> class.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <param name="npgsqlDbType">The database type of the range subtype.</param>
    protected NpgsqlTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
        : base(parameters)
    {
        NpgsqlDbType = npgsqlDbType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is not NpgsqlParameter npgsqlParameter)
        {
            throw new InvalidOperationException(
                $"Npgsql-specific type mapping {GetType().Name} being used with non-Npgsql parameter type {parameter.GetType().Name}");
        }

        base.ConfigureParameter(parameter);
        npgsqlParameter.NpgsqlDbType = NpgsqlDbType;
    }

    /// <summary>
    ///     Generates the SQL representation of a literal value meant to be embedded in another literal value, e.g. in a range.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    ///     The generated string.
    /// </returns>
    public virtual string GenerateEmbeddedSqlLiteral(object? value)
    {
        value = ConvertUnderlyingEnumValueToEnum(value);

        if (Converter != null)
        {
            value = Converter.ConvertToProvider(value);
        }

        return GenerateEmbeddedProviderValueSqlLiteral(value);
    }

    /// <summary>
    ///     Generates the SQL representation of a literal value without conversion, meant to be embedded in another literal value,
    ///     e.g. in a range.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    ///     The generated string.
    /// </returns>
    public virtual string GenerateEmbeddedProviderValueSqlLiteral(object? value)
        => value == null
            ? "NULL"
            : GenerateEmbeddedNonNullSqlLiteral(value);

    /// <summary>
    ///     Generates the SQL representation of a non-null literal value, meant to be embedded in another literal value, e.g. in a range.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    ///     The generated string.
    /// </returns>
    protected virtual string GenerateEmbeddedNonNullSqlLiteral(object value)
        => GenerateNonNullSqlLiteral(value);

    // Copied from RelationalTypeMapping
    private object? ConvertUnderlyingEnumValueToEnum(object? value)
        => value?.GetType().IsInteger() == true && ClrType.UnwrapNullableType().IsEnum
            ? Enum.ToObject(ClrType.UnwrapNullableType(), value)
            : value;
}
