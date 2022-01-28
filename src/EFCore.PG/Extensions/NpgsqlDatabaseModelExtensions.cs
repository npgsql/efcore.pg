using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class NpgsqlDatabaseModelExtensions
{
    public static PostgresExtension GetOrAddPostgresExtension(
        this DatabaseModel model,
        string? schema,
        string name,
        string? version)
        => PostgresExtension.GetOrAddPostgresExtension(model, schema, name, version);

    public static IReadOnlyList<PostgresExtension> GetPostgresExtensions(this DatabaseModel model)
        => PostgresExtension.GetPostgresExtensions(model).ToArray();

    public static IReadOnlyList<PostgresEnum> GetPostgresEnums(this DatabaseModel model)
        => PostgresEnum.GetPostgresEnums(model).ToArray();
}