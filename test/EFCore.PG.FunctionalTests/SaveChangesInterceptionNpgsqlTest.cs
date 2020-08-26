// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public abstract class SaveChangesInterceptionNpgsqlTestBase : SaveChangesInterceptionTestBase
    {
        protected SaveChangesInterceptionNpgsqlTestBase(InterceptionNpgsqlFixtureBase fixture)
            : base(fixture)
        {
        }

        public abstract class InterceptionNpgsqlFixtureBase : InterceptionFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkNpgsql(), injectedInterceptors);
        }

        public class SaveChangesInterceptionNpgsqlTest
            : SaveChangesInterceptionNpgsqlTestBase, IClassFixture<SaveChangesInterceptionNpgsqlTest.InterceptionNpgsqlFixture>
        {
            public SaveChangesInterceptionNpgsqlTest(InterceptionNpgsqlFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
            {
                protected override string StoreName => "SaveChangesInterception";

                protected override bool ShouldSubscribeToDiagnosticListener => false;

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                {
                    new NpgsqlDbContextOptionsBuilder(base.AddOptions(builder))
                        .ExecutionStrategy(d => new NpgsqlExecutionStrategy(d));
                    return builder;
                }
            }
        }

        public class SaveChangesInterceptionWithDiagnosticsNpgsqlTest
            : SaveChangesInterceptionNpgsqlTestBase,
                IClassFixture<SaveChangesInterceptionWithDiagnosticsNpgsqlTest.InterceptionNpgsqlFixture>
        {
            public SaveChangesInterceptionWithDiagnosticsNpgsqlTest(InterceptionNpgsqlFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionNpgsqlFixture : InterceptionNpgsqlFixtureBase
            {
                protected override string StoreName => "SaveChangesInterceptionWithDiagnostics";

                protected override bool ShouldSubscribeToDiagnosticListener => true;

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                {
                    new NpgsqlDbContextOptionsBuilder(base.AddOptions(builder))
                        .ExecutionStrategy(d => new NpgsqlExecutionStrategy(d));
                    return builder;
                }
            }
        }
    }
}
