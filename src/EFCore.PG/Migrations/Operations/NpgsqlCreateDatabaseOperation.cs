using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations
{
    public class NpgsqlCreateDatabaseOperation : DatabaseOperation
    {
        public virtual string Name { get;[param: NotNull] set; }
        [CanBeNull]
        public virtual string Template { get; set; }
        [CanBeNull]
        public virtual string Tablespace { get; set; }
    }
}
