using NodaTime;

namespace Microsoft.EntityFrameworkCore.TestModels.NodaTime;

public class NodaTimeTypes
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public int Id { get; set; }
    public Instant Instant { get; set; }
    public LocalDateTime LocalDateTime { get; set; }
    public ZonedDateTime ZonedDateTime { get; set; }
    public LocalDate LocalDate { get; set; }
    public LocalDate LocalDate2 { get; set; }
    public LocalTime LocalTime { get; set; }
    public OffsetTime OffsetTime { get; set; }
    public Period Period { get; set; } = null!;
    public Duration Duration { get; set; }
    public DateInterval DateInterval { get; set; } = null!;
    public NpgsqlRange<LocalDate> LocalDateRange { get; set; }
    public Interval Interval { get; set; }
    public NpgsqlRange<Instant> InstantRange { get; set; }
    public long Long { get; set; }

    public string TimeZoneId { get; set; } = null!;
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}

