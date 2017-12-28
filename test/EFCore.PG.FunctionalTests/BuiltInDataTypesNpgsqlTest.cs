using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class BuiltInDataTypesNpgsqlTest : BuiltInDataTypesTestBase<BuiltInDataTypesNpgsqlTest.BuiltInDataTypesNpgsqlFixture>
    {
        public BuiltInDataTypesNpgsqlTest(BuiltInDataTypesNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        [Fact]
        public virtual void Can_query_using_any_mapped_data_type()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Tinyint = 80,
                        Smallint = 79,
                        Int = 999,
                        Bigint = 78L,
                        Real = 84.4f,
                        Double_precision = 85.5,
                        Decimal = 101.7m,
                        Numeric = 103.9m,

                        Text = "Gumball Rules!",
                        Bytea = new byte[] { 86 },

                        Timestamp = new DateTime(2015, 1, 2, 10, 11, 12),
                        Timestamptz = new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc),
                        Date = new DateTime(2015, 1, 2, 0, 0, 0),
                        Time = new TimeSpan(11, 15, 12),
                        Timetz = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)),
                        Interval = new TimeSpan(11, 15, 12),

                        Uuid = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),
                        Bool = true,

                        Macaddr = PhysicalAddress.Parse("08-00-2B-01-02-03"),
                        Point = new NpgsqlPoint(5.2, 3.3),
                        Jsonb = @"{""a"": ""b""}",
                        Hstore = new Dictionary<string, string> { { "a", "b" } },

                        //SomeComposite = new SomeComposite { SomeNumber = 8, SomeText = "foo" }

                        PrimitiveArray = new[] { 2, 3 },
                        NonPrimitiveArray = new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") },

                        Xid = (uint)int.MaxValue + 1,
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999);

                byte? param1 = 80;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Tinyint == param1));

                short? param2 = 79;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Smallint == param2));

                long? param3 = 78L;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Bigint == param3));

                float? param4 = 84.4f;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Real == param4));

                double? param5 = 85.5;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Double_precision == param5));

                decimal? param6 = 101.7m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Decimal == param6));

                decimal? param7 = 103.9m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Numeric == param7));

                var param8 = "Gumball Rules!";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Text == param8));

                var param9 = new byte[] { 86 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Bytea == param9));

                DateTime? param10 = new DateTime(2015, 1, 2, 10, 11, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Timestamp == param10));

                DateTime? param11 = new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Timestamptz == param11));

                DateTime? param12 = new DateTime(2015, 1, 2, 0, 0, 0);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Date == param12));

                TimeSpan? param13 = new TimeSpan(11, 15, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Time == param13));

                DateTimeOffset? param14 = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2));
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Timetz == param14));

                TimeSpan? param15 = new TimeSpan(11, 15, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Interval == param15));

                Guid? param16 = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Uuid == param16));

                bool? param17 = true;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Bool == param17));

                var param18 = PhysicalAddress.Parse("08-00-2B-01-02-03");
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Macaddr.Equals(param18)));

                // PostgreSQL doesn't support equality comparison on point
                // NpgsqlPoint? param19 = new NpgsqlPoint(5.2, 3.3);
                // Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Point == param19));

                var param20 = @"{""a"": ""b""}";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Jsonb == param20));

                var param21 = new Dictionary<string, string> { { "a", "b" } };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Hstore == param21));

                //SomeComposite param22 = new SomeComposite { SomeNumber = 8, SomeText = "foo" };
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.SomeComposite.Equals(param20)));

                var param23 = new[] { 2, 3 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.PrimitiveArray == param23));

                var param24 = new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.NonPrimitiveArray == param24));

                var param25 = (uint)int.MaxValue + 1;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Xid == param25));
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

                byte? param1 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Bigint == param1));

                short? param2 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Smallint == param2));

                long? param3 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Bigint == param3));

                float? param4 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Real == param4));

                double? param5 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Double_precision == param5));

                decimal? param6 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Decimal == param6));

                decimal? param7 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Numeric == param7));

                string param8 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Text == param8));

                byte[] param9 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Bytea == param9));

                DateTime? param10 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Timestamp == param10));

                DateTime? param11 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Timestamptz == param11));

                DateTime? param12 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Date == param12));

                TimeSpan? param13 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Time == param13));

                DateTimeOffset? param14 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Timetz == param14));

                TimeSpan? param15 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Interval == param15));

                Guid? param16 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Uuid == param16));

                bool? param17 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Bool == param17));

                PhysicalAddress param18 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Macaddr == param18));

                NpgsqlPoint? param19 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Point == param19));

                string param20 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Jsonb == param20));

                Dictionary<string, string> param21 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Hstore == param21));

                //SomeComposite param22 = null;
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SomeComposite == param20));

                int[] param23 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.PrimitiveArray == param23));

                PhysicalAddress[] param24 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.NonPrimitiveArray == param24));

                uint? param25 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Xid == param25));
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(
                    new MappedDataTypes
                    {
                        Tinyint = 80,
                        Smallint = 79,
                        Int = 77,
                        Bigint = 78L,
                        Real = 84.4f,
                        Double_precision = 85.5,
                        Decimal = 101.1m,
                        Numeric = 103.3m,

                        Text = "Gumball Rules!",
                        Bytea = new byte[] { 86 },

                        Timestamp = new DateTime(2016, 1, 2, 11, 11, 12),
                        Timestamptz = new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc),
                        Date = new DateTime(2015, 1, 2, 10, 11, 12),
                        Time = new TimeSpan(11, 15, 12),
                        Timetz = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)),
                        Interval = new TimeSpan(11, 15, 12),

                        Uuid = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),
                        Bool = true,

                        Macaddr = PhysicalAddress.Parse("08-00-2B-01-02-03"),
                        Point = new NpgsqlPoint(5.2, 3.3),
                        Jsonb = @"{""a"": ""b""}",
                        Hstore = new Dictionary<string, string> { { "a", "b" } },

                        //SomeComposite = new SomeComposite { SomeNumber = 8, SomeText = "foo" }
                        PrimitiveArray = new[] { 2, 3 },
                        NonPrimitiveArray = new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") },

                        Xid = (uint)int.MaxValue + 1,
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var connString = context.Database.GetDbConnection().ConnectionString;
                var entity = context.Set<MappedDataTypes>().Single(e => e.Int == 77);

                Assert.Equal(80, entity.Tinyint);
                Assert.Equal(79, entity.Smallint);
                Assert.Equal(77, entity.Int);
                Assert.Equal(78, entity.Bigint);
                Assert.Equal(84.4f, entity.Real);
                Assert.Equal(85.5, entity.Double_precision);
                Assert.Equal(101.1m, entity.Decimal);
                Assert.Equal(103.3m, entity.Numeric);

                Assert.Equal("Gumball Rules!", entity.Text);
                Assert.Equal(new byte[] { 86 }, entity.Bytea);

                Assert.Equal(new DateTime(2016, 1, 2, 11, 11, 12), entity.Timestamp);
                Assert.Equal(new DateTime(2016, 1, 2, 11, 11, 12), entity.Timestamptz);
                Assert.Equal(new DateTime(2015, 1, 2, 0, 0, 0), entity.Date);
                Assert.Equal(new TimeSpan(11, 15, 12), entity.Time);
                Assert.Equal(new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)), entity.Timetz);
                Assert.Equal(new TimeSpan(11, 15, 12), entity.Interval);

                Assert.Equal(new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"), entity.Uuid);
                Assert.True(entity.Bool);

                Assert.Equal(PhysicalAddress.Parse("08-00-2B-01-02-03"), entity.Macaddr);
                Assert.Equal(new NpgsqlPoint(5.2, 3.3), entity.Point);
                Assert.Equal(@"{""a"": ""b""}", entity.Jsonb);
                Assert.Equal(new Dictionary<string, string> { { "a", "b" } }, entity.Hstore);

                //Assert.Equal(new SomeComposite { SomeNumber = 8, SomeText = "foo" }, entity.SomeComposite);
                Assert.Equal(new[] { 2, 3 }, entity.PrimitiveArray);
                Assert.Equal(new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") }, entity.NonPrimitiveArray);
                Assert.Equal((uint)int.MaxValue + 1, entity.Xid);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Tinyint = 80,
                        Smallint = 79,
                        Int = 77,
                        Bigint = 78L,
                        Real = 84.4f,
                        Double_precision = 85.5,
                        Decimal = 101.1m,
                        Numeric = 103.3m,

                        Text = "Gumball Rules!",
                        Bytea = new byte[] { 86 },

                        Timestamp = new DateTime(2016, 1, 2, 11, 11, 12),
                        Timestamptz = new DateTime(2016, 1, 2, 11, 11, 12, DateTimeKind.Utc),
                        Date = new DateTime(2015, 1, 2, 10, 11, 12),
                        Time = new TimeSpan(11, 15, 12),
                        Timetz = new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)),
                        Interval = new TimeSpan(11, 15, 12),

                        Uuid = new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),
                        Bool = true,

                        Macaddr = PhysicalAddress.Parse("08-00-2B-01-02-03"),
                        Point = new NpgsqlPoint(5.2, 3.3),
                        Jsonb = @"{""a"": ""b""}",
                        Hstore = new Dictionary<string, string> { { "a", "b" } },

                        //SomeComposite = new SomeComposite { SomeNumber = 8, SomeText = "foo" }
                        PrimitiveArray = new[] { 2, 3 },
                        NonPrimitiveArray = new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") },
                        Xid = (uint)int.MaxValue + 1,
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 77);

                Assert.Equal(80, entity.Tinyint.Value);
                Assert.Equal(79, entity.Smallint.Value);
                Assert.Equal(77, entity.Int);
                Assert.Equal(78, entity.Bigint);
                Assert.Equal(84.4f, entity.Real);
                Assert.Equal(85.5, entity.Double_precision);
                Assert.Equal(101.1m, entity.Decimal);
                Assert.Equal(103.3m, entity.Numeric);

                Assert.Equal("Gumball Rules!", entity.Text);
                Assert.Equal(new byte[] { 86 }, entity.Bytea);

                Assert.Equal(new DateTime(2016, 1, 2, 11, 11, 12), entity.Timestamp);
                Assert.Equal(new DateTime(2016, 1, 2, 11, 11, 12), entity.Timestamptz);
                Assert.Equal(new DateTime(2015, 1, 2, 0, 0, 0), entity.Date);
                Assert.Equal(new TimeSpan(11, 15, 12), entity.Time);
                Assert.Equal(new DateTimeOffset(1, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)), entity.Timetz);
                Assert.Equal(new TimeSpan(11, 15, 12), entity.Interval);

                Assert.Equal(new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"), entity.Uuid);
                Assert.True(entity.Bool);

                Assert.Equal(PhysicalAddress.Parse("08-00-2B-01-02-03"), entity.Macaddr);
                Assert.Equal(new NpgsqlPoint(5.2, 3.3), entity.Point);
                Assert.Equal(@"{""a"": ""b""}", entity.Jsonb);
                Assert.Equal(new Dictionary<string, string> { { "a", "b" } }, entity.Hstore);

                //Assert.Equal(new SomeComposite { SomeNumber = 8, SomeText = "foo" }, entity.SomeComposite);

                Assert.Equal(new[] { 2, 3 }, entity.PrimitiveArray);
                Assert.Equal(new[] { PhysicalAddress.Parse("08-00-2B-01-02-03"), PhysicalAddress.Parse("08-00-2B-01-02-04") }, entity.NonPrimitiveArray);
                Assert.Equal((uint)int.MaxValue + 1, entity.Xid);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Int = 78
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 78);

                Assert.Null(entity.Tinyint);
                Assert.Null(entity.Smallint);
                Assert.Null(entity.Bigint);
                Assert.Null(entity.Real);
                Assert.Null(entity.Double_precision);
                Assert.Null(entity.Decimal);
                Assert.Null(entity.Numeric);

                Assert.Null(entity.Text);
                Assert.Null(entity.Bytea);

                Assert.Null(entity.Timestamp);
                Assert.Null(entity.Timestamptz);
                Assert.Null(entity.Date);
                Assert.Null(entity.Time);
                Assert.Null(entity.Timetz);
                Assert.Null(entity.Interval);

                Assert.Null(entity.Uuid);
                Assert.Null(entity.Bool);

                Assert.Null(entity.Macaddr);
                Assert.Null(entity.Point);
                Assert.Null(entity.Jsonb);
                Assert.Null(entity.Hstore);

                //Assert.Null(entity.SomeComposite);
                Assert.Null(entity.PrimitiveArray);
                Assert.Null(entity.NonPrimitiveArray);
                Assert.Null(entity.Xid);
            }
        }

        public class BuiltInDataTypesNpgsqlFixture : BuiltInDataTypesFixtureBase
        {

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                MakeRequired<MappedDataTypes>(modelBuilder);

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
                });

                modelBuilder.Entity<MappedDataTypes>(b =>
                {
                    b.HasKey(e => e.Int);
                    b.Property(e => e.Int)
                     .ValueGeneratedNever();
                });

                modelBuilder.Entity<MappedNullableDataTypes>(b =>
                {
                    b.HasKey(e => e.Int);
                    b.Property(e => e.Int)
                     .ValueGeneratedNever();
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

                MapColumnTypes<MappedDataTypes>(modelBuilder);
                MapColumnTypes<MappedNullableDataTypes>(modelBuilder);

                MapSizedColumnTypes<MappedSizedDataTypes>(modelBuilder);
                MapSizedColumnTypes<MappedScaledDataTypes>(modelBuilder);
                MapPreciseColumnTypes<MappedPrecisionAndScaledDataTypes>(modelBuilder);

                // MapColumnTypes automatically mapped column types based on the property name, but
                // this doesn't work for Tinyint. Remap.
                modelBuilder.Entity<MappedDataTypes>().Property(e => e.Tinyint).HasColumnType("smallint");
                modelBuilder.Entity<MappedNullableDataTypes>().Property(e => e.Tinyint).HasColumnType("smallint");

                // Jsonb in .NET is a regular string
                modelBuilder.Entity<MappedDataTypes>().Property(e => e.Jsonb).HasColumnType("jsonb");
                modelBuilder.Entity<MappedNullableDataTypes>().Property(e => e.Jsonb).HasColumnType("jsonb");

                // Arrays
                modelBuilder.Entity<MappedDataTypes>().Property(e => e.PrimitiveArray).HasColumnType("_int4");
                modelBuilder.Entity<MappedNullableDataTypes>().Property(e => e.PrimitiveArray).HasColumnType("_int4");
                modelBuilder.Entity<MappedDataTypes>().Property(e => e.NonPrimitiveArray).HasColumnType("_macaddr");
                modelBuilder.Entity<MappedNullableDataTypes>().Property(e => e.NonPrimitiveArray).HasColumnType("_macaddr");

                modelBuilder.Entity<MappedDataTypes>().Property(e => e.Xid).HasColumnType("xid");
                modelBuilder.Entity<MappedNullableDataTypes>().Property(e => e.Xid).HasColumnType("xid");

                // TimeTz
                modelBuilder.Entity<MappedDataTypes>().Property(e => e.Timetz).HasColumnType("timetz");
                modelBuilder.Entity<MappedNullableDataTypes>().Property(e => e.Timetz).HasColumnType("timetz");
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(
                    c => c
                        .Log(RelationalEventId.QueryClientEvaluationWarning));

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            private static void MapColumnTypes<TEntity>(ModelBuilder modelBuilder) where TEntity : class
            {
                var entityType = modelBuilder.Entity<TEntity>().Metadata;

                foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties)
                {
                    var columnType = propertyInfo.Name;

                    if (columnType.EndsWith("Max"))
                    {
                        columnType = columnType.Substring(0, columnType.IndexOf("Max")) + "(max)";
                    }

                    columnType = columnType.Replace('_', ' ');

                    entityType.GetOrAddProperty(propertyInfo).Relational().ColumnType = columnType;
                }
            }

            private static void MapSizedColumnTypes<TEntity>(ModelBuilder modelBuilder) where TEntity : class
            {
                var entityType = modelBuilder.Entity<TEntity>().Metadata;

                foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties.Where(p => p.Name != "Id"))
                {
                    entityType.GetOrAddProperty(propertyInfo).Relational().ColumnType = propertyInfo.Name.Replace('_', ' ') + "(3)";
                }
            }

            private static void MapPreciseColumnTypes<TEntity>(ModelBuilder modelBuilder) where TEntity : class
            {
                var entityType = modelBuilder.Entity<TEntity>().Metadata;

                foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties.Where(p => p.Name != "Id"))
                {
                    entityType.GetOrAddProperty(propertyInfo).Relational().ColumnType = propertyInfo.Name.Replace('_', ' ') + "(5, 2)";
                }
            } 
        }

        public class MappedDataTypes
        {
            public byte Tinyint { get; set; }
            public short Smallint { get; set; }
            public int Int { get; set; }
            public long Bigint { get; set; }
            public float Real { get; set; }
            public double Double_precision { get; set; }
            public decimal Decimal { get; set; }
            public decimal Numeric { get; set; }

            public string Text { get; set; }
            public byte[] Bytea { get; set; }

            public DateTime Timestamp { get; set; }
            public DateTime Timestamptz { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan Time { get; set; }
            public DateTimeOffset Timetz { get; set; }
            public TimeSpan Interval { get; set; }

            public Guid Uuid { get; set; }
            public bool Bool { get; set; }

            // Types supported only on PostgreSQL
            public PhysicalAddress Macaddr { get; set; }
            public NpgsqlPoint Point { get; set; }
            public string Jsonb { get; set; }
            public Dictionary<string, string> Hstore { get; set; }

            // Composite
            //public SomeComposite SomeComposite { get; set; }

            // Array
            public int[] PrimitiveArray { get; set; }
            public PhysicalAddress[] NonPrimitiveArray { get; set; }

            public uint Xid { get; set; }
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

        public class MappedNullableDataTypes
        {
            public byte? Tinyint { get; set; }
            public short? Smallint { get; set; }
            public int? Int { get; set; }
            public long? Bigint { get; set; }
            public float? Real { get; set; }
            public double? Double_precision { get; set; }
            public decimal? Decimal { get; set; }
            public decimal? Numeric { get; set; }

            public string Text { get; set; }
            public byte[] Bytea { get; set; }

            public DateTime? Timestamp { get; set; }
            public DateTime? Timestamptz { get; set; }
            public DateTime? Date { get; set; }
            public TimeSpan? Time { get; set; }
            public DateTimeOffset? Timetz { get; set; }
            public TimeSpan? Interval { get; set; }

            public Guid? Uuid { get; set; }
            public bool? Bool { get; set; }

            // Types supported only on PostgreSQL
            public PhysicalAddress Macaddr { get; set; }
            public NpgsqlPoint? Point { get; set; }
            public string Jsonb { get; set; }
            public Dictionary<string, string> Hstore { get; set; }

            // Composite
            //public SomeComposite SomeComposite { get; set; }

            // Array
            public int[] PrimitiveArray { get; set; }
            public PhysicalAddress[] NonPrimitiveArray { get; set; }

            public uint? Xid { get; set; }
        }
        // TODO: Other tests from NpgsqlBuiltInDataTypesNpgsqlTest?
    }
}
