using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class LoggingNpgsqlTest : LoggingRelationalTestBase<NpgsqlDbContextOptionsBuilder, NpgsqlOptionsExtension>
    {
        /*
        [Fact]
        public void Logs_context_initialization_admin_database()
        {
            Assert.Equal(
                ExpectedMessage("AdminDatabase " + DefaultOptions),
                ActualMessage(CreateOptionsBuilder(b => ((NpgsqlDbContextOptionsBuilder)b).UseAdminDatabase("foo"))));
        }*/

        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            Action<RelationalDbContextOptionsBuilder<NpgsqlDbContextOptionsBuilder, NpgsqlOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder().UseNpgsql("Data Source=LoggingNpgsqlTest.db", relationalAction);

        protected override string ProviderName => "Npgsql.EntityFrameworkCore.PostgreSQL";
    }
}
