// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

/// <inheritdoc />
public interface IMutablePostgresExtension : IReadOnlyPostgresExtension
{
    /// <summary>
    ///     Gets the <see cref="IMutableModel" /> in which this PostgreSQL extension is defined.
    /// </summary>
    new IMutableModel? Model { get; }

    // TODO: Schema?
}
