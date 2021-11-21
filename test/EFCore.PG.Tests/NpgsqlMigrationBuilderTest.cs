namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NpgsqlMigrationBuilderTest
{
    [Fact]
    public void IsNpgsql_when_using_Npgsql()
    {
        var migrationBuilder = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");
        Assert.True(migrationBuilder.IsNpgsql());
    }

    [Fact]
    public void Not_IsNpgsql_when_using_different_provider()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.InMemory");
        Assert.False(migrationBuilder.IsNpgsql());
    }
}
