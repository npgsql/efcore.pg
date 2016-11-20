using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.FunctionalTests
{
    public class NpgsqlDatabaseModelFixture : IDisposable
    {
        public NpgsqlDatabaseModelFixture()
        {
            TestStore = NpgsqlTestStore.CreateScratch();
        }

        public DatabaseModel CreateModel(string createSql, TableSelectionSet selection = null, ILogger logger = null)
        {
            TestStore.ExecuteNonQuery("DROP SCHEMA public CASCADE; CREATE SCHEMA public; " + createSql);

            return new NpgsqlDatabaseModelFactory(new TestLoggerFactory(logger))
                .Create(TestStore.ConnectionString, selection ?? TableSelectionSet.All);
        }

        public NpgsqlTestStore TestStore { get; }

        public void ExecuteNonQuery(string sql) => TestStore.ExecuteNonQuery(sql);

        public void Dispose() => TestStore.Dispose();

        class TestLoggerFactory : ILoggerFactory
        {
            readonly ILogger _logger;

            public TestLoggerFactory(ILogger logger)
            {
                _logger = logger ?? new TestLogger();
            }

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose()
            {
            }
        }

        class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();

            public void Dispose()
            {
            }
        }

        public class TestLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => Items.Add(new { logLevel, eventId, state, exception });

            public bool IsEnabled(LogLevel logLevel) => true;

            public ICollection<dynamic> Items = new List<dynamic>();
        }
    }
}
