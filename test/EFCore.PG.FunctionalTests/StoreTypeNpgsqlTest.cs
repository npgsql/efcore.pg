namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class StoreTypeNpgsqlTest : StoreTypeRelationalTestBase
{
    public override Task DateTime_Unspecified()
        => TestType(
            new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Unspecified),
            new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Unspecified),
            onModelCreating: mb => mb.Entity<StoreTypeEntity<DateTime>>(b =>
            {
                // The PG provider maps DateTime properties to 'timestamp with time zone' by default, which requires
                // Kind=Utc. Map to 'timestamp without time zone'.
                b.Property(e => e.Value).HasColumnType("timestamp without time zone");
                b.Property(e => e.OtherValue).HasColumnType("timestamp without time zone");
                b.ComplexProperty(e => e.Container).Property(c => c.Value).HasColumnType("timestamp without time zone");
                b.ComplexProperty(e => e.Container).Property(c => c.OtherValue).HasColumnType("timestamp without time zone");
            }));

    public override Task DateTime_Local()
        => TestType(
            new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Local),
            new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Local),
            onModelCreating: mb => mb.Entity<StoreTypeEntity<DateTime>>(b =>
            {
                // The PG provider maps DateTime properties to 'timestamp with time zone' by default, which requires
                // Kind=Utc. Map to 'timestamp without time zone'.
                b.Property(e => e.Value).HasColumnType("timestamp without time zone");
                b.Property(e => e.OtherValue).HasColumnType("timestamp without time zone");
                b.ComplexProperty(e => e.Container).Property(c => c.Value).HasColumnType("timestamp without time zone");
                b.ComplexProperty(e => e.Container).Property(c => c.OtherValue).HasColumnType("timestamp without time zone");
            }));

    // PostgreSQL does not support persisting the offset, and so the provider accepts only DateTimeOffsets
    // with offset zero.
    public override Task DateTimeOffset()
        => TestType(
            new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(0)),
            new DateTimeOffset(2021, 1, 5, 12, 30, 45, TimeSpan.FromHours(0)));

    protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
}
