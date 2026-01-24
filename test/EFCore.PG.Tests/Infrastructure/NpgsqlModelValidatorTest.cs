using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;

// ReSharper disable InconsistentNaming
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

public class NpgsqlModelValidatorTest
{
    [Fact]
    public void Throws_for_WithoutOverlaps_on_primary_key_without_range_type()
    {
        // Configure PG 18 so version check passes, but range type check fails
        var modelBuilder = CreateConventionModelBuilder(
            o => o.UseNpgsql("Host=localhost", npgsqlOptions => npgsqlOptions.SetPostgresVersion(18, 0)));

        modelBuilder.Entity<EntityWithIntPeriod>(b => b.HasKey(e => new { e.Id, e.Period }).WithoutOverlaps());

        VerifyError(
            "WITHOUT OVERLAPS on primary key in entity type 'EntityWithIntPeriod' requires the last column to be a PostgreSQL range type (e.g. daterange, tsrange, tstzrange), but property 'Period' has type 'int'.",
            modelBuilder);
    }

    [Fact]
    public void Throws_for_WithoutOverlaps_on_alternate_key_without_range_type()
    {
        // Configure PG 18 so version check passes, but range type check fails
        var modelBuilder = CreateConventionModelBuilder(
            o => o.UseNpgsql("Host=localhost", npgsqlOptions => npgsqlOptions.SetPostgresVersion(18, 0)));

        modelBuilder.Entity<EntityWithIntPeriod>(
            b =>
            {
                b.HasKey(e => e.Id);
                b.HasAlternateKey(e => new { e.Name, e.Period }).WithoutOverlaps();
            });

        VerifyError(
            "WITHOUT OVERLAPS on alternate key {'Name', 'Period'} in entity type 'EntityWithIntPeriod' requires the last column to be a PostgreSQL range type (e.g. daterange, tsrange, tstzrange), but property 'Period' has type 'int'.",
            modelBuilder);
    }

    [Fact]
    public void Throws_for_WithoutOverlaps_on_primary_key_below_postgres_18()
    {
        // Configure PG 17 to test version check
        var modelBuilder = CreateConventionModelBuilder(
            o => o.UseNpgsql("Host=localhost", npgsqlOptions => npgsqlOptions.SetPostgresVersion(17, 0)));

        // Use int for Period so EF Core's base validation (IComparable check) doesn't run first
        modelBuilder.Entity<EntityWithIntPeriod>(b => b.HasKey(e => new { e.Id, e.Period }).WithoutOverlaps());

        // Our version check happens before the range type check
        VerifyError(
            "WITHOUT OVERLAPS on primary key in entity type 'EntityWithIntPeriod' requires PostgreSQL 18.0 or later.",
            modelBuilder);
    }

    [ConditionalFact]
    public void Throws_for_Period_on_foreign_key_without_without_overlaps_on_principal()
    {
        // Configure PG 18 so version check passes
        var modelBuilder = CreateConventionModelBuilder(
            o => o.UseNpgsql("Host=localhost", npgsqlOptions => npgsqlOptions.SetPostgresVersion(18, 0)));

        // Use int for Period to avoid EF Core's base validation issues with NpgsqlRange<DateTime>
        // Principal key does NOT have WithoutOverlaps configured
        modelBuilder.Entity<PrincipalWithIntPeriod>(
            b =>
            {
                b.HasKey(e => new { e.Id, e.Period });
            });
        modelBuilder.Entity<DependentWithIntPeriod>(
            b =>
            {
                b.HasKey(e => e.Id);
                b.HasOne<PrincipalWithIntPeriod>().WithMany()
                    .HasForeignKey(e => new { e.PrincipalId, e.Period })
                    .HasPrincipalKey(e => new { e.Id, e.Period })
                    .WithPeriod();
            });

        // The PERIOD validation should fail because principal key doesn't have WITHOUT OVERLAPS
        // Note: We would also fail the range type check, but the WITHOUT OVERLAPS check comes first
        VerifyError(
            "PERIOD on foreign key '{'PrincipalId', 'Period'}' in entity type 'DependentWithIntPeriod' requires the referenced primary key in entity type 'PrincipalWithIntPeriod' to be configured with WITHOUT OVERLAPS.",
            modelBuilder);
    }

    private class EntityWithIntPeriod
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Period { get; set; }
    }

    private class PrincipalWithIntPeriod
    {
        public int Id { get; set; }
        public int Period { get; set; }
    }

    private class DependentWithIntPeriod
    {
        public int Id { get; set; }
        public int PrincipalId { get; set; }
        public int Period { get; set; }
    }

    protected virtual TestHelpers.TestModelBuilder CreateConventionModelBuilder(
        Func<DbContextOptionsBuilder, DbContextOptionsBuilder> configureContext = null)
        => NpgsqlTestHelpers.Instance.CreateConventionBuilder(configureContext: configureContext);

    protected virtual void VerifyError(string expectedMessage, TestHelpers.TestModelBuilder modelBuilder)
    {
        var message = Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message;
        Assert.StartsWith(expectedMessage, message);
    }

    protected virtual IModel Validate(TestHelpers.TestModelBuilder modelBuilder)
        => modelBuilder.FinalizeModel();
}
