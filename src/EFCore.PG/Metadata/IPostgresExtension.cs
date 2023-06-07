// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

/// <inheritdoc />
public interface IPostgresExtension : IReadOnlyPostgresExtension
{
    /// <summary>
    ///     Gets the database schema that contains the extension.
    /// </summary>
    new IModel? Model { get; }
}
