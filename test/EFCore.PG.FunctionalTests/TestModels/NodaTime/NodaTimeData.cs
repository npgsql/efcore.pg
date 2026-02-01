using Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;
using NodaTime;

namespace Microsoft.EntityFrameworkCore.TestModels.NodaTime;

internal class NodaTimeData : ISetSource
{
    private static readonly Period DefaultPeriod = Period.FromYears(2018)
        + Period.FromMonths(4)
        + Period.FromDays(20)
        + Period.FromHours(10)
        + Period.FromMinutes(31)
        + Period.FromSeconds(23)
        + Period.FromMilliseconds(666);

    private IReadOnlyList<NodaTimeTypes> NodaTimeTypes { get; } = CreateNodaTimeTypes();

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(NodaTimeTypes))
        {
            return (IQueryable<TEntity>)NodaTimeTypes.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    public static IReadOnlyList<NodaTimeTypes> CreateNodaTimeTypes()
    {
        var localDateTime = new LocalDateTime(2018, 4, 20, 10, 31, 33, 666);
        var zonedDateTime = localDateTime.InUtc();
        var instant = zonedDateTime.ToInstant();
        var duration = Duration.FromMilliseconds(20)
            .Plus(Duration.FromSeconds(8))
            .Plus(Duration.FromMinutes(4))
            .Plus(Duration.FromHours(9))
            .Plus(Duration.FromDays(27));

        return new List<NodaTimeTypes>
        {
            new()
            {
                Id = 1,
                LocalDateTime = localDateTime,
                ZonedDateTime = zonedDateTime,
                Instant = instant,
                LocalDate = localDateTime.Date,
                LocalDate2 = localDateTime.Date + Period.FromDays(1),
                LocalTime = localDateTime.TimeOfDay,
                OffsetTime = new OffsetTime(new LocalTime(10, 31, 33, 666), Offset.Zero),
                Period = DefaultPeriod,
                Duration = duration,
                DateInterval = new DateInterval(localDateTime.Date, localDateTime.Date.PlusDays(4)), // inclusive
                LocalDateRange = new NpgsqlRange<LocalDate>(localDateTime.Date, localDateTime.Date.PlusDays(5)), // exclusive
                Interval = new Interval(instant, instant + Duration.FromDays(5)),
                InstantRange = new NpgsqlRange<Instant>(instant, true, instant + Duration.FromDays(5), false),
                Long = 1,
                TimeZoneId = "Europe/Berlin"
            }
        };
    }
}
