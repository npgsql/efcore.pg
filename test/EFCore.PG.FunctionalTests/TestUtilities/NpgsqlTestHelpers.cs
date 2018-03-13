using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class NpgsqlTestHelpers : TestHelpers
    {
        protected NpgsqlTestHelpers()
        {
        }

        public static NpgsqlTestHelpers Instance { get; } = new NpgsqlTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkNpgsql();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(new NpgsqlConnection("Database=DummyDatabase"));
    }
}
