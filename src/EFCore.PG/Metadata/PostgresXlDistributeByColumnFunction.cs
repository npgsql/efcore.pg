// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public enum PostgresXlDistributeByColumnFunction
    {
        None = 0,
        Hash = 1,
        Modulo = 2,
    }
}
