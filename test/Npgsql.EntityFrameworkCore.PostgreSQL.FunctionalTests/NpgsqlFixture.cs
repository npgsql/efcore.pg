// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class NpgsqlFixture
    {
        public readonly IServiceProvider ServiceProvider;

        public NpgsqlFixture()
        {
            ServiceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .BuildServiceProvider();
        }
    }
}
