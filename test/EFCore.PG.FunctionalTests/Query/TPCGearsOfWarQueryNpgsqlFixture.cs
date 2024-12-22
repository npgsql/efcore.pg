using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class TPCGearsOfWarQueryNpgsqlFixture : TPCGearsOfWarQueryRelationalFixture
{
    static TPCGearsOfWarQueryNpgsqlFixture()
    {
        // TODO: Switch to using NpgsqlDataSource
#pragma warning disable CS0618 // Type or member is obsolete
        NpgsqlConnection.GlobalTypeMapper.EnableRecordsAsTuples();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<CogTag>().Property(c => c.IssueDate).HasColumnType("timestamp without time zone");
        modelBuilder.Entity<City>().Property(g => g.Location).HasColumnType("varchar(100)");
    }

    private GearsOfWarData? _expectedData;

    public override ISetSource GetExpectedData()
    {
        if (_expectedData is null)
        {
            _expectedData = (GearsOfWarData)base.GetExpectedData();

            // GearsOfWarData contains DateTimeOffsets with various offsets, which we don't support. Change these to UTC.
            // Also chop sub-microsecond precision which PostgreSQL does not support.
            foreach (var mission in _expectedData.Missions)
            {
                mission.Timeline = new DateTimeOffset(
                    mission.Timeline.Ticks - (mission.Timeline.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero);
                mission.Duration = new TimeSpan(
                    mission.Duration.Ticks - (mission.Duration.Ticks % (TimeSpan.TicksPerMillisecond / 1000)));
            }
        }

        return _expectedData;
    }

    protected override Task SeedAsync(GearsOfWarContext context)
        // GearsOfWarData contains DateTimeOffsets with various offsets, which we don't support. Change these to UTC.
        // Also chop sub-microsecond precision which PostgreSQL does not support.
        => SeedForNpgsqlAsync(context);

    public static async Task SeedForNpgsqlAsync(GearsOfWarContext context)
    {
        var squads = GearsOfWarData.CreateSquads();
        var missions = GearsOfWarData.CreateMissions();
        var squadMissions = GearsOfWarData.CreateSquadMissions();
        var cities = GearsOfWarData.CreateCities();
        var weapons = GearsOfWarData.CreateWeapons();
        var tags = GearsOfWarData.CreateTags();
        var gears = GearsOfWarData.CreateGears();
        var locustLeaders = GearsOfWarData.CreateLocustLeaders();
        var factions = GearsOfWarData.CreateFactions();
        var locustHighCommands = GearsOfWarData.CreateHighCommands();

        foreach (var mission in missions)
        {
            mission.Timeline = new DateTimeOffset(
                mission.Timeline.Ticks - (mission.Timeline.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero);
            mission.Duration = new TimeSpan(
                mission.Duration.Ticks - (mission.Duration.Ticks % (TimeSpan.TicksPerMillisecond / 1000)));
        }

        GearsOfWarData.WireUp(
            squads, missions, squadMissions, cities, weapons, tags, gears, locustLeaders, factions, locustHighCommands);

        context.Squads.AddRange(squads);
        context.Missions.AddRange(missions);
        context.SquadMissions.AddRange(squadMissions);
        context.Cities.AddRange(cities);
        context.Weapons.AddRange(weapons);
        context.Tags.AddRange(tags);
        context.Gears.AddRange(gears);
        context.LocustLeaders.AddRange(locustLeaders);
        context.Factions.AddRange(factions);
        context.LocustHighCommands.AddRange(locustHighCommands);
        await context.SaveChangesAsync();

        GearsOfWarData.WireUp2(locustLeaders, factions);

        await context.SaveChangesAsync();
    }
}
