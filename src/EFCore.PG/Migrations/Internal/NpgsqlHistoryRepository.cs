using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal
{
    public class NpgsqlHistoryRepository : HistoryRepository
    {
        public NpgsqlHistoryRepository(HistoryRepositoryDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override string ExistsSql
        {
            get
            {
                var builder = new StringBuilder();

                builder.Append("SELECT EXISTS (SELECT 1 FROM pg_catalog.pg_class c JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace WHERE ");

                var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

                if (TableSchema != null)
                {
                    builder
                        .Append("n.nspname=")
                        .Append(stringTypeMapping.GenerateSqlLiteral(TableSchema))
                        .Append(" AND ");
                }

                builder
                    .Append("c.relname=")
                    .Append(stringTypeMapping.GenerateSqlLiteral(TableName))
                    .Append(");");

                return builder.ToString();
            }
        }

        protected override bool InterpretExistsResult(object? value) => (bool?)value == true;

        public override string GetCreateIfNotExistsScript()
        {
            var script = GetCreateScript();
            return script.Insert(script.IndexOf("CREATE TABLE", StringComparison.Ordinal) + 12, " IF NOT EXISTS");
        }

        public override string GetBeginIfNotExistsScript(string migrationId) => $@"
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM {SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema)} WHERE ""{MigrationIdColumnName}"" = '{migrationId}') THEN";

        public override string GetBeginIfExistsScript(string migrationId) => $@"
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM {SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema)} WHERE ""{MigrationIdColumnName}"" = '{migrationId}') THEN";

        public override string GetEndIfScript() =>
            @"    END IF;
END $EF$;";
    }
}
