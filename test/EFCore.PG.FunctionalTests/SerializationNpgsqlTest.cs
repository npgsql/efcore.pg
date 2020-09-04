using Microsoft.EntityFrameworkCore;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class SerializationNpgsqlTest : SerializationTestBase<F1NpgsqlFixture>
    {
        public SerializationNpgsqlTest(F1NpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
