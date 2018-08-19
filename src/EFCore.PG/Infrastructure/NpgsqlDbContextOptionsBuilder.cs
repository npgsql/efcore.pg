#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using System.Collections.Generic;
using System.Net.Security;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

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
        /// Configures the <see cref="DbContext"/> to use the specified <see cref="NpgsqlEntityFrameworkPlugin"/>.
        /// </summary>
        /// <param name="plugin">The plugin to configure.</param>
        public virtual void UsePlugin([NotNull] NpgsqlEntityFrameworkPlugin plugin)
            => WithOption(e => e.WithPlugin(plugin));

        /// <summary>
        /// Connect to this database for administrative operations (creating/dropping databases).
        /// </summary>
        /// <param name="dbName">The name of the database for administrative operations.</param>
        public virtual void UseAdminDatabase([CanBeNull] string dbName)
            => WithOption(e => e.WithAdminDatabase(dbName));

        /// <summary>
        /// Configures the backend version to target.
        /// </summary>
        /// <param name="postgresVersion">The backend version to target.</param>
        public virtual void SetPostgresVersion([CanBeNull] Version postgresVersion)
            => WithOption(e => e.WithPostgresVersion(postgresVersion));

        /// <summary>
        /// Maps a user-defined PostgreSQL range type for use with all DbContexts.
        /// </summary>
        /// <param name="rangeName">The name of the PostgreSQL range type to be mapped.</param>
        /// <param name="elementClrType">
        ///     The CLR type of the range's element. The actual mapped type will be an <code>NpgsqlRange{T}</code> over
        ///     this type.
        /// </param>
        /// <param name="subtypeName">
        ///     Optionally, the name of the range's subtype or element. This is usually not needed - the subtype will be
        ///     inferred based on <paramref name="elementClrType"/></param>
        /// <example>
        /// To map a range of PostgreSQL real, use the following:
        /// <code>NpgsqlTypeMappingSource.MapRange("real", typeof(float));</code>
        /// </example>
        public virtual void MapRange([NotNull] string rangeName, [NotNull] Type elementClrType,
            string subtypeName = null)
            => WithOption(e => e.WithRangeMapping(rangeName, elementClrType, subtypeName));

        /// <summary>
        /// Appends NULLS FIRST to all ORDER BY clauses. This is important for the tests which were written
        /// for SQL Server. Note that to fully implement null-first ordering indexes also need to be generated
        /// accordingly, and since this isn't done this feature isn't publicly exposed.
        /// </summary>
        /// <param name="reverseNullOrdering">True to enable reverse null ordering; otherwise, false.</param>
        internal virtual void ReverseNullOrdering(bool reverseNullOrdering = true)
            => WithOption(e => e.WithReverseNullOrdering(reverseNullOrdering));

        #region Authentication

        /// <summary>
        /// Configures the <see cref="DbContext"/> to use the specified <see cref="ProvideClientCertificatesCallback"/>.
        /// </summary>
        /// <param name="callback">The callback to use.</param>
        public virtual void ProvideClientCertificatesCallback([CanBeNull] ProvideClientCertificatesCallback callback)
            => WithOption(e => e.WithProvideClientCertificatesCallback(callback));

        /// <summary>
        /// Configures the <see cref="DbContext"/> to use the specified <see cref="RemoteCertificateValidationCallback"/>.
        /// </summary>
        /// <param name="callback">The callback to use.</param>
        public virtual void RemoteCertificateValidationCallback([CanBeNull] RemoteCertificateValidationCallback callback)
            => WithOption(e => e.WithRemoteCertificateValidationCallback(callback));

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
            [NotNull] ICollection<string> errorCodesToAdd)
            => ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, maxRetryCount, maxRetryDelay, errorCodesToAdd));

        #endregion Retrying execution strategy
    }
}
