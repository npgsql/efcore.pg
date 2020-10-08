using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ArrayTests;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ArrayQueryTest : IClassFixture<ArrayQueryTest.ArrayArrayQueryFixture>
    {
        ArrayArrayQueryFixture Fixture { get; }

        public ArrayQueryTest(ArrayArrayQueryFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Roundtrip

        [Fact]
        public void Roundtrip()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => e.Id == 1);

            Assert.Equal(new[] { 3, 4 }, x.IntArray);
            Assert.Equal(new List<int> { 3, 4 }, x.IntList);
            Assert.Equal(new int?[] { 3, 4, null}, x.NullableIntArray);
            Assert.Equal(new List<int?> { 3, 4, null}, x.NullableIntList);
        }

        #endregion

        #region Indexers

        [Fact]
        public void Array_index_with_constant()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.IntArray[0] == 3)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntArray""[1] = 3
LIMIT 2");
        }

        [Fact]
        public void List_index_with_constant()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.IntList[0] == 3)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntList""[1] = 3
LIMIT 2");
        }

        [Fact]
        public void Nullable_array_index_with_constant()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.NullableIntArray[0] == 3)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""NullableIntArray""[1] = 3
LIMIT 2");
        }

        [Fact]
        public void Nullable_list_index_with_constant()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.NullableIntList[0] == 3)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""NullableIntList""[1] = 3
LIMIT 2");
        }

        [Fact]
        public void Index_with_non_constant()
        {
            using var ctx = CreateContext();
            // ReSharper disable once ConvertToConstant.Local
            var x = 0;
            var id = ctx.SomeEntities
                .Where(e => e.IntArray[x] == 3)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"@__x_0='0'

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntArray""[@__x_0 + 1] = 3
LIMIT 2");
        }

        [Fact]
        public void List_index_with_non_constant()
        {
            using var ctx = CreateContext();
            // ReSharper disable once ConvertToConstant.Local
            var x = 0;
            var id = ctx.SomeEntities
                .Where(e => e.IntList[x] == 3)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"@__x_0='0'

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntList""[@__x_0 + 1] = 3
LIMIT 2");
        }

        #endregion

        #region SequenceEqual

        [Theory]
        [MemberData(nameof(IsListData))]
        public void SequenceEqual_with_parameter(bool list)
        {
            using var ctx = CreateContext();
            var arr = new[] { 3, 4 };
            var id = ctx.SomeEntities
                .Where(e => e.IntArray.SequenceEqual(arr))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();

            Assert.Equal(1, id);
            AssertSql(list,
                @"@__arr_0='System.Int32[]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntArray"" = @__arr_0
LIMIT 2");
        }

        [Theory]
        [MemberData(nameof(IsListData))]
        public void SequenceEqual_with_array_literal(bool list)
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.IntArray.SequenceEqual(new[] { 3, 4 }))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();

            Assert.Equal(1, id);
            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntArray"" = ARRAY[3,4]::integer[]
LIMIT 2");
        }

        [Theory]
        [MemberData(nameof(IsListData))]
        public void SequenceEqual_over_nullable_with_parameter(bool list)
        {
            using var ctx = CreateContext();
            var arr = new int?[] { 3, 4, null };
            var id = ctx.SomeEntities
                .Where(e => e.NullableIntArray.SequenceEqual(arr))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();

            Assert.Equal(1, id);
            AssertSql(list,
                @"@__arr_0='System.Nullable`1[System.Int32][]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""NullableIntArray"" = @__arr_0
LIMIT 2");
        }

        #endregion

        #region Containment

        // See also tests in NorthwindMiscellaneousQueryNpgsqlTest

        [Theory]
        [MemberData(nameof(IsListData))]
        public void Array_column_Contains_literal_item(bool list)
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.IntArray.Contains(3))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();

            Assert.Equal(1, id);
            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntArray"" @> ARRAY[3]::integer[]
LIMIT 2");
        }

        [Theory]
        [MemberData(nameof(IsListData))]
        public void Array_column_Contains_parameter_item(bool list)
        {
            using var ctx = CreateContext();
            // ReSharper disable once ConvertToConstant.Local
            var p = 3;
            var id = ctx.SomeEntities
                .Where(e => e.IntArray.Contains(p))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();

            Assert.Equal(1, id);
            AssertSql(list,
                @"@__p_0='3'

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntArray"" @> ARRAY[@__p_0]::integer[]
LIMIT 2");
        }

        [Theory]
        [MemberData(nameof(IsListData))]
        public void Array_column_Contains_column_item(bool list)
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.IntArray.Contains(e.Id + 2))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();

            Assert.Equal(1, id);
            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntArray"" @> ARRAY[s.""Id"" + 2]::integer[]
