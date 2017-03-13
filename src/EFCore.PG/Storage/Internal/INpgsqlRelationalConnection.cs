namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public interface INpgsqlRelationalConnection : IRelationalConnection
    {
        INpgsqlRelationalConnection CreateMasterConnection();
    }
}
