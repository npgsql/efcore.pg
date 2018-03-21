using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class NpgsqlDatabaseCleaner : RelationalDatabaseCleaner
    {
        NpgsqlSqlGenerationHelper _sqlGenerationHelper;

        public NpgsqlDatabaseCleaner()
            => _sqlGenerationHelper = new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());

        protected override IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
            => new NpgsqlDatabaseModelFactory(
                new DiagnosticsLogger<DbLoggerCategory.Scaffolding>(
                    loggerFactory,
                    new LoggingOptions(),
                    new DiagnosticListener("Fake")));

        protected override bool AcceptIndex(DatabaseIndex index)
            => false;

        protected override string BuildCustomEndingSql(DatabaseModel databaseModel)
        {
            var sb = new StringBuilder();

            foreach (var extension in PostgresExtension.GetPostgresExtensions(databaseModel))
                sb
                    .Append("DROP EXTENSION ")
                    .Append(_sqlGenerationHelper.DelimitIdentifier(extension.Name, extension.Schema))
                    .AppendLine(" CASCADE;");

            foreach (var enumDef in PostgresEnum.GetPostgresEnums(databaseModel))
                sb
                    .Append("DROP TYPE ")
                    .Append(_sqlGenerationHelper.DelimitIdentifier(enumDef.Name, enumDef.Schema))
                    .AppendLine(" CASCADE;");

            return sb.ToString();
        }
    }
}
