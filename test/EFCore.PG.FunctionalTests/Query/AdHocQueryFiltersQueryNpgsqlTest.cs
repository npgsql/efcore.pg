namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocQueryFiltersQueryNpgsqlTest(NonSharedFixture fixture)
    : AdHocQueryFiltersQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public override async Task GroupBy_aggregate_over_required_navigation_with_query_filter(bool async)
    {
        await base.GroupBy_aggregate_over_required_navigation_with_query_filter(async);

        AssertSql(
            """
SELECT d."GroupId" AS "Key", count(*)::int AS "Count", (
    SELECT max(p0."Value")
    FROM "Dependents" AS d0
    INNER JOIN (
        SELECT p."Id", p."Value"
        FROM "Principals" AS p
        WHERE NOT (p."Filtered")
    ) AS p0 ON d0."PrincipalId" = p0."Id"
    WHERE d."GroupId" = d0."GroupId") AS "MaxValue"
FROM "Dependents" AS d
GROUP BY d."GroupId"
""",
            //
            """
SELECT d."GroupId" AS "Key", count(*)::int AS "Count", max(p."Value") AS "MaxValue"
FROM "Dependents" AS d
INNER JOIN "Principals" AS p ON d."PrincipalId" = p."Id"
GROUP BY d."GroupId"
""");
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Row_value_projection_over_filtered_navigation(bool async)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        NpgsqlConnection.GlobalTypeMapper.EnableRecordsAsTuples();
#pragma warning restore CS0618 // Type or member is obsolete

        var contextFactory = await InitializeNonSharedTest<Context3933>(
            seed: async context =>
            {
                context.Orders.Add(new Context3933.Order
                {
                    Id = 1,
                    VendorLinks =
                    [
                        new Context3933.VendorLink
                        {
                            Vendor = new Context3933.Vendor
                            {
                                OrgId = 1,
                                IsActive = true,
                                Representatives = [new Context3933.Representative { IsActive = true }]
                            }
                        }
                    ]
                });
                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var query = context.Orders
            .Where(x => x.Id == 1)
            .Select(x => ValueTuple.Create(
                x.Id,
                x.VendorLinks.Any(vl => vl.Vendor!.IsActive && vl.Vendor.Representatives.Any(r => r.IsActive))));

        var result = async ? await query.FirstOrDefaultAsync() : query.FirstOrDefault();

        Assert.Equal((1, true), result);

        AssertSql(
            """
@ef_filter__OrgId='1'

SELECT (o."Id", EXISTS (
    SELECT 1
    FROM "VendorLink" AS v
    LEFT JOIN (
        SELECT v0."Id", v0."IsActive"
        FROM "Vendor" AS v0
        WHERE v0."OrgId" = @ef_filter__OrgId
    ) AS v1 ON v."VendorId" = v1."Id"
    WHERE o."Id" = v."OrderId" AND v1."IsActive" AND EXISTS (
        SELECT 1
        FROM "Representative" AS r
        WHERE v1."Id" IS NOT NULL AND v1."Id" = r."VendorId" AND r."IsActive")))
FROM "Orders" AS o
WHERE o."Id" = 1
LIMIT 1
""");
    }

    protected class Context3933(DbContextOptions options) : DbContext(options)
    {
        public short OrgId { get; set; } = 1;
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Vendor>().HasQueryFilter(x => x.OrgId == OrgId);

        public class Order
        {
            public int Id { get; set; }
            public List<VendorLink> VendorLinks { get; set; } = [];
        }

        public class VendorLink
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public int? VendorId { get; set; }
            public Vendor? Vendor { get; set; }
        }

        public class Vendor
        {
            public int Id { get; set; }
            public short OrgId { get; set; }
            // Removing this unused property prevents the regression from reproducing.
            public Guid VendorIdentifier { get; set; }
            public bool IsActive { get; set; }
            public List<Representative> Representatives { get; set; } = [];
        }

        public class Representative
        {
            public int Id { get; set; }
            public int VendorId { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
