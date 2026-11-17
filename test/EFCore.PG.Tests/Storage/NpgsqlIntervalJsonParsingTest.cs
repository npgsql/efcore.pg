#if DEBUG

using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage;

/// <summary>
///     Tests reading intervals (TimeSpan, NodaTime Duration) from JSON, with both values of the Npgsql.ParseOldEFCoreJsonTimeSpans
///     AppContext switch, which determines the meaning of a negative value with a days component and a sign on the days only.
/// </summary>
[Collection("ParseOldEFCoreJsonTimeSpansTest")]
public class NpgsqlIntervalJsonParsingTest
{
    [Theory]
    // Older versions of the provider wrote a sign on the days only, however for PostgreSQL the sign applies to the days only.
    // Values written by the current version have a sign on both the days and the time, and are unambiguous
    [InlineData("-1 02:00:00", true, "-1.02:00:00")]
    [InlineData("-1 02:00:00", false, "-22:00:00")]
    [InlineData("-3 04:05:06.007", true, "-3.04:05:06.007")]
    [InlineData("-3 04:05:06.007", false, "-2.19:54:53.993")]
    [InlineData("-10675199 02:48:05.47758", true, "-10675199.02:48:05.47758")]
    [InlineData("-10675199 02:48:05.47758", false, "-10675198.21:11:54.52242")]
    [InlineData("-1 -02:00:00", true, "-1.02:00:00")]
    [InlineData("-1 -02:00:00", false, "-1.02:00:00")]
    [InlineData("-3 -04:05:06.007", true, "-3.04:05:06.007")]
    [InlineData("-3 -04:05:06.007", false, "-3.04:05:06.007")]
    // A sign on the time only
    [InlineData("1 -02:00:00", true, "22:00:00")]
    [InlineData("1 -02:00:00", false, "22:00:00")]
    // Values without a days component, or without a negative sign, aren't affected
    [InlineData("-02:00:00", true, "-02:00:00")]
    [InlineData("-02:00:00", false, "-02:00:00")]
    [InlineData("-00:00:00.000001", true, "-00:00:00.000001")]
    [InlineData("-00:00:00.000001", false, "-00:00:00.000001")]
    [InlineData("1 02:00:00", true, "1.02:00:00")]
    [InlineData("1 02:00:00", false, "1.02:00:00")]
    [InlineData("00:00:00", true, "00:00:00")]
    [InlineData("00:00:00", false, "00:00:00")]
    // The days alone are out of range for a TimeSpan, but the value isn't (e.g. an interval computed in SQL)
    [InlineData("10675200 -23:00:00", true, "10675199.01:00:00")]
    [InlineData("10675200 -23:00:00", false, "10675199.01:00:00")]
    [InlineData("-10675200 23:00:00", false, "-10675199.01:00:00")]
    // TimeSpan.MaxValue/MinValue, truncated to microseconds
    [InlineData("10675199 02:48:05.47758", true, "10675199.02:48:05.47758")]
    [InlineData("-10675199 -02:48:05.47758", true, "-10675199.02:48:05.47758")]
    [InlineData("-10675199 -02:48:05.47758", false, "-10675199.02:48:05.47758")]
    public void Read_interval_from_json(string json, bool parseOldEFCoreJsonTimeSpans, string expected)
    {
        var expectedTimeSpan = TimeSpan.Parse(expected, CultureInfo.InvariantCulture);

        var previous = NpgsqlTypeMappingSource.ParseOldEFCoreJsonTimeSpans;
        NpgsqlTypeMappingSource.ParseOldEFCoreJsonTimeSpans = parseOldEFCoreJsonTimeSpans;
        try
        {
            Assert.Equal(expectedTimeSpan, ReadJson(Mapper.FindMapping(typeof(TimeSpan))!, json));
            Assert.Equal(Duration.FromTimeSpan(expectedTimeSpan), ReadJson(Mapper.FindMapping(typeof(Duration))!, json));
        }
        finally
        {
            NpgsqlTypeMappingSource.ParseOldEFCoreJsonTimeSpans = previous;
        }
    }

    [Theory]
    // No time component
    [InlineData("123")]
    [InlineData("-123")]
    // A days component in the time
    [InlineData("1 1.02:00:00")]
    // More than one sign on the time
    [InlineData("-1 --02:00:00")]
    [InlineData("1 --02:00:00")]
    public void Read_malformed_interval_from_json_throws(string json)
    {
        Assert.Throws<FormatException>(() => ReadJson(Mapper.FindMapping(typeof(TimeSpan))!, json));
        Assert.Throws<FormatException>(() => ReadJson(Mapper.FindMapping(typeof(Duration))!, json));
    }

    [Theory]
    [InlineData("10675200 00:00:00", true)]
    [InlineData("10675199 23:00:00", false)]
    [InlineData("-10675200 -00:00:00", false)]
    // With the old meaning, the time is negated too (-10675200 days -23 hours)
    [InlineData("-10675200 23:00:00", true)]
    public void Read_out_of_range_interval_from_json_throws(string json, bool parseOldEFCoreJsonTimeSpans)
    {
        var previous = NpgsqlTypeMappingSource.ParseOldEFCoreJsonTimeSpans;
        NpgsqlTypeMappingSource.ParseOldEFCoreJsonTimeSpans = parseOldEFCoreJsonTimeSpans;
        try
        {
            Assert.Throws<OverflowException>(() => ReadJson(Mapper.FindMapping(typeof(TimeSpan))!, json));
            Assert.Throws<OverflowException>(() => ReadJson(Mapper.FindMapping(typeof(Duration))!, json));
        }
        finally
        {
            NpgsqlTypeMappingSource.ParseOldEFCoreJsonTimeSpans = previous;
        }
    }

    [Fact]
    public void ParseOldEFCoreJsonTimeSpans_is_on_by_default()
        => Assert.True(NpgsqlTypeMappingSource.ParseOldEFCoreJsonTimeSpans);

    private static object ReadJson(RelationalTypeMapping mapping, string json)
    {
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(Encoding.UTF8.GetBytes($"\"{json}\"")), null);
        readerManager.MoveNext();
        return mapping.JsonValueReaderWriter!.FromJson(ref readerManager, existingObject: null);
    }

    private static readonly NpgsqlTypeMappingSource Mapper = new(
        new TypeMappingSourceDependencies(
            new ValueConverterSelector(new ValueConverterSelectorDependencies()),
            new JsonValueReaderWriterSource(new JsonValueReaderWriterSourceDependencies()),
            []),
        new RelationalTypeMappingSourceDependencies(
        [
            new NpgsqlNodaTimeTypeMappingSourcePlugin(new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()))
        ]),
        new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
        new NpgsqlSingletonOptions());
}

[CollectionDefinition("ParseOldEFCoreJsonTimeSpansTest", DisableParallelization = true)]
public class ParseOldEFCoreJsonTimeSpansTestCollection;

#endif
