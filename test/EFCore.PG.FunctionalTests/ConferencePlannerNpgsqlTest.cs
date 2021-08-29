using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class ConferencePlannerNpgsqlTest : ConferencePlannerTestBase<ConferencePlannerNpgsqlTest.ConferencePlannerNpgsqlFixture>
    {
        public ConferencePlannerNpgsqlTest(ConferencePlannerNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        // Overridden to use UTC DateTimeOffsets
        public override async Task SessionsController_Post()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var track = context.Tracks.AsNoTracking().OrderBy(e => e.Id).First();

                    var controller = new SessionsController(context);

                    var result = await controller.Post(
                        new Session
                        {
                            Abstract = "Pandas eat bamboo all dat.",
                            Title = "Pandas!",
                            // Npgsql customizations
                            StartTime = DateTimeOffset.UtcNow,
                            EndTime = DateTimeOffset.UtcNow.AddHours(1),
                            TrackId = track.Id
                        });

                    var newSession = context.Sessions.AsNoTracking().Single(e => e.Title == "Pandas!");

                    Assert.Equal(newSession.Id, result.Id);
                    Assert.Null(result.Speakers);
                    Assert.NotNull(result.Track);
                    Assert.Equal(track.Id, result.Track.Id);
                });
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class ConferencePlannerNpgsqlFixture : ConferencePlannerFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            // We don't support DateTimeOffset with non-zero offsets, so we unfortunately need to override the entire seeding method.
            // See https://github.com/dotnet/efcore/issues/26068
            protected override void Seed(ApplicationDbContext context)
            {
                var attendees1 = new List<Attendee>
                {
                    new()
                    {
                        EmailAddress = "sonicrainboom@sample.com",
                        FirstName = "Rainbow",
                        LastName = "Dash",
                        UserName = "RainbowDash"
                    },
                    new()
                    {
                        EmailAddress = "solovely@sample.com",
                        FirstName = "Flutter",
                        LastName = "Shy",
                        UserName = "Fluttershy"
                    }
                };

                var attendees2 = new List<Attendee>
                {
                    new()
                    {
                        EmailAddress = "applesforever@sample.com",
                        FirstName = "Apple",
                        LastName = "Jack",
                        UserName = "Applejack"
                    },
                    new()
                    {
                        EmailAddress = "precious@sample.com",
                        FirstName = "Rarity",
                        LastName = "",
                        UserName = "Rarity"
                    }
                };

                var attendees3 = new List<Attendee>
                {
                    new()
                    {
                        EmailAddress = "princess@sample.com",
                        FirstName = "Twilight",
                        LastName = "Sparkle",
                        UserName = "Princess"
                    },
                    new()
                    {
                        EmailAddress = "pinkie@sample.com",
                        FirstName = "Pinkie",
                        LastName = "Pie",
                        UserName = "Pinks"
                    }
                };

                using var document = JsonDocument.Parse(ConferenceData);

                var tracks = new Dictionary<int, Track>();
                var speakers = new Dictionary<Guid, Speaker>();

                var root = document.RootElement;
                foreach (var dayJson in root.EnumerateArray())
                {
                    foreach (var roomJson in dayJson.GetProperty("rooms").EnumerateArray())
                    {
                        var roomId = roomJson.GetProperty("id").GetInt32();
                        if (!tracks.TryGetValue(roomId, out var track))
                        {
                            track = new Track
                            {
                                Name = roomJson.GetProperty("name").GetString(),
                                Sessions = new List<Session>()
                            };

                            tracks[roomId] = track;
                        }

                        foreach (var sessionJson in roomJson.GetProperty("sessions").EnumerateArray())
                        {
                            var sessionSpeakers = new List<Speaker>();
                            foreach (var speakerJson in sessionJson.GetProperty("speakers").EnumerateArray())
                            {
                                var speakerId = speakerJson.GetProperty("id").GetGuid();
                                if (!speakers.TryGetValue(speakerId, out var speaker))
                                {
                                    speaker = new Speaker { Name = speakerJson.GetProperty("name").GetString() };

                                    speakers[speakerId] = speaker;
                                }

                                sessionSpeakers.Add(speaker);
                            }

                            var session = new Session
                            {
                                Title = sessionJson.GetProperty("title").GetString(),
                                Abstract = sessionJson.GetProperty("description").GetString(),
                                // Npgsql customizations
                                StartTime = sessionJson.GetProperty("startsAt").GetDateTime().ToUniversalTime(),
                                EndTime = sessionJson.GetProperty("endsAt").GetDateTime().ToUniversalTime()
                            };

                            session.SessionSpeakers = sessionSpeakers.Select(
                                s => new SessionSpeaker { Session = session, Speaker = s }).ToList();

                            var trackName = track.Name;
                            var attendees = trackName.Contains("1") ? attendees1
                                : trackName.Contains("2") ? attendees2
                                : trackName.Contains("3") ? attendees3
                                : attendees1.Concat(attendees2).Concat(attendees3).ToList();

                            session.SessionAttendees = attendees.Select(
                                a => new SessionAttendee { Session = session, Attendee = a }).ToList();

                            track.Sessions.Add(session);
                        }
                    }
                }

                context.AddRange(tracks.Values);
                context.SaveChanges();
            }
        }
    }
}
