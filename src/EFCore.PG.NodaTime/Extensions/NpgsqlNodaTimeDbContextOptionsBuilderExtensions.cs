using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// NodaTime specific extension methods for <see cref="NpgsqlDbContextOptionsBuilder"/>.
/// </summary>
public static class NpgsqlNodaTimeDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configure NodaTime type mappings for Entity Framework.
    /// </summary>
    /// <returns> The options builder so that further configuration can be chained. </returns>
    public static NpgsqlDbContextOptionsBuilder UseNodaTime(
        this NpgsqlDbContextOptionsBuilder optionsBuilder)
    {
        Check.NotNull(optionsBuilder, nameof(optionsBuilder));

        // TODO: Global-only setup at the ADO.NET level for now, optionally allow per-connection?
#pragma warning disable CS0618 // NpgsqlConnection.GlobalTypeMapper is obsolete
        NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
#pragma warning restore CS0618

        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

        var extension = coreOptionsBuilder.Options.FindExtension<NpgsqlNodaTimeOptionsExtension>()
            ?? new NpgsqlNodaTimeOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}
