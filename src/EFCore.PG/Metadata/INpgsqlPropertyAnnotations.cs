using Microsoft.EntityFrameworkCore.Metadata;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public interface INpgsqlPropertyAnnotations : IRelationalPropertyAnnotations
    {
        NpgsqlValueGenerationStrategy? ValueGenerationStrategy { get; }
        string HiLoSequenceName { get; }
        string HiLoSequenceSchema { get; }
        ISequence FindHiLoSequence();
        string Comment { get; }
    }
}
