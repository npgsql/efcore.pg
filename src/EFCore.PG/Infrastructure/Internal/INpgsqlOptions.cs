using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal
{
    public interface INpgsqlOptions : ISingletonOptions
    {
        /// <summary>
        ///     Reflects the option set by <see cref="NpgsqlDbContextOptionsBuilder.ReverseNullOrdering" />.
        /// </summary>
        bool ReverseNullOrderingEnabled { get; }
    }
}
