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

        #region Tests

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

        #endregion Tests

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
                            b.MapRange(typeof(float), "floatrange");
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
                            FloatRange = new NpgsqlRange<float>(0, false, false, 10, false, false)
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

        /// <summary>
        /// Represents an entity suitable for testing range operators.
        /// </summary>
        public class RangeTestEntity
        {
            /// <summary>
            /// The primary key.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The range of integers.
            /// </summary>
            public NpgsqlRange<int> Range { get; set; }

            /// <summary>
            /// The range of integers.
            /// </summary>
            public NpgsqlRange<float> FloatRange { get; set; }
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
            public RangeContext(DbContextOptions options) : base(options) { }

            /// <inheritdoc />
            protected override void OnModelCreating(ModelBuilder builder)
                => builder.ForNpgsqlHasRange("floatrange", "real");
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