LIMIT 2");
        }

        [Theory]
        [MemberData(nameof(IsListData))]
        public void Array_column_Contains_null_constant(bool list)
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.StringArray.Contains(null))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();

            Assert.Equal(2, id);
            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE (array_position(s.""StringArray"", NULL) IS NOT NULL)
LIMIT 2");
        }

        [Theory]
        [MemberData(nameof(IsListData))]
        public void Array_column_Contains_null_parameter_does_not_work(bool list)
        {
            using var ctx = CreateContext();
            string p = null;
            var results = ctx.SomeEntities
                .Where(e => e.StringArray.Contains(p))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .ToList();

            // We incorrectly miss arrays containing non-constant nulls, because detecting those
            // would prevent index use.
            Assert.Empty(results);
            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""StringArray"" @> ARRAY[NULL]::text[]");
        }

        [Fact]
        public void Array_constant_Contains()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => new[] { "foo", "xxx" }.Contains(e.NullableText))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""NullableText"" IN ('foo', 'xxx')
LIMIT 2");
        }

        [Fact]
        public void Array_param_Contains_nullable_column()
        {
            using var ctx = CreateContext();
            var array = new[] { "foo", "xxx" };
            var id = ctx.SomeEntities
                .Where(e => array.Contains(e.NullableText))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"@__array_0='System.String[]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""NullableText"" = ANY (@__array_0) OR ((s.""NullableText"" IS NULL) AND (array_position(@__array_0, NULL) IS NOT NULL))
LIMIT 2");
        }

        [Fact]
        public void Array_param_Contains_non_nullable_column()
        {
            using var ctx = CreateContext();
            var array = new[] { 1 };
            var id = ctx.SomeEntities
                .Where(e => array.Contains(e.Id))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"@__array_0='System.Int32[]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""Id"" = ANY (@__array_0)
LIMIT 2");
        }

        [Fact]
        public void Array_param_with_null_Contains_non_nullable_not_found()
        {
            using var ctx = CreateContext();
            var array = new[] { "unknown1", "unknown2", null };
            var count = ctx.SomeEntities.Count(e => array.Contains(e.NonNullableText));

            Assert.Equal(0, count);
            AssertSql(
                @"@__array_0='System.String[]' (DbType = Object)

SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE s.""NonNullableText"" = ANY (@__array_0)");
        }

        [Fact]
        public void Array_param_with_null_Contains_non_nullable_not_found_negated()
        {
            using var ctx = CreateContext();
            var array = new[] { "unknown1", "unknown2", null };
            var count = ctx.SomeEntities.Count(e => !array.Contains(e.NonNullableText));

            Assert.Equal(2, count);

            AssertSql(
                @"@__array_0='System.String[]' (DbType = Object)

SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE NOT (s.""NonNullableText"" = ANY (@__array_0) AND (s.""NonNullableText"" = ANY (@__array_0) IS NOT NULL))");
        }

        [Fact]
        public void Array_param_with_null_Contains_nullable_not_found()
        {
            using var ctx = CreateContext();
            var array = new[] { "unknown1", "unknown2", null };
            var count = ctx.SomeEntities.Count(e => array.Contains(e.NullableText));

            Assert.Equal(0, count);
            AssertSql(
                @"@__array_0='System.String[]' (DbType = Object)

SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE s.""NullableText"" = ANY (@__array_0) OR ((s.""NullableText"" IS NULL) AND (array_position(@__array_0, NULL) IS NOT NULL))");
        }

        [Fact]
        public void Array_param_with_null_Contains_nullable_not_found_negated()
        {
            using var ctx = CreateContext();
            var array = new[] { "unknown1", "unknown2", null };
            var count = ctx.SomeEntities.Count(e => !array.Contains(e.NullableText));

            Assert.Equal(2, count);
            AssertSql(
                @"@__array_0='System.String[]' (DbType = Object)

SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE NOT (s.""NullableText"" = ANY (@__array_0) AND (s.""NullableText"" = ANY (@__array_0) IS NOT NULL)) AND ((s.""NullableText"" IS NOT NULL) OR (array_position(@__array_0, NULL) IS NULL))");
        }

        [Fact]
        public void Byte_array_parameter_contains_column()
        {
            using var ctx = CreateContext();
            var values = new byte[] { 20 };
            var id = ctx.SomeEntities
                .Where(e => values.Contains(e.Byte))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(2, id);
            // Note: EF Core prints the parameter as a bytea, but it's actually a smallint[] (otherwise ANY would fail)
            AssertSql(
                @"@__values_0='0x14' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""Byte"" = ANY (@__values_0)
LIMIT 2");
        }

        #endregion

        #region Length/Count

        [Fact]
        public void ArrayLength()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.IntArray.Length == 2)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE cardinality(s.""IntArray"") = 2
LIMIT 2");
        }

        [Fact]
        public void NullableArrayLength()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.NullableIntArray.Length == 3)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE cardinality(s.""NullableIntArray"") = 3
LIMIT 2");
        }

        [Fact]
        public void ListCount()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.IntList.Count == 2)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE cardinality(s.""IntList"") = 2
