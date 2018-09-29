using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    /// <summary>
    /// Provides unit tests for range operator translations.
    /// </summary>
    public class RangeQueryNpgsqlTest : IClassFixture<RangeQueryNpgsqlTest.RangeQueryNpgsqlFixture>
    {
        #region Tests

        #region Operators

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

                AssertContainsSql("WHERE (x.\"Range\" @> @__range_0) = TRUE");
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

                AssertContainsSql("WHERE NOT ((x.\"Range\" @> @__range_0) = TRUE)");
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

                AssertContainsSql("WHERE (x.\"Range\" @> @__value_0)");
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

                AssertContainsSql("WHERE NOT ((x.\"Range\" @> @__value_0) = TRUE)");
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

                AssertContainsSql("WHERE (@__range_0 <@ x.\"Range\") = TRUE");
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

                AssertContainsSql("WHERE NOT ((@__range_0 <@ x.\"Range\") = TRUE)");
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

                AssertContainsSql("WHERE (x.\"Range\" && @__range_0)");
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

                AssertContainsSql("WHERE NOT ((x.\"Range\" && @__range_0) = TRUE)");
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

                AssertContainsSql("WHERE (x.\"Range\" << @__range_0)");
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

                AssertContainsSql("WHERE NOT ((x.\"Range\" << @__range_0) = TRUE)");
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

                AssertContainsSql("WHERE (x.\"Range\" >> @__range_0)");
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

                AssertContainsSql("WHERE NOT ((x.\"Range\" >> @__range_0) = TRUE)");
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

                AssertContainsSql("WHERE (x.\"Range\" &> @__range_0)");
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

                AssertContainsSql("WHERE NOT ((x.\"Range\" &> @__range_0) = TRUE)");
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

                AssertContainsSql("WHERE (x.\"Range\" &< @__range_0)");
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

                AssertContainsSql("WHERE NOT ((x.\"Range\" &< @__range_0) = TRUE)");
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

                AssertContainsSql("WHERE (x.\"Range\" -|- @__range_0) = TRUE");
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

                AssertContainsSql("WHERE NOT ((x.\"Range\" -|- @__range_0) = TRUE)");
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

                AssertContainsSql("SELECT (x.\"Range\" + @__range_0)");
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

                AssertContainsSql("SELECT (x.\"Range\" * @__range_0)");
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

                AssertContainsSql("SELECT (x.\"Range\" - @__range_0)");
            }
        }

        #endregion

        #region User-defined ranges

        [Fact]
        public void UserDefined()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                var e = context.RangeTestEntities.Single(x => x.FloatRange.UpperBound > 5);
                Assert.Equal(e.FloatRange.LowerBound, 0);
                Assert.Equal(e.FloatRange.UpperBound, 10);
            }
        }

        [Fact]
        public void UserDefinedSchemaQualified()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                var e = context.RangeTestEntities.Single(x => x.SchemaRange == NpgsqlRange<double>.Parse("(0,10)"));
                AssertContainsSql("WHERE x.\"SchemaRange\" = @__Parse_0");
                Assert.Equal(e.SchemaRange.LowerBound, 0);
                Assert.Equal(e.SchemaRange.UpperBound, 10);
            }
        }

        #endregion

        #region Functions

        [Fact]
        public void RangeLowerBound()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                try
                {
                    var _ = context.RangeTestEntities.Select(x => x.Range.LowerBound).ToArray();
                }
                catch
                {
                    AssertContainsSql("SELECT COALESCE(lower(x.\"Range\"), 0)");
                }

                AssertContainsSql("SELECT COALESCE(lower(x.\"Range\"), 0)");
            }
        }

        [Fact]
        public void RangeUpperBound()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Select(x => x.Range.UpperBound).ToArray();
                AssertContainsSql("SELECT COALESCE(upper(x.\"Range\"), 0)");
            }
        }

        [Fact]
        public void RangeIsEmpty()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Select(x => x.Range.IsEmpty).ToArray();
                AssertContainsSql("SELECT isempty(x.\"Range\")");
            }
        }

        [Fact]
        public void RangeLowerBoundIsInclusive()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Select(x => x.Range.LowerBoundIsInclusive).ToArray();
                AssertContainsSql("SELECT lower_inc(x.\"Range\")");
            }
        }

        [Fact]
        public void RangeUpperBoundIsInclusive()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Select(x => x.Range.UpperBoundIsInclusive).ToArray();
                AssertContainsSql("SELECT upper_inc(x.\"Range\")");
            }
        }

        [Fact]
        public void RangeLowerBoundInfinite()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Select(x => x.Range.LowerBoundInfinite).ToArray();
                AssertContainsSql("SELECT lower_inf(x.\"Range\")");
            }
        }

        [Fact]
        public void RangeUpperBoundInfinite()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Select(x => x.Range.UpperBoundInfinite).ToArray();
                AssertContainsSql("SELECT upper_inf(x.\"Range\")");
            }
        }

        [Fact]
        public void RangeMergeRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                var _ = context.RangeTestEntities.Select(x => x.Range.Merge(x.Range)).ToArray();
                AssertContainsSql("SELECT range_merge(x.\"Range\", x.\"Range\")");
            }
        }

        #endregion

        #endregion

        #region Setup

        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        RangeQueryNpgsqlFixture Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture">The fixture of resources for testing.</param>
        public RangeQueryNpgsqlTest(RangeQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
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
        public class RangeQueryNpgsqlFixture : IDisposable
        {
            /// <summary>
            /// The <see cref="NpgsqlTestStore"/> used for testing.
            /// </summary>
            readonly NpgsqlTestStore _testStore;

            /// <summary>
            /// The <see cref="DbContextOptions"/> used for testing.
            /// </summary>
            readonly DbContextOptions _options;

            /// <summary>
            /// The logger factory used for testing.
            /// </summary>
            public TestSqlLoggerFactory TestSqlLoggerFactory { get; }

            /// <summary>
            /// Initializes a <see cref="RangeQueryNpgsqlFixture"/>.
            /// </summary>
            // ReSharper disable once UnusedMember.Global
            public RangeQueryNpgsqlFixture()
            {
                TestSqlLoggerFactory = new TestSqlLoggerFactory();

                _testStore = NpgsqlTestStore.CreateScratch();

                _options =
                    new DbContextOptionsBuilder()
                        .UseNpgsql(_testStore.ConnectionString, b =>
                        {
                            b.MapRange("floatrange", typeof(float));
                            b.MapRange<double>("Schema_Range", "test");
                            b.ApplyConfiguration();
                        })
                        .UseInternalServiceProvider(
                            new ServiceCollection()
                                .AddEntityFrameworkNpgsql()
                                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                                .BuildServiceProvider())
                        .Options;

                using (RangeContext context = CreateContext())
                {
                    context.Database.EnsureCreated();

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

            /// <summary>
            /// Creates a new <see cref="RangeContext"/>.
            /// </summary>
            /// <returns>
            /// A <see cref="RangeContext"/> for testing.
            /// </returns>
            public RangeContext CreateContext()
            {
                return new RangeContext(_options);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _testStore.Dispose();
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
        public class RangeContext : DbContext
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
                => builder.ForNpgsqlHasRange("floatrange", "real")
                          .ForNpgsqlHasRange("test", "Schema_Range", "double precision");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        public void AssertContainsSql(string sql)
        {
            Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);
        }

        #endregion
    }
}
