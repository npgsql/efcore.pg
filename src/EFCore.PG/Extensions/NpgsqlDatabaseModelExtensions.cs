using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlDatabaseModelExtensions
    {
        [NotNull]
        public static PostgresExtension GetOrAddPostgresExtension(
            [NotNull] this DatabaseModel model,
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string version)
            => PostgresExtension.GetOrAddPostgresExtension(model, schema, name, version);

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<PostgresExtension> GetPostgresExtensions([NotNull] this DatabaseModel model)
            => PostgresExtension.GetPostgresExtensions(model).ToArray();

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<PostgresEnum> GetPostgresEnums([NotNull] this DatabaseModel model)
            => PostgresEnum.GetPostgresEnums(model).ToArray();
    }
}
