// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Extensions;

internal static class NpgsqlConnectionExtensions
{
    internal static bool IsCockroachDb(this NpgsqlConnection connection)
    {
        return connection.PostgresParameters.TryGetValue("crdb_version", out var _);
    }
}
