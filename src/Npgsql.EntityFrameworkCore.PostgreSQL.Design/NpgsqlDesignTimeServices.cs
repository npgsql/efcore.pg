using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class NpgsqlDesignTimeServices
    {
        public virtual IServiceCollection ConfigureDesignTimeServices([NotNull] IServiceCollection serviceCollection)
            => serviceCollection
                .AddSingleton<IScaffoldingModelFactory, NpgsqlScaffoldingModelFactory>()
                .AddSingleton<IRelationalAnnotationProvider, NpgsqlAnnotationProvider>()
                .AddSingleton<IRelationalTypeMapper, NpgsqlTypeMapper>()
                .AddSingleton<IDatabaseModelFactory, NpgsqlDatabaseModelFactory>();
    }
}
