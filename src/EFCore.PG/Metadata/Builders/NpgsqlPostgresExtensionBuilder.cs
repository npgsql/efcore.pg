using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public class NpgsqlPostgresExtensionBuilder
    {
        private readonly PostgresExtension _postgresExtension;

        public NpgsqlPostgresExtensionBuilder([NotNull] PostgresExtension postgresExtension)
        {
            Check.NotNull(postgresExtension, nameof(postgresExtension));

            _postgresExtension = postgresExtension;
        }

        public virtual PostgresExtension Metadata => _postgresExtension;

        public virtual NpgsqlPostgresExtensionBuilder Schema(string schema)
        {
            _postgresExtension.Schema = schema;

            return this;
        }

        public virtual NpgsqlPostgresExtensionBuilder Version(string version)
        {
            _postgresExtension.Version = version;

            return this;
        }
    }
}
