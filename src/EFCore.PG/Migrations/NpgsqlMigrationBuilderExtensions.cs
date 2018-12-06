using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    // TODO: This is unused. Can we remove it?
    public static class NpgsqlMigrationBuilderExtensions
    {
        public static MigrationBuilder EnsurePostgresExtension(
            this MigrationBuilder builder,
            [NotNull] string name,
            string schema = null,
            string version = null
        )
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NullButNotEmpty(version, nameof(schema));

            var op = new AlterDatabaseOperation();
            op.Npgsql().GetOrAddPostgresExtension(schema, name, version);
            builder.Operations.Add(op);

            return builder;
        }

        [Obsolete("Use EnsurePostgresExtension instead")]
        public static MigrationBuilder CreatePostgresExtension(
            this MigrationBuilder builder,
            [NotNull] string name,
            string schema = null,
            string version = null)
            => EnsurePostgresExtension(builder, name, schema, version);

        [Obsolete("This no longer does anything and should be removed.")]
        public static MigrationBuilder DropPostgresExtension(
            this MigrationBuilder builder,
            [NotNull] string name)
            => builder;
    }
}
