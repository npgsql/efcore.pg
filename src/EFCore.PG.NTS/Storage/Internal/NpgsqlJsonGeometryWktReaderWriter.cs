// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

// ReSharper disable once CheckNamespace
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     Reads and writes JSON using the well-known-text format for <see cref="Geometry" /> values.
/// </summary>
public sealed class NpgsqlJsonGeometryWktReaderWriter : JsonValueReaderWriter<Geometry>
{
    private static readonly WKTReader WktReader = new();

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static NpgsqlJsonGeometryWktReaderWriter Instance { get; } = new();

    private NpgsqlJsonGeometryWktReaderWriter()
    {
    }

    /// <inheritdoc />
    public override Geometry FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => WktReader.Read(manager.CurrentReader.GetString());

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, Geometry value)
        => writer.WriteStringValue(value.ToText());
}
