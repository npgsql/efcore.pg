using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Extension methods for <see cref="AlterDatabaseOperation" /> for Npgsql-specific metadata.
/// </summary>
public static class NpgsqlAlterDatabaseOperationExtensions
{
    public static IReadOnlyList<PostgresCollation> GetPostgresCollations(this AlterDatabaseOperation operation)
        => PostgresCollation.GetCollations(operation).ToArray();

    public static IReadOnlyList<PostgresCollation> GetOldPostgresCollations(this AlterDatabaseOperation operation)
        => PostgresCollation.GetCollations(operation.OldDatabase).ToArray();

    public static IReadOnlyList<PostgresExtension> GetPostgresExtensions(this AlterDatabaseOperation operation)
        => PostgresExtension.GetPostgresExtensions(operation).ToArray();

    public static IReadOnlyList<PostgresExtension> GetOldPostgresExtensions(this AlterDatabaseOperation operation)
        => PostgresExtension.GetPostgresExtensions(operation.OldDatabase).ToArray();

    public static IReadOnlyList<PostgresEnum> GetPostgresEnums(this AlterDatabaseOperation operation)
        => PostgresEnum.GetPostgresEnums(operation).ToArray();

    public static IReadOnlyList<PostgresEnum> GetOldPostgresEnums(this AlterDatabaseOperation operation)
        => PostgresEnum.GetPostgresEnums(operation.OldDatabase).ToArray();

    public static IReadOnlyList<PostgresRange> GetPostgresRanges(this AlterDatabaseOperation operation)
        => PostgresRange.GetPostgresRanges(operation).ToArray();

    public static IReadOnlyList<PostgresRange> GetOldPostgresRanges(this AlterDatabaseOperation operation)
        => PostgresRange.GetPostgresRanges(operation.OldDatabase).ToArray();

    public static PostgresExtension GetOrAddPostgresExtension(
        this AlterDatabaseOperation operation,
        string? schema,
        string name,
        string? version)
        => PostgresExtension.GetOrAddPostgresExtension(operation, schema, name, version);
}