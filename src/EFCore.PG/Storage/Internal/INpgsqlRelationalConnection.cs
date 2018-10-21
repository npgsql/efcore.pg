using Microsoft.EntityFrameworkCore.Storage;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    public interface INpgsqlRelationalConnection : IRelationalConnection
    {
        INpgsqlRelationalConnection CreateMasterConnection();
    }
}
