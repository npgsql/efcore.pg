using JetBrains.Annotations;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NodaTimeDbContextOptionsExtensions
    {
        public static NpgsqlDbContextOptionsBuilder UseNodaTime(
            [NotNull] this NpgsqlDbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            // TODO: Global-only setup at the ADO.NET level for now, optionally allow per-connection?
            NpgsqlConnection.GlobalTypeMapper.UseNodatime();

            optionsBuilder.UsePlugin(new NodaTimePlugin());

            return optionsBuilder;
        }
    }
}
