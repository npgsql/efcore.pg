using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlRegconfigTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlRegconfigTypeMapping() : base("regconfig", typeof(uint), NpgsqlDbType.Regconfig) { }

        protected NpgsqlRegconfigTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Regconfig) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlRegconfigTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"'{EscapeSqlLiteral((string)value)}'";

        string EscapeSqlLiteral([NotNull] string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
    }
}