LIMIT 2");
        }

        [Fact]
        public void Array_Length_on_EF_Property()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => EF.Property<int[]>(e, nameof(SomeArrayEntity.IntArray)).Length == 2)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE cardinality(s.""IntArray"") = 2
LIMIT 2");
        }

        [Fact]
        public void Length_on_literal_not_translated()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => new[] { 1, 2 }.Length == e.Id)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(2, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE 2 = s.""Id""
LIMIT 2");
        }

        #endregion

        #region Any/All

        [Theory]
        [MemberData(nameof(IsListData))]
        public void Any_no_predicate(bool list)
        {
            using var ctx = CreateContext();
            var count = ctx.SomeEntities
                .Where(e => e.IntArray.Any())
                .OverArrayOrList(list)
                .Count();

            Assert.Equal(2, count);
            AssertSql(list,
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE cardinality(s.""IntArray"") > 0");
        }

        [Fact]
        public void Any_like()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.Like(e.NullableText, p)))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(2, id);

            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""NullableText"" LIKE ANY (ARRAY['a%','b%','c%']::text[])
LIMIT 2");
        }

        [Fact]
        public void Any_ilike()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.ILike(e.NullableText, p)))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(2, id);

            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""NullableText"" ILIKE ANY (ARRAY['a%','b%','c%']::text[])
LIMIT 2");
        }

        [Fact]
        public void Any_like_anonymous()
        {
            using var ctx = CreateContext();
            var patterns = new[] { "a%", "b%", "c%" };

            var _ = ctx.SomeEntities
                .Select(
                    x => new
                    {
                        Array = x.IntArray,
                        Text = x.NullableText
                    })
                .Where(x => patterns.Any(p => EF.Functions.Like(x.Text, p)))
                .ToList();

            AssertSql(
                @"@__patterns_0='System.String[]' (DbType = Object)

SELECT s.""IntArray"" AS ""Array"", s.""NullableText"" AS ""Text""
FROM ""SomeEntities"" AS s
WHERE s.""NullableText"" LIKE ANY (@__patterns_0)");
        }

        [Fact]
        public void All_like()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => new[] { "b%", "%r" }.All(p => EF.Functions.Like(e.NullableText, p)))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(2, id);

            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""NullableText"" LIKE ALL (ARRAY['b%','%r']::text[])
LIMIT 2");
        }

        [Fact]
        public void All_ilike()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => new[] { "B%", "%r" }.All(p => EF.Functions.ILike(e.NullableText, p)))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(2, id);

            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""NullableText"" ILIKE ALL (ARRAY['B%','%r']::text[])
LIMIT 2");
        }

        [Theory]
        [MemberData(nameof(IsListData))]
        public void Any_Contains(bool list)
        {
            using var ctx = CreateContext();

            var id = ctx.SomeEntities
                .Where(e => new[] { 2, 3 }.Any(p => e.IntArray.Contains(p)))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();
            Assert.Equal(1, id);

            var count = ctx.SomeEntities
                .Where(e => new[] { 1, 2 }.Any(p => e.IntArray.Contains(p)))
                .OverArrayOrList(list)
                .Count();
            Assert.Equal(0, count);

            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE ARRAY[2,3]::integer[] && s.""IntArray""
LIMIT 2",
                //
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE ARRAY[1,2]::integer[] && s.""IntArray""");
        }

        [Theory]
        [MemberData(nameof(IsListData))]
        public void All_Contains(bool list)
        {
            using var ctx = CreateContext();

            var id = ctx.SomeEntities
                .Where(e => new[] { 5, 6 }.All(p => e.IntArray.Contains(p)))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();
            Assert.Equal(2, id);

            var count = ctx.SomeEntities
                .Where(e => new[] { 4, 5, 6 }.All(p => e.IntArray.Contains(p)))
                .OverArrayOrList(list)
                .Count();
            Assert.Equal(0, count);

            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE ARRAY[5,6]::integer[] <@ s.""IntArray""
LIMIT 2",
                //
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE ARRAY[4,5,6]::integer[] <@ s.""IntArray""");
        }

        #endregion

        #region bytea

        [Fact]
        public void Index_bytea_with_constant()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.Bytea[0] == 3)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE get_byte(s.""Bytea"", 0) = 3
