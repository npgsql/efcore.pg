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
        static readonly string NpgsqlProviderName = typeof(MigrationBuilderExtensions).GetTypeInfo().Assembly.GetName().Name;

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

            var operation = new NpgsqlEnsurePostgresExtensionOperation
            {
                Name = name,
                Schema = schema,
                Version = version
            };

            if (builder.ActiveProvider == NpgsqlProviderName)
                builder.Operations.Add(operation);

            return new OperationBuilder<NpgsqlEnsurePostgresExtensionOperation>(operation);
        }

        [Obsolete("Use EnsurePostgresExtension instead")]
        public static OperationBuilder<NpgsqlEnsurePostgresExtensionOperation> CreatePostgresExtension(
            this MigrationBuilder builder,
            [NotNull] string name,
            string schema = null,
            string version = null
        )
            => EnsurePostgresExtension(builder, name, schema, version);

        public static OperationBuilder<NpgsqlDropPostgresExtensionOperation> DropPostgresExtension(
            this MigrationBuilder builder,
            [NotNull] string name
        )
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new NpgsqlDropPostgresExtensionOperation { Name = name };

            if (builder.ActiveProvider == NpgsqlProviderName)
                builder.Operations.Add(operation);

            return new OperationBuilder<NpgsqlDropPostgresExtensionOperation>(operation);
        }
    }
}
