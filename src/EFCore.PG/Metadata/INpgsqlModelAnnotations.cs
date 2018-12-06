using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public interface INpgsqlModelAnnotations : IRelationalModelAnnotations
    {
        NpgsqlValueGenerationStrategy? ValueGenerationStrategy { get; }
        string HiLoSequenceName { get; }
        string HiLoSequenceSchema { get; }
        string DatabaseTemplate { get; }
        string Tablespace { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<PostgresExtension> PostgresExtensions { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<PostgresEnum> PostgresEnums { get; }

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<PostgresRange> PostgresRanges { get; }
    }
}
