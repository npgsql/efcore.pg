namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

/// <summary>
///     Implemented by value converters over PostgreSQL arrays.
/// </summary>
public interface INpgsqlArrayConverter
{
    /// <summary>
    ///     The value converter for the element type of the array.
    /// </summary>
    ValueConverter ElementConverter { get; }
}
