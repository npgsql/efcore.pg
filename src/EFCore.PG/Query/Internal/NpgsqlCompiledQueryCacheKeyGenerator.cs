using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
    {
        public NpgsqlCompiledQueryCacheKeyGenerator(
            [NotNull] CompiledQueryCacheKeyGeneratorDependencies dependencies,
            [NotNull] RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        public override object GenerateCacheKey(Expression query, bool async)
            => new NpgsqlCompiledQueryCacheKey(
                GenerateCacheKeyCore(query, async),
                RelationalDependencies.ContextOptions.FindExtension<NpgsqlOptionsExtension>()?.ReverseNullOrdering ?? false);

        struct NpgsqlCompiledQueryCacheKey
        {
            readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;
            readonly bool _reverseNullOrdering;

            public NpgsqlCompiledQueryCacheKey(
                RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey, bool reverseNullOrdering)
            {
                _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
                _reverseNullOrdering = reverseNullOrdering;
            }

            public override bool Equals(object obj)
                => !(obj is null)
                   && obj is NpgsqlCompiledQueryCacheKey key
                   && Equals(key);

            bool Equals(NpgsqlCompiledQueryCacheKey other)
                => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
                   && _reverseNullOrdering == other._reverseNullOrdering;

            public override int GetHashCode() => HashCode.Combine(_relationalCompiledQueryCacheKey, _reverseNullOrdering);
        }
    }
}
