namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;

/// <summary>
///     This API supports the Entity Framework Core infrastructure and is not intended to be used
///     directly from your code. This API may change or be removed in future releases.
/// </summary>
public interface INpgsqlValueGeneratorCache : IValueGeneratorCache
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    NpgsqlSequenceValueGeneratorState GetOrAddSequenceState(
        IProperty property,
        IRelationalConnection connection);
}