// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public enum PostgresXlDistributeByStrategy
    {
        None = 0,
        Replication = 1,
        Roundrobin = 2,
        Column = 3,
    }
}
