using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class BuiltInDataTypesNpgsqlTest : BuiltInDataTypesTestBase<BuiltInDataTypesNpgsqlTest.BuiltInDataTypesNpgsqlFixture>
    {
        public BuiltInDataTypesNpgsqlTest(BuiltInDataTypesNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Fact]
        public void Sql_translation_uses_type_mapper_when_constant()
        {
            using (var context = CreateContext())
            {
                var results
                    = context.Set<MappedNullableDataTypes>()
                        .Where(e => e.TimeSpanAsTime == new TimeSpan(0, 1, 2))
                        .Select(e => e.Int)
                        .ToList();

                Assert.Equal(0, results.Count);
                Assert.Equal(
                    @"SELECT e.""Int""
FROM ""MappedNullableDataTypes"" AS e
WHERE e.""TimeSpanAsTime"" = TIME '00:01:02'",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Sql_translation_uses_type_mapper_when_parameter()
        {
            using (var context = CreateContext())
            {
                var timeSpan = new TimeSpan(2, 1, 0);

                var results
                    = context.Set<MappedNullableDataTypes>()
                        .Where(e => e.TimeSpanAsTime == timeSpan)
                        .Select(e => e.Int)
                        .ToList();

                Assert.Equal(0, results.Count);
                Assert.Equal(
                    @"@__timeSpan_0='02:01:00' (DbType = Object)

SELECT e.""Int""
FROM ""MappedNullableDataTypes"" AS e
WHERE e.""TimeSpanAsTime"" = @__timeSpan_0",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public virtual void Can_query_using_any_mapped_data_type()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Int = 999,
                        LongAsBigint = 78L,
                        ShortAsSmallint = 79,
                        ByteAsSmallint = 80,
                        UintAsInt = uint.MaxValue,
                        UlongAsBigint = ulong.MaxValue,
                        UShortAsSmallint = ushort.MaxValue,
                        UintAsBigint = uint.MaxValue,
                        UShortAsInt = ushort.MaxValue,

                        BoolAsBoolean = true,

                        DecimalAsMoney = 81.1m,
                        Decimal = 101.7m,
                        DecimalAsNumeric = 103.9m,
                        FloatAsReal = 84.4f,
                        DoubleAsDoublePrecision = 85.5,

                        DateTimeAsTimestamp = new DateTime(2015, 1, 2, 10, 11, 12),
                        DateTimeAsTimestamptz = new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc),
                        DateTimeAsDate = new DateTime(2015, 1, 2, 0, 0, 0),
                        TimeSpanAsTime = new TimeSpan(11, 15, 12),
                        DateTimeOffsetAsTimetz = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)),
                        TimeSpanAsInterval = new TimeSpan(11, 15, 12),

                        StringAsText = "Gumball Rules!",
                        StringAsVarchar = "Gumball Rules OK",
                        BytesAsBytea = new byte[] { 86 },

                        GuidAsUuid = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),

                        EnumAsText = StringEnum16.Value4,
                        EnumAsVarchar = StringEnumU16.Value4,

                        PhysicalAddressAsMacaddr = PhysicalAddress.Parse("08-00-2B-01-02-03"),
                        NpgsqlPointAsPoint = new NpgsqlPoint(5.2, 3.3),
                        StringAsJsonb = @"{""a"": ""b""}",
                        StringAsJson = @"{""a"": ""b""}",
                        DictionaryAsHstore = new Dictionary<string, string> { { "a", "b" } },
                        NpgsqlRangeAsRange = new NpgsqlRange<int>(4, true, 8, false),

                        IntArrayAsIntArray= new[] { 2, 3 },
                        PhysicalAddressArrayAsMacaddrArray= new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") },

                        UintAsXid = (uint)int.MaxValue + 1,
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999);

                long? param1 = 78L;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.LongAsBigint == param1));

                short? param2 = 79;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.ShortAsSmallint == param2));

                byte? param2a = 80;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.ByteAsSmallint == param2a));

                uint? param3 = uint.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UintAsInt == param3));

                ulong? param4 = ulong.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UlongAsBigint == param4));

                ushort? param5 = ushort.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UShortAsSmallint == param5));

                uint? param6 = uint.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UintAsBigint == param6));

                ushort? param7 = ushort.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UShortAsInt == param7));

                bool? param8 = true;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BoolAsBoolean == param8));

                // PostgreSQL doesn't support comparing money to decimal
                //decimal? param9 = 81.1m;
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsMoney == param9));

                decimal? param10 = 101.7m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Decimal == param10));

                decimal? param11 = 103.9m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DecimalAsNumeric == param11));

                float? param12 = 84.4f;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.FloatAsReal == param12));

                double? param13 = 85.5;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DoubleAsDoublePrecision == param13));

                DateTime? param14 = new DateTime(2015, 1, 2, 10, 11, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsTimestamp == param14));

                DateTime? param15 = new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsTimestamptz == param15));

                DateTime? param16 = new DateTime(2015, 1, 2, 0, 0, 0);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeAsDate == param16));

                TimeSpan? param17 = new TimeSpan(11, 15, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.TimeSpanAsTime == param17));

                DateTimeOffset? param18 = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2));
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DateTimeOffsetAsTimetz == param18));

                TimeSpan? param19 = new TimeSpan(11, 15, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.TimeSpanAsInterval == param19));

                var param20 = "Gumball Rules!";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsText == param20));

                var param21 = "Gumball Rules OK";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsVarchar == param21));

                var param22 = new byte[] { 86 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.BytesAsBytea == param22));

                Guid? param23 = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.GuidAsUuid == param23));

                StringEnum16? param24 = StringEnum16.Value4;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.EnumAsText == param24));

                StringEnumU16? param25 = StringEnumU16.Value4;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.EnumAsVarchar == param25));

                var param26 = PhysicalAddress.Parse("08-00-2B-01-02-03");
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.PhysicalAddressAsMacaddr.Equals(param26)));

                // PostgreSQL doesn't support equality comparison on point
                // NpgsqlPoint? param27 = new NpgsqlPoint(5.2, 3.3);
                // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Point == param27));

                var param28 = @"{""a"": ""b""}";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsJsonb == param28));

                // operator does not exist: json = json (https://stackoverflow.com/questions/32843213/operator-does-not-exist-json-json)
                // var param29 = @"{""a"": ""b""}";
                // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.StringAsJson == param29));

                var param30 = new Dictionary<string, string> { { "a", "b" } };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.DictionaryAsHstore == param30));

                var param31 = new NpgsqlRange<int>(4, true, 8, false);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.NpgsqlRangeAsRange == param31));

                var param32 = new[] { 2, 3 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.IntArrayAsIntArray == param32));

                var param33 = new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.PhysicalAddressArrayAsMacaddrArray == param33));

                var param34 = (uint)int.MaxValue + 1;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.UintAsXid == param34));
            }
        }


        [Fact]
        public virtual void Can_query_using_any_mapped_data_types_with_nulls()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Int = 911,
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911);

                long? param1 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.LongAsBigint == param1));

                short? param2 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ShortAsSmallint == param2));
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && (long?)(int?)e.ShortAsSmallint == param2));

                byte? param2a = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ByteAsSmallint == param2a));
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && (long?)(int?)e.ByteAsSmallint == param2a));

                uint? param3 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsInt == param3));

                ulong? param4 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UlongAsBigint == param4));

                ushort? param5 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsSmallint == param5));

                uint? param6 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsBigint == param6));

                ushort? param7 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsInt == param7));

                bool? param8 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BoolAsBoolean == param8));

                decimal? param9 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsMoney == param9));

                decimal? param10 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Decimal == param10));

                decimal? param11 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsNumeric == param11));

                float? param12 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.FloatAsReal == param12));

                double? param13 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DoubleAsDoublePrecision == param13));

                DateTime? param14 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsTimestamp == param14));

                DateTime? param15 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsTimestamptz == param15));

                DateTime? param16 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsDate == param16));

                TimeSpan? param17 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.TimeSpanAsTime == param17));

                DateTimeOffset? param18 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeOffsetAsTimetz == param18));

                TimeSpan? param19 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.TimeSpanAsInterval == param19));

                string param20 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsText == param20));

                string param21 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsVarchar == param21));

                byte[] param22 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsBytea == param22));

                Guid? param23 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.GuidAsUuid == param23));

                StringEnum16? param24 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.EnumAsText == param24));

                StringEnumU16? param25 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.EnumAsVarchar == param25));

                PhysicalAddress param26 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.PhysicalAddressAsMacaddr == param26));

                NpgsqlPoint? param27 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.NpgsqlPointAsPoint == param27));

                string param28 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsJsonb == param28));

                string param29 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsJson == param29));

                Dictionary<string, string> param30 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DictionaryAsHstore == param30));

                NpgsqlRange<int>? param31 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.NpgsqlRangeAsRange == param31));

                int[] param32 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.IntArrayAsIntArray == param32));

                PhysicalAddress[] param33 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.PhysicalAddressArrayAsMacaddrArray== param33));

                uint? param34 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsXid == param34));
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types()
        {
            var entity = CreateMappedDataTypes(77);
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(entity);

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0='77'
@p1='True'
@p2='80'
@p3='0x56' (Nullable = false)
@p4='2015-01-02T00:00:00' (DbType = Date)
@p5='2015-01-02T10:11:12' (DbType = DateTime)
@p6='2016-01-02T11:11:12' (DbType = DateTimeOffset)
@p7='0001-01-01T12:00:00.0000000+02:00' (DbType = Object)
@p8='101.7'
@p9='81.1'
@p10='103.9'
@p11='System.Collections.Generic.Dictionary`2[System.String,System.String]' (Nullable = false) (DbType = Object)
@p12='85.5'
@p13='Value4' (Nullable = false)
@p14='Value4' (Nullable = false)
@p15='84.4'
@p16='a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
@p17='System.Int32[]' (Nullable = false) (DbType = Object)
@p18='78'
@p19='(5.2,3.3)' (DbType = Object)
@p20='[4,8)' (DbType = Object)
@p21='System.Net.NetworkInformation.PhysicalAddress[]' (Nullable = false) (DbType = Object)
@p22='08002B010203' (Nullable = false) (DbType = Object)
@p23='79'
@p24='{""a"": ""b""}' (Nullable = false)
@p25='{""a"": ""b""}' (Nullable = false) (DbType = Object)
@p26='Gumball Rules!' (Nullable = false)
@p27='Gumball Rules OK' (Nullable = false)
@p28='11:15:12' (DbType = Object)
@p29='11:15:12' (DbType = Object)
@p30='65535'
@p31='-1'
@p32='4294967295'
@p33='-1'
@p34='2147483648' (DbType = Object)
@p35='-1'",
                    parameters,
                    ignoreLineEndingDifferences: true);
        }

        string DumpParameters()
            => Fixture.TestSqlLoggerFactory.Parameters.Single().Replace(", ", EOL);

        static void AssertMappedDataTypes(MappedDataTypes entity, int id)
        {
            var expected = CreateMappedDataTypes(id);
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.LongAsBigint);
            Assert.Equal(79, entity.ShortAsSmallint);
            Assert.Equal(uint.MaxValue, entity.UintAsInt);
            Assert.Equal(ulong.MaxValue, entity.UlongAsBigint);
            Assert.Equal(ushort.MaxValue, entity.UShortAsSmallint);
            Assert.Equal(uint.MaxValue, entity.UintAsBigint);
            Assert.Equal(ushort.MaxValue, entity.UShortAsInt);

            Assert.True(entity.BoolAsBoolean);

            Assert.Equal(81.1m, entity.DecimalAsMoney);
            Assert.Equal(101.7m, entity.Decimal);
            Assert.Equal(103.9m, entity.DecimalAsNumeric);
            Assert.Equal(84.4f, entity.FloatAsReal);
            Assert.Equal(85.5, entity.DoubleAsDoublePrecision);

            Assert.Equal(new DateTime(2015, 1, 2, 10, 11, 12), entity.DateTimeAsTimestamp);
            Assert.Equal(new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc), entity.DateTimeAsTimestamptz);
            Assert.Equal(new DateTime(2015, 1, 2, 0, 0, 0), entity.DateTimeAsDate);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
            Assert.Equal(new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)), entity.DateTimeOffsetAsTimetz);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsInterval);

            Assert.Equal("Gumball Rules!", entity.StringAsText);
            Assert.Equal("Gumball Rules OK", entity.StringAsVarchar);
            Assert.Equal(new byte[] { 86 }, entity.BytesAsBytea);

            Assert.Equal(new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"), entity.GuidAsUuid);

            Assert.Equal(StringEnum16.Value4, entity.EnumAsText);
            Assert.Equal(StringEnumU16.Value4, entity.EnumAsVarchar);

            Assert.Equal(PhysicalAddress.Parse("08-00-2B-01-02-03"), entity.PhysicalAddressAsMacaddr);
            Assert.Equal(new NpgsqlPoint(5.2, 3.3), entity.NpgsqlPointAsPoint);
            Assert.Equal(@"{""a"": ""b""}", entity.StringAsJsonb);
            Assert.Equal(@"{""a"": ""b""}", entity.StringAsJson);
            Assert.Equal(new Dictionary<string, string> { { "a", "b" } }, entity.DictionaryAsHstore);
            Assert.Equal(new NpgsqlRange<int>(4, true, 8, false), entity.NpgsqlRangeAsRange);

            Assert.Equal(new[] { 2, 3 }, entity.IntArrayAsIntArray);
            Assert.Equal(new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") }, entity.PhysicalAddressArrayAsMacaddrArray);

            Assert.Equal((uint)int.MaxValue + 1, entity.UintAsXid);
        }

        static MappedDataTypes CreateMappedDataTypes(int id)
            => new MappedDataTypes
            {
                Int = id,
                LongAsBigint = 78L,
                ShortAsSmallint = 79,
                ByteAsSmallint = 80,
                UintAsInt = uint.MaxValue,
                UlongAsBigint = ulong.MaxValue,
                UShortAsSmallint = ushort.MaxValue,
                UintAsBigint = uint.MaxValue,
                UShortAsInt = ushort.MaxValue,

                BoolAsBoolean = true,

                DecimalAsMoney = 81.1m,
                Decimal = 101.7m,
                DecimalAsNumeric = 103.9m,
                FloatAsReal = 84.4f,
                DoubleAsDoublePrecision = 85.5,

                DateTimeAsTimestamp = new DateTime(2015, 1, 2, 10, 11, 12),
                DateTimeAsTimestamptz = new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc),
                DateTimeAsDate = new DateTime(2015, 1, 2, 0, 0, 0),
                TimeSpanAsTime = new TimeSpan(11, 15, 12),
                DateTimeOffsetAsTimetz = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)),
                TimeSpanAsInterval = new TimeSpan(11, 15, 12),

                StringAsText = "Gumball Rules!",
                StringAsVarchar = "Gumball Rules OK",
                BytesAsBytea = new byte[] { 86 },

                GuidAsUuid = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),

                EnumAsText = StringEnum16.Value4,
                EnumAsVarchar = StringEnumU16.Value4,

                PhysicalAddressAsMacaddr = PhysicalAddress.Parse("08-00-2B-01-02-03"),
                NpgsqlPointAsPoint = new NpgsqlPoint(5.2, 3.3),
                StringAsJsonb = @"{""a"": ""b""}",
                StringAsJson = @"{""a"": ""b""}",
                DictionaryAsHstore = new Dictionary<string, string> { { "a", "b" } },
                NpgsqlRangeAsRange = new NpgsqlRange<int>(4, true, 8, false),

                IntArrayAsIntArray= new[] { 2, 3 },
                PhysicalAddressArrayAsMacaddrArray= new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") },

                UintAsXid = (uint)int.MaxValue + 1,
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 78), 78);
            }
        }

        static void AssertNullMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Null(entity.LongAsBigint);
            Assert.Null(entity.ShortAsSmallint);
            Assert.Null(entity.ByteAsSmallint);
            Assert.Null(entity.UintAsInt);
            Assert.Null(entity.UlongAsBigint);
            Assert.Null(entity.UShortAsSmallint);
            Assert.Null(entity.UintAsBigint);
            Assert.Null(entity.UShortAsInt);

            Assert.Null(entity.BoolAsBoolean);

            Assert.Null(entity.DecimalAsMoney);
            Assert.Null(entity.Decimal);
            Assert.Null(entity.DecimalAsNumeric);
            Assert.Null(entity.FloatAsReal);
            Assert.Null(entity.DoubleAsDoublePrecision);

            Assert.Null(entity.DateTimeAsTimestamp);
            Assert.Null(entity.DateTimeAsTimestamptz);
            Assert.Null(entity.DateTimeAsDate);
            Assert.Null(entity.TimeSpanAsTime);
            Assert.Null(entity.DateTimeOffsetAsTimetz);
            Assert.Null(entity.TimeSpanAsInterval);

            Assert.Null(entity.StringAsText);
            Assert.Null(entity.StringAsVarchar);
            Assert.Null(entity.BytesAsBytea);

            Assert.Null(entity.GuidAsUuid);

            Assert.Null(entity.EnumAsText);
            Assert.Null(entity.EnumAsVarchar);

            Assert.Null(entity.PhysicalAddressAsMacaddr);
            Assert.Null(entity.NpgsqlPointAsPoint);
            Assert.Null(entity.StringAsJsonb);
            Assert.Null(entity.StringAsJson);
            Assert.Null(entity.DictionaryAsHstore);
            Assert.Null(entity.NpgsqlRangeAsRange);

            Assert.Null(entity.IntArrayAsIntArray);
            Assert.Null(entity.PhysicalAddressArrayAsMacaddrArray);

            Assert.Null(entity.UintAsXid);
        }

        string Sql => Fixture.TestSqlLoggerFactory.Sql;

        static readonly string EOL = Environment.NewLine;

        public class BuiltInDataTypesNpgsqlFixture : BuiltInDataTypesFixtureBase
        {
            public override bool StrictEquality => false;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => false;

            public override bool SupportsLargeStringComparisons => true;

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.HasPostgresExtension("hstore");

                MakeRequired<MappedDataTypes>(modelBuilder);

                modelBuilder.Entity<BuiltInDataTypes>(b =>
                {
                    b.Ignore(dt => dt.TestUnsignedInt16);
                    b.Ignore(dt => dt.TestUnsignedInt32);
                    b.Ignore(dt => dt.TestUnsignedInt64);
                    b.Ignore(dt => dt.TestCharacter);
                    b.Ignore(dt => dt.TestSignedByte);
                    b.Ignore(dt => dt.TestDateTimeOffset);
                    b.Ignore(dt => dt.TestByte);
                    //b.Ignore(dt => dt.EnumU16);
                    //b.Ignore(dt => dt.EnumU32);
                    //b.Ignore(dt => dt.EnumU64);
                    //b.Ignore(dt => dt.EnumS8);
                });

                modelBuilder.Entity<BuiltInNullableDataTypes>(b =>
                {
                    b.Ignore(dt => dt.TestNullableUnsignedInt16);
                    b.Ignore(dt => dt.TestNullableUnsignedInt32);
                    b.Ignore(dt => dt.TestNullableUnsignedInt64);
                    b.Ignore(dt => dt.TestNullableCharacter);
                    b.Ignore(dt => dt.TestNullableSignedByte);
                    b.Ignore(dt => dt.TestNullableDateTimeOffset);
                    b.Ignore(dt => dt.TestNullableByte);
                    //b.Ignore(dt => dt.EnumU16);
                    //b.Ignore(dt => dt.EnumU32);
                    //b.Ignore(dt => dt.EnumU64);
                    //b.Ignore(dt => dt.EnumS8);
                });

                modelBuilder.Entity<MappedDataTypes>(
                    b =>
                    {
                        b.HasKey(e => e.Int);
                        b.Property(e => e.Int).ValueGeneratedNever();
                    });

                modelBuilder.Entity<MappedNullableDataTypes>(
                    b =>
                    {
                        b.HasKey(e => e.Int);
                        b.Property(e => e.Int).ValueGeneratedNever();
                    });

                modelBuilder.Entity<MappedSizedDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                modelBuilder.Entity<MappedScaledDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(
                    c => c
                        .Log(RelationalEventId.QueryClientEvaluationWarning));

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();
        }

        protected enum StringEnum16 : short
        {
            Value1 = 1,
            Value2 = 2,
            Value4 = 4
        }

        protected enum StringEnumU16 : ushort
        {
            Value1 = 1,
            Value2 = 2,
            Value4 = 4
        }

        protected class MappedDataTypes
        {
            [Column(TypeName = "int")]
            public int Int { get; set; }

            [Column(TypeName = "bigint")]
            public long LongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short ShortAsSmallint { get; set; }

            [Column(TypeName = "smallint")]
            public byte ByteAsSmallint { get; set; }

            [Column(TypeName = "int")]
            public uint UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public uint UintAsBigint { get; set; }

            [Column(TypeName = "bigint")]
            public ulong UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public ushort UShortAsSmallint { get; set; }

            [Column(TypeName = "int")]
            public ushort UShortAsInt { get; set; }

            //[Column(TypeName = "tinyint")]
            //public sbyte SByteAsTinyint { get; set; }

            [Column(TypeName = "boolean")]
            public bool BoolAsBoolean { get; set; }

            [Column(TypeName = "numeric")]
            public decimal Decimal { get; set; }  // decimal is just an alias for numeric

            [Column(TypeName = "numeric")]
            public decimal DecimalAsNumeric { get; set; }

            [Column(TypeName = "money")]
            public decimal DecimalAsMoney { get; set; }

            [Column(TypeName = "double precision")]
            public double DoubleAsDoublePrecision { get; set; }

            [Column(TypeName = "real")]
            public float FloatAsReal { get; set; }

            [Column(TypeName = "timestamp")]
            public DateTime DateTimeAsTimestamp { get; set; }

            [Column(TypeName = "timestamptz")]
            public DateTime DateTimeAsTimestamptz { get; set; }

            [Column(TypeName = "date")]
            public DateTime DateTimeAsDate { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan TimeSpanAsTime { get; set; }

            [Column(TypeName = "timetz")]
            public DateTimeOffset DateTimeOffsetAsTimetz { get; set; }

            [Column(TypeName = "interval")]
            public TimeSpan TimeSpanAsInterval { get; set; }

            [Column(TypeName = "text")]
            public string StringAsText { get; set; }

            [Column(TypeName = "varchar")]
            public string StringAsVarchar { get; set; }

            //[Column(TypeName = "text")]
            //public char CharAsText { get; set; }

            //[Column(TypeName = "varchar")]
            //public char CharAsVarchar { get; set; }

            [Column(TypeName = "bytea")]
            public byte[] BytesAsBytea { get; set; }

            [Column(TypeName = "uuid")]
            public Guid GuidAsUuid { get; set; }

            [Column(TypeName = "text")]
            public StringEnum16 EnumAsText { get; set; }

            [Column(TypeName = "varchar")]
            public StringEnumU16 EnumAsVarchar { get; set; }

            // PostgreSQL-specific types from here

            [Column(TypeName = "macaddr")]
            public PhysicalAddress PhysicalAddressAsMacaddr { get; set; }

            [Column(TypeName = "point")]
            public NpgsqlPoint NpgsqlPointAsPoint { get; set; }

            [Column(TypeName = "jsonb")]
            public string StringAsJsonb { get; set; }

            [Column(TypeName = "json")]
            public string StringAsJson { get; set; }

            [Column(TypeName = "hstore")]
            public Dictionary<string, string> DictionaryAsHstore { get; set; }

            [Column(TypeName = "int4range")]
            public NpgsqlRange<int> NpgsqlRangeAsRange { get; set; }

            [Column(TypeName = "int[]")]
            public int[] IntArrayAsIntArray { get; set; }

            [Column(TypeName = "macaddr[]")]
            public PhysicalAddress[] PhysicalAddressArrayAsMacaddrArray { get; set; }

            [Column(TypeName = "xid")]
            public uint UintAsXid { get; set; }
        }

        public class MappedSizedDataTypes
        {
            public int Id { get; set; }
            /*
            public string Char { get; set; }
            public string Character { get; set; }
            public string Varchar { get; set; }
            public string Char_varying { get; set; }
            public string Character_varying { get; set; }
            public string Nchar { get; set; }
            public string National_character { get; set; }
            public string Nvarchar { get; set; }
            public string National_char_varying { get; set; }
            public string National_character_varying { get; set; }
            public byte[] Binary { get; set; }
            public byte[] Varbinary { get; set; }
            public byte[] Binary_varying { get; set; }
            */
        }

        public class MappedScaledDataTypes
        {
            public int Id { get; set; }
            /*
            public float Float { get; set; }
            public float Double_precision { get; set; }
            public DateTimeOffset Datetimeoffset { get; set; }
            public DateTime Datetime2 { get; set; }
            public decimal Decimal { get; set; }
            public decimal Dec { get; set; }
            public decimal Numeric { get; set; }
            */
        }

        public class MappedPrecisionAndScaledDataTypes
        {
            public int Id { get; set; }
            /*
            public decimal Decimal { get; set; }
            public decimal Dec { get; set; }
            public decimal Numeric { get; set; }
            */
        }

        protected class MappedNullableDataTypes
        {
            [Column(TypeName = "int")]
            public int? Int { get; set; }

            [Column(TypeName = "bigint")]
            public long? LongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short? ShortAsSmallint { get; set; }

            [Column(TypeName = "smallint")]
            public byte? ByteAsSmallint { get; set; }

            [Column(TypeName = "int")]
            public uint? UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public uint? UintAsBigint { get; set; }

            [Column(TypeName = "bigint")]
            public ulong? UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public ushort? UShortAsSmallint { get; set; }

            [Column(TypeName = "int")]
            public ushort? UShortAsInt { get; set; }

            //[Column(TypeName = "tinyint")]
            //public sbyte? SByteAsTinyint { get; set; }

            [Column(TypeName = "boolean")]
            public bool? BoolAsBoolean { get; set; }

            [Column(TypeName = "numeric")]
            public decimal? Decimal { get; set; }  // decimal is just an alias for numeric

            [Column(TypeName = "numeric")]
            public decimal? DecimalAsNumeric { get; set; }

            [Column(TypeName = "money")]
            public decimal? DecimalAsMoney { get; set; }

            [Column(TypeName = "double precision")]
            public double? DoubleAsDoublePrecision { get; set; }

            [Column(TypeName = "real")]
            public float? FloatAsReal { get; set; }

            [Column(TypeName = "timestamp")]
            public DateTime? DateTimeAsTimestamp { get; set; }

            [Column(TypeName = "timestamptz")]
            public DateTime? DateTimeAsTimestamptz { get; set; }

            [Column(TypeName = "date")]
            public DateTime? DateTimeAsDate { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan? TimeSpanAsTime { get; set; }

            [Column(TypeName = "timetz")]
            public DateTimeOffset? DateTimeOffsetAsTimetz { get; set; }

            [Column(TypeName = "interval")]
            public TimeSpan? TimeSpanAsInterval { get; set; }

            [Column(TypeName = "text")]
            public string StringAsText { get; set; }

            [Column(TypeName = "varchar")]
            public string StringAsVarchar { get; set; }

            //[Column(TypeName = "text")]
            //public char? CharAsText { get; set; }

            //[Column(TypeName = "varchar")]
            //public char? CharAsVarchar { get; set; }

            [Column(TypeName = "bytea")]
            public byte[] BytesAsBytea { get; set; }

            [Column(TypeName = "uuid")]
            public Guid? GuidAsUuid { get; set; }

            [Column(TypeName = "text")]
            public StringEnum16? EnumAsText { get; set; }

            [Column(TypeName = "varchar")]
            public StringEnumU16? EnumAsVarchar { get; set; }

            // PostgreSQL-specific types from here

            [Column(TypeName = "macaddr")]
            public PhysicalAddress PhysicalAddressAsMacaddr { get; set; }

            [Column(TypeName = "point")]
            public NpgsqlPoint? NpgsqlPointAsPoint { get; set; }

            [Column(TypeName = "jsonb")]
            public string StringAsJsonb { get; set; }

            [Column(TypeName = "json")]
            public string StringAsJson { get; set; }

            [Column(TypeName = "hstore")]
            public Dictionary<string, string> DictionaryAsHstore { get; set; }

            [Column(TypeName = "int4range")]
            public NpgsqlRange<int>? NpgsqlRangeAsRange { get; set; }

            [Column(TypeName = "int[]")]
            public int[] IntArrayAsIntArray { get; set; }

            [Column(TypeName = "macaddr[]")]
            public PhysicalAddress[] PhysicalAddressArrayAsMacaddrArray { get; set; }

            [Column(TypeName = "xid")]
            public uint? UintAsXid { get; set; }
        }
    }
}
