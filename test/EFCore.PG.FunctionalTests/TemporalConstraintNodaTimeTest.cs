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

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Converted_range_keys(bool temporal)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: modelBuilder =>
            {
                modelBuilder.Entity<Room<long>>(b =>
                {
                    b.ToTable("IntervalRooms");
                    b.HasKey(r => new { r.Id, r.Validity }).WithoutOverlaps(temporal);
                    b.Property(r => r.Validity)
                        .HasConversion(
                            v => new Interval(Instant.FromUnixTimeSeconds(v), Instant.FromUnixTimeSeconds(v + 1)),
                            v => v.Start.ToUnixTimeSeconds())
                        .ValueGeneratedNever();
                });

                modelBuilder.Entity<Room<int>>(b =>
                {
                    b.ToTable("DateIntervalRooms");
                    b.HasKey(r => new { r.Id, r.Validity }).WithoutOverlaps(temporal);
                    b.Property(r => r.Validity)
                        .HasConversion(
                            v => new DateInterval(new LocalDate(2025, 1, 1).PlusDays(v), new LocalDate(2025, 1, 1).PlusDays(v)),
                            v => v.Start.DayOfYear - 1)
                        .ValueGeneratedNever();
                });
            },
            onConfiguring: b => new NpgsqlDbContextOptionsBuilder(b).SetPostgresVersion(18, 0).UseNodaTime(),
            addServices: s => s.AddEntityFrameworkNpgsqlNodaTime());

        await using var context = contextFactory.CreateDbContext();
        context.AddRange(
            new Room<long> { Validity = 2, Name = "Second" },
            new Room<long> { Validity = 1, Name = "First" },
            new Room<int> { Validity = 2, Name = "Second" },
            new Room<int> { Validity = 1, Name = "First" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        Assert.Equal([1L, 2L], await context.Set<Room<long>>().OrderBy(r => r.Validity).Select(r => r.Validity).ToListAsync());
        Assert.Equal([1, 2], await context.Set<Room<int>>().OrderBy(r => r.Validity).Select(r => r.Validity).ToListAsync());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Converted_NpgsqlRange_keys(bool crossType)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: modelBuilder =>
            {
                modelBuilder.Entity<Room<NpgsqlRange<int>>>(b =>
                {
                    b.HasKey(r => new { r.Id, r.Validity }).WithoutOverlaps();
                    var property = b.Property(r => r.Validity);

                    if (crossType)
                    {
                        property.HasConversion(
                            v => new NpgsqlRange<long>(v.LowerBound, v.LowerBoundIsInclusive, v.UpperBound, v.UpperBoundIsInclusive),
                            v => new NpgsqlRange<int>((int)v.LowerBound, v.LowerBoundIsInclusive, (int)v.UpperBound, v.UpperBoundIsInclusive));
                    }
                    else
                    {
                        property.HasConversion(
                            v => new NpgsqlRange<int>(v.LowerBound, true, v.UpperBound, false),
                            v => v);
                    }
                });
            },
            onConfiguring: b => new NpgsqlDbContextOptionsBuilder(b).SetPostgresVersion(18, 0));

        await using var context = contextFactory.CreateDbContext();
        context.AddRange(
            new Room<NpgsqlRange<int>> { Id = 1, Validity = new NpgsqlRange<int>(3, true, 4, false), Name = "Second" },
            new Room<NpgsqlRange<int>> { Id = 1, Validity = new NpgsqlRange<int>(1, true, 2, false), Name = "First" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rooms = await context.Set<Room<NpgsqlRange<int>>>().OrderBy(r => r.Validity).ToListAsync();
        Assert.Equal(["First", "Second"], rooms.Select(r => r.Name));

        foreach (var room in rooms)
        {
            room.Name = "Updated";
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        Assert.All(await context.Set<Room<NpgsqlRange<int>>>().ToListAsync(), r => Assert.Equal("Updated", r.Name));
    }

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
