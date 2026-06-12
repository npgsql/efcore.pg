using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

namespace Microsoft.EntityFrameworkCore;

[MinimumPostgresVersion(18, 0)]
public class TemporalConstraintTest : IClassFixture<TemporalConstraintTest.TemporalConstraintFixture>
{
    private TemporalConstraintFixture Fixture { get; }

    public TemporalConstraintTest(TemporalConstraintFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Insert

    [ConditionalFact]
    public Task Can_insert_and_roundtrip_room_with_without_overlaps_primary_key()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Rooms.Add(
                    new Room
                    {
                        Id = 100,
                        Validity = new NpgsqlRange<DateTime>(
                            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc)),
                        Name = "New Room"
                    });

                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();

                var loaded = await context.Rooms.SingleAsync(r => r.Id == 100);
                Assert.Equal("New Room", loaded.Name);
                Assert.Equal(
                    new NpgsqlRange<DateTime>(
                        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc)),
                    loaded.Validity);
            });

    [ConditionalFact]
    public Task Can_insert_reservation_with_period_foreign_key()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Reservations.Add(
                    new Reservation
                    {
                        RoomId = 1,
                        Validity = new NpgsqlRange<DateTime>(
                            new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                            new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc)),
                        Description = "New reservation"
                    });
                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();

                var reservation = await context.Reservations.SingleAsync(r => r.Description == "New reservation");
                Assert.Equal(1, reservation.RoomId);
            });

    #endregion

    #region Query

    [ConditionalFact]
    public async Task Range_contains_timestamp()
    {
        using var context = CreateContext();

        // Find rooms valid at a specific point in time
        var pointInTime = new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        var rooms = await context.Rooms
            .Where(r => r.Validity.Contains(pointInTime))
            .OrderBy(r => r.Id)
            .ToListAsync();

        // Room 1 (full year) and Room 2 H1 (Jan-Jun) are valid in March
        Assert.Equal(2, rooms.Count);
        Assert.Equal("Conference Room A", rooms[0].Name);
        Assert.Equal("Conference Room B (H1)", rooms[1].Name);
    }

    [ConditionalFact]
    public async Task Range_contains_range()
    {
        using var context = CreateContext();

        // Find rooms whose validity fully contains the given range
        var queryRange = new NpgsqlRange<DateTime>(
            new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 5, 31, 0, 0, 0, DateTimeKind.Utc));
        var rooms = await context.Rooms
            .Where(r => r.Validity.Contains(queryRange))
            .OrderBy(r => r.Id)
            .ToListAsync();

        // Only Room 1 (full year) fully contains Feb-May; Room 2 H1 (Jan-Jun) does as well
        Assert.Equal(2, rooms.Count);
        Assert.Equal("Conference Room A", rooms[0].Name);
        Assert.Equal("Conference Room B (H1)", rooms[1].Name);
    }

    [ConditionalFact]
    public async Task Range_overlaps()
    {
        using var context = CreateContext();

        // Find rooms whose validity overlaps with a range spanning both halves of the year
        var queryRange = new NpgsqlRange<DateTime>(
            new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc));
        var rooms = await context.Rooms
            .Where(r => r.Validity.Overlaps(queryRange))
            .OrderBy(r => r.Id)
            .ThenBy(r => r.Validity)
            .ToListAsync();

        // All three room rows overlap with Jun-Aug
        Assert.Equal(3, rooms.Count);
        Assert.Equal("Conference Room A", rooms[0].Name);
        Assert.Equal("Conference Room B (H1)", rooms[1].Name);
        Assert.Equal("Conference Room B (H2)", rooms[2].Name);
    }

    [ConditionalFact]
    public async Task Range_contained_by()
    {
        using var context = CreateContext();

        // Find reservations whose validity is contained within a given range
        var queryRange = new NpgsqlRange<DateTime>(
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 3, 31, 0, 0, 0, DateTimeKind.Utc));
        var reservations = await context.Reservations
            .Where(r => r.Validity.ContainedBy(queryRange))
            .OrderBy(r => r.Id)
            .ToListAsync();

        // "Team meeting" (Feb) and "Workshop" (Mar) are within Jan-Mar; "Planning session" (Apr) is not
        Assert.Equal(2, reservations.Count);
        Assert.Equal("Team meeting", reservations[0].Description);
        Assert.Equal("Workshop", reservations[1].Description);
    }

    [ConditionalFact]
    public async Task Range_is_strictly_left_of()
    {
        using var context = CreateContext();

        // Find reservations whose validity is entirely before a given range
        var queryRange = new NpgsqlRange<DateTime>(
            new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc));
        var reservations = await context.Reservations
            .Where(r => r.Validity.IsStrictlyLeftOf(queryRange))
            .OrderBy(r => r.Id)
            .ToListAsync();

        // "Team meeting" (Feb) and "Workshop" (Mar) are entirely before Apr
        Assert.Equal(2, reservations.Count);
        Assert.Equal("Team meeting", reservations[0].Description);
        Assert.Equal("Workshop", reservations[1].Description);
    }

    [ConditionalFact]
    public async Task Navigate_through_period_foreign_key()
    {
        using var context = CreateContext();

        // Navigate from reservation to room via the PERIOD FK and project temporal columns
        var result = await context.Reservations
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .Select(r => new { r.Description, RoomName = r.Room!.Name, RoomValidity = r.Room.Validity })
            .FirstAsync();

        Assert.Equal("Team meeting", result.Description);
        Assert.Equal("Conference Room A", result.RoomName);
        Assert.Equal(
            new NpgsqlRange<DateTime>(
                new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)),
            result.RoomValidity);
    }

    [ConditionalFact]
    public void Period_foreign_key_with_tracking_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => context.Reservations
                .OrderBy(r => r.Id)
                .Select(r => new { r.Description, RoomName = r.Room!.Name })
                .First());

        Assert.Equal(NpgsqlStrings.PeriodForeignKeyTrackingNotSupported, exception.Message);
    }

    [ConditionalFact]
    public async Task Include_collection_with_range_filter()
    {
        using var context = CreateContext();

        // Load rooms valid at a specific timestamp, including their reservations
        var pointInTime = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var rooms = await context.Rooms
            .AsNoTracking()
            .Include(r => r.Reservations)
            .Where(r => r.Validity.Contains(pointInTime))
            .OrderBy(r => r.Id)
            .ToListAsync();
        Assert.Equal("Conference Room A", rooms[0].Name);
        Assert.Equal(2, rooms[0].Reservations.Count);
        Assert.Equal("Conference Room B (H2)", rooms[1].Name);
        Assert.Empty(rooms[1].Reservations);
    }

    #endregion

    #region Update

    [ConditionalFact]
    public Task Can_update_entity_with_without_overlaps_primary_key()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var room = await context.Rooms.SingleAsync(r => r.Id == 1);
                room.Name = "Updated Room";
                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();

                var loaded = await context.Rooms.SingleAsync(r => r.Id == 1);
                Assert.Equal("Updated Room", loaded.Name);
            });

    [ConditionalFact]
    public Task Can_update_entity_with_period_foreign_key()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var reservation = await context.Reservations.OrderBy(r => r.Id).FirstAsync();
                reservation.Description = "Updated description";
                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();

                var loaded = await context.Reservations.OrderBy(r => r.Id).FirstAsync();
                Assert.Equal("Updated description", loaded.Description);
            });

    #endregion

    #region Delete

    [ConditionalFact]
    public Task Can_delete_entity_with_without_overlaps_primary_key()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                // Room 2 H2 has no reservations, so it can be deleted
                var room = await context.Rooms.SingleAsync(
                    r => r.Id == 2
                        && r.Validity == new NpgsqlRange<DateTime>(
                            new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                            new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)));
                context.Rooms.Remove(room);
                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();

                var remaining = await context.Rooms.Where(r => r.Id == 2).ToListAsync();
                Assert.Single(remaining);
            });

    [ConditionalFact]
    public Task Can_delete_entity_with_period_foreign_key()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var reservation = await context.Reservations.OrderBy(r => r.Id).FirstAsync();
                context.Reservations.Remove(reservation);
                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();

                var remaining = await context.Reservations.ToListAsync();
                Assert.Equal(2, remaining.Count);
            });

    #endregion

    private TemporalConstraintContext CreateContext()
        => Fixture.CreateContext();

    private Task ExecuteWithStrategyInTransactionAsync(
        Func<TemporalConstraintContext, Task> testOperation)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction, testOperation);

    private static void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class TemporalConstraintContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Reservation> Reservations => Set<Reservation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>(b =>
            {
                b.HasKey(r => new { r.Id, r.Validity }).WithoutOverlaps();

                b.HasAlternateKey(r => new { r.RoomNumber, r.Validity }).WithoutOverlaps();

                b.Property(r => r.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<Reservation>(b =>
            {
                b.HasKey(r => r.Id);

                b.HasOne(r => r.Room)
                    .WithMany(r => r.Reservations)
                    .HasForeignKey(r => new { r.RoomId, r.Validity })
                    .HasPrincipalKey(r => new { r.Id, r.Validity })
                    .WithPeriod();

                b.Property(r => r.Description).HasMaxLength(500);
            });
        }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public class Room
    {
        public int Id { get; set; }
        public NpgsqlRange<DateTime> Validity { get; set; }
        public int RoomNumber { get; set; }
        public string Name { get; set; } = null!;
        public List<Reservation> Reservations { get; set; } = [];
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public class Reservation
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public NpgsqlRange<DateTime> Validity { get; set; }
        public string Description { get; set; } = null!;
        public Room? Room { get; set; }
    }

    public class TemporalConstraintFixture : SharedStoreFixtureBase<TemporalConstraintContext>
    {
        protected override string StoreName
            => "TemporalConstraintTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);

            // Note that we have [MinimumPostgresVersion(18, 0)] on the whole class
            new NpgsqlDbContextOptionsBuilder(optionsBuilder)
                .SetPostgresVersion(18, 0);

            return optionsBuilder;
        }

        protected override async Task SeedAsync(TemporalConstraintContext context)
        {
            // Room 1: single validity period, has reservations
            context.Rooms.Add(
                new Room
                {
                    Id = 1,
                    RoomNumber = 101,
                    Validity = new NpgsqlRange<DateTime>(
                        new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)),
                    Name = "Conference Room A"
                });

            // Room 2: same Id, two non-overlapping validity periods (temporal versioning)
            context.Rooms.Add(
                new Room
                {
                    Id = 2,
                    RoomNumber = 102,
                    Validity = new NpgsqlRange<DateTime>(
                        new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2025, 6, 30, 0, 0, 0, DateTimeKind.Utc)),
                    Name = "Conference Room B (H1)"
                });
            context.Rooms.Add(
                new Room
                {
                    Id = 2,
                    RoomNumber = 103,
                    Validity = new NpgsqlRange<DateTime>(
                        new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)),
                    Name = "Conference Room B (H2)"
                });

            await context.SaveChangesAsync();

            // Reservations referencing Room 1 (PERIOD FK - validity must be contained in room's validity)
            context.Reservations.Add(
                new Reservation
                {
                    RoomId = 1,
                    Validity = new NpgsqlRange<DateTime>(
                        new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2025, 2, 28, 0, 0, 0, DateTimeKind.Utc)),
                    Description = "Team meeting"
                });
            context.Reservations.Add(
                new Reservation
                {
                    RoomId = 1,
                    Validity = new NpgsqlRange<DateTime>(
                        new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2025, 4, 30, 0, 0, 0, DateTimeKind.Utc)),
                    Description = "Planning session"
                });

            // Reservation referencing Room 2 H1 (PERIOD FK)
            context.Reservations.Add(
                new Reservation
                {
                    RoomId = 2,
                    Validity = new NpgsqlRange<DateTime>(
                        new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2025, 3, 31, 0, 0, 0, DateTimeKind.Utc)),
                    Description = "Workshop"
                });

            await context.SaveChangesAsync();
        }

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
