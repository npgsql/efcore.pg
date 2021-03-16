using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal
{
    public class NpgsqlModificationCommandBatchFactory : IModificationCommandBatchFactory
    {
        private readonly ModificationCommandBatchFactoryDependencies _dependencies;
        private readonly IDbContextOptions _options;

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
