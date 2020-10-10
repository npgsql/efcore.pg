using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods on <see cref="DbContextOptionsBuilder"/> and <see cref="DbContextOptionsBuilder{T}"/>
    /// used to configure a <see cref="DbContext"/> to context to a PostgreSQL database with Npgsql.
    /// </summary>
    public static class NpgsqlDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Configures the context to connect to a PostgreSQL server with Npgsql, but without initially setting any
        /// <see cref="DbConnection" /> or connection string.
        /// </para>
        /// <para>
        /// The connection or connection string must be set before the <see cref="DbContext" /> is used to connect
        /// to a database. Set a connection using <see cref="RelationalDatabaseFacadeExtensions.SetDbConnection" />.
        /// Set a connection string using <see cref="RelationalDatabaseFacadeExtensions.SetConnectionString" />.
        /// </para>
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="npgsqlOptionsAction">An optional action to allow additional Npgsql-specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseNpgsql(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [CanBeNull] Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(GetOrCreateExtension(optionsBuilder));

            ConfigureWarnings(optionsBuilder);

            npgsqlOptionsAction?.Invoke(new NpgsqlDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        /// Configures the context to connect to a PostgreSQL database with Npgsql.
        /// </summary>
        /// <param name="optionsBuilder">A builder for setting options on the context.</param>
        /// <param name="connectionString">The connection string of the database to connect to.</param>
        /// <param name="npgsqlOptionsAction">An optional action to allow additional Npgsql-specific configuration.</param>
        /// <returns>
        /// The options builder so that further configuration can be chained.
        /// </returns>
        [NotNull]
        public static DbContextOptionsBuilder UseNpgsql(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = (NpgsqlOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            npgsqlOptionsAction?.Invoke(new NpgsqlDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        /// Configures the context to connect to a PostgreSQL database with Npgsql.
        /// </summary>
        /// <param name="optionsBuilder">A builder for setting options on the context.</param>
        /// <param name="connection">
        /// An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
        /// in the open state then EF will not open or close the connection. If the connection is in the closed
        /// state then EF will open and close the connection as needed.
        /// </param>
        /// <param name="npgsqlOptionsAction">An optional action to allow additional Npgsql-specific configuration.</param>
        /// <returns>
        /// The options builder so that further configuration can be chained.
        /// </returns>
        [NotNull]
        public static DbContextOptionsBuilder UseNpgsql(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var extension = (NpgsqlOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnection(connection);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            npgsqlOptionsAction?.Invoke(new NpgsqlDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        /// <para>
        /// Configures the context to connect to a PostgreSQL server with Npgsql, but without initially setting any
        /// <see cref="DbConnection" /> or connection string.
        /// </para>
        /// <para>
        /// The connection or connection string must be set before the <see cref="DbContext" /> is used to connect
        /// to a database. Set a connection using <see cref="RelationalDatabaseFacadeExtensions.SetDbConnection" />.
        /// Set a connection string using <see cref="RelationalDatabaseFacadeExtensions.SetConnectionString" />.
        /// </para>
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="npgsqlOptionsAction">An optional action to allow additional Npgsql-specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder<TContext> UseNpgsql<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [CanBeNull] Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseNpgsql(
                (DbContextOptionsBuilder)optionsBuilder, npgsqlOptionsAction);

        /// <summary>
        /// Configures the context to connect to a PostgreSQL database with Npgsql.
        /// </summary>
        /// <param name="optionsBuilder">A builder for setting options on the context.</param>
        /// <param name="connectionString">The connection string of the database to connect to.</param>
        /// <param name="npgsqlOptionsAction">An optional action to allow additional Npgsql-configuration.</param>
        /// <returns>
        /// The options builder so that further configuration can be chained.
        /// </returns>
        [NotNull]
        public static DbContextOptionsBuilder<TContext> UseNpgsql<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseNpgsql(
                (DbContextOptionsBuilder)optionsBuilder, connectionString, npgsqlOptionsAction);

        /// <summary>
        /// Configures the context to connect to a PostgreSQL database with Npgsql.
        /// </summary>
        /// <param name="optionsBuilder">A builder for setting options on the context.</param>
        /// <param name="connection">
        /// An existing <see cref="DbConnection" />to be used to connect to the database. If the connection is
        /// in the open state then EF will not open or close the connection. If the connection is in the closed
        /// state then EF will open and close the connection as needed.
        /// </param>
        /// <param name="npgsqlOptionsAction">An optional action to allow additional Npgsql-specific configuration.</param>
        /// <returns>
        /// The options builder so that further configuration can be chained.
        /// </returns>
        [NotNull]
        public static DbContextOptionsBuilder<TContext> UseNpgsql<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseNpgsql(
                (DbContextOptionsBuilder)optionsBuilder, connection, npgsqlOptionsAction);

        /// <summary>
        /// Returns an existing instance of <see cref="NpgsqlOptionsExtension"/>, or a new instance if one does not exist.
        /// </summary>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/> to search.</param>
        /// <returns>
        /// An existing instance of <see cref="NpgsqlOptionsExtension"/>, or a new instance if one does not exist.
        /// </returns>
        static NpgsqlOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.Options.FindExtension<NpgsqlOptionsExtension>() is NpgsqlOptionsExtension existing
                ? new NpgsqlOptionsExtension(existing)
                : new NpgsqlOptionsExtension();

        static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsExtension = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
                                       ?? new CoreOptionsExtension();

            coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
                coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                    RelationalEventId.AmbientTransactionWarning, WarningBehavior.Throw));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
        }
    }
}
