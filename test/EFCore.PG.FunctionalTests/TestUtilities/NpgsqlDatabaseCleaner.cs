using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

public class NpgsqlDatabaseCleaner : RelationalDatabaseCleaner
{
    private readonly NpgsqlSqlGenerationHelper _sqlGenerationHelper;

    public NpgsqlDatabaseCleaner()
    {
        _sqlGenerationHelper = new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
    }

    protected override IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
        => new NpgsqlDatabaseModelFactory(
            new DiagnosticsLogger<DbLoggerCategory.Scaffolding>(
                loggerFactory,
                new LoggingOptions(),
                new DiagnosticListener("Fake"),
                new NpgsqlLoggingDefinitions(),
                new NullDbContextLogger()));

    protected override bool AcceptIndex(DatabaseIndex index)
        => false;

    public override void Clean(DatabaseFacade facade)
    {
        // The following is somewhat hacky
        // PostGIS creates some system tables (e.g. spatial_ref_sys) which can't be dropped until the extension
        // is dropped. But our tests create some user tables which depend on PostGIS. So we clean out PostGIS
        // and all tables that depend on it (CASCADE) before the database model is built.
        var creator = facade.GetService<IRelationalDatabaseCreator>();
        var connection = facade.GetService<IRelationalConnection>();
        if (creator.Exists())
        {
            connection.Open();
            try
            {
                var conn = (NpgsqlConnection)connection.DbConnection;
                DropExtensions(conn);
                DropTypes(conn);
                DropFunctions(conn);
                DropCollations(conn);
            }
            finally
            {
                connection.Close();
            }
        }

        base.Clean(facade);
    }

    private void DropExtensions(NpgsqlConnection conn)
    {
        const string getExtensions = @"
SELECT name FROM pg_available_extensions WHERE installed_version IS NOT NULL AND name <> 'plpgsql'";

        List<string> extensions;
        using (var cmd = new NpgsqlCommand(getExtensions, conn))
        {
            using var reader = cmd.ExecuteReader();
            extensions = reader.Cast<DbDataRecord>().Select(r => r.GetString(0)).ToList();
        }

        if (extensions.Any())
        {
            var dropExtensionsSql = string.Join("", extensions.Select(e => $"DROP EXTENSION \"{e}\" CASCADE;"));
            using var cmd = new NpgsqlCommand(dropExtensionsSql, conn);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    ///     Drop user-defined ranges and enums, cascading to all tables which depend on them
    /// </summary>
    private void DropTypes(NpgsqlConnection conn)
    {
        const string getUserDefinedRangesEnums = @"
SELECT ns.nspname, typname
FROM pg_type
JOIN pg_namespace AS ns ON ns.oid = pg_type.typnamespace
WHERE typtype IN ('r', 'e') AND nspname <> 'pg_catalog'";

        (string Schema, string Name)[] userDefinedTypes;
        using (var cmd = new NpgsqlCommand(getUserDefinedRangesEnums, conn))
        {
            using var reader = cmd.ExecuteReader();
            userDefinedTypes = reader.Cast<DbDataRecord>().Select(r => (r.GetString(0), r.GetString(1))).ToArray();
        }

        if (userDefinedTypes.Any())
        {
            var dropTypes = string.Concat(userDefinedTypes.Select(t => $@"DROP TYPE ""{t.Schema}"".""{t.Name}"" CASCADE;"));
            using var cmd = new NpgsqlCommand(dropTypes, conn);
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    ///     Drop all user-defined functions and procedures
    /// </summary>
    private void DropFunctions(NpgsqlConnection conn)
    {
        const string getUserDefinedFunctions = @"
SELECT 'DROP ROUTINE ""' || nspname || '"".""' || proname || '""(' || oidvectortypes(proargtypes) || ');' FROM pg_proc
JOIN pg_namespace AS ns ON ns.oid = pg_proc.pronamespace
WHERE
        nspname NOT IN ('pg_catalog', 'information_schema') AND
    NOT EXISTS (
            SELECT * FROM pg_depend AS dep
            WHERE dep.classid = (SELECT oid FROM pg_class WHERE relname = 'pg_proc') AND
                    dep.objid = pg_proc.oid AND
                    deptype = 'e');";

        string dropSql;
        using (var cmd = new NpgsqlCommand(getUserDefinedFunctions, conn))
        {
            using var reader = cmd.ExecuteReader();
            dropSql = string.Join("", reader.Cast<DbDataRecord>().Select(r => r.GetString(0)));
        }

        if (dropSql != "")
        {
            using var cmd = new NpgsqlCommand(dropSql, conn);
            cmd.ExecuteNonQuery();
        }
    }

    private void DropCollations(NpgsqlConnection conn)
    {
        if (conn.PostgreSqlVersion < new Version(9, 1))
        {
            return;
        }

        const string getUserCollations = @"SELECT nspname, collname
FROM pg_collation coll
    JOIN pg_namespace ns ON ns.oid=coll.collnamespace
    JOIN pg_authid auth ON auth.oid = coll.collowner WHERE rolname <> 'postgres';
";

        (string Schema, string Name)[] userDefinedTypes;
        using (var cmd = new NpgsqlCommand(getUserCollations, conn))
        {
            using var reader = cmd.ExecuteReader();
            userDefinedTypes = reader.Cast<DbDataRecord>().Select(r => (r.GetString(0), r.GetString(1))).ToArray();
        }

        if (userDefinedTypes.Any())
        {
            var dropTypes = string.Concat(userDefinedTypes.Select(t => $@"DROP COLLATION ""{t.Schema}"".""{t.Name}"" CASCADE;"));
            using var cmd = new NpgsqlCommand(dropTypes, conn);
            cmd.ExecuteNonQuery();
        }
    }

    protected override string BuildCustomSql(DatabaseModel databaseModel)
        // Some extensions create tables (e.g. PostGIS), so we must drop them first.
        => databaseModel.GetPostgresExtensions()
            .Select(e => _sqlGenerationHelper.DelimitIdentifier(e.Name, e.Schema))
            .Aggregate(
                new StringBuilder(),
                (builder, s) => builder.Append("DROP EXTENSION ").Append(s).Append(";"),
                builder => builder.ToString());

    protected override string BuildCustomEndingSql(DatabaseModel databaseModel)
        => databaseModel.GetPostgresEnums()
            .Select(e => _sqlGenerationHelper.DelimitIdentifier(e.Name, e.Schema))
            .Aggregate(
                new StringBuilder(),
                (builder, s) => builder.Append("DROP TYPE ").Append(s).Append(" CASCADE;"),
                builder => builder.ToString());
}
