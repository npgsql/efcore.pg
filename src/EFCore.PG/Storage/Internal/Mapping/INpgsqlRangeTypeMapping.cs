using System.Collections;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
///     A type mapping for a PostgreSQL range type.
/// </summary>
public interface INpgsqlRangeTypeMapping
{
    /// <summary>
    ///     Creates a comparer providing a total ordering for range key and foreign key values in the update pipeline.
    /// </summary>
    IComparer CreateCurrentValueComparer();
}
