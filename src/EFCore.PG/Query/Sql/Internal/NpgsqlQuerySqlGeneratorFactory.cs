using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal
{
    /// <summary>
    /// The default factory for Npgsql-specific query SQL generators.
    /// </summary>
    public class NpgsqlQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        /// <summary>
        /// Represents options for Npgsql that can only be set by the service provider.
        /// </summary>
        [NotNull] readonly INpgsqlOptions _npgsqlOptions;

        /// <inheritdoc />
        public NpgsqlQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] INpgsqlOptions npgsqlOptions)
            : base(dependencies)
            => _npgsqlOptions = Check.NotNull(npgsqlOptions, nameof(npgsqlOptions));

        /// <inheritdoc />
        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new NpgsqlQuerySqlGenerator(
                Dependencies,
                Check.NotNull(selectExpression, nameof(selectExpression)),
                _npgsqlOptions.ReverseNullOrderingEnabled);
    }
}
