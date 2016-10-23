using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Tests
{
    public class NpgsqlTestHelpers : RelationalTestHelpers
    {
        protected NpgsqlTestHelpers()
        {
        }

        public new static NpgsqlTestHelpers Instance { get; } = new NpgsqlTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkNpgsql();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(new NpgsqlConnection("Database=DummyDatabase"));
    }
}
