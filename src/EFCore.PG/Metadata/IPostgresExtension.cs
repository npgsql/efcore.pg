using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public interface IPostgresExtension
    {
        IAnnotatable Annotatable { get; }
        string Name { get; }
        string Schema { get; }
        string Version { get; }
    }
}
