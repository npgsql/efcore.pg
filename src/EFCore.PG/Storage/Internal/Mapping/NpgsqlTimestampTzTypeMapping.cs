using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlTimestampTzTypeMapping : NpgsqlTypeMapping
{
    public NpgsqlTimestampTzTypeMapping(Type clrType)
        : base("timestamp with time zone", clrType, NpgsqlDbType.TimestampTz) {}

    protected NpgsqlTimestampTzTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.TimestampTz) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlTimestampTzTypeMapping(parameters);

    protected override string ProcessStoreType(RelationalTypeMappingParameters parameters, string storeType, string _)
        => parameters.Precision is null ? storeType : $"timestamp({parameters.Precision}) with time zone";

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"TIMESTAMPTZ '{GenerateLiteralCore(value)}'";

    protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
        => @$"""{GenerateLiteralCore(value)}""";

    private string GenerateLiteralCore(object value)
    {
        switch (value)
        {
            case DateTime dateTime:
                if (!NpgsqlTypeMappingSource.DisableDateTimeInfinityConversions)
                {
                    if (dateTime == DateTime.MinValue)
                    {
                        return "-infinity";
                    }

                    if (dateTime == DateTime.MaxValue)
                    {
                        return "infinity";
                    }
                }

                return dateTime.Kind switch
                {
                    DateTimeKind.Utc => dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFFFF", CultureInfo.InvariantCulture) + 'Z',

                    DateTimeKind.Unspecified => NpgsqlTypeMappingSource.LegacyTimestampBehavior || dateTime == default
                        ? dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFFFF", CultureInfo.InvariantCulture) + 'Z'
                        : throw new InvalidCastException(
                            $"'timestamp with time zone' literal cannot be generated for {dateTime.Kind} DateTime: a UTC DateTime is required"),

                    DateTimeKind.Local => NpgsqlTypeMappingSource.LegacyTimestampBehavior
                        ? dateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFzzz", CultureInfo.InvariantCulture)
                        : throw new InvalidCastException(
                            $"'timestamp with time zone' literal cannot be generated for {dateTime.Kind} DateTime: a UTC DateTime is required"),

                    _ => throw new ArgumentOutOfRangeException()
                };

            case DateTimeOffset dateTimeOffset:
                if (!NpgsqlTypeMappingSource.DisableDateTimeInfinityConversions)
                {
                    if (dateTimeOffset == DateTimeOffset.MinValue)
                    {
                        return "-infinity";
                    }

                    if (dateTimeOffset == DateTimeOffset.MaxValue)
                    {
                        return "infinity";
                    }
                }

                return dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFzzz", CultureInfo.InvariantCulture);

            default:
                throw new InvalidCastException(
                    $"Attempted to generate timestamptz literal for type {value.GetType()}, only DateTime and DateTimeOffset are supported");
        }
    }
}