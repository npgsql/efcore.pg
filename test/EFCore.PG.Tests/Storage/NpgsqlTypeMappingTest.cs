using System.Collections;
using System.Collections.Immutable;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Npgsql.NameTranslation;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage;

public class NpgsqlTypeMappingTest
{
    #region Numeric

    [Fact]
    public void GenerateSqlLiteral_returns_decimal_literal()
    {
        Assert.Equal(
            "1.878787",
            GetMapping(typeof(decimal), "numeric").GenerateSqlLiteral(1.878787m));

        Assert.Equal(
            "1.878787",
            GetMapping(typeof(float), "numeric").GenerateSqlLiteral(1.878787m));

        Assert.Equal(
            "1.878787",
            GetMapping(typeof(double), "numeric").GenerateSqlLiteral(1.878787m));
    }

    #endregion Numeric

    #region Date/Time

    [Fact]
    public void DateTime_type_maps_to_timestamptz_by_default()
        => Assert.Equal("timestamp with time zone", GetMapping(typeof(DateTime)).StoreType);

    [Fact]
    public void Timestamp_maps_to_DateTime_by_default()
        => Assert.Same(typeof(DateTime), GetMapping("timestamp without time zone").ClrType);

    [Fact]
    public void Timestamptz_maps_to_DateTime_by_default()
        => Assert.Same(typeof(DateTime), GetMapping("timestamp with time zone").ClrType);

    [Fact]
    public void DateTime_with_precision()
        => Assert.Equal(
            "timestamp(3) with time zone",
            Mapper.FindMapping(typeof(DateTime), "timestamp with time zone", precision: 3)!.StoreType);

