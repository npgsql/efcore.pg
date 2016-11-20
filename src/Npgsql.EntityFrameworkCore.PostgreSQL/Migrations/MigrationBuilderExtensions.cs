using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public static class MigrationBuilderExtensions
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
            var extension = PostgresExtension.GetOrAddPostgresExtension(op, name);
            extension.Schema = schema;
            extension.Version = version;
            builder.Operations.Add(op);

            return builder;
        }

        [Obsolete("Use EnsurePostgresExtension instead")]
        public static MigrationBuilder CreatePostgresExtension(
            this MigrationBuilder builder,
            [NotNull] string name,
            string schema = null,
            string version = null
        )
            => EnsurePostgresExtension(builder, name, schema, version);

        [Obsolete("This no longer does anything and should be removed.")]
        public static MigrationBuilder DropPostgresExtension(
            this MigrationBuilder builder,
            [NotNull] string name
        )
            => builder;
    }
}
