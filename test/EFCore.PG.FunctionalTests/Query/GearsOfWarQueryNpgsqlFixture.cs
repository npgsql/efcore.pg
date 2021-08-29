using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class GearsOfWarQueryNpgsqlFixture : GearsOfWarQueryRelationalFixture
    {
        protected override string StoreName => "GearsOfWarQueryTest";
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

        private GearsOfWarData _expectedData;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.HasPostgresExtension("uuid-ossp");

            modelBuilder.Entity<CogTag>().Property(c => c.IssueDate).HasColumnType("timestamp without time zone");
            modelBuilder.Entity<City>().Property(g => g.Location).HasColumnType("varchar(100)");
        }

        public override ISetSource GetExpectedData()
        {
            if (_expectedData is null)
            {
                _expectedData = new GearsOfWarData();

                // GearsOfWarData contains DateTimeOffsets with various offsets, which we don't support. Change these to UTC.
                // Also chop sub-microsecond precision which PostgreSQL does not support.
                foreach (var mission in _expectedData.Missions)
                {
                    mission.Timeline = new DateTimeOffset(
                        mission.Timeline.Ticks - (mission.Timeline.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero);
                }
            }

            return _expectedData;
        }

        protected override void Seed(GearsOfWarContext context)
        {
            // GearsOfWarData contains DateTimeOffsets with various offsets, which we don't support. Change these to UTC.
            // Also chop sub-microsecond precision which PostgreSQL does not support.
            // See https://github.com/dotnet/efcore/issues/26068

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
                // var newThing = new DateTimeOffset(orig.Ticks - (orig.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero);

                mission.Timeline = new DateTimeOffset(
                    mission.Timeline.Ticks - (mission.Timeline.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero);
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
            context.SaveChanges();

            GearsOfWarData.WireUp2(locustLeaders, factions);

            context.SaveChanges();
        }
    }
}
