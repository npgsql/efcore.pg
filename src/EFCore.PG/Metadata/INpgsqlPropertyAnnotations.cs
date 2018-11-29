using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public interface INpgsqlPropertyAnnotations : IRelationalPropertyAnnotations
    {
        [CanBeNull]
        NpgsqlValueGenerationStrategy? ValueGenerationStrategy { get; }

        [CanBeNull]
        string HiLoSequenceName { get; }

        [CanBeNull]
        string HiLoSequenceSchema { get; }

        [CanBeNull]
        ISequence FindHiLoSequence();

        [CanBeNull]
        string Comment { get; }
    }
}
