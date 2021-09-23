using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class ValueConvertersEndToEndNpgsqlTest
        : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndNpgsqlTest.ValueConvertersEndToEndSqlServerFixture>
    {
        public ValueConvertersEndToEndNpgsqlTest(ValueConvertersEndToEndSqlServerFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "DateTime and DateTimeOffset, https://github.com/dotnet/efcore/issues/26068")]
        public override void Can_insert_and_read_back_with_conversions(int[] valueOrder)
            => base.Can_insert_and_read_back_with_conversions(valueOrder);

        [ConditionalTheory]
        [InlineData(nameof(ConvertingEntity.BoolAsChar), "character(1)", false)]
        [InlineData(nameof(ConvertingEntity.BoolAsNullableChar), "character(1)", false)]
        [InlineData(nameof(ConvertingEntity.BoolAsString), "character varying(3)", false)]
        [InlineData(nameof(ConvertingEntity.BoolAsInt), "integer", false)]
        [InlineData(nameof(ConvertingEntity.BoolAsNullableString), "character varying(3)", false)]
        [InlineData(nameof(ConvertingEntity.BoolAsNullableInt), "integer", false)]
        [InlineData(nameof(ConvertingEntity.IntAsLong), "bigint", false)]
        [InlineData(nameof(ConvertingEntity.IntAsNullableLong), "bigint", false)]
        [InlineData(nameof(ConvertingEntity.BytesAsString), "text", false)]
        [InlineData(nameof(ConvertingEntity.BytesAsNullableString), "text", false)]
        [InlineData(nameof(ConvertingEntity.CharAsString), "character varying(1)", false)]
        [InlineData(nameof(ConvertingEntity.CharAsNullableString), "character varying(1)", false)]
        [InlineData(nameof(ConvertingEntity.DateTimeOffsetToBinary), "bigint", false)]
        [InlineData(nameof(ConvertingEntity.DateTimeOffsetToNullableBinary), "bigint", false)]
        [InlineData(nameof(ConvertingEntity.DateTimeOffsetToString), "character varying(48)", false)]
        [InlineData(nameof(ConvertingEntity.DateTimeOffsetToNullableString), "character varying(48)", false)]
        [InlineData(nameof(ConvertingEntity.DateTimeToBinary), "bigint", false)]
        [InlineData(nameof(ConvertingEntity.DateTimeToNullableBinary), "bigint", false)]
        [InlineData(nameof(ConvertingEntity.DateTimeToString), "character varying(48)", false)]
        [InlineData(nameof(ConvertingEntity.DateTimeToNullableString), "character varying(48)", false)]
        [InlineData(nameof(ConvertingEntity.EnumToString), "text", false)]
        [InlineData(nameof(ConvertingEntity.EnumToNullableString), "text", false)]
        [InlineData(nameof(ConvertingEntity.EnumToNumber), "bigint", false)]
        [InlineData(nameof(ConvertingEntity.EnumToNullableNumber), "bigint", false)]
        [InlineData(nameof(ConvertingEntity.GuidToString), "character varying(36)", false)]
        [InlineData(nameof(ConvertingEntity.GuidToNullableString), "character varying(36)", false)]
        [InlineData(nameof(ConvertingEntity.GuidToBytes), "bytea", false)]
        [InlineData(nameof(ConvertingEntity.GuidToNullableBytes), "bytea", false)]
        [InlineData(nameof(ConvertingEntity.IPAddressToString), "character varying(45)", false)]
        [InlineData(nameof(ConvertingEntity.IPAddressToNullableString), "character varying(45)", false)]
        [InlineData(nameof(ConvertingEntity.IPAddressToBytes), "bytea", false)]
        [InlineData(nameof(ConvertingEntity.IPAddressToNullableBytes), "bytea", false)]
        [InlineData(nameof(ConvertingEntity.PhysicalAddressToString), "character varying(20)", false)]
        [InlineData(nameof(ConvertingEntity.PhysicalAddressToNullableString), "character varying(20)", false)]
        [InlineData(nameof(ConvertingEntity.PhysicalAddressToBytes), "bytea", false)]
        [InlineData(nameof(ConvertingEntity.PhysicalAddressToNullableBytes), "bytea", false)]
        [InlineData(nameof(ConvertingEntity.NumberToString), "character varying(64)", false)]
        [InlineData(nameof(ConvertingEntity.NumberToNullableString), "character varying(64)", false)]
        [InlineData(nameof(ConvertingEntity.NumberToBytes), "bytea", false)]
        [InlineData(nameof(ConvertingEntity.NumberToNullableBytes), "bytea", false)]
        [InlineData(nameof(ConvertingEntity.StringToBool), "boolean", false)]
        [InlineData(nameof(ConvertingEntity.StringToNullableBool), "boolean", false)]
        [InlineData(nameof(ConvertingEntity.StringToBytes), "bytea", false)]
        [InlineData(nameof(ConvertingEntity.StringToNullableBytes), "bytea", false)]
        [InlineData(nameof(ConvertingEntity.StringToChar), "character(1)", false)]
        [InlineData(nameof(ConvertingEntity.StringToNullableChar), "character(1)", false)]
        [InlineData(nameof(ConvertingEntity.StringToDateTime), "timestamp with time zone", false)]
        [InlineData(nameof(ConvertingEntity.StringToNullableDateTime), "timestamp with time zone", false)]
        // [InlineData(nameof(ConvertingEntity.StringToDateTimeOffset), "timestamp with time zone", false)]
        // [InlineData(nameof(ConvertingEntity.StringToNullableDateTimeOffset), "timestamp with time zone", false)]
        [InlineData(nameof(ConvertingEntity.StringToEnum), "integer", false)]
        [InlineData(nameof(ConvertingEntity.StringToNullableEnum), "integer", false)]
        [InlineData(nameof(ConvertingEntity.StringToGuid), "uuid", false)]
        [InlineData(nameof(ConvertingEntity.StringToNullableGuid), "uuid", false)]
        [InlineData(nameof(ConvertingEntity.StringToNumber), "smallint", false)]
        [InlineData(nameof(ConvertingEntity.StringToNullableNumber), "smallint", false)]
        [InlineData(nameof(ConvertingEntity.StringToTimeSpan), "interval", false)]
        [InlineData(nameof(ConvertingEntity.StringToNullableTimeSpan), "interval", false)]
        [InlineData(nameof(ConvertingEntity.TimeSpanToTicks), "bigint", false)]
        [InlineData(nameof(ConvertingEntity.TimeSpanToNullableTicks), "bigint", false)]
        [InlineData(nameof(ConvertingEntity.TimeSpanToString), "character varying(48)", false)]
        [InlineData(nameof(ConvertingEntity.TimeSpanToNullableString), "character varying(48)", false)]
        [InlineData(nameof(ConvertingEntity.UriToString), "text", false)]
        [InlineData(nameof(ConvertingEntity.UriToNullableString), "text", false)]
        [InlineData(nameof(ConvertingEntity.NullableCharAsString), "character varying(1)", true)]
        [InlineData(nameof(ConvertingEntity.NullableCharAsNullableString), "character varying(1)", true)]
        [InlineData(nameof(ConvertingEntity.NullableBoolAsChar), "character(1)", true)]
        [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableChar), "character(1)", true)]
        [InlineData(nameof(ConvertingEntity.NullableBoolAsString), "character varying(3)", true)]
        [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableString), "character varying(3)", true)]
        [InlineData(nameof(ConvertingEntity.NullableBoolAsInt), "integer", true)]
        [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableInt), "integer", true)]
        [InlineData(nameof(ConvertingEntity.NullableIntAsLong), "bigint", true)]
        [InlineData(nameof(ConvertingEntity.NullableIntAsNullableLong), "bigint", true)]
        [InlineData(nameof(ConvertingEntity.NullableBytesAsString), "text", true)]
        [InlineData(nameof(ConvertingEntity.NullableBytesAsNullableString), "text", true)]
        [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToBinary), "bigint", true)]
        [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToNullableBinary), "bigint", true)]
        [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToString), "character varying(48)", true)]
        [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToNullableString), "character varying(48)", true)]
        [InlineData(nameof(ConvertingEntity.NullableDateTimeToBinary), "bigint", true)]
        [InlineData(nameof(ConvertingEntity.NullableDateTimeToNullableBinary), "bigint", true)]
        [InlineData(nameof(ConvertingEntity.NullableDateTimeToString), "character varying(48)", true)]
        [InlineData(nameof(ConvertingEntity.NullableDateTimeToNullableString), "character varying(48)", true)]
        [InlineData(nameof(ConvertingEntity.NullableEnumToString), "text", true)]
        [InlineData(nameof(ConvertingEntity.NullableEnumToNullableString), "text", true)]
        [InlineData(nameof(ConvertingEntity.NullableEnumToNumber), "bigint", true)]
        [InlineData(nameof(ConvertingEntity.NullableEnumToNullableNumber), "bigint", true)]
        [InlineData(nameof(ConvertingEntity.NullableGuidToString), "character varying(36)", true)]
        [InlineData(nameof(ConvertingEntity.NullableGuidToNullableString), "character varying(36)", true)]
        [InlineData(nameof(ConvertingEntity.NullableGuidToBytes), "bytea", true)]
        [InlineData(nameof(ConvertingEntity.NullableGuidToNullableBytes), "bytea", true)]
        [InlineData(nameof(ConvertingEntity.NullableIPAddressToString), "character varying(45)", true)]
        [InlineData(nameof(ConvertingEntity.NullableIPAddressToNullableString), "character varying(45)", true)]
        [InlineData(nameof(ConvertingEntity.NullableIPAddressToBytes), "bytea", true)]
        [InlineData(nameof(ConvertingEntity.NullableIPAddressToNullableBytes), "bytea", true)]
        [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToString), "character varying(20)", true)]
        [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToNullableString), "character varying(20)", true)]
        [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToBytes), "bytea", true)]
        [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToNullableBytes), "bytea", true)]
        [InlineData(nameof(ConvertingEntity.NullableNumberToString), "character varying(64)", true)]
        [InlineData(nameof(ConvertingEntity.NullableNumberToNullableString), "character varying(64)", true)]
        [InlineData(nameof(ConvertingEntity.NullableNumberToBytes), "bytea", true)]
        [InlineData(nameof(ConvertingEntity.NullableNumberToNullableBytes), "bytea", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToBool), "boolean", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToNullableBool), "boolean", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToBytes), "bytea", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToNullableBytes), "bytea", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToChar), "character(1)", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToNullableChar), "character(1)", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToDateTime), "timestamp with time zone", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToNullableDateTime), "timestamp with time zone", true)]
        //[InlineData(nameof(ConvertingEntity.NullableStringToDateTimeOffset), "timestamp with time zone", true)]
        //[InlineData(nameof(ConvertingEntity.NullableStringToNullableDateTimeOffset), "timestamp with time zone", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToEnum), "integer", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToNullableEnum), "integer", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToGuid), "uuid", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToNullableGuid), "uuid", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToNumber), "smallint", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToNullableNumber), "smallint", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToTimeSpan), "interval", true)]
        [InlineData(nameof(ConvertingEntity.NullableStringToNullableTimeSpan), "interval", true)]
        [InlineData(nameof(ConvertingEntity.NullableTimeSpanToTicks), "bigint", true)]
        [InlineData(nameof(ConvertingEntity.NullableTimeSpanToNullableTicks), "bigint", true)]
        [InlineData(nameof(ConvertingEntity.NullableTimeSpanToString), "character varying(48)", true)]
        [InlineData(nameof(ConvertingEntity.NullableTimeSpanToNullableString), "character varying(48)", true)]
        [InlineData(nameof(ConvertingEntity.NullableUriToString), "text", true)]
        [InlineData(nameof(ConvertingEntity.NullableUriToNullableString), "text", true)]
        [InlineData(nameof(ConvertingEntity.NullStringToNonNullString), "text", false)]
        [InlineData(nameof(ConvertingEntity.NonNullStringToNullString), "text", true)]
        public virtual void Properties_with_conversions_map_to_appropriately_null_columns(
            string propertyName,
            string databaseType,
            bool isNullable)
        {
            using var context = CreateContext();

            var property = context.Model.FindEntityType(typeof(ConvertingEntity))!.FindProperty(propertyName);

            Assert.Equal(databaseType, property!.GetColumnType());
            Assert.Equal(isNullable, property!.IsNullable);
        }

        public class ValueConvertersEndToEndSqlServerFixture : ValueConvertersEndToEndFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => NpgsqlTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<ConvertingEntity>(
                    b =>
                    {
                        // We map DateTimeOffset to PG 'timestamp with time zone', which doesn't store the time zone - so lossless
                        // round-tripping is impossible.
                        b.Ignore(e => e.StringToDateTimeOffset);
                        b.Ignore(e => e.StringToNullableDateTimeOffset);
                        b.Ignore(e => e.NullableStringToDateTimeOffset);
                        b.Ignore(e => e.NullableStringToNullableDateTimeOffset);
                    });
            }
        }
    }
}
