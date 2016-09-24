using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public class NpgsqlCSharpMigrationOperationGenerator : CSharpMigrationOperationGenerator
    {
        readonly CSharpHelper _code;

        public NpgsqlCSharpMigrationOperationGenerator(
            [NotNull] CSharpHelper codeHelper,
            [NotNull] IDatabaseProviderServices providerServices
        ) : base(codeHelper)
        {
            _code = codeHelper;
        }

        protected override void Generate([NotNull] MigrationOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var asCreateExtensionOperation = operation as NpgsqlEnsurePostgresExtensionOperation;
            if (asCreateExtensionOperation != null)
            {
                Generate(asCreateExtensionOperation, builder);
                return;
            }

            var asDropExtensionOperation = operation as NpgsqlDropPostgresExtensionOperation;
            if (asDropExtensionOperation != null)
            {
                Generate(asDropExtensionOperation, builder);
                return;
            }

            throw new InvalidOperationException(DesignCoreStrings.UnknownOperation(operation.GetType()));
        }

        protected virtual void Generate([NotNull] NpgsqlEnsurePostgresExtensionOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append(".EnsurePostgresExtension(");

            if (operation.Schema == null && operation.Version == null)
            {
                builder.Append(_code.Literal(operation.Name));
            }
            else
            {
                using (builder.Indent())
                {
                    builder
                        .Append("name: ")
                        .Append(_code.Literal(operation.Name));

                    if (operation.Schema != null)
                    {
                        builder
                            .AppendLine(",")
                            .Append("schema: ")
                            .Append(_code.Literal(operation.Schema));
                    }

                    if (operation.Version != null)
                    {
                        builder
                            .AppendLine(",")
                            .Append("version: ")
                            .Append(_code.Literal(operation.Version));
                    }
                }
            }
            builder.Append(")");

            Annotations(operation.GetAnnotations(), builder);
        }

        protected virtual void Generate([NotNull] NpgsqlDropPostgresExtensionOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropPostgresExtension(")
                .Append(_code.Literal(operation.Name))
                .Append(")");

            Annotations(operation.GetAnnotations(), builder);
        }
    }
}
