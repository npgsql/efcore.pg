using Microsoft.EntityFrameworkCore.TestModels.NodaTime;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query.Translations.NodaTime;

public class NodaTimeQueryNpgsqlFixture : SharedStoreFixtureBase<NodaTimeContext>, IQueryFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "NodaTimeQueryTest";

    // Set the PostgreSQL TimeZone parameter to something local, to ensure that operations which take TimeZone into account
    // don't depend on the database's time zone, and also that operations which shouldn't take TimeZone into account indeed
    // don't.
    // We also instruct the test store to pass a connection string to UseNpgsql() instead of a DbConnection - that's required to allow
    // EF's UseNodaTime() to function properly and instantiate an NpgsqlDataSource internally.
    protected override ITestStoreFactory TestStoreFactory
        => new NpgsqlTestStoreFactory(connectionStringOptions: "-c TimeZone=Europe/Berlin", useConnectionString: true);

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    private NodaTimeData? _expectedData;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection).AddEntityFrameworkNpgsqlNodaTime();

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var optionsBuilder = base.AddOptions(builder);
        new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseNodaTime();

        return optionsBuilder;
    }

    protected override Task SeedAsync(NodaTimeContext context)
        => NodaTimeContext.SeedAsync(context);

    public Func<DbContext> GetContextCreator()
        => CreateContext;

    public ISetSource GetExpectedData()
        => _expectedData ??= new NodaTimeData();

    public IReadOnlyDictionary<Type, object> EntitySorters
        => new Dictionary<Type, Func<object, object?>> { { typeof(NodaTimeTypes), e => ((NodaTimeTypes)e).Id } }
            .ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters
        => new Dictionary<Type, Action<object, object>>
        {
            {
                typeof(NodaTimeTypes), (e, a) =>
                {
                    Assert.Equal(e is null, a is null);
                    if (e is not null && a is not null)
                    {
                        var ee = (NodaTimeTypes)e;
                        var aa = (NodaTimeTypes)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.LocalDateTime, aa.LocalDateTime);
                        Assert.Equal(ee.ZonedDateTime, aa.ZonedDateTime);
                        Assert.Equal(ee.Instant, aa.Instant);
                        Assert.Equal(ee.LocalDate, aa.LocalDate);
                        Assert.Equal(ee.LocalDate2, aa.LocalDate2);
                        Assert.Equal(ee.LocalTime, aa.LocalTime);
                        Assert.Equal(ee.OffsetTime, aa.OffsetTime);
                        Assert.Equal(ee.Period, aa.Period);
                        Assert.Equal(ee.Duration, aa.Duration);
                        Assert.Equal(ee.DateInterval, aa.DateInterval);
                        // Assert.Equal(ee.DateRange, aa.DateRange);
                        Assert.Equal(ee.Long, aa.Long);
                        Assert.Equal(ee.TimeZoneId, aa.TimeZoneId);
                    }
                }
            }
        }.ToDictionary(e => e.Key, e => (object)e.Value);
}
