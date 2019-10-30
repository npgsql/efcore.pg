using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class RangeQueryNpgsqlTest : IClassFixture<RangeQueryNpgsqlTest.RangeQueryNpgsqlFixture>
    {
        RangeQueryNpgsqlFixture Fixture { get; }

        public RangeQueryNpgsqlTest(RangeQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Operators

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeContainsRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.Contains(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" @> @__range_0)");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotContainRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Contains(range))
                           .ToArray();

                AssertContainsSql(@"WHERE NOT ((r.""Range"" @> @__range_0))");
            }
        }

        [Theory]
        [MemberData(nameof(IntegerTheoryData))]
        public void RangeContainsValue(int value)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.Contains(value))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" @> @__value_0)");
            }
        }

        [Theory]
        [MemberData(nameof(IntegerTheoryData))]
        public void RangeDoesNotContainValue(int value)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Contains(value))
                           .ToArray();

                AssertContainsSql(@"WHERE NOT ((r.""Range"" @> @__value_0))");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeContainedByRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => range.ContainedBy(x.Range))
                           .ToArray();

                AssertContainsSql(@"@__range_0 <@ r.""Range""");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeNotContainedByRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => !range.ContainedBy(x.Range))
                           .ToArray();

                AssertContainsSql(@"WHERE NOT ((@__range_0 <@ r.""Range""))");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeEqualsRange_Operator(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range == range)
                           .ToArray();

                AssertContainsSql(@"r.""Range"" = @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeEqualsRange_Method(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.Equals(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" = @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotEqualsRange_Operator(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range != range)
                           .ToArray();

                AssertContainsSql(@"r.""Range"" <> @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotEqualsRange_Method(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Equals(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" <> @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeOverlapsRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.Overlaps(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" && @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotOverlapRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Overlaps(range))
                           .ToArray();

                AssertContainsSql(@"WHERE NOT ((r.""Range"" && @__range_0))");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsStrictlyLeftOfRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.IsStrictlyLeftOf(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" << @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsNotStrictlyLeftOfRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.IsStrictlyLeftOf(range))
                           .ToArray();

                AssertContainsSql(@"WHERE NOT ((r.""Range"" << @__range_0))");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsStrictlyRightOfRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.IsStrictlyRightOf(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" >> @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsNotStrictlyRightOfRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.IsStrictlyRightOf(range))
                           .ToArray();

                AssertContainsSql(@"WHERE NOT ((r.""Range"" >> @__range_0))");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotExtendLeftOfRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.DoesNotExtendLeftOf(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" &> @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesExtendLeftOfRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.DoesNotExtendLeftOf(range))
                           .ToArray();

                AssertContainsSql(@"WHERE NOT ((r.""Range"" &> @__range_0))");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotExtendRightOfRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.DoesNotExtendRightOf(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" &< @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesExtendRightOfRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.DoesNotExtendRightOf(range))
                           .ToArray();

                AssertContainsSql(@"WHERE NOT ((r.""Range"" &< @__range_0))");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsAdjacentToRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.IsAdjacentTo(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" -|- @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsNotAdjacentToRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.IsAdjacentTo(range))
                           .ToArray();

                AssertContainsSql(@"WHERE NOT ((r.""Range"" -|- @__range_0))");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeUnionRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Select(x => x.Range.Union(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" + @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIntersectsRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                var _ =
                    context.RangeTestEntities
                           .Select(x => x.Range.Intersect(range))
                           .ToArray();

                AssertContainsSql(@"r.""Range"" * @__range_0");
            }
        }

        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeExceptRange(NpgsqlRange<int> range)
        {
            using (var context = Fixture.CreateContext())
            {
                try
                {
                    var _ =
                        context.RangeTestEntities
                               .Select(x => x.Range.Except(range))
                               .ToArray();
                }
                catch (PostgresException)
                {
                    // ignore: Npgsql.PostgresException : 22000: result of range difference would not be contiguous.
                }

                AssertContainsSql(@"r.""Range"" - @__range_0");
            }
        }

        #endregion

        #region User-defined ranges

        [Fact]
        public void UserDefined()
        {
            using (var context = Fixture.CreateContext())
            {
                var e = context.RangeTestEntities.Single(x => x.FloatRange.UpperBound > 5);
                Assert.Equal(0, e.FloatRange.LowerBound);
                Assert.Equal(10, e.FloatRange.UpperBound);
            }
        }

        [Fact]
        public void UserDefinedSchemaQualified()
        {
            using (var context = Fixture.CreateContext())
            {
                var e = context.RangeTestEntities.Single(x => x.SchemaRange == NpgsqlRange<double>.Parse("(0,10)"));
                AssertContainsSql(@"WHERE r.""SchemaRange"" = '(0,10)'::test.""Schema_Range""");
                Assert.Equal(0, e.SchemaRange.LowerBound);
                Assert.Equal(10, e.SchemaRange.UpperBound);
            }
        }

        #endregion

        #region Functions

        [Fact]
        public void RangeLowerBound()
        {
            using (var context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Where(x => x.Range.LowerBound == 0).ToArray();
                AssertContainsSql(@"COALESCE(lower(r.""Range""), 0)");
            }
        }

        [Fact]
        public void RangeUpperBound()
        {
            using (var context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Where(x => x.Range.UpperBound == 10).ToArray();
                AssertContainsSql(@"COALESCE(upper(r.""Range""), 0)");
            }
        }

        [Fact]
        public void RangeIsEmpty()
        {
            using (var context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Where(x => x.Range.IsEmpty).ToArray();
                AssertContainsSql(@"isempty(r.""Range"")");
            }
        }

        [Fact]
        public void RangeLowerBoundIsInclusive()
        {
            using (var context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Where(x => x.Range.LowerBoundIsInclusive).ToArray();
                AssertContainsSql(@"lower_inc(r.""Range"")");
            }
        }

        [Fact]
        public void RangeUpperBoundIsInclusive()
        {
            using (var context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Where(x => x.Range.UpperBoundIsInclusive).ToArray();
                AssertContainsSql(@"upper_inc(r.""Range"")");
            }
        }

        [Fact]
        public void RangeLowerBoundInfinite()
        {
            using (var context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Where(x => x.Range.LowerBoundInfinite).ToArray();
                AssertContainsSql(@"lower_inf(r.""Range"")");
            }
        }

        [Fact]
        public void RangeUpperBoundInfinite()
        {
            using (var context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Where(x => x.Range.UpperBoundInfinite).ToArray();
                AssertContainsSql(@"upper_inf(r.""Range"")");
            }
        }

        [Fact]
        public void RangeMergeRange()
        {
            using (var context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Select(x => x.Range.Merge(x.Range)).ToArray();
                AssertContainsSql(@"range_merge(r.""Range"", r.""Range"")");
            }
        }

        #endregion

        #region TheoryData

        /// <summary>
        /// Provides theory data for integers.
        /// </summary>
        public static IEnumerable<object[]> IntegerTheoryData => Enumerable.Range(-10, 10).Select(x => new object[] { x });

        /// <summary>
        /// Provides theory data for ranges.
        /// </summary>
        public static IEnumerable<object[]> RangeTheoryData =>
            new List<object[]>
            {
                // (0,5)
                new object[] { new NpgsqlRange<int>(0, false, false, 5, false, false) },
                // [0,5]
                new object[] { new NpgsqlRange<int>(0, true, false, 5, true, false) },
                // (,)
                new object[] { new NpgsqlRange<int>(0, false, true, 0, false, true) },
                // (,)
                new object[] { new NpgsqlRange<int>(0, false, true, 5, false, true) },
                // (0,)
                new object[] { new NpgsqlRange<int>(0, false, false, 0, false, true) },
                // (0,)
                new object[] { new NpgsqlRange<int>(0, false, false, 5, false, true) },
                // (,5)
                new object[] { new NpgsqlRange<int>(0, false, true, 5, false, false) }
            };

        #endregion

        #region Fixtures

        /// <summary>
        /// Represents a fixture suitable for testing range operators.
        /// </summary>
        public class RangeQueryNpgsqlFixture : SharedStoreFixtureBase<RangeContext>
        {
            protected override string StoreName => "RangeQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override void Seed(RangeContext context) => RangeContext.Seed(context);

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var optionsBuilder = base.AddOptions(builder);
                var npgsqlOptionsBuilder = new NpgsqlDbContextOptionsBuilder(optionsBuilder);
                npgsqlOptionsBuilder.MapRange("floatrange", typeof(float));
                npgsqlOptionsBuilder.MapRange<double>("Schema_Range", "test");
                return optionsBuilder;
            }
        }

        public class RangeTestEntity
        {
            public int Id { get; set; }
            public NpgsqlRange<int> Range { get; set; }
            public NpgsqlRange<float> FloatRange { get; set; }
            public NpgsqlRange<double> SchemaRange { get; set; }
        }

        /// <summary>
        /// Represents a database suitable for testing range operators.
        /// </summary>
        public class RangeContext : PoolableDbContext
        {
            /// <summary>
            /// Represents a set of entities with <see cref="NpgsqlRange{T}"/> properties.
            /// </summary>
            public DbSet<RangeTestEntity> RangeTestEntities { get; set; }

            /// <summary>
            /// Initializes a <see cref="RangeContext"/>.
            /// </summary>
            /// <param name="options">
            /// The options to be used for configuration.
            /// </param>
            public RangeContext(DbContextOptions options) : base(options) {}

            /// <inheritdoc />
            protected override void OnModelCreating(ModelBuilder builder)
                => builder.HasPostgresRange("floatrange", "real")
                          .HasPostgresRange("test", "Schema_Range", "double precision");

            public static void Seed(RangeContext context)
            {
                context.RangeTestEntities.AddRange(
                    new RangeTestEntity
                    {
                        Id = 1,
                        // (0, 10)
                        Range = new NpgsqlRange<int>(0, false, false, 10, false, false),
                        FloatRange = new NpgsqlRange<float>(0, false, false, 10, false, false),
                        SchemaRange = new NpgsqlRange<double>(0, false, false, 10, false, false)
                    },
                    new RangeTestEntity
                    {
                        Id = 2,
                        // [0, 10)
                        Range = new NpgsqlRange<int>(0, true, false, 10, false, false)
                    },
                    new RangeTestEntity
                    {
                        Id = 3,
                        // [0, 10]
                        Range = new NpgsqlRange<int>(0, true, false, 10, true, false)
                    },
                    new RangeTestEntity
                    {
                        Id = 4,
                        // [0, ∞)
                        Range = new NpgsqlRange<int>(0, true, false, 0, false, true)
                    },
                    new RangeTestEntity
                    {
                        Id = 5,
                        // (-∞, 10]
                        Range = new NpgsqlRange<int>(0, false, true, 10, true, false)
                    },
                    new RangeTestEntity
                    {
                        Id = 6,
                        // (-∞, ∞)
                        Range = new NpgsqlRange<int>(0, false, true, 0, false, true)
                    },
                    new RangeTestEntity
                    {
                        Id = 7,
                        // (-∞, ∞)
                        Range = new NpgsqlRange<int>(0, false, true, 0, false, true)
                    });

                context.SaveChanges();
            }
        }

        #endregion

        #region Helpers

        void AssertContainsSql(string sql) => Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);

        #endregion
    }
}
