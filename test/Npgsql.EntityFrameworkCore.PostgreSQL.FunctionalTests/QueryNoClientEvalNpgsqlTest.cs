// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class QueryNoClientEvalNpgsqlTest : QueryNoClientEvalTestBase<QueryNoClientEvalNpgsqlFixture>
    {
        public QueryNoClientEvalNpgsqlTest(QueryNoClientEvalNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
