using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class TestNpgsqlRetryingExecutionStrategy : NpgsqlRetryingExecutionStrategy
    {
        const bool ErrorNumberDebugMode = false;

        static readonly string[] AdditionalSqlStates =
        {
            "XX000"
        };

        public TestNpgsqlRetryingExecutionStrategy()
            : base(
                new DbContext(
                    new DbContextOptionsBuilder()
                        .EnableServiceProviderCaching(false)
                        .UseNpgsql(TestEnvironment.DefaultConnection).Options),
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

#pragma warning disable 162
            if (ErrorNumberDebugMode &&
                exception is PostgresException postgresException)
            {
                var message = $"Didn't retry on {postgresException.SqlState}";
                throw new InvalidOperationException(message, exception);
            }
#pragma warning restore 162

            return exception is InvalidOperationException invalidOperationException
                   && invalidOperationException.Message == "Internal .Net Framework Data Provider error 6.";
        }

        public new virtual TimeSpan? GetNextDelay(Exception lastException)
        {
            ExceptionsEncountered.Add(lastException);
            return base.GetNextDelay(lastException);
        }
    }
}
