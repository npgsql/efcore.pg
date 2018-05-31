using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlRetryingExecutionStrategy : ExecutionStrategy
    {
        private readonly ICollection<string> _additionalErrorCodes;

        /// <summary>
        ///     Creates a new instance of <see cref="NpgsqlRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <remarks>
        ///     The default retry limit is 6, which means that the total amount of time spent before failing is about a minute.
        /// </remarks>
        public NpgsqlRetryingExecutionStrategy(
            [NotNull] DbContext context)
            : this(context, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NpgsqlRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        public NpgsqlRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies)
            : this(dependencies, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NpgsqlRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        public NpgsqlRetryingExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount)
            : this(context, maxRetryCount, DefaultMaxDelay, errorCodesToAdd: null)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NpgsqlRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        public NpgsqlRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies,
            int maxRetryCount)
            : this(dependencies, maxRetryCount, DefaultMaxDelay, errorCodesToAdd: null)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NpgsqlRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context"> The context on which the operations will be invoked. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        /// <param name="errorCodesToAdd"> Additional error codes that should be considered transient. </param>
        public NpgsqlRetryingExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<string> errorCodesToAdd)
            : base(context,
                maxRetryCount,
                maxRetryDelay)
        {
            _additionalErrorCodes = errorCodesToAdd;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NpgsqlRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing service dependencies. </param>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        /// <param name="errorCodesToAdd"> Additional SQL error numbers that should be considered transient. </param>
        public NpgsqlRetryingExecutionStrategy(
            [NotNull] ExecutionStrategyDependencies dependencies,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<string> errorCodesToAdd)
            : base(dependencies, maxRetryCount, maxRetryDelay)
        {
            _additionalErrorCodes = errorCodesToAdd;
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            if (_additionalErrorCodes != null)
            {
                // TODO: Unlike SqlException, which seems to also wrap various transport/IO errors
                // and expose them via error codes, we have NpgsqlException with an inner exception.
                // Would be good to provide a way to add these into the additional list.
                var postgresException = exception as PostgresException;
                if (postgresException != null)
                {
                    if (_additionalErrorCodes.Contains(postgresException.SqlState))
                        return true;
                }
            }

            return NpgsqlTransientExceptionDetector.ShouldRetryOn(exception);
        }
    }
}
