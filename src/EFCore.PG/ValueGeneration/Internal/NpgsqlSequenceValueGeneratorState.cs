namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;

/// <summary>
///     This API supports the Entity Framework Core infrastructure and is not intended to be used
///     directly from your code. This API may change or be removed in future releases.
/// </summary>
public class NpgsqlSequenceValueGeneratorState : HiLoValueGeneratorState
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public NpgsqlSequenceValueGeneratorState(ISequence sequence)
        : base(Check.NotNull(sequence, nameof(sequence)).IncrementBy)
    {
        Sequence = sequence;
    }

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public virtual ISequence Sequence { get; }
}