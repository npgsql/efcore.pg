using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public class NpgsqlExecutionStrategy : IExecutionStrategy
    {
        private ExecutionStrategyDependencies Dependencies { get; }

        public NpgsqlExecutionStrategy([NotNull] ExecutionStrategyDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        public virtual bool RetriesOnFailure => false;

        public virtual TResult Execute<TState, TResult>(
            TState state,
            Func<DbContext, TState, TResult> operation,
            Func<DbContext, TState, ExecutionResult<TResult>> verifySucceeded)
        {
            try
            {
#pragma warning disable EF1001
                return operation(Dependencies.CurrentDbContext.Context, state);
#pragma warning restore EF1001
            }
            catch (Exception ex)
            {
                if (ExecutionStrategy.CallOnWrappedException(ex, NpgsqlTransientExceptionDetector.ShouldRetryOn))
                {
                    throw new InvalidOperationException("An exception has been raised that is likely due to a transient failure.", ex);
                }

                throw;
            }
        }

        public virtual async Task<TResult> ExecuteAsync<TState, TResult>(
            TState state,
            Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
            Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>> verifySucceeded,
            CancellationToken cancellationToken)
        {
            try
            {
#pragma warning disable EF1001
                return await operation(Dependencies.CurrentDbContext.Context, state, cancellationToken);
#pragma warning restore EF1001
            }
            catch (Exception ex)
            {
                if (ExecutionStrategy.CallOnWrappedException(ex, NpgsqlTransientExceptionDetector.ShouldRetryOn))
                {
                    throw new InvalidOperationException("An exception has been raised that is likely due to a transient failure.", ex);
                }

                throw;
            }
        }
    }
}
