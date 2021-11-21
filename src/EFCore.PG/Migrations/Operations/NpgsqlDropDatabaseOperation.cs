namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;

public class NpgsqlDropDatabaseOperation : MigrationOperation
{
    public virtual string Name { get; set; } = null!;
}