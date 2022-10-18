// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

/// <summary>
///     Represents a PostgreSQL extension in the model.
/// </summary>
public interface IReadOnlyPostgresExtension
{
    /// <summary>
    ///     Gets the name of the extension in the database.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the database schema that contains the extension.
    /// </summary>
    string? Schema { get; }

    /// <summary>
    ///     Gets the extension version.
    /// </summary>
    string? Version { get; }

    /// <summary>
    ///     Gets the model in which this extension is defined.
    /// </summary>
    IReadOnlyModel? Model { get; }

    /// <summary>
    ///     <para>
    ///         Creates a human-readable representation of the given metadata.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the returned string.
    ///         It is designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    /// <param name="options">Options for generating the string.</param>
    /// <param name="indent">The number of indent spaces to use before each new line.</param>
    /// <returns>A human-readable representation.</returns>
    string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        builder
            .Append(indentString)
            .Append("PG Extension: ");

        if (Schema is not null)
        {
            builder.Append(Schema).Append('.');
        }

        builder.Append(Name);

        if (Version is not null)
        {
            builder.Append(" (Version=").Append(Version).Append(')');
        }

        return builder.ToString();
    }
}
