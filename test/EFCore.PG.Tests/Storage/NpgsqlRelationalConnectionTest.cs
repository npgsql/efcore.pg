using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage;

public class NpgsqlRelationalConnectionTest
{
    [ConditionalFact]
    public void CloneWith_with_connection_and_connection_string()
    {
        var services = NpgsqlTestHelpers.Instance.CreateContextServices(
            new DbContextOptionsBuilder()
                .UseNpgsql("Host=localhost;Database=DummyDatabase")
                .Options);

        var relationalConnection = (NpgsqlRelationalConnection)services.GetRequiredService<IRelationalConnection>();

        var clone = relationalConnection.CloneWith("Host=localhost;Database=DummyDatabase;Application Name=foo");

        Assert.Equal("Host=localhost;Database=DummyDatabase;Application Name=foo", clone.ConnectionString);
    }
}