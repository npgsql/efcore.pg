using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore;

[MinimumPostgresVersion(18, 0)]
public class TemporalConstraintNodaTimeTest(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "TemporalConstraintNodaTimeTest";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => new NpgsqlTestStoreFactory(useConnectionString: true);

    [ConditionalFact]
    public Task Interval_temporal_constraints()
        => TemporalConstraints(
            new Interval(null, Instant.FromUtc(2025, 1, 1, 0, 0)),
            new Interval(Instant.FromUtc(2025, 1, 1, 0, 0), null));

    [ConditionalFact]
    public Task DateInterval_temporal_constraints()
        => TemporalConstraints(
            new DateInterval(new LocalDate(2025, 1, 1), new LocalDate(2025, 6, 30)),
            new DateInterval(new LocalDate(2025, 7, 1), new LocalDate(2025, 12, 31)));

    private async Task TemporalConstraints<T>(T firstPeriod, T secondPeriod)
    {
        var contextFactory = await InitializeNonSharedTest<TemporalContext<T>>(
            onConfiguring: b => new NpgsqlDbContextOptionsBuilder(b).SetPostgresVersion(18, 0).UseNodaTime(),
            addServices: s => s.AddEntityFrameworkNpgsqlNodaTime());

        await using var context = contextFactory.CreateDbContext();
        context.AddRange(
            new Room<T> { Id = 1, Validity = secondPeriod, RoomNumber = 101, Name = "Second" },
            new Room<T> { Id = 1, Validity = firstPeriod, RoomNumber = 101, Name = "First" });
        await context.SaveChangesAsync();

        context.AddRange(
            new Reservation<T> { RoomId = 1, Validity = secondPeriod, Description = "Second" },
            new Reservation<T> { RoomId = 1, Validity = firstPeriod, Description = "First" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rooms = await context.Set<Room<T>>().OrderBy(r => r.Validity).ToListAsync();
        Assert.Equal([firstPeriod, secondPeriod], rooms.Select(r => r.Validity));

        var reservations = await context.Set<Reservation<T>>().OrderBy(r => r.Validity).ToListAsync();
        Assert.Equal([firstPeriod, secondPeriod], reservations.Select(r => r.Validity));

        foreach (var room in rooms)
        {
            room.Name = "Updated";
        }

        foreach (var reservation in reservations)
        {
            reservation.Description = "Updated";
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        Assert.All(await context.Set<Room<T>>().ToListAsync(), r => Assert.Equal("Updated", r.Name));
        Assert.All(await context.Set<Reservation<T>>().ToListAsync(), r => Assert.Equal("Updated", r.Description));

        context.RemoveRange(context.Set<Reservation<T>>());
        await context.SaveChangesAsync();
        context.RemoveRange(context.Set<Room<T>>());
        await context.SaveChangesAsync();

        Assert.Empty(await context.Set<Room<T>>().ToListAsync());
        Assert.Empty(await context.Set<Reservation<T>>().ToListAsync());
    }

    private class TemporalContext<T>(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room<T>>(b =>
            {
                b.HasKey(r => new { r.Id, r.Validity }).WithoutOverlaps();
                b.HasAlternateKey(r => new { r.RoomNumber, r.Validity }).WithoutOverlaps();
            });

            modelBuilder.Entity<Reservation<T>>(b =>
            {
                b.HasKey(r => r.Id);
                b.HasOne<Room<T>>().WithMany()
                    .HasForeignKey(r => new { r.RoomId, r.Validity })
                    .WithPeriod();
            });
        }
    }

    private class Room<T>
    {
        public int Id { get; set; }
        public T Validity { get; set; } = default!;
        public int RoomNumber { get; set; }
        public string Name { get; set; } = null!;
    }

    private class Reservation<T>
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public T Validity { get; set; } = default!;
        public string Description { get; set; } = null!;
    }
}