    [Fact]
    public void GenerateSqlLiteral_returns_date_literal()
    {
        Assert.Equal(
            "DATE '2015-03-12'",
            GetMapping(typeof(DateTime), "date").GenerateSqlLiteral(new DateTime(2015, 3, 12)));

        Assert.Equal(
            "DATE '2015-03-12'",
            GetMapping("date").GenerateSqlLiteral(new DateOnly(2015, 3, 12)));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_date_infinity_literals()
    {
        Assert.Equal(
            "DATE '-infinity'",
            GetMapping(typeof(DateTime), "date").GenerateSqlLiteral(DateTime.MinValue));

        Assert.Equal(
            "DATE 'infinity'",
            GetMapping(typeof(DateTime), "date").GenerateSqlLiteral(DateTime.MaxValue));

        Assert.Equal(
            "DATE '-infinity'",
            GetMapping("date").GenerateSqlLiteral(DateOnly.MinValue));

        Assert.Equal(
            "DATE 'infinity'",
            GetMapping("date").GenerateSqlLiteral(DateOnly.MaxValue));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_timestamp_literal()
    {
        var mapping = GetMapping("timestamp without time zone");
        Assert.Equal(
            "TIMESTAMP '1997-12-17T07:37:16'",
            mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Local)));
        Assert.Equal(
            "TIMESTAMP '1997-12-17T07:37:16'",
            mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Unspecified)));
        Assert.Equal(
            "TIMESTAMP '1997-12-17T07:37:16.345'",
            mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, 345)));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_timestamp_infinity_literals()
    {
        Assert.Equal(
            "TIMESTAMP '-infinity'",
            GetMapping("timestamp without time zone").GenerateSqlLiteral(DateTime.MinValue));

        Assert.Equal(
            "TIMESTAMP 'infinity'",
            GetMapping("timestamp without time zone").GenerateSqlLiteral(DateTime.MaxValue));
    }

    [Fact]
    public void GenerateSqlLiteral_timestamp_does_not_support_utc_datetime()
        => Assert.Throws<ArgumentException>(() => GetMapping("timestamp without time zone").GenerateSqlLiteral(DateTime.UtcNow));

    [Fact]
    public void GenerateSqlLiteral_timestamp_does_not_support_datetimeoffset()
        => Assert.Throws<InvalidCastException>(
            () => GetMapping("timestamp without time zone").GenerateSqlLiteral(new DateTimeOffset()));

    [Fact]
    public void GenerateCodeLiteral_returns_DateTime_utc_literal()
        => Assert.Equal(
            @"new DateTime(2020, 1, 1, 12, 30, 4, 567, DateTimeKind.Utc)",
            CodeLiteral(new DateTime(2020, 1, 1, 12, 30, 4, 567, DateTimeKind.Utc)));

    [Fact]
    public void GenerateCodeLiteral_returns_DateTime_local_literal()
        => Assert.Equal(
            @"new DateTime(2020, 1, 1, 12, 30, 4, 567, DateTimeKind.Local)",
            CodeLiteral(new DateTime(2020, 1, 1, 12, 30, 4, 567, DateTimeKind.Local)));

    [Fact]
    public void GenerateCodeLiteral_returns_DateTime_unspecified_literal()
        => Assert.Equal(
            @"new DateTime(2020, 1, 1, 12, 30, 4, 567, DateTimeKind.Unspecified)",
            CodeLiteral(new DateTime(2020, 1, 1, 12, 30, 4, 567, DateTimeKind.Unspecified)));

    [Fact]
    public void GenerateSqlLiteral_returns_timestamptz_datetime_literal()
    {
        var mapping = GetMapping("timestamptz");
        Assert.Equal(
            "TIMESTAMPTZ '1997-12-17T07:37:16Z'",
            mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Utc)));
        Assert.Equal(
            "TIMESTAMPTZ '1997-12-17T07:37:16.345678Z'",
            mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, 345, DateTimeKind.Utc).AddTicks(6780)));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_timestamptz_infinity_literals()
    {
        Assert.Equal(
            "TIMESTAMPTZ '-infinity'",
            GetMapping("timestamptz").GenerateSqlLiteral(DateTime.MinValue));

        Assert.Equal(
            "TIMESTAMPTZ 'infinity'",
            GetMapping("timestamptz").GenerateSqlLiteral(DateTime.MaxValue));

        Assert.Equal(
            "TIMESTAMPTZ '-infinity'",
            GetMapping(typeof(DateTimeOffset), "timestamptz").GenerateSqlLiteral(DateTimeOffset.MinValue));

        Assert.Equal(
            "TIMESTAMPTZ 'infinity'",
            GetMapping(typeof(DateTimeOffset), "timestamptz").GenerateSqlLiteral(DateTimeOffset.MaxValue));
    }

    [Fact]
    public void GenerateSqlLiteral_timestamptz_does_not_support_local_datetime()
        => Assert.Throws<ArgumentException>(() => GetMapping("timestamp with time zone").GenerateSqlLiteral(DateTime.Now));

    [Fact]
    public void GenerateSqlLiteral_timestamptz_does_not_support_unspecified_datetime()
        => Assert.Throws<ArgumentException>(
            () => GetMapping("timestamp with time zone")
                .GenerateSqlLiteral(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)));

    [Fact]
    public void GenerateSqlLiteral_returns_timestamptz_datetimeoffset_literal()
    {
        var mapping = GetMapping("timestamptz");
        Assert.Equal(
            "TIMESTAMPTZ '1997-12-17T07:37:16+02:00'",
            mapping.GenerateSqlLiteral(new DateTimeOffset(1997, 12, 17, 7, 37, 16, TimeSpan.FromHours(2))));
        Assert.Equal(
            "TIMESTAMPTZ '1997-12-17T07:37:16.345+02:00'",
            mapping.GenerateSqlLiteral(new DateTimeOffset(1997, 12, 17, 7, 37, 16, 345, TimeSpan.FromHours(2))));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_time_literal()
    {
        var mapping = GetMapping("time");

        Assert.Equal(
            "TIME '04:05:06.123456'",
            mapping.GenerateSqlLiteral(new TimeSpan(0, 4, 5, 6, 123).Add(TimeSpan.FromTicks(4560))));
        Assert.Equal(
            "TIME '04:05:06.000123'",
            mapping.GenerateSqlLiteral(new TimeSpan(0, 4, 5, 6).Add(TimeSpan.FromTicks(1230))));
        Assert.Equal("TIME '04:05:06.123'", mapping.GenerateSqlLiteral(new TimeSpan(0, 4, 5, 6, 123)));
        Assert.Equal("TIME '04:05:06'", mapping.GenerateSqlLiteral(new TimeSpan(4, 5, 6)));

        Assert.Equal(
            "TIME '04:05:06.123456'",
            mapping.GenerateSqlLiteral(new TimeOnly(4, 5, 6, 123).Add(TimeSpan.FromTicks(4560))));
        Assert.Equal(
            "TIME '04:05:06.000123'",
            mapping.GenerateSqlLiteral(new TimeOnly(4, 5, 6).Add(TimeSpan.FromTicks(1230))));
        Assert.Equal("TIME '04:05:06.123'", mapping.GenerateSqlLiteral(new TimeOnly(4, 5, 6, 123)));
        Assert.Equal("TIME '04:05:06'", mapping.GenerateSqlLiteral(new TimeOnly(4, 5, 6)));
        Assert.Equal("TIME '13:05:06'", mapping.GenerateSqlLiteral(new TimeOnly(13, 5, 6)));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_timetz_literal()
    {
        var mapping = GetMapping("timetz");
        Assert.Equal(
            "TIMETZ '04:05:06.123456+3'",
            mapping.GenerateSqlLiteral(
                new DateTimeOffset(2015, 3, 12, 4, 5, 6, 123, TimeSpan.FromHours(3))
                    .AddTicks(4560)));
        Assert.Equal(
            "TIMETZ '04:05:06.789+3'", mapping.GenerateSqlLiteral(new DateTimeOffset(2015, 3, 12, 4, 5, 6, 789, TimeSpan.FromHours(3))));
        Assert.Equal("TIMETZ '04:05:06-3'", mapping.GenerateSqlLiteral(new DateTimeOffset(2015, 3, 12, 4, 5, 6, TimeSpan.FromHours(-3))));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_interval_literal()
    {
        var mapping = GetMapping("interval");
        Assert.Equal(
            "INTERVAL '3 04:05:06.007008'", mapping.GenerateSqlLiteral(
                new TimeSpan(3, 4, 5, 6, 7)
                    .Add(TimeSpan.FromTicks(80))));
        Assert.Equal("INTERVAL '3 04:05:06.007'", mapping.GenerateSqlLiteral(new TimeSpan(3, 4, 5, 6, 7)));
        Assert.Equal("INTERVAL '3 04:05:06'", mapping.GenerateSqlLiteral(new TimeSpan(3, 4, 5, 6)));
        Assert.Equal("INTERVAL '04:05:06'", mapping.GenerateSqlLiteral(new TimeSpan(4, 5, 6)));
        Assert.Equal("INTERVAL '-3 04:05:06.007'", mapping.GenerateSqlLiteral(new TimeSpan(-3, -4, -5, -6, -7)));
    }

    #endregion Date/Time

    #region Networking

    [Fact]
    public void GenerateSqlLiteral_returns_macaddr_literal()
        => Assert.Equal("MACADDR '001122334455'", GetMapping("macaddr").GenerateSqlLiteral(PhysicalAddress.Parse("00-11-22-33-44-55")));

    [Fact]
    public void GenerateCodeLiteral_returns_macaddr_literal()
        => Assert.Equal(
            @"System.Net.NetworkInformation.PhysicalAddress.Parse(""001122334455"")",
            CodeLiteral(PhysicalAddress.Parse("00-11-22-33-44-55")));

    [Fact]
    public void GenerateSqlLiteral_returns_macaddr8_literal()
        => Assert.Equal(
            "MACADDR8 '0011223344556677'", GetMapping("macaddr8").GenerateSqlLiteral(PhysicalAddress.Parse("00-11-22-33-44-55-66-77")));

    [Fact]
    public void GenerateCodeLiteral_returns_macaddr8_literal()
        => Assert.Equal(
            @"System.Net.NetworkInformation.PhysicalAddress.Parse(""0011223344556677"")",
            CodeLiteral(PhysicalAddress.Parse("00-11-22-33-44-55-66-77")));

    [Fact]
    public void GenerateSqlLiteral_returns_inet_literal()
        => Assert.Equal("INET '192.168.1.1'", GetMapping("inet").GenerateSqlLiteral(IPAddress.Parse("192.168.1.1")));

    [Fact]
    public void GenerateCodeLiteral_returns_inet_literal()
        => Assert.Equal(@"System.Net.IPAddress.Parse(""192.168.1.1"")", CodeLiteral(IPAddress.Parse("192.168.1.1")));

    [Fact]
    public void GenerateSqlLiteral_returns_cidr_literal()
        => Assert.Equal("CIDR '192.168.1.0/24'", GetMapping("cidr").GenerateSqlLiteral(new NpgsqlCidr(IPAddress.Parse("192.168.1.0"), 24)));

    [Fact]
    public void GenerateCodeLiteral_returns_cidr_literal()
        => Assert.Equal(
            @"new NpgsqlTypes.NpgsqlCidr(System.Net.IPAddress.Parse(""192.168.1.0""), (byte)24)",
            CodeLiteral(new NpgsqlCidr(IPAddress.Parse("192.168.1.0"), 24)));

    #endregion Networking

    #region Geometric

    [Fact]
    public void GenerateSqlLiteral_returns_point_literal()
        => Assert.Equal("POINT '(3.5,4.5)'", GetMapping("point").GenerateSqlLiteral(new NpgsqlPoint(3.5, 4.5)));

    [Fact]
    public void GenerateCodeLiteral_returns_point_literal()
        => Assert.Equal("new NpgsqlTypes.NpgsqlPoint(3.5, 4.5)", CodeLiteral(new NpgsqlPoint(3.5, 4.5)));

    [Fact]
    public void GenerateSqlLiteral_returns_line_literal()
        => Assert.Equal("LINE '{3.5,4.5,10}'", GetMapping("line").GenerateSqlLiteral(new NpgsqlLine(3.5, 4.5, 10)));

    [Fact]
    public void GenerateCodeLiteral_returns_line_literal()
        => Assert.Equal("new NpgsqlTypes.NpgsqlLine(3.5, 4.5, 10.0)", CodeLiteral(new NpgsqlLine(3.5, 4.5, 10)));

    [Fact]
    public void GenerateSqlLiteral_returns_lseg_literal()
        => Assert.Equal("LSEG '[(3.5,4.5),(5.5,6.5)]'", GetMapping("lseg").GenerateSqlLiteral(new NpgsqlLSeg(3.5, 4.5, 5.5, 6.5)));

    [Fact]
    public void GenerateCodeLiteral_returns_lseg_literal()
        => Assert.Equal("new NpgsqlTypes.NpgsqlLSeg(3.5, 4.5, 5.5, 6.5)", CodeLiteral(new NpgsqlLSeg(3.5, 4.5, 5.5, 6.5)));

    [Fact]
    public void GenerateSqlLiteral_returns_box_literal()
        => Assert.Equal("BOX '((4,3),(2,1))'", GetMapping("box").GenerateSqlLiteral(new NpgsqlBox(1, 2, 3, 4)));

    [Fact]
    public void GenerateCodeLiteral_returns_box_literal()
        => Assert.Equal("new NpgsqlTypes.NpgsqlBox(3.0, 4.0, 1.0, 2.0)", CodeLiteral(new NpgsqlBox(1, 2, 3, 4)));

    [Fact]
    public void GenerateSqlLiteral_returns_path_closed_literal()
        => Assert.Equal(
            "PATH '((1,2),(3,4))'", GetMapping("path").GenerateSqlLiteral(
                new NpgsqlPath(
                    new NpgsqlPoint(1, 2),
                    new NpgsqlPoint(3, 4)
                )));

    [Fact]
    public void GenerateCodeLiteral_returns_closed_path_literal()
        => Assert.Equal(
            "new NpgsqlTypes.NpgsqlPath(new NpgsqlPoint[] { new NpgsqlTypes.NpgsqlPoint(1.0, 2.0), new NpgsqlTypes.NpgsqlPoint(3.0, 4.0) }, false)",
            CodeLiteral(
                new NpgsqlPath(
                    new NpgsqlPoint(1, 2),
                    new NpgsqlPoint(3, 4)
                )));

    [Fact]
    public void GenerateSqlLiteral_returns_path_open_literal()
        => Assert.Equal(
            "PATH '[(1,2),(3,4)]'", GetMapping("path").GenerateSqlLiteral(
                new NpgsqlPath(
                    new NpgsqlPoint(1, 2),
                    new NpgsqlPoint(3, 4)
                ) { Open = true }));

    [Fact]
    public void GenerateCodeLiteral_returns_open_path_literal()
        => Assert.Equal(
            "new NpgsqlTypes.NpgsqlPath(new NpgsqlPoint[] { new NpgsqlTypes.NpgsqlPoint(1.0, 2.0), new NpgsqlTypes.NpgsqlPoint(3.0, 4.0) }, true)",
            CodeLiteral(
                new NpgsqlPath(
                    new NpgsqlPoint(1, 2),
                    new NpgsqlPoint(3, 4)
                ) { Open = true }));

    [Fact]
    public void GenerateSqlLiteral_returns_polygon_literal()
        => Assert.Equal(
            "POLYGON '((1,2),(3,4))'", GetMapping("polygon").GenerateSqlLiteral(
                new NpgsqlPolygon(
                    new NpgsqlPoint(1, 2),
                    new NpgsqlPoint(3, 4)
                )));

    [Fact]
    public void GenerateCodeLiteral_returns_polygon_literal()
        => Assert.Equal(
            "new NpgsqlTypes.NpgsqlPolygon(new NpgsqlPoint[] { new NpgsqlTypes.NpgsqlPoint(1.0, 2.0), new NpgsqlTypes.NpgsqlPoint(3.0, 4.0) })",
            CodeLiteral(
                new NpgsqlPolygon(
                    new NpgsqlPoint(1, 2),
                    new NpgsqlPoint(3, 4)
                )));

    [Fact]
    public void GenerateSqlLiteral_returns_circle_literal()
        => Assert.Equal("CIRCLE '<(3.5,4.5),5.5>'", GetMapping("circle").GenerateSqlLiteral(new NpgsqlCircle(3.5, 4.5, 5.5)));

    [Fact]
    public void GenerateCodeLiteral_returns_circle_literal()
        => Assert.Equal("new NpgsqlTypes.NpgsqlCircle(3.5, 4.5, 5.5)", CodeLiteral(new NpgsqlCircle(3.5, 4.5, 5.5)));

    #endregion Geometric

    #region Misc

    [Fact]
    public void GenerateSqlLiteral_returns_bool_literal()
        => Assert.Equal("TRUE", GetMapping("bool").GenerateSqlLiteral(true));

    [Fact]
    public void GenerateSqlLiteral_returns_varbit_literal()
        => Assert.Equal("B'10'", GetMapping("varbit").GenerateSqlLiteral(new BitArray(new[] { true, false })));

    [Fact]
    public void GenerateCodeLiteral_returns_varbit_literal()
        => Assert.Equal("new System.Collections.BitArray(new bool[] { true, false })", CodeLiteral(new BitArray(new[] { true, false })));

    [Fact]
    public void GenerateSqlLiteral_returns_bit_literal()
        => Assert.Equal("B'10'", GetMapping("bit").GenerateSqlLiteral(new BitArray(new[] { true, false })));

    [Fact]
    public void GenerateCodeLiteral_returns_bit_literal()
        => Assert.Equal("new System.Collections.BitArray(new bool[] { true, false })", CodeLiteral(new BitArray(new[] { true, false })));

    [Fact(Skip = "https://github.com/dotnet/efcore/pull/30939")]
    public void ValueComparer_hstore_array()
    {
        // This exercises array's comparer when the element has its own non-null comparer
        var source = new[] { new Dictionary<string, string> { { "k1", "v1" } }, new Dictionary<string, string> { { "k2", "v2" } }, };

        var comparer = GetMapping(typeof(Dictionary<string, string>[])).Comparer;
        var snapshot = (Dictionary<string, string>[])comparer.Snapshot(source);
        Assert.Equal(source, snapshot);
        Assert.NotSame(source, snapshot);
        Assert.True(comparer.Equals(source, snapshot));
        source[1]["k2"] = "v8";
        Assert.False(comparer.Equals(source, snapshot));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_bytea_literal()
        => Assert.Equal(@"BYTEA E'\\xDEADBEEF'", GetMapping("bytea").GenerateSqlLiteral(new byte[] { 222, 173, 190, 239 }));

    [Fact]
    public void GenerateSqlLiteral_returns_hstore_literal()
        => Assert.Equal(
            @"HSTORE '""k1""=>""v1"",""k2""=>""v2""'",
            GetMapping("hstore").GenerateSqlLiteral(new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" } }));

    [Fact]
    public void GenerateSqlLiteral_returns_BigInteger_literal()
    {
        var mapping = GetMapping(typeof(BigInteger));

        Assert.Equal(@"0", mapping.GenerateSqlLiteral(BigInteger.Zero));
        Assert.Equal(int.MaxValue.ToString(), mapping.GenerateSqlLiteral(new BigInteger(int.MaxValue)));
        Assert.Equal(int.MinValue.ToString(), mapping.GenerateSqlLiteral(new BigInteger(int.MinValue)));
    }

    [Fact]
    public void GenerateCodeLiteral_returns_BigInteger_literal()
        => Assert.Equal(
            @"BigInteger.Parse(""18446744073709551615"", NumberFormatInfo.InvariantInfo)",
            CodeLiteral(new BigInteger(ulong.MaxValue)));

    [Fact]
    public void ValueComparer_hstore_as_Dictionary()
    {
        var source = new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" } };

        var comparer = GetMapping("hstore").Comparer;
        var snapshot = (Dictionary<string, string>)comparer.Snapshot(source);
        Assert.Equal(source, snapshot);
        Assert.NotSame(source, snapshot);
        Assert.True(comparer.Equals(source, snapshot));
        snapshot.Remove("k1");
        Assert.False(comparer.Equals(source, snapshot));
    }

    [Fact]
    public void ValueComparer_hstore_as_ImmutableDictionary()
    {
        var source = ImmutableDictionary<string, string>.Empty
            .Add("k1", "v1")
            .Add("k2", "v2");

        var comparer = Mapper.FindMapping(typeof(ImmutableDictionary<string, string>), "hstore").Comparer;
        var snapshot = comparer.Snapshot(source);
        Assert.Equal(source, snapshot);
        Assert.True(comparer.Equals(source, snapshot));
        source = source.Remove("k1");
        Assert.False(comparer.Equals(source, snapshot));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_enum_literal()
    {
        var mapping = new NpgsqlEnumTypeMapping("dummy_enum", typeof(DummyEnum), new NpgsqlSnakeCaseNameTranslator());

        Assert.Equal("'sad'::dummy_enum", mapping.GenerateSqlLiteral(DummyEnum.Sad));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_enum_uppercase_literal()
    {
        var mapping = new NpgsqlEnumTypeMapping(@"""DummyEnum""", typeof(DummyEnum), new NpgsqlSnakeCaseNameTranslator());

        Assert.Equal(@"'sad'::""DummyEnum""", mapping.GenerateSqlLiteral(DummyEnum.Sad));
    }

    private enum DummyEnum
    {
        // ReSharper disable once UnusedMember.Local
        Happy,
        Sad
    }

    [Fact]
    public void GenerateSqlLiteral_returns_tid_literal()
        => Assert.Equal(@"TID '(0,1)'", GetMapping("tid").GenerateSqlLiteral(new NpgsqlTid(0, 1)));

    [Fact]
    public void GenerateSqlLiteral_returns_pg_lsn_literal()
        => Assert.Equal(@"PG_LSN '12345/67890'", GetMapping("pg_lsn").GenerateSqlLiteral(NpgsqlLogSequenceNumber.Parse("12345/67890")));

    #endregion Misc

    #region Array

    [Fact]
    public void GenerateSqlLiteral_returns_array_literal()
        => Assert.Equal("ARRAY[3,4]::integer[]", GetMapping(typeof(int[])).GenerateSqlLiteral(new[] { 3, 4 }));

    [Fact]
    public void GenerateSqlLiteral_returns_array_empty_literal()
        => Assert.Equal("ARRAY[]::smallint[]", GetMapping(typeof(short[])).GenerateSqlLiteral(Array.Empty<short>()));

    [Fact(Skip = "https://github.com/dotnet/efcore/pull/30939")]
    public void ValueComparer_int_array()
    {
        // This exercises array's comparer when the element doesn't have a comparer, but it implements
        // IEquatable<T>
        var source = new[] { 2, 3, 4 };

        var comparer = GetMapping(typeof(int[])).Comparer;
        var snapshot = (int[])comparer.Snapshot(source);
        Assert.Equal(source, snapshot);
        Assert.NotSame(source, snapshot);
        Assert.True(comparer.Equals(source, snapshot));
        snapshot[1] = 8;
        Assert.False(comparer.Equals(source, snapshot));
    }

    [Fact(Skip = "https://github.com/dotnet/efcore/pull/30939")]
    public void ValueComparer_int_list()
    {
        var source = new List<int>
        {
            2,
            3,
            4
        };

        var comparer = GetMapping(typeof(List<int>)).Comparer;
        var snapshot = (List<int>)comparer.Snapshot(source);
        Assert.Equal(source, snapshot);
        Assert.NotSame(source, snapshot);
        Assert.True(comparer.Equals(source, snapshot));
        snapshot[1] = 8;
        Assert.False(comparer.Equals(source, snapshot));
    }

    [Fact(Skip = "https://github.com/dotnet/efcore/pull/30939")]
    public void ValueComparer_nullable_int_array()
    {
        var source = new int?[] { 2, 3, 4, null };

        var comparer = GetMapping(typeof(int?[])).Comparer;
        var snapshot = (int?[])comparer.Snapshot(source);
        Assert.Equal(source, snapshot);
        Assert.NotSame(source, snapshot);
        Assert.True(comparer.Equals(source, snapshot));
        snapshot[1] = 8;
        Assert.False(comparer.Equals(source, snapshot));
    }

    [Fact(Skip = "https://github.com/dotnet/efcore/pull/30939")]
    public void ValueComparer_nullable_int_list()
    {
        var source = new List<int?>
        {
            2,
            3,
            4,
            null
        };

        var comparer = GetMapping(typeof(List<int?>)).Comparer;
        var snapshot = (List<int?>)comparer.Snapshot(source);
        Assert.Equal(source, snapshot);
        Assert.NotSame(source, snapshot);
        Assert.True(comparer.Equals(source, snapshot));
        snapshot[1] = 8;
        Assert.False(comparer.Equals(source, snapshot));
    }

    [Fact(Skip = "https://github.com/dotnet/efcore/pull/30939")]
    public void ValueComparer_nullable_array_with_iequatable_element()
    {
        var source = new NpgsqlPoint?[] { new NpgsqlPoint(1, 1), null };

        var comparer = GetMapping(typeof(NpgsqlPoint?[])).Comparer;
        var snapshot = (NpgsqlPoint?[])comparer.Snapshot(source);
        Assert.Equal(source, snapshot);
        Assert.NotSame(source, snapshot);
        Assert.True(comparer.Equals(source, snapshot));
        snapshot[1] = new NpgsqlPoint(2, 2);
        Assert.False(comparer.Equals(source, snapshot));
    }

    #endregion Array

    #region Ranges

    [Fact]
    public void GenerateSqlLiteral_returns_range_empty_literal()
    {
        var value = NpgsqlRange<int>.Empty;
        var literal = GetMapping("int4range").GenerateSqlLiteral(value);
        Assert.Equal("'empty'::int4range", literal);
    }

    [Fact]
    public void GenerateCodeLiteral_returns_range_empty_literal()
        => Assert.Equal("new NpgsqlTypes.NpgsqlRange<int>(0, false, 0, false)", CodeLiteral(NpgsqlRange<int>.Empty));

    [Fact]
    public void GenerateSqlLiteral_returns_range_inclusive_literal()
    {
        var value = new NpgsqlRange<int>(4, 7);
        var literal = GetMapping("int4range").GenerateSqlLiteral(value);
        Assert.Equal("'[4,7]'::int4range", literal);
    }

    [Fact]
    public void GenerateCodeLiteral_returns_range_inclusive_literal()
        => Assert.Equal("new NpgsqlTypes.NpgsqlRange<int>(4, 7)", CodeLiteral(new NpgsqlRange<int>(4, 7)));

    [Fact]
    public void GenerateSqlLiteral_returns_range_inclusive_exclusive_literal()
    {
        var value = new NpgsqlRange<int>(4, false, 7, true);
        var literal = GetMapping("int4range").GenerateSqlLiteral(value);
        Assert.Equal("'(4,7]'::int4range", literal);
    }

    [Fact]
    public void GenerateCodeLiteral_returns_range_inclusive_exclusive_literal()
        => Assert.Equal("new NpgsqlTypes.NpgsqlRange<int>(4, false, 7, true)", CodeLiteral(new NpgsqlRange<int>(4, false, 7, true)));

    [Fact]
    public void GenerateSqlLiteral_returns_range_infinite_literal()
    {
        var value = new NpgsqlRange<int>(0, false, true, 7, true, false);
        var literal = GetMapping("int4range").GenerateSqlLiteral(value);
        Assert.Equal("'(,7]'::int4range", literal);
    }

    [Fact]
    public void GenerateCodeLiteral_returns_range_infinite_literal()
        => Assert.Equal(
            "new NpgsqlTypes.NpgsqlRange<int>(0, false, true, 7, true, false)",
            CodeLiteral(new NpgsqlRange<int>(0, false, true, 7, true, false)));

    // Tests for the built-in ranges

    [Fact]
    public void GenerateSqlLiteral_returns_int4range_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping("int4range");
        Assert.Equal("integer", mapping.SubtypeMapping.StoreType);

        var value = new NpgsqlRange<int>(4, 7);
        Assert.Equal("'[4,7]'::int4range", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_int8range_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping("int8range");
        Assert.Equal("bigint", mapping.SubtypeMapping.StoreType);

        var value = new NpgsqlRange<long>(4, 7);
        Assert.Equal("'[4,7]'::int8range", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_numrange_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping("numrange");
        Assert.Equal("numeric", mapping.SubtypeMapping.StoreType);

        var value = new NpgsqlRange<decimal>(4, 7);
        Assert.Equal("'[4.0,7.0]'::numrange", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_tsrange_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping("tsrange");
        Assert.Equal("timestamp without time zone", mapping.SubtypeMapping.StoreType);

        var value = new NpgsqlRange<DateTime>(new DateTime(2020, 1, 1, 12, 0, 0), new DateTime(2020, 1, 2, 12, 0, 0));
        Assert.Equal(@"'[""2020-01-01T12:00:00"",""2020-01-02T12:00:00""]'::tsrange", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_tstzrange_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping("tstzrange");
        Assert.Equal("timestamp with time zone", mapping.SubtypeMapping.StoreType);

        var value = new NpgsqlRange<DateTime>(
            new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 2, 12, 0, 0, DateTimeKind.Utc));
        Assert.Equal(@"'[""2020-01-01T12:00:00Z"",""2020-01-02T12:00:00Z""]'::tstzrange", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_daterange_DateOnly_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping("daterange");
        Assert.Equal("date", mapping.SubtypeMapping.StoreType);
        Assert.Equal("date", ((NpgsqlRangeTypeMapping)GetMapping(typeof(NpgsqlRange<DateOnly>))).SubtypeMapping.StoreType);

        var value = new NpgsqlRange<DateOnly>(new DateOnly(2020, 1, 1), new DateOnly(2020, 1, 2));
        Assert.Equal(@"'[2020-01-01,2020-01-02]'::daterange", mapping.GenerateSqlLiteral(value));
    }

    [Fact]
    public void GenerateSqlLiteral_returns_daterange_DateTime_literal()
    {
        var mapping = (NpgsqlRangeTypeMapping)GetMapping(typeof(NpgsqlRange<DateTime>), "daterange");
        Assert.Equal("date", mapping.SubtypeMapping.StoreType);

        var value = new NpgsqlRange<DateTime>(new DateTime(2020, 1, 1), new DateTime(2020, 1, 2));
        Assert.Equal(@"'[2020-01-01,2020-01-02]'::daterange", mapping.GenerateSqlLiteral(value));
    }

    #endregion Ranges

    #region Multiranges

    [Fact]
    public void GenerateSqlLiteral_returns_multirange_literal()
    {
        var value = new NpgsqlRange<int>[]
        {
            new(4, 7),
            new(9, lowerBoundIsInclusive: true, 10, upperBoundIsInclusive: false),
            new(
                13, lowerBoundIsInclusive: false, lowerBoundInfinite: false, default, upperBoundIsInclusive: false,
                upperBoundInfinite: true)
        };
        var literal = GetMapping("int4multirange").GenerateSqlLiteral(value);
        Assert.Equal("'{[4,7], [9,10), (13,)}'::int4multirange", literal);
    }

    [Fact]
    public void GenerateCodeLiteral_returns_multirange_array_literal()
    {
        var value = new NpgsqlRange<int>[]
        {
            new(4, 7),
            new(9, lowerBoundIsInclusive: true, 10, upperBoundIsInclusive: false),
            new(
                13, lowerBoundIsInclusive: false, lowerBoundInfinite: false, default, upperBoundIsInclusive: false,
                upperBoundInfinite: true)
        };
        var literal = CodeLiteral(value);
        Assert.Equal(
            "new[] { new NpgsqlTypes.NpgsqlRange<int>(4, 7), new NpgsqlTypes.NpgsqlRange<int>(9, true, 10, false), new NpgsqlTypes.NpgsqlRange<int>(13, false, false, 0, false, true) }",
            literal);
    }

    [Fact]
    public void GenerateCodeLiteral_returns_multirange_list_literal()
    {
        var value = new List<NpgsqlRange<int>>
        {
            new(4, 7),
            new(9, lowerBoundIsInclusive: true, 10, upperBoundIsInclusive: false),
            new(
                13, lowerBoundIsInclusive: false, lowerBoundInfinite: false, default, upperBoundIsInclusive: false,
                upperBoundInfinite: true)
        };
        var literal = CodeLiteral(value);
        Assert.Equal(
            "new List<NpgsqlRange<int>> { new NpgsqlTypes.NpgsqlRange<int>(4, 7), new NpgsqlTypes.NpgsqlRange<int>(9, true, 10, false), new NpgsqlTypes.NpgsqlRange<int>(13, false, false, 0, false, true) }",
            literal);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_multirange_empty_literal()
    {
        var value = Array.Empty<NpgsqlRange<int>>();
        var literal = GetMapping("int4multirange").GenerateSqlLiteral(value);
        Assert.Equal("'{}'::int4multirange", literal);
    }

    [Fact]
    public void GenerateCodeLiteral_returns_multirange_empty_array_literal()
    {
        var value = Array.Empty<NpgsqlRange<int>>();
        var literal = CodeLiteral(value);
        Assert.Equal("new NpgsqlRange<int>[0]", literal);
    }

    #endregion Multiranges

    #region Full text search

#pragma warning disable CS0618 // Full-text search client-parsing is obsolete
    [Fact]
    public void GenerateSqlLiteral_returns_tsquery_literal()
        => Assert.Equal(
            @"TSQUERY '''a'' & ''b'''",
            GetMapping("tsquery").GenerateSqlLiteral(NpgsqlTsQuery.Parse("a & b")));

    [Fact]
    public void GenerateSqlLiteral_returns_tsvector_literal()
        => Assert.Equal(
            @"TSVECTOR '''a'' ''b'''",
            GetMapping("tsvector").GenerateSqlLiteral(NpgsqlTsVector.Parse("a b")));
#pragma warning restore CS0618

    [Fact]
    public void GenerateSqlLiteral_returns_ranking_normalization_literal()
        => Assert.Equal(
            $"{(int)NpgsqlTsRankingNormalization.DivideByLength}",
            GetMapping(typeof(NpgsqlTsRankingNormalization))
                .GenerateSqlLiteral(NpgsqlTsRankingNormalization.DivideByLength));

    #endregion Full text search

    #region Json

    [Fact]
    public void GenerateSqlLiteral_returns_jsonb_string_literal()
        => Assert.Equal("""'{"a":1}'""", GetMapping("jsonb").GenerateSqlLiteral("""{"a":1}"""));

    [Fact]
    public void GenerateSqlLiteral_returns_json_string_literal()
        => Assert.Equal("""'{"a":1}'""", GetMapping("json").GenerateSqlLiteral("""{"a":1}"""));

    [Fact]
    public void GenerateSqlLiteral_returns_jsonb_object_literal()
    {
        var literal = Mapper.FindMapping(typeof(Customer), "jsonb").GenerateSqlLiteral(SampleCustomer);
        Assert.Equal(
            """'{"Name":"Joe","Age":25,"IsVip":false,"Orders":[{"Price":99.5,"ShippingAddress":"Some address 1","ShippingDate":"2019-10-01T00:00:00"},{"Price":23,"ShippingAddress":"Some address 2","ShippingDate":"2019-10-10T00:00:00"}]}'""",
            literal);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_json_object_literal()
    {
        var literal = Mapper.FindMapping(typeof(Customer), "json").GenerateSqlLiteral(SampleCustomer);
        Assert.Equal(
            """'{"Name":"Joe","Age":25,"IsVip":false,"Orders":[{"Price":99.5,"ShippingAddress":"Some address 1","ShippingDate":"2019-10-01T00:00:00"},{"Price":23,"ShippingAddress":"Some address 2","ShippingDate":"2019-10-10T00:00:00"}]}'""",
            literal);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_jsonb_document_literal()
    {
        var json = """{"Name":"Joe","Age":25}""";
        var literal = Mapper.FindMapping(typeof(JsonDocument), "jsonb").GenerateSqlLiteral(JsonDocument.Parse(json));
        Assert.Equal($"'{json}'", literal);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_json_document_literal()
    {
        var json = """{"Name":"Joe","Age":25}""";
        var literal = Mapper.FindMapping(typeof(JsonDocument), "json").GenerateSqlLiteral(JsonDocument.Parse(json));
        Assert.Equal($"'{json}'", literal);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_jsonb_element_literal()
    {
        var json = """{"Name":"Joe","Age":25}""";
        var literal = Mapper.FindMapping(typeof(JsonElement), "jsonb").GenerateSqlLiteral(JsonDocument.Parse(json).RootElement);
        Assert.Equal($"'{json}'", literal);
    }

    [Fact]
    public void GenerateSqlLiteral_returns_json_element_literal()
    {
        var json = """{"Name":"Joe","Age":25}""";
        var literal = Mapper.FindMapping(typeof(JsonElement), "json").GenerateSqlLiteral(JsonDocument.Parse(json).RootElement);
        Assert.Equal($"'{json}'", literal);
    }

    [Fact]
    public void GenerateCodeLiteral_returns_json_document_literal()
        => Assert.Equal(
            """System.Text.Json.JsonDocument.Parse("{\"Name\":\"Joe\",\"Age\":25}", new System.Text.Json.JsonDocumentOptions())""",
            CodeLiteral(JsonDocument.Parse(@"{""Name"":""Joe"",""Age"":25}")));

    [Fact]
    public void GenerateCodeLiteral_returns_json_element_literal()
        // TODO: https://github.com/dotnet/efcore/issues/32192
        => Assert.Throws<NotSupportedException>(() => CodeLiteral(JsonDocument.Parse(@"{""Name"":""Joe"",""Age"":25}").RootElement));

    // Assert.Equal(
    //     """System.Text.Json.JsonDocument.Parse("{\"Name\":\"Joe\",\"Age\":25}", new System.Text.Json.JsonDocumentOptions()).RootElement""",
    //     CodeLiteral(JsonDocument.Parse(@"{""Name"":""Joe"",""Age"":25}").RootElement));
    [Fact]
    public void ValueComparer_JsonDocument()
    {
        var json = """{"Name":"Joe","Age":25}""";
        var source = JsonDocument.Parse(json);

        var comparer = GetMapping(typeof(JsonDocument)).Comparer;
        var snapshot = (JsonDocument)comparer.Snapshot(source);
        Assert.Same(source, snapshot);
        Assert.True(comparer.Equals(source, snapshot));
    }

    [Fact]
    public void ValueComparer_JsonElement()
    {
        var json = """{"Name":"Joe","Age":25}""";
        var source = JsonDocument.Parse(json).RootElement;

        var comparer = GetMapping(typeof(JsonElement)).Comparer;
        var snapshot = (JsonElement)comparer.Snapshot(source);
        Assert.True(comparer.Equals(source, snapshot));
        Assert.False(comparer.Equals(source, JsonDocument.Parse(json).RootElement));
    }

    private static readonly Customer SampleCustomer = new()
    {
        Name = "Joe",
        Age = 25,
        IsVip = false,
        Orders = new[]
        {
            new Order
            {
                Price = 99.5m,
                ShippingAddress = "Some address 1",
                ShippingDate = new DateTime(2019, 10, 1)
            },
            new Order
            {
                Price = 23,
                ShippingAddress = "Some address 2",
                ShippingDate = new DateTime(2019, 10, 10)
            }
        }
    };

    public class Customer
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public bool IsVip { get; set; }
        public Order[] Orders { get; set; }
    }

    public class Order
    {
        public decimal Price { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime ShippingDate { get; set; }
    }

    #endregion Json

    #region Support

    private static readonly NpgsqlTypeMappingSource Mapper = new(
        new TypeMappingSourceDependencies(
            new ValueConverterSelector(new ValueConverterSelectorDependencies()),
            new JsonValueReaderWriterSource(new JsonValueReaderWriterSourceDependencies()),
            Array.Empty<ITypeMappingSourcePlugin>()
        ),
        new RelationalTypeMappingSourceDependencies(Array.Empty<IRelationalTypeMappingSourcePlugin>()),
        new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
        new NpgsqlSingletonOptions()
    );

    private static RelationalTypeMapping GetMapping(string storeType)
        => Mapper.FindMapping(storeType);

    private static RelationalTypeMapping GetMapping(Type clrType)
        => Mapper.FindMapping(clrType);

    private static RelationalTypeMapping GetMapping(Type clrType, string storeType)
        => Mapper.FindMapping(clrType, storeType);

    private static readonly CSharpHelper CsHelper = new(Mapper);

    private static string CodeLiteral(object value)
        => CsHelper.UnknownLiteral(value);

    #endregion Support
}
