using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public abstract class CommandInterceptionNpgsqlTestBase : CommandInterceptionTestBase
    {
        public CommandInterceptionNpgsqlTestBase(InterceptionNpgsqlFixtureBase fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/16701")]
        public override Task Intercept_non_query_one_app_and_one_injected_interceptor(bool async) => null;

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/16701")]
        public override Task Intercept_non_query_passively(bool async, bool inject) => null;

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/16701")]
        public override Task Intercept_non_query_to_mutate_command(bool async, bool inject) => null;

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/16701")]
        public override Task Intercept_non_query_to_replace_execution(bool async, bool inject) => null;

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/16701")]
        public override Task Intercept_non_query_with_explicitly_composed_app_interceptor(bool async) => null;

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/16701")]
        public override Task Intercept_non_query_with_two_injected_interceptors(bool async) => null;

        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/16701")]
        public override Task Intercept_non_query_to_replace_result(bool async, bool inject) => null;

        public abstract class InterceptionNpgsqlFixtureBase : InterceptionFixtureBase
        {
            protected override string StoreName => "CommandInterception";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkNpgsql(), injectedInterceptors);
        }

        public class CommandInterceptionNpgsqlTest
            : CommandInterceptionNpgsqlTestBase, IClassFixture<CommandInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
        {
            public CommandInterceptionNpgsqlTest(InterceptionNpgsqlFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => false;
            }
        }

        public class CommandInterceptionWithDiagnosticsNpgsqlTest
            : CommandInterceptionNpgsqlTestBase, IClassFixture<CommandInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture>
        {
            public CommandInterceptionWithDiagnosticsNpgsqlTest(InterceptionNpgsqlFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => true;
            }
        }
    }
}
