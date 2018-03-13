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

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure
{
    public class NpgsqlDbContextOptionsBuilder
        : RelationalDbContextOptionsBuilder<NpgsqlDbContextOptionsBuilder, NpgsqlOptionsExtension>
    {
        public NpgsqlDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }

        /// <summary>
        /// Connect to this database for administrative operations (creating/dropping databases).
        /// Defaults to 'postgres'.
        /// </summary>
        public virtual void UseAdminDatabase(string dbName) => WithOption(e => e.WithAdminDatabase(dbName));

        /// <summary>
        /// Appends NULLS FIRST to all ORDER BY clauses. This is important for the tests which were written
        /// for SQL Server. Note that to fully implement null-first ordering indexes also need to be generated
        /// accordingly, and since this isn't done this feature isn't publicly exposed.
        /// </summary>
        /// <param name="orderNullsFirst"></param>
        internal virtual void OrderNullsFirst(bool orderNullsFirst = true)
            => WithOption(e => e.WithNullFirstOrdering(orderNullsFirst));

        #region Authentication

        public virtual void ProvideClientCertificatesCallback(ProvideClientCertificatesCallback callback)
            => WithOption(e => e.WithProvideClientCertificatesCallback(callback));

        public virtual void RemoteCertificateValidationCallback(RemoteCertificateValidationCallback callback)
            => WithOption(e => e.WithRemoteCertificateValidationCallback(callback));

        #endregion Authentication

        #region Retrying execution strategy

        /// <summary>
        ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
        /// </summary>
        public virtual NpgsqlDbContextOptionsBuilder EnableRetryOnFailure()
            => ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c));

        /// <summary>
        ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
        /// </summary>
        public virtual NpgsqlDbContextOptionsBuilder EnableRetryOnFailure(int maxRetryCount)
            => ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, maxRetryCount));

        /// <summary>
        ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        /// <param name="errorCodesToAdd"> Additional error codes that should be considered transient. </param>
        public virtual NpgsqlDbContextOptionsBuilder EnableRetryOnFailure(
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [NotNull] ICollection<string> errorCodesToAdd)
            => ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c, maxRetryCount, maxRetryDelay, errorCodesToAdd));

        #endregion Retrying execution strategy
    }
}
