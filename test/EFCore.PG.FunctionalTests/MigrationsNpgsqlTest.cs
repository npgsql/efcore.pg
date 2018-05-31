using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class MigrationsNpgsqlTest : MigrationsTestBase<MigrationsNpgsqlFixture>
    {
        public MigrationsNpgsqlTest(MigrationsNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public override void Can_generate_idempotent_up_scripts()
        {
            base.Can_generate_idempotent_up_scripts();

            Assert.Equal(
@"CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
    ""MigrationId"" varchar(150) NOT NULL,
    ""ProductVersion"" varchar(32) NOT NULL,
    CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
);


DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '00000000000001_Migration1') THEN
    CREATE TABLE ""Table1"" (
        ""Id"" integer NOT NULL,
        CONSTRAINT ""PK_Table1"" PRIMARY KEY (""Id"")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '00000000000001_Migration1') THEN
    INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
    VALUES ('00000000000001_Migration1', '7.0.0-test');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '00000000000002_Migration2') THEN
    ALTER TABLE ""Table1"" RENAME TO ""Table2"";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '00000000000002_Migration2') THEN
    INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
    VALUES ('00000000000002_Migration2', '7.0.0-test');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '00000000000003_Migration3') THEN
    INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
    VALUES ('00000000000003_Migration3', '7.0.0-test');
    END IF;
END $$;
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void Can_generate_idempotent_down_scripts()
        {
            base.Can_generate_idempotent_down_scripts();

            Assert.Equal(@"
DO $$
BEGIN
    IF EXISTS(SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '00000000000002_Migration2') THEN
    ALTER TABLE ""Table2"" RENAME TO ""Table1"";
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS(SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '00000000000002_Migration2') THEN
    DELETE FROM ""__EFMigrationsHistory""
    WHERE ""MigrationId"" = '00000000000002_Migration2';
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS(SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '00000000000001_Migration1') THEN
    DROP TABLE ""Table1"";
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS(SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '00000000000001_Migration1') THEN
    DELETE FROM ""__EFMigrationsHistory""
    WHERE ""MigrationId"" = '00000000000001_Migration1';
    END IF;
END $$;
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        protected override void AssertFirstMigration(DbConnection connection)
        {
            var sql = GetDatabaseSchemaAsync(connection);
            Assert.Equal(
                @"
CreatedTable
    Id int4 NOT NULL
    ColumnWithDefaultToDrop int4 NULL DEFAULT 0
    ColumnWithDefaultToAlter int4 NULL DEFAULT 1
",
                sql,
                ignoreLineEndingDifferences: true);
        }

        protected override void BuildSecondMigration(MigrationBuilder migrationBuilder)
        {
            base.BuildSecondMigration(migrationBuilder);

            for (var i = migrationBuilder.Operations.Count - 1; i >= 0; i--)
            {
                var operation = migrationBuilder.Operations[i];
                if (operation is AlterColumnOperation
                    || operation is DropColumnOperation)
                {
                    migrationBuilder.Operations.RemoveAt(i);
                }
            }
        }

        protected override void AssertSecondMigration(DbConnection connection)
        {
            var sql = GetDatabaseSchemaAsync(connection);
            Assert.Equal(
                @"
CreatedTable
    Id int4 NOT NULL
    ColumnWithDefaultToDrop int4 NULL DEFAULT 0
    ColumnWithDefaultToAlter int4 NULL DEFAULT 1
",
                sql,
                ignoreLineEndingDifferences: true);
        }

        private string GetDatabaseSchemaAsync(DbConnection connection)
        {
            var builder = new IndentedStringBuilder();
            var command = connection.CreateCommand();
            command.CommandText = @"
SELECT table_name,
	column_name,
	udt_name,
	is_nullable = 'YES',
	column_default
FROM information_schema.columns
WHERE table_catalog = @db
	AND table_schema = 'public'
ORDER BY table_name, ordinal_position
";

            var dbName = connection.Database;
            command.Parameters.Add(new NpgsqlParameter { ParameterName = "db", Value = dbName });

            using (var reader = command.ExecuteReader())
            {
                var first = true;
                string lastTable = null;
                while (reader.Read())
                {
                    var currentTable = reader.GetString(0);
                    if (currentTable != lastTable)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            builder.DecrementIndent();
                        }

                        builder
                            .AppendLine()
                            .AppendLine(currentTable)
                            .IncrementIndent();

                        lastTable = currentTable;
                    }

                    builder
                        .Append(reader[1]) // Name
                        .Append(" ")
                        .Append(reader[2]) // Type
                        .Append(" ")
                        .Append(reader.GetBoolean(3) ? "NULL" : "NOT NULL");

                    if (!reader.IsDBNull(4))
                    {
                        builder
                            .Append(" DEFAULT ")
                            .Append(reader[4]);
                    }

                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}
