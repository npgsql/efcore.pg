using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class NpgsqlModelValidator : RelationalModelValidator
    {
        readonly IRelationalAnnotationProvider _relationalExtensions;

        public NpgsqlModelValidator([NotNull] ILogger<RelationalModelValidator> loggerFactory, [NotNull] IRelationalAnnotationProvider relationalExtensions, [NotNull] IRelationalTypeMapper typeMapper)
            : base(loggerFactory, relationalExtensions, typeMapper)
        {
            _relationalExtensions = relationalExtensions;
        }

        public override void Validate(IModel model)
        {
            base.Validate(model);

            EnsureUuidExtensionIfNeeded(model);
        }

        protected virtual void EnsureUuidExtensionIfNeeded(IModel model)
        {
            var generatedUuidProperty = (
                from e in model.GetEntityTypes()
                from p in e.GetProperties()
                where p.ClrType.UnwrapNullableType() == typeof(Guid)
                let defaultValueSql = (string)_relationalExtensions.For(p).DefaultValueSql
                where defaultValueSql != null && defaultValueSql.Contains("uuid_generate")
                select p
            ).FirstOrDefault();

            if (generatedUuidProperty != null &&
                model.Npgsql().PostgresExtensions.All(e => e.Name != "uuid-ossp"))
            {
                ShowError(@"Database-generated UUIDs require the PostgreSQL uuid-ossp extension. Add .HasPostgresExtension(""uuid-ossp"") to your context's OnModelCreating.");
            }
        }
    }
}
