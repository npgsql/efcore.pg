using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

/// <summary>
///     This API supports the Entity Framework Core infrastructure and is not intended to be used
///     directly from your code. This API may change or be removed in future releases.
/// </summary>
public class NpgsqlSequentialGuidValueGenerator : ValueGenerator<Guid>
{
    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <returns>The value to be assigned to a property.</returns>
    public override Guid Next(EntityEntry entry)
        => Guid.CreateVersion7();

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public override bool GeneratesTemporaryValues => false;
}
