using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;

public class NpgsqlCreateDatabaseOperation : DatabaseOperation
{
    public virtual string Name { get; set; } = null!;
    public virtual string? Template { get; set; }
    public virtual string? Tablespace { get; set; }
}