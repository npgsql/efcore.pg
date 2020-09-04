using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations
{
    public class NpgsqlCreateDatabaseOperation : DatabaseOperation
    {
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string Template { get; [param: CanBeNull] set; }
        public virtual string Tablespace { get; [param: CanBeNull] set; }
    }
}
