using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindCaseInsensitiveFixture<TModelCustomizer> : NorthwindQueryNpgsqlFixture<TModelCustomizer>
        where TModelCustomizer : IModelCustomizer, new()
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new NpgsqlDbContextOptionsBuilder(builder).SetCaseInsensitive(true);

            return base.AddOptions(builder);
        }
    }
}
