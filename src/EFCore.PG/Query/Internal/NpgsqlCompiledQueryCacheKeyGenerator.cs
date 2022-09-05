using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlCompiledQueryCacheKeyGenerator(
        CompiledQueryCacheKeyGeneratorDependencies dependencies,
        RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object GenerateCacheKey(Expression query, bool async)
        => new NpgsqlCompiledQueryCacheKey(
            GenerateCacheKeyCore(query, async),
            RelationalDependencies.ContextOptions.FindExtension<NpgsqlOptionsExtension>()?.ReverseNullOrdering ?? false);

    private struct NpgsqlCompiledQueryCacheKey
    {
        private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;
        private readonly bool _reverseNullOrdering;

        public NpgsqlCompiledQueryCacheKey(
            RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey, bool reverseNullOrdering)
        {
            _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
            _reverseNullOrdering = reverseNullOrdering;
        }

        public override bool Equals(object? obj)
            => !(obj is null)
                && obj is NpgsqlCompiledQueryCacheKey key
                && Equals(key);

        private bool Equals(NpgsqlCompiledQueryCacheKey other)
            => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
                && _reverseNullOrdering == other._reverseNullOrdering;

        public override int GetHashCode() => HashCode.Combine(_relationalCompiledQueryCacheKey, _reverseNullOrdering);
    }
}
