using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Pipeline
{
    /// <summary>
    /// The default factory for Npgsql-specific query SQL generators.
    /// </summary>
    public class NpgsqlQuerySqlGeneratorFactory : QuerySqlGeneratorFactory
    {
        readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        readonly ISqlGenerationHelper _sqlGenerationHelper;

        /// <summary>
        /// Represents options for Npgsql that can only be set by the service provider.
        /// </summary>
        [NotNull] readonly INpgsqlOptions _npgsqlOptions;

        public NpgsqlQuerySqlGeneratorFactory(
            IRelationalCommandBuilderFactory commandBuilderFactory,
            ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] INpgsqlOptions npgsqlOptions)
            : base(commandBuilderFactory, sqlGenerationHelper)
        {
            _commandBuilderFactory = commandBuilderFactory;
            _sqlGenerationHelper = sqlGenerationHelper;
            _npgsqlOptions = Check.NotNull(npgsqlOptions, nameof(npgsqlOptions));
        }

        public override QuerySqlGenerator Create()
            => new NpgsqlQuerySqlGenerator(
                _commandBuilderFactory,
                _sqlGenerationHelper,
                _npgsqlOptions.ReverseNullOrderingEnabled,
                _npgsqlOptions.PostgresVersion);
    }
}
