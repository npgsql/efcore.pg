using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;

public class NpgsqlModificationCommandBatchFactory : IModificationCommandBatchFactory
{
    private const int DefaultMaxBatchSize = 1000;

    private readonly ModificationCommandBatchFactoryDependencies _dependencies;
    private readonly int _maxBatchSize;

    public NpgsqlModificationCommandBatchFactory(
        ModificationCommandBatchFactoryDependencies dependencies,
        IDbContextOptions options)
    {
        Check.NotNull(dependencies, nameof(dependencies));
        Check.NotNull(options, nameof(options));

        _dependencies = dependencies;

        _maxBatchSize = options.Extensions.OfType<NpgsqlOptionsExtension>().FirstOrDefault()?.MaxBatchSize ?? DefaultMaxBatchSize;

        if (_maxBatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RelationalOptionsExtension.MaxBatchSize), RelationalStrings.InvalidMaxBatchSize(_maxBatchSize));
        }
    }

    public virtual ModificationCommandBatch Create()
        => new NpgsqlModificationCommandBatch(_dependencies, _maxBatchSize);
}
