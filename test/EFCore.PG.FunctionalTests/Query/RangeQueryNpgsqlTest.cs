using System.Collections.Generic;
using System.Linq;
using NpgsqlTypes;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    /// <summary>
    /// Provides unit tests for range operator translations.
    /// </summary>
    public class RangeQueryNpgsqlTest : IClassFixture<RangeQueryNpgsqlFixture>
    {
        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        RangeQueryNpgsqlFixture Fixture { get; }

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

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture">The fixture of resources for testing.</param>
        public RangeQueryNpgsqlTest(RangeQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        public void AssertContainsSql(string sql)
        {
            Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T},NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeContainsRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.Contains(range))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" @> @__range_0 = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotContainRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Contains(range))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Range\" @> @__range_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(IntegerTheoryData))]
        public void RangeContainsValue(int value)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.Contains(value))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" @> @__value_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(IntegerTheoryData))]
        public void RangeDoesNotContainValue(int value)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Contains(value))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Range\" @> @__value_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeContainedByRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => range.ContainedBy(x.Range))
                           .ToArray();

                AssertContainsSql("WHERE @__range_0 <@ x.\"Range\" = TRUE");
            }
        }

        /// <summary>
        /// Tests inverse containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeNotContainedByRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => !range.ContainedBy(x.Range))
                           .ToArray();

                AssertContainsSql("WHERE NOT (@__range_0 <@ x.\"Range\" = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/> via the symbolic operator.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeEqualsRange_Operator(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range == range)
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" = @__range_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/> via method call.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeEqualsRange_Method(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.Equals(range))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" = @__range_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/> via symbolic operator.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotEqualsRange_Operator(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range != range)
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" <> @__range_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/> via method call.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotEqualsRange_Method(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Equals(range))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" <> @__range_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.Overlaps{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeOvelapsRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.Overlaps(range))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" && @__range_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlRangeExtensions.Overlaps{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotOvelapRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Overlaps(range))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Range\" && @__range_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.IsStrictlyLeftOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsStrictlyLeftOfRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.IsStrictlyLeftOf(range))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" << @__range_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlRangeExtensions.IsStrictlyLeftOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsNotStrictlyLeftOfRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.IsStrictlyLeftOf(range))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Range\" << @__range_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.IsStrictlyRightOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsStrictlyRightOfRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.IsStrictlyRightOf(range))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" >> @__range_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlRangeExtensions.IsStrictlyRightOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsNotStrictlyRightOfRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.IsStrictlyRightOf(range))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Range\" >> @__range_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.DoesNotExtendLeftOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotExtendLeftOfRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.DoesNotExtendLeftOf(range))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" &> @__range_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlRangeExtensions.DoesNotExtendLeftOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesExtendLeftOfRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.DoesNotExtendLeftOf(range))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Range\" &> @__range_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.DoesNotExtendRightOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesNotExtendRightOfRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.DoesNotExtendRightOf(range))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" &< @__range_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlRangeExtensions.DoesNotExtendRightOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeDoesExtendRightOfRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.DoesNotExtendRightOf(range))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Range\" &< @__range_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.IsAdjacentTo{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsAdjacentToRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => x.Range.IsAdjacentTo(range))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Range\" -|- @__range_0");
            }
        }

        /// <summary>
        /// Tests inverse translation for <see cref="NpgsqlRangeExtensions.IsAdjacentTo{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIsNotAdjacentToRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] _ =
                    context.RangeTestEntities
                           .Where(x => !x.Range.IsAdjacentTo(range))
                           .ToArray();

                AssertContainsSql("WHERE NOT (x.\"Range\" -|- @__range_0 = TRUE)");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.Union{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeUnionRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                NpgsqlRange<int>[] _ =
                    context.RangeTestEntities
                           .Select(x => x.Range.Union(range))
                           .ToArray();

                AssertContainsSql("SELECT x.\"Range\" + @__range_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.Intersect{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeIntersectsRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                NpgsqlRange<int>[] _ =
                    context.RangeTestEntities
                           .Select(x => x.Range.Intersect(range))
                           .ToArray();

                AssertContainsSql("SELECT x.\"Range\" * @__range_0");
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRangeExtensions.Except{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void RangeExceptRange(NpgsqlRange<int> range)
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                try
                {
                    NpgsqlRange<int>[] _ =
                        context.RangeTestEntities
                               .Select(x => x.Range.Except(range))
                               .ToArray();
                }
                catch (PostgresException)
                {
                    // ignore: Npgsql.PostgresException : 22000: result of range difference would not be contiguous.
                }

                AssertContainsSql("SELECT x.\"Range\" - @__range_0");
            }
        }
    }
}
