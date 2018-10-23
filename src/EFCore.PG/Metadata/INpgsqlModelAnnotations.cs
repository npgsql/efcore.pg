using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public interface INpgsqlModelAnnotations : IRelationalModelAnnotations
    {
        NpgsqlValueGenerationStrategy? ValueGenerationStrategy { get; }
        string HiLoSequenceName { get; }
        string HiLoSequenceSchema { get; }
        IReadOnlyList<IPostgresExtension> PostgresExtensions { get; }
        string DatabaseTemplate { get; }
        string Tablespace { get; }
    }
}
