using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public static class MigrationBuilderExtensions
    {
        [Obsolete("See the Npgsql 1.1.0 migration notes on PostgreSQL extensions")]
        public static OperationBuilder<NpgsqlEnsurePostgresExtensionOperation> EnsurePostgresExtension(
            this MigrationBuilder builder,
            [NotNull] string name,
            string schema = null,
            string version = null
        )
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NullButNotEmpty(version, nameof(schema));

            return new OperationBuilder<NpgsqlEnsurePostgresExtensionOperation>(new NpgsqlEnsurePostgresExtensionOperation
            {
                Name = name,
                Schema = schema,
                Version = version
            });
        }

        [Obsolete("See the Npgsql 1.1.0 migration notes on PostgreSQL extensions")]
        public static OperationBuilder<NpgsqlEnsurePostgresExtensionOperation> CreatePostgresExtension(
            this MigrationBuilder builder,
            [NotNull] string name,
            string schema = null,
            string version = null
        )
            => EnsurePostgresExtension(builder, name, schema, version);

        [Obsolete("See the Npgsql 1.1.0 migration notes on PostgreSQL extensions")]
        public static OperationBuilder<NpgsqlDropPostgresExtensionOperation> DropPostgresExtension(
            this MigrationBuilder builder,
            [NotNull] string name
        )
        {
            Check.NotEmpty(name, nameof(name));

            return new OperationBuilder<NpgsqlDropPostgresExtensionOperation>(new NpgsqlDropPostgresExtensionOperation { Name = name });
        }
    }
}
