// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class QueryNoClientEvalNpgsqlFixture : NorthwindQueryNpgsqlFixture
    {
        protected override void ConfigureOptions(NpgsqlDbContextOptionsBuilder NpgsqlDbContextOptionsBuilder)
            => NpgsqlDbContextOptionsBuilder.QueryClientEvaluationBehavior(QueryClientEvaluationBehavior.Throw);
    }
}
