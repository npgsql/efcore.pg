// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.BulkUpdates;

public class TPHFiltersInheritanceBulkUpdatesNpgsqlFixture : TPHInheritanceBulkUpdatesNpgsqlFixture
{
    public override bool EnableFilters
        => true;
}
