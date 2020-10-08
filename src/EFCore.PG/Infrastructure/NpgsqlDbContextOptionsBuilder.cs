using System;
using System.Collections.Generic;
using System.Net.Security;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure
{
    /// <summary>
    /// Allows for options specific to PostgreSQL to be configured for a <see cref="DbContext"/>.
    /// </summary>
    public class NpgsqlDbContextOptionsBuilder
        : RelationalDbContextOptionsBuilder<NpgsqlDbContextOptionsBuilder, NpgsqlOptionsExtension>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlDbContextOptionsBuilder"/> class.
        /// </summary>
        /// <param name="optionsBuilder"> The core options builder.</param>
        public NpgsqlDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder) {}

        /// <summary>
        /// Connect to this database for administrative operations (creating/dropping databases).
        /// </summary>
        /// <param name="dbName">The name of the database for administrative operations.</param>
        public virtual NpgsqlDbContextOptionsBuilder UseAdminDatabase([CanBeNull] string dbName)
            => WithOption(e => e.WithAdminDatabase(dbName));

        /// <summary>
        /// Configures the backend version to target.
        /// </summary>
        /// <param name="postgresVersion">The backend version to target.</param>
        public virtual NpgsqlDbContextOptionsBuilder SetPostgresVersion([CanBeNull] Version postgresVersion)
            => WithOption(e => e.WithPostgresVersion(postgresVersion));

        /// <summary>
        /// Configures the backend version to target.
        /// </summary>
        /// <param name="major">The PostgreSQL major version to target.</param>
        /// <param name="minor">The PostgreSQL minor version to target.</param>
        public virtual NpgsqlDbContextOptionsBuilder SetPostgresVersion(int major, int minor)
            => SetPostgresVersion(new Version(major, minor));

        /// <summary>
        /// Maps a user-defined PostgreSQL range type for use.
        /// </summary>
        /// <typeparam name="TSubtype">
        /// The CLR type of the range's subtype (or element).
        /// The actual mapped type will be an <see cref="NpgsqlRange{T}"/> over this type.
        /// </typeparam>
        /// <param name="rangeName">The name of the PostgreSQL range type to be mapped.</param>
        /// <param name="schemaName">The name of the PostgreSQL schema in which the range is defined.</param>
        /// <param name="subtypeName">
        /// Optionally, the name of the range's PostgreSQL subtype (or element).
        /// This is usually not needed - the subtype will be inferred based on <typeparamref name="TSubtype"/>.
        /// </param>
        /// <example>
        /// To map a range of PostgreSQL real, use the following:
        /// <code>NpgsqlTypeMappingSource.MapRange{float}("floatrange");</code>
        /// </example>
        public virtual NpgsqlDbContextOptionsBuilder MapRange<TSubtype>(
            [NotNull] string rangeName,
            [CanBeNull] string schemaName = null,
            [CanBeNull] string subtypeName = null)
            => MapRange(rangeName, typeof(TSubtype), schemaName, subtypeName);

        /// <summary>
        /// Maps a user-defined PostgreSQL range type for use.
        /// </summary>
        /// <param name="rangeName">The name of the PostgreSQL range type to be mapped.</param>
        /// <param name="schemaName">The name of the PostgreSQL schema in which the range is defined.</param>
        /// <param name="subtypeClrType">
        /// The CLR type of the range's subtype (or element).
        /// The actual mapped type will be an <see cref="NpgsqlRange{T}"/> over this type.
        /// </param>
        /// <param name="subtypeName">
        /// Optionally, the name of the range's PostgreSQL subtype (or element).
        /// This is usually not needed - the subtype will be inferred based on <paramref name="subtypeClrType"/>.
        /// </param>
        /// <example>
        /// To map a range of PostgreSQL real, use the following:
        /// <code>NpgsqlTypeMappingSource.MapRange("floatrange", typeof(float));</code>
        /// </example>
        public virtual NpgsqlDbContextOptionsBuilder MapRange(
            [NotNull] string rangeName,
            [NotNull] Type subtypeClrType,
            [CanBeNull] string schemaName = null,
            [CanBeNull] string subtypeName = null)
            => WithOption(e => e.WithUserRangeDefinition(rangeName, schemaName, subtypeClrType, subtypeName));

        /// <summary>
        /// Appends NULLS FIRST to all ORDER BY clauses. This is important for the tests which were written
        /// for SQL Server. Note that to fully implement null-first ordering indexes also need to be generated
        /// accordingly, and since this isn't done this feature isn't publicly exposed.
        /// </summary>
        /// <param name="reverseNullOrdering">True to enable reverse null ordering; otherwise, false.</param>
        internal virtual NpgsqlDbContextOptionsBuilder ReverseNullOrdering(bool reverseNullOrdering = true)
            => WithOption(e => e.WithReverseNullOrdering(reverseNullOrdering));

        #region Authentication

        /// <summary>
        /// Configures the <see cref="DbContext"/> to use the specified <see cref="ProvideClientCertificatesCallback"/>.
        /// </summary>
        /// <param name="callback">The callback to use.</param>
        public virtual NpgsqlDbContextOptionsBuilder ProvideClientCertificatesCallback(
            [CanBeNull] ProvideClientCertificatesCallback callback)
            => WithOption(e => e.WithProvideClientCertificatesCallback(callback));

        /// <summary>
        /// Configures the <see cref="DbContext"/> to use the specified <see cref="RemoteCertificateValidationCallback"/>.
        /// </summary>
        /// <param name="callback">The callback to use.</param>
        public virtual NpgsqlDbContextOptionsBuilder RemoteCertificateValidationCallback(
            [CanBeNull] RemoteCertificateValidationCallback callback)
            => WithOption(e => e.WithRemoteCertificateValidationCallback(callback));

        /// <summary>
        /// Configures the <see cref="DbContext"/> to use the specified <see cref="ProvidePasswordCallback"/>.
        /// </summary>
        /// <param name="callback">The callback to use.</param>
        public virtual NpgsqlDbContextOptionsBuilder ProvidePasswordCallback(
            [CanBeNull] ProvidePasswordCallback callback)
            => WithOption(e => e.WithProvidePasswordCallback(callback));

        #endregion Authentication

        #region Retrying execution strategy

        /// <summary>
        /// Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="NpgsqlDbContextOptionsBuilder"/> configured to use
        /// the default retrying <see cref="IExecutionStrategy" />.
        /// </returns>
        [NotNull]
        public virtual NpgsqlDbContextOptionsBuilder EnableRetryOnFailure()
            => ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c));

        /// <summary>
        /// Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="NpgsqlDbContextOptionsBuilder"/> with the specified parameters.
        /// </returns>
        [NotNull]
        public virtual NpgsqlDbContextOptionsBuilder EnableRetryOnFailure(int maxRetryCount)
            => ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, maxRetryCount));

        /// <summary>
        /// Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
        /// <param name="maxRetryDelay">The maximum delay between retries.</param>
        /// <param name="errorCodesToAdd">Additional error codes that should be considered transient.</param>
        /// <returns>
        /// An instance of <see cref="NpgsqlDbContextOptionsBuilder"/> with the specified parameters.
        /// </returns>
        [NotNull]
        public virtual NpgsqlDbContextOptionsBuilder EnableRetryOnFailure(
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<string> errorCodesToAdd)
            => ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, maxRetryCount, maxRetryDelay, errorCodesToAdd));

        #endregion Retrying execution strategy
    }
}
