namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

public interface INpgsqlArrayConverter
{
    ValueConverter ElementConverter { get; }
}