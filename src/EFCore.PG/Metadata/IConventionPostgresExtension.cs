// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

/// <inheritdoc />
public interface IConventionPostgresExtension : IReadOnlyPostgresExtension
{
    /// <summary>
    ///     Gets the <see cref="IConventionModel" /> in which this PostgreSQL extension is defined.
    /// </summary>
    new IConventionModel? Model { get; }

    /// <summary>
    ///     Gets the configuration source for this <see cref="IConventionSequence" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IConventionSequence" />.</returns>
    ConfigurationSource GetConfigurationSource();

    // TODO: Schema, version?
}
