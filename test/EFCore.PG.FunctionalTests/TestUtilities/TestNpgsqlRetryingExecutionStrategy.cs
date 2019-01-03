using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class TestNpgsqlRetryingExecutionStrategy : NpgsqlRetryingExecutionStrategy
    {
        static readonly string[] AdditionalSqlStates =
        {
            "XX000"
        };

        // ReSharper disable once UnusedMember.Global
        public TestNpgsqlRetryingExecutionStrategy()
            : base(
                new DbContext(new DbContextOptionsBuilder().UseNpgsql(TestEnvironment.DefaultConnection).Options),
                DefaultMaxRetryCount, DefaultMaxDelay, AdditionalSqlStates)
        {
        }

        public TestNpgsqlRetryingExecutionStrategy(DbContext context)
            : base(context, DefaultMaxRetryCount, DefaultMaxDelay, AdditionalSqlStates)
        {
        }

        public TestNpgsqlRetryingExecutionStrategy(DbContext context, TimeSpan maxDelay)
            : base(context, DefaultMaxRetryCount, maxDelay, AdditionalSqlStates)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public TestNpgsqlRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
            : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay, AdditionalSqlStates)
        {
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            if (base.ShouldRetryOn(exception))
            {
                return true;
            }

            if (exception is PostgresException postgresException)
            {
                var message = $"Didn't retry on {postgresException.SqlState}";
                throw new InvalidOperationException(message, exception);
            }

            return false;
        }

        // ReSharper disable once UnusedMember.Global
        public new virtual TimeSpan? GetNextDelay(Exception lastException)
        {
            ExceptionsEncountered.Add(lastException);
            return base.GetNextDelay(lastException);
        }

        public new static bool Suspended
        {
            // ReSharper disable once UnusedMember.Global
            get => ExecutionStrategy.Suspended;
            set => ExecutionStrategy.Suspended = value;
        }
    }
}
