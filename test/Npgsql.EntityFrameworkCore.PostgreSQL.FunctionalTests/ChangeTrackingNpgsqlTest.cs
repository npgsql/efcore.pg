// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class ChangeTrackingNpgsqlTest : ChangeTrackingTestBase<NorthwindQueryNpgsqlFixture>
    {
        public ChangeTrackingNpgsqlTest(NorthwindQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public override void Multiple_entities_can_revert()
        { }
    }
}
