using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

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
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            return $@"
SELECT EXISTS (
    SELECT 1 FROM pg_catalog.pg_class c
    JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
    WHERE n.nspname={stringTypeMapping.GenerateSqlLiteral(TableSchema ?? "public")} AND
          c.relname={stringTypeMapping.GenerateSqlLiteral(TableName)}
)";
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
