// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceRelationshipsQueryNpgsqlTest : InheritanceRelationshipsQueryTestBase<InheritanceRelationshipsQueryNpgsqlFixture>
    {
        public InheritanceRelationshipsQueryNpgsqlTest(InheritanceRelationshipsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override void ClearLog() {}
    }
}