LIMIT 2");
        }

        public void Index_text_with_constant()
        {
            using var ctx = CreateContext();
            var actual = ctx.SomeEntities.Where(e => e.NullableText[0] == 'f').ToList();

            Assert.Single(actual);
            AssertSql(
                @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (get_byte(s.""SomeBytea"", 0) = 3) AND get_byte(s.""SomeBytea"", 0) IS NOT NULL");
        }

        #endregion

        #region Support

        protected ArrayArrayQueryContext CreateContext() => Fixture.CreateContext();

        void AssertSql(bool list, params string[] expected)
            => AssertSql(list
                ? expected.Select(e => e
                    .Replace(@"""IntArray""", @"""IntList""")
                    .Replace(@"""StringArray""", @"""StringList""")).ToArray()
                : expected);

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public static IEnumerable<object[]> IsListData = new[] { new object[] { false }, new object[] { true } };

        public class ArrayArrayQueryContext : PoolableDbContext
        {
            public DbSet<SomeArrayEntity> SomeEntities { get; set; }

            public ArrayArrayQueryContext(DbContextOptions options) : base(options) {}

            public static void Seed(ArrayArrayQueryContext context)
            {
                context.SomeEntities.AddRange(
                    new SomeArrayEntity
                    {
                        Id = 1,
                        IntArray = new[] { 3, 4 },
                        IntList = new List<int> { 3, 4 },
                        NullableIntArray = new int?[] { 3, 4, null },
                        NullableIntList = new List<int?> { 3, 4, null },
                        Bytea = new byte[] { 3, 4 },
                        ByteArray = new byte[] { 3, 4 },
                        StringArray = new[] { "3", "4" },
                        StringList = new List<string> { "3", "4" },
                        IntMatrix = new[,] { { 5, 6 }, { 7, 8 } },
                        NullableText = "foo",
                        NonNullableText = "foo",
                        Byte = 10
                    },
                    new SomeArrayEntity
                    {
                        Id = 2,
                        IntArray = new[] { 5, 6, 7 },
                        IntList = new List<int> { 5, 6, 7 },
                        NullableIntArray = new int?[] { 5, 6, 7, null },
                        NullableIntList = new List<int?> { 5, 6, 7, null },
                        Bytea = new byte[] { 5, 6, 7 },
                        ByteArray = new byte[] { 5, 6, 7 },
                        StringArray = new[] { "5", "6", "7", null },
                        StringList = new List<string> { "5", "6", "7", null },
                        IntMatrix = new[,] { { 10, 11 }, { 12, 13 } },
                        NullableText = "bar",
                        NonNullableText = "bar",
                        Byte = 20
                    });
                context.SaveChanges();
            }
        }

        public class SomeArrayEntity
        {
            public int Id { get; set; }
            public int[] IntArray { get; set; }
            public List<int> IntList { get; set; }
            public int?[] NullableIntArray { get; set; }
            public List<int?> NullableIntList { get; set; }
            public int[,] IntMatrix { get; set; }
            public byte[] Bytea { get; set; }
            public byte[] ByteArray { get; set; }
            public string[] StringArray { get; set; }
            public List<string> StringList { get; set; }
            public string NullableText { get; set; }
            [Required]
            public string NonNullableText { get; set; }
            public byte Byte { get; set; }
        }

        public class ArrayArrayQueryFixture : SharedStoreFixtureBase<ArrayArrayQueryContext>
        {
            protected override string StoreName => "ArrayArrayQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override void Seed(ArrayArrayQueryContext context) => ArrayArrayQueryContext.Seed(context);
        }

        #endregion
    }
}

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ArrayTests
{
    using SomeArrayEntity = ArrayQueryTest.SomeArrayEntity;

    static class QueryableExtensions
    {
        internal static IQueryable<T> OverArrayOrList<T>(this IQueryable<T> source, bool list)
        {
            Check.NotNull(source, nameof(source));

            return source.Provider is EntityQueryProvider && list
                ? source.Provider.CreateQuery<T>(
                    new ArrayToListReplacingExpressionVisitor().Visit(source.Expression))
                : source;
        }
    }

    class ArrayToListReplacingExpressionVisitor : ExpressionVisitor
    {
        static readonly PropertyInfo IntArray = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.IntArray));
        static readonly PropertyInfo IntList = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.IntList));
        static readonly PropertyInfo StringArray = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.StringArray));
        static readonly PropertyInfo StringList = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.StringList));

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member == IntArray)
                return Expression.MakeMemberAccess(node.Expression, IntList);
            if (node.Member == StringArray)
                return Expression.MakeMemberAccess(node.Expression, StringList);
            return node;
        }
    }
}
