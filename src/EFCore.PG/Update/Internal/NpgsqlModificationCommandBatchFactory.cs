using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal
{
    public class NpgsqlModificationCommandBatchFactory : IModificationCommandBatchFactory
    {
        readonly ModificationCommandBatchFactoryDependencies _dependencies;
        readonly IDbContextOptions _options;

        public NpgsqlModificationCommandBatchFactory(
            [NotNull] ModificationCommandBatchFactoryDependencies dependencies,
            [NotNull] IDbContextOptions options)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(options, nameof(options));

            _dependencies = dependencies;
            _options = options;
        }

        public virtual ModificationCommandBatch Create()
        {
            var optionsExtension = _options.Extensions.OfType<NpgsqlOptionsExtension>().FirstOrDefault();

            return new NpgsqlModificationCommandBatch(_dependencies, optionsExtension?.MaxBatchSize);
        }
    }
}
