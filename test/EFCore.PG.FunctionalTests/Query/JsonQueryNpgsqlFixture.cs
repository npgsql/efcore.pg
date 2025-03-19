using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonQueryNpgsqlFixture : JsonQueryRelationalFixture, IQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    private JsonQueryData? _expectedData;
    private readonly IReadOnlyDictionary<Type, object> _entityAsserters;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var optionsBuilder = base.AddOptions(builder);
        new NpgsqlDbContextOptionsBuilder(optionsBuilder).SetPostgresVersion(TestEnvironment.PostgresVersion);
        return optionsBuilder;
    }

    public JsonQueryNpgsqlFixture()
    {
        var entityAsserters = base.EntityAsserters.ToDictionary();

        entityAsserters[typeof(JsonEntityAllTypes)] = (object e, object a) =>
        {
            Assert.Equal(e is null, a is null);

            if (e is not null && a is not null)
            {
                var ee = (JsonEntityAllTypes)e;
                var aa = (JsonEntityAllTypes)a;

                Assert.Equal(ee.Id, aa.Id);

                AssertAllTypes(ee.Reference, aa.Reference);

                Assert.Equal(ee.Collection?.Count ?? 0, aa.Collection?.Count ?? 0);
                for (var i = 0; i < ee.Collection!.Count; i++)
                {
                    AssertAllTypes(ee.Collection[i], aa.Collection![i]);
                }
            }
        };

        entityAsserters[typeof(JsonOwnedAllTypes)] = (object e, object a) =>
        {
            Assert.Equal(e is null, a is null);

            if (e is not null && a is not null)
            {
                var ee = (JsonOwnedAllTypes)e;
                var aa = (JsonOwnedAllTypes)a;

                AssertAllTypes(ee, aa);
            }
        };

        _entityAsserters = entityAsserters;
    }

    IReadOnlyDictionary<Type, object> IQueryFixtureBase.EntityAsserters
        => _entityAsserters;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // The tests seed Unspecified DateTimes, but our default mapping for DateTime is timestamptz, which requires UTC.
        // Map these properties to "timestamp without time zone".
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        configurationBuilder.Properties<List<DateTime>>().HaveColumnType("timestamp without time zone[]");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // The following are ignored since we do not support mapping IList<T> (as opposed to array/List) on regular properties
        // (since that's not supported at the ADO.NET layer). However, we do support IList<T> inside JSON documents, since that doesn't
        // rely on ADO.NET support.
        modelBuilder.Entity<JsonEntityAllTypes>(
            b =>
            {
                b.Ignore(j => j.TestEnumCollection);
                b.Ignore(j => j.TestUnsignedInt16Collection);
                b.Ignore(j => j.TestNullableEnumCollection);
                b.Ignore(j => j.TestNullableEnumWithIntConverterCollection);
                b.Ignore(j => j.TestCharacterCollection);
                b.Ignore(j => j.TestNullableInt32Collection);
                b.Ignore(j => j.TestUnsignedInt64Collection);

                b.Ignore(j => j.TestByteCollection);
                b.Ignore(j => j.TestBooleanCollection);
                b.Ignore(j => j.TestDateTimeOffsetCollection);
                b.Ignore(j => j.TestDoubleCollection);
                b.Ignore(j => j.TestInt16Collection);
            });

        // These use collection types which are unsupported for arrays at the Npgsql level - we currently only support List/array.
        modelBuilder.Entity<JsonEntityAllTypes>(
            b =>
            {
                b.Ignore(j => j.TestInt64Collection);
                b.Ignore(j => j.TestGuidCollection);
            });

        // Ignore nested collections - these aren't supported on PostgreSQL (no arrays of arrays).
        // TODO: Remove these after syncing to 9.0.0-rc.1, and extending from the relational test base and fixture
        modelBuilder.Entity<JsonEntityAllTypes>(
            b =>
            {
                b.Ignore(j => j.TestDefaultStringCollectionCollection);
                b.Ignore(j => j.TestMaxLengthStringCollectionCollection);

                b.Ignore(j => j.TestInt16CollectionCollection);
                b.Ignore(j => j.TestInt32CollectionCollection);
                b.Ignore(j => j.TestInt64CollectionCollection);
                b.Ignore(j => j.TestDoubleCollectionCollection);
                b.Ignore(j => j.TestSingleCollectionCollection);
                b.Ignore(j => j.TestCharacterCollectionCollection);
                b.Ignore(j => j.TestBooleanCollectionCollection);

                b.Ignore(j => j.TestNullableInt32CollectionCollection);
                b.Ignore(j => j.TestNullableEnumCollectionCollection);
                b.Ignore(j => j.TestNullableEnumWithIntConverterCollectionCollection);
                b.Ignore(j => j.TestNullableEnumWithConverterThatHandlesNullsCollection);
            });

        modelBuilder.Entity<JsonEntityAllTypes>().OwnsOne(
            x => x.Reference, b =>
            {
                b.Ignore(j => j.TestDefaultStringCollectionCollection);
                b.Ignore(j => j.TestMaxLengthStringCollectionCollection);

                b.Ignore(j => j.TestInt16CollectionCollection);
                b.Ignore(j => j.TestInt32CollectionCollection);
                b.Ignore(j => j.TestInt64CollectionCollection);
                b.Ignore(j => j.TestDoubleCollectionCollection);
                b.Ignore(j => j.TestSingleCollectionCollection);
                b.Ignore(j => j.TestBooleanCollectionCollection);
                b.Ignore(j => j.TestCharacterCollectionCollection);

                b.Ignore(j => j.TestNullableInt32CollectionCollection);
                b.Ignore(j => j.TestNullableEnumCollectionCollection);
                b.Ignore(j => j.TestNullableEnumWithIntConverterCollectionCollection);
                b.Ignore(j => j.TestNullableEnumWithConverterThatHandlesNullsCollection);
            });

        modelBuilder.Entity<JsonEntityAllTypes>().OwnsMany(
            x => x.Collection, b =>
            {
                b.Ignore(j => j.TestDefaultStringCollectionCollection);
                b.Ignore(j => j.TestMaxLengthStringCollectionCollection);

                b.Ignore(j => j.TestInt16CollectionCollection);
                b.Ignore(j => j.TestInt32CollectionCollection);
                b.Ignore(j => j.TestInt64CollectionCollection);
                b.Ignore(j => j.TestDoubleCollectionCollection);
                b.Ignore(j => j.TestSingleCollectionCollection);
                b.Ignore(j => j.TestBooleanCollectionCollection);
                b.Ignore(j => j.TestBooleanCollectionCollection);
                b.Ignore(j => j.TestCharacterCollectionCollection);

                b.Ignore(j => j.TestNullableInt32CollectionCollection);
                b.Ignore(j => j.TestNullableEnumCollectionCollection);
                b.Ignore(j => j.TestNullableEnumWithIntConverterCollectionCollection);
                b.Ignore(j => j.TestNullableEnumWithConverterThatHandlesNullsCollection);
            });
    }

    public override ISetSource GetExpectedData()
    {
        if (_expectedData is null)
        {
            _expectedData = (JsonQueryData)base.GetExpectedData();

            // The test data contains DateTimeOffsets with various offsets, which we don't support. Change these to UTC.
            // Also chop sub-microsecond precision which PostgreSQL does not support.
            foreach (var j in _expectedData.JsonEntitiesAllTypes)
            {
                j.Reference.TestDateTimeOffset = new DateTimeOffset(
                    j.Reference.TestDateTimeOffset.Ticks
                    - (j.Reference.TestDateTimeOffset.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero);

                foreach (var j2 in j.Collection)
                {
                    j2.TestDateTimeOffset = new DateTimeOffset(
                        j2.TestDateTimeOffset.Ticks - (j2.TestDateTimeOffset.Ticks % (TimeSpan.TicksPerMillisecond / 1000)),
                        TimeSpan.Zero);
                }

                j.TestDateTimeOffsetCollection = j.TestDateTimeOffsetCollection.Select(
                    dto => new DateTimeOffset(dto.Ticks - (dto.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero)).ToList();
            }
        }

        return _expectedData;
    }

    protected override async Task SeedAsync(JsonQueryContext context)
    {
        // The test data contains DateTimeOffsets with various offsets, which we don't support. Change these to UTC.
        // Also chop sub-microsecond precision which PostgreSQL does not support.
        // See https://github.com/dotnet/efcore/issues/26068

        var jsonEntitiesBasic = JsonQueryData.CreateJsonEntitiesBasic();
        var entitiesBasic = JsonQueryData.CreateEntitiesBasic();
        var jsonEntitiesBasicForReference = JsonQueryData.CreateJsonEntitiesBasicForReference();
        var jsonEntitiesBasicForCollection = JsonQueryData.CreateJsonEntitiesBasicForCollection();
        JsonQueryData.WireUp(jsonEntitiesBasic, entitiesBasic, jsonEntitiesBasicForReference, jsonEntitiesBasicForCollection);

        var jsonEntitiesCustomNaming = JsonQueryData.CreateJsonEntitiesCustomNaming();
        var jsonEntitiesSingleOwned = JsonQueryData.CreateJsonEntitiesSingleOwned();
        var jsonEntitiesInheritance = JsonQueryData.CreateJsonEntitiesInheritance();
        var jsonEntitiesAllTypes = JsonQueryData.CreateJsonEntitiesAllTypes();
        var jsonEntitiesConverters = JsonQueryData.CreateJsonEntitiesConverters();

        context.JsonEntitiesBasic.AddRange(jsonEntitiesBasic);
        context.EntitiesBasic.AddRange(entitiesBasic);
        context.JsonEntitiesBasicForReference.AddRange(jsonEntitiesBasicForReference);
        context.JsonEntitiesBasicForCollection.AddRange(jsonEntitiesBasicForCollection);
        context.JsonEntitiesCustomNaming.AddRange(jsonEntitiesCustomNaming);
        context.JsonEntitiesSingleOwned.AddRange(jsonEntitiesSingleOwned);
        context.JsonEntitiesInheritance.AddRange(jsonEntitiesInheritance);
        context.JsonEntitiesAllTypes.AddRange(jsonEntitiesAllTypes);
        context.JsonEntitiesConverters.AddRange(jsonEntitiesConverters);

        foreach (var j in jsonEntitiesAllTypes)
        {
            j.Reference.TestDateTimeOffset = new DateTimeOffset(
                j.Reference.TestDateTimeOffset.Ticks - (j.Reference.TestDateTimeOffset.Ticks % (TimeSpan.TicksPerMillisecond / 1000)),
                TimeSpan.Zero);

            foreach (var j2 in j.Collection)
            {
                j2.TestDateTimeOffset = new DateTimeOffset(
                    j2.TestDateTimeOffset.Ticks - (j2.TestDateTimeOffset.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero);
            }

            j.TestDateTimeOffsetCollection = j.TestDateTimeOffsetCollection.Select(
                dto => new DateTimeOffset(dto.Ticks - (dto.Ticks % (TimeSpan.TicksPerMillisecond / 1000)), TimeSpan.Zero)).ToList();
        }

        await context.SaveChangesAsync();
    }

    public static new void AssertAllTypes(JsonOwnedAllTypes expected, JsonOwnedAllTypes actual)
    {
        Assert.Equal(expected.TestDefaultString, actual.TestDefaultString);
        Assert.Equal(expected.TestMaxLengthString, actual.TestMaxLengthString);
        Assert.Equal(expected.TestBoolean, actual.TestBoolean);
        Assert.Equal(expected.TestCharacter, actual.TestCharacter);
        Assert.Equal(expected.TestDateTime, actual.TestDateTime);
        Assert.Equal(expected.TestDateTimeOffset, actual.TestDateTimeOffset);
        Assert.Equal(expected.TestDouble, actual.TestDouble);
        Assert.Equal(expected.TestGuid, actual.TestGuid);
        Assert.Equal(expected.TestInt16, actual.TestInt16);
        Assert.Equal(expected.TestInt32, actual.TestInt32);
        Assert.Equal(expected.TestInt64, actual.TestInt64);
        Assert.Equal(expected.TestSignedByte, actual.TestSignedByte);
        Assert.Equal(expected.TestSingle, actual.TestSingle);
        Assert.Equal(expected.TestTimeSpan, actual.TestTimeSpan);
        Assert.Equal(expected.TestDateOnly, actual.TestDateOnly);
        Assert.Equal(expected.TestTimeOnly, actual.TestTimeOnly);
        Assert.Equal(expected.TestUnsignedInt16, actual.TestUnsignedInt16);
        Assert.Equal(expected.TestUnsignedInt32, actual.TestUnsignedInt32);
        Assert.Equal(expected.TestUnsignedInt64, actual.TestUnsignedInt64);
        Assert.Equal(expected.TestNullableInt32, actual.TestNullableInt32);
        Assert.Equal(expected.TestEnum, actual.TestEnum);
        Assert.Equal(expected.TestEnumWithIntConverter, actual.TestEnumWithIntConverter);
        Assert.Equal(expected.TestNullableEnum, actual.TestNullableEnum);
        Assert.Equal(expected.TestNullableEnumWithIntConverter, actual.TestNullableEnumWithIntConverter);
        Assert.Equal(expected.TestNullableEnumWithConverterThatHandlesNulls, actual.TestNullableEnumWithConverterThatHandlesNulls);

        AssertPrimitiveCollection(expected.TestDefaultStringCollection, actual.TestDefaultStringCollection);
        AssertPrimitiveCollection(expected.TestMaxLengthStringCollection, actual.TestMaxLengthStringCollection);
        AssertPrimitiveCollection(expected.TestBooleanCollection, actual.TestBooleanCollection);
        AssertPrimitiveCollection(expected.TestCharacterCollection, actual.TestCharacterCollection);
        AssertPrimitiveCollection(expected.TestDateTimeCollection, actual.TestDateTimeCollection);
        AssertPrimitiveCollection(expected.TestDateTimeOffsetCollection, actual.TestDateTimeOffsetCollection);
        AssertPrimitiveCollection(expected.TestDoubleCollection, actual.TestDoubleCollection);
        AssertPrimitiveCollection(expected.TestGuidCollection, actual.TestGuidCollection);
        AssertPrimitiveCollection((IList<short>)expected.TestInt16Collection, (IList<short>)actual.TestInt16Collection);
        AssertPrimitiveCollection(expected.TestInt32Collection, actual.TestInt32Collection);
        AssertPrimitiveCollection(expected.TestInt64Collection, actual.TestInt64Collection);
        AssertPrimitiveCollection(expected.TestSignedByteCollection, actual.TestSignedByteCollection);
        AssertPrimitiveCollection(expected.TestSingleCollection, actual.TestSingleCollection);
        AssertPrimitiveCollection(expected.TestTimeSpanCollection, actual.TestTimeSpanCollection);
        AssertPrimitiveCollection(expected.TestDateOnlyCollection, actual.TestDateOnlyCollection);
        AssertPrimitiveCollection(expected.TestTimeOnlyCollection, actual.TestTimeOnlyCollection);
        AssertPrimitiveCollection(expected.TestUnsignedInt16Collection, actual.TestUnsignedInt16Collection);
        AssertPrimitiveCollection(expected.TestUnsignedInt32Collection, actual.TestUnsignedInt32Collection);
        AssertPrimitiveCollection(expected.TestUnsignedInt64Collection, actual.TestUnsignedInt64Collection);
        AssertPrimitiveCollection(expected.TestNullableInt32Collection, actual.TestNullableInt32Collection);
        AssertPrimitiveCollection(expected.TestEnumCollection, actual.TestEnumCollection);
        AssertPrimitiveCollection(expected.TestEnumWithIntConverterCollection, actual.TestEnumWithIntConverterCollection);
        AssertPrimitiveCollection(expected.TestNullableEnumCollection, actual.TestNullableEnumCollection);
        AssertPrimitiveCollection(
            expected.TestNullableEnumWithIntConverterCollection, actual.TestNullableEnumWithIntConverterCollection);

        // AssertPrimitiveCollection(
        //     expected.TestNullableEnumWithConverterThatHandlesNullsCollection,
        //     actual.TestNullableEnumWithConverterThatHandlesNullsCollection);
    }
}
