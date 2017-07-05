// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlQueryNpgsqlTest : FromSqlQueryTestBase<NorthwindQueryNpgsqlFixture>
    {
        public FromSqlQueryNpgsqlTest(NorthwindQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/6563")]
        public override void Bad_data_error_handling_invalid_cast_key() {}
        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/6563")]
        public override void Bad_data_error_handling_invalid_cast() {}
        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/6563")]
        public override void Bad_data_error_handling_invalid_cast_projection() {}
        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/6563")]
        public override void Bad_data_error_handling_invalid_cast_no_tracking() {}

        [Fact(Skip="https://github.com/aspnet/EntityFramework/issues/3548")]
        public override void From_sql_queryable_simple_projection_composed() {}

        protected override DbParameter CreateDbParameter(string name, object value)
            => new NpgsqlParameter
            {
                ParameterName = name,
                Value = value
            };
    }
}
