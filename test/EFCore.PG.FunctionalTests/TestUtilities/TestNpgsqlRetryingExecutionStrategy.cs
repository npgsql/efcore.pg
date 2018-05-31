using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class TestNpgsqlRetryingExecutionStrategy : NpgsqlRetryingExecutionStrategy
    {
        private static readonly string[] _additionalSqlStates =
        {
            "XX000"
        };

        public TestNpgsqlRetryingExecutionStrategy()
            : base(
                new DbContext(new DbContextOptionsBuilder().UseNpgsql(TestEnvironment.DefaultConnection).Options),
                DefaultMaxRetryCount, DefaultMaxDelay, _additionalSqlStates)
        {
        }

        public TestNpgsqlRetryingExecutionStrategy(DbContext context)
            : base(context, DefaultMaxRetryCount, DefaultMaxDelay, _additionalSqlStates)
        {
        }

        public TestNpgsqlRetryingExecutionStrategy(DbContext context, TimeSpan maxDelay)
            : base(context, DefaultMaxRetryCount, maxDelay, _additionalSqlStates)
        {
        }

        public TestNpgsqlRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
            : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay, _additionalSqlStates)
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

        public new virtual TimeSpan? GetNextDelay(Exception lastException)
        {
            ExceptionsEncountered.Add(lastException);
            return base.GetNextDelay(lastException);
        }

        public new static bool Suspended
        {
            get => ExecutionStrategy.Suspended;
            set => ExecutionStrategy.Suspended = value;
        }
    }
}
