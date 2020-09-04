using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlMigrationBuilderExtensions
    {
        /// <summary>
        /// Returns true if the active provider in a migration is the Npgsql provider.
        /// </summary>
        /// The migrationBuilder from the parameters on <see cref="Migration.Up(MigrationBuilder)" /> or
        /// <see cref="Migration.Down(MigrationBuilder)" />.
        /// <returns>True if Npgsql is being used; false otherwise.</returns>
        public static bool IsNpgsql([NotNull] this MigrationBuilder builder)
            => builder.ActiveProvider.Equals(
                typeof(NpgsqlMigrationBuilderExtensions).GetTypeInfo().Assembly.GetName().Name,
                StringComparison.Ordinal);

        public static MigrationBuilder EnsurePostgresExtension(
            [NotNull] this MigrationBuilder builder,
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] string version = null
        )
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NullButNotEmpty(version, nameof(schema));

            var op = new AlterDatabaseOperation();
            op.GetOrAddPostgresExtension(schema, name, version);
            builder.Operations.Add(op);

            return builder;
        }

        [Obsolete("Use EnsurePostgresExtension instead")]
        public static MigrationBuilder CreatePostgresExtension(
            [NotNull] this MigrationBuilder builder,
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] string version = null)
            => EnsurePostgresExtension(builder, name, schema, version);

        [Obsolete("This no longer does anything and should be removed.")]
        public static MigrationBuilder DropPostgresExtension(
            [NotNull] this MigrationBuilder builder,
            [NotNull] string name)
            => builder;
    }
}
