using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using NpgsqlTypes;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage
{
    public class NpgsqlTypeMappingTest
    {
        #region Date/Time

        [Fact]
        public void GenerateSqlLiteral_returns_date_literal()
            => Assert.Equal("DATE '2015-03-12'",
                GetMapping("date").GenerateSqlLiteral(new DateTime(2015, 3, 12)));

        [Fact]
        public void GenerateSqlLiteral_returns_timestamp_literal()
        {
            var mapping = GetMapping("timestamp");
            Assert.Equal("TIMESTAMP '1997-12-17 07:37:16'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Utc)));
            Assert.Equal("TIMESTAMP '1997-12-17 07:37:16'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Local)));
            Assert.Equal("TIMESTAMP '1997-12-17 07:37:16'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Unspecified)));
            Assert.Equal("TIMESTAMP '1997-12-17 07:37:16.345'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, 345)));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_timestamptz_datetime_literal()
        {
            var mapping = GetMapping("timestamptz");
            Assert.Equal("TIMESTAMPTZ '1997-12-17 07:37:16 UTC'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Utc)));
            Assert.Equal("TIMESTAMPTZ '1997-12-17 07:37:16 UTC'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Unspecified)));

            var offset = TimeZoneInfo.Local.BaseUtcOffset.Hours;
            var offsetStr = offset < 10 ? $"0{offset}" : offset.ToString();
            Assert.StartsWith($"TIMESTAMPTZ '1997-12-17 07:37:16+{offsetStr}",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Local)));

            Assert.Equal("TIMESTAMPTZ '1997-12-17 07:37:16.345 UTC'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, 345, DateTimeKind.Utc)));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_timestamptz_datetimeoffset_literal()
        {
            var mapping = GetMapping("timestamptz");
            Assert.Equal("TIMESTAMPTZ '1997-12-17 07:37:16+02:00'",
                mapping.GenerateSqlLiteral(new DateTimeOffset(1997, 12, 17, 7, 37, 16, TimeSpan.FromHours(2))));
            Assert.Equal("TIMESTAMPTZ '1997-12-17 07:37:16.345+02:00'",
                mapping.GenerateSqlLiteral(new DateTimeOffset(1997, 12, 17, 7, 37, 16, 345, TimeSpan.FromHours(2))));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_time_literal()
        {
            var mapping = GetMapping("time");
            Assert.Equal("TIME '04:05:06.789'", mapping.GenerateSqlLiteral(new TimeSpan(0, 4, 5, 6, 789)));
            Assert.Equal("TIME '04:05:06'", mapping.GenerateSqlLiteral(new TimeSpan(4, 5, 6)));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_timetz_literal()
        {
            var mapping = GetMapping("timetz");
            Assert.Equal("TIMETZ '04:05:06.789+3'", mapping.GenerateSqlLiteral(new DateTimeOffset(2015, 3, 12, 4, 5, 6, 789, TimeSpan.FromHours(3))));
            Assert.Equal("TIMETZ '04:05:06-3'", mapping.GenerateSqlLiteral(new DateTimeOffset(2015, 3, 12, 4, 5, 6, TimeSpan.FromHours(-3))));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_interval_literal()
        {
            var mapping = GetMapping("interval");
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
        public void GenerateSqlLiteral_returns_macaddr8_literal()
            => Assert.Equal("MACADDR8 '0011223344556677'", GetMapping("macaddr8").GenerateSqlLiteral(PhysicalAddress.Parse("00-11-22-33-44-55-66-77")));

        [Fact]
        public void GenerateSqlLiteral_returns_inet_literal()
            => Assert.Equal("INET '192.168.1.1'", GetMapping("inet").GenerateSqlLiteral(IPAddress.Parse("192.168.1.1")));

        [Fact]
        public void GenerateSqlLiteral_returns_cidr_literal()
            => Assert.Equal("CIDR '192.168.1.0/24'", GetMapping("cidr").GenerateSqlLiteral(new NpgsqlInet(IPAddress.Parse("192.168.1.0"), 24)));

        #endregion Networking

        #region Geometric

        [Fact]
        public void GenerateSqlLiteral_returns_point_literal()
            => Assert.Equal("POINT '(3.5,4.5)'", GetMapping("point").GenerateSqlLiteral(new NpgsqlPoint(3.5, 4.5)));

        [Fact]
        public void GenerateSqlLiteral_returns_line_literal()
            => Assert.Equal("LINE '{3.5,4.5,10}'", GetMapping("line").GenerateSqlLiteral(new NpgsqlLine(3.5, 4.5, 10)));

        [Fact]
        public void GenerateSqlLiteral_returns_lseg_literal()
            => Assert.Equal("LSEG '[(3.5,4.5),(5.5,6.5)]'", GetMapping("lseg").GenerateSqlLiteral(new NpgsqlLSeg(3.5, 4.5, 5.5, 6.5)));

        [Fact]
        public void GenerateSqlLiteral_returns_box_literal()
            => Assert.Equal("BOX '((2,1),(4,3))'", GetMapping("box").GenerateSqlLiteral(new NpgsqlBox(1, 2, 3, 4)));

        [Fact]
        public void GenerateSqlLiteral_returns_path_closed_literal()
            => Assert.Equal("PATH '((1,2),(3,4))'", GetMapping("path").GenerateSqlLiteral(new NpgsqlPath(
                new NpgsqlPoint(1, 2),
                new NpgsqlPoint(3, 4)
            )));

        [Fact]
        public void GenerateSqlLiteral_returns_path_open_literal()
            => Assert.Equal("PATH '[(1,2),(3,4)]'", GetMapping("path").GenerateSqlLiteral(new NpgsqlPath(
                new NpgsqlPoint(1, 2),
                new NpgsqlPoint(3, 4)
            ) { Open = true }));

        [Fact]
        public void GenerateSqlLiteral_returns_polygon_literal()
            => Assert.Equal("POLYGON '((1,2),(3,4))'", GetMapping("polygon").GenerateSqlLiteral(new NpgsqlPolygon(
                new NpgsqlPoint(1, 2),
                new NpgsqlPoint(3, 4)
            )));

        [Fact]
        public void GenerateSqlLiteral_returns_circle_literal()
            => Assert.Equal("CIRCLE '<(3.5,4.5),5.5>'", GetMapping("circle").GenerateSqlLiteral(new NpgsqlCircle(3.5, 4.5, 5.5)));

        #endregion Geometric

        #region Misc

        [Fact]
        public void GenerateSqlLiteral_returns_bool_literal()
            => Assert.Equal("TRUE", GetMapping("bool").GenerateSqlLiteral(true));

        [Fact]
        public void GenerateSqlLiteral_returns_varbit_literal()
            => Assert.Equal("VARBIT B'10'", GetMapping("varbit").GenerateSqlLiteral(new BitArray(new[] { true, false })));

        [Fact]
        public void GenerateSqlLiteral_returns_bit_literal()
            => Assert.Equal("BIT B'10'", GetMapping("bit").GenerateSqlLiteral(new BitArray(new[] { true, false })));

        [Fact]
        public void GenerateSqlLiteral_returns_array_literal()
            => Assert.Equal("ARRAY[3,4]", GetMapping(typeof(int[])).GenerateSqlLiteral(new[] {3, 4}));

        [Fact]
        public void GenerateSqlLiteral_returns_bytea_literal()
            => Assert.Equal(@"BYTEA E'\\xDEADBEEF'", GetMapping("bytea").GenerateSqlLiteral(new byte[] { 222, 173, 190, 239 }));

        [Fact]
        public void GenerateSqlLiteral_returns_hstore_literal()
            => Assert.Equal(@"HSTORE '""k1""=>""v1"",""k2""=>""v2""'",
                GetMapping("hstore").GenerateSqlLiteral(new Dictionary<string, string>
            {
                { "k1", "v1" },
                { "k2", "v2" }
            }));

        [Fact]
        public void ValueComparer_hstore()
        {
            var source = new Dictionary<string, string>
            {
                { "k1", "v1"},
                { "k2", "v2"}
            };

            var comparer = GetMapping("hstore").Comparer;
            var snapshot = (Dictionary<string, string>)comparer.Snapshot(source);
            Assert.Equal(source, snapshot);
            Assert.True(comparer.Equals(source, snapshot));
            snapshot.Remove("k1");
            Assert.False(comparer.Equals(source, snapshot));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_jsonb_literal()
            => Assert.Equal(@"JSONB '{""a"":1}'", GetMapping("jsonb").GenerateSqlLiteral(@"{""a"":1}"));

        [Fact]
        public void GenerateSqlLiteral_returns_json_literal()
            => Assert.Equal(@"JSON '{""a"":1}'", GetMapping("json").GenerateSqlLiteral(@"{""a"":1}"));

        #endregion Misc

        #region Ranges

        [Fact]
        public void GenerateSqlLiteral_returns_range_empty_literal()
        {
            var value = NpgsqlRange<int>.Empty;
            var literal = GetMapping("int4range").GenerateSqlLiteral(value);
            Assert.Equal("'empty'::int4range", literal);
        }

        [Fact]
        public void GenerateSqlLiteral_returns_range_inclusive_literal()
        {
            var value = new NpgsqlRange<int>(4, 7);
            var literal = GetMapping("int4range").GenerateSqlLiteral(value);
            Assert.Equal("'[4,7]'::int4range", literal);
        }

        [Fact]
        public void GenerateSqlLiteral_returns_range_inclusive_exclusive_literal()
        {
            var value = new NpgsqlRange<int>(4, false, 7, true);
            var literal = GetMapping("int4range").GenerateSqlLiteral(value);
            Assert.Equal("'(4,7]'::int4range", literal);
        }

        [Fact]
        public void GenerateSqlLiteral_returns_range_infinite_literal()
        {
            var value = new NpgsqlRange<int>(0, false, true, 7, true, false);
            var literal = GetMapping("int4range").GenerateSqlLiteral(value);
            Assert.Equal("'(,7]'::int4range", literal);
        }

        #endregion Ranges

        #region Support

        public static RelationalTypeMapping GetMapping(string storeType)
            => new NpgsqlTypeMappingSource(
                    new TypeMappingSourceDependencies (
                        new ValueConverterSelector(new ValueConverterSelectorDependencies())
                    ), new RelationalTypeMappingSourceDependencies())
                .FindMapping(storeType);

        public static RelationalTypeMapping GetMapping(Type clrType)
            => (RelationalTypeMapping)new NpgsqlTypeMappingSource(
                    new TypeMappingSourceDependencies (
                        new ValueConverterSelector(new ValueConverterSelectorDependencies())
                    ), new RelationalTypeMappingSourceDependencies())
                .FindMapping(clrType);

        #endregion Support
    }
}
