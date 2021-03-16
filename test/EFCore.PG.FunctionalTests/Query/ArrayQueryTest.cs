using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ArrayTests;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ArrayQueryTest : IClassFixture<ArrayQueryTest.ArrayArrayQueryFixture>
    {
        private ArrayArrayQueryFixture Fixture { get; }

        // ReSharper disable once UnusedParameter.Local
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
        public void Array_column_Any_equality_operator(bool list)
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.StringArray.Any(p => p == "3"))
                .OverArrayOrList(list)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);

            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""StringArray"" @> ARRAY['3']::text[]
LIMIT 2");
        }

        [Theory]
        [MemberData(nameof(IsListData))]
        public void Array_column_Any_Equals(bool list)
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.StringArray.Any(p => "3".Equals(p)))
                .OverArrayOrList(list)
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);

            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""StringArray"" @> ARRAY['3']::text[]
LIMIT 2");
        }

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
                .Where(e => e.NullableStringArray.Contains(null))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();

            Assert.Equal(1, id);
            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE (array_position(s.""NullableStringArray"", NULL) IS NOT NULL)
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

        [Theory]
        [MemberData(nameof(IsListData))]
        public void Nullable_array_column_Contains_literal_item(bool list)
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.NullableIntArray.Contains(3))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();

            Assert.Equal(1, id);
            AssertSql(list,
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""NullableIntArray"" @> ARRAY[3]::integer[]
LIMIT 2");
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
        public void List_param_Contains_non_nullable_column()
        {
            using var ctx = CreateContext();
            var list = new List<int> { 1 };
            var id = ctx.SomeEntities
                .Where(e => list.Contains(e.Id))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"@__list_0='System.Collections.Generic.List`1[System.Int32]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""Id"" = ANY (@__list_0)
LIMIT 2");
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

        [Fact]
        public void Array_param_Contains_value_converted_column()
        {
            using var ctx = CreateContext();
            var array = new IntWrapper[] { new(2), new(3) };
            var id = ctx.SomeEntities
                .Where(e => array.Contains(e.ValueConvertedScalar))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(2, id);
            AssertSql(
                @"@__array_0='System.Int32[]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""ValueConvertedScalar"" = ANY (@__array_0)
LIMIT 2");
        }

        [Fact]
        public void List_param_Contains_value_converted_column()
        {
            using var ctx = CreateContext();
            var list = new List<IntWrapper> { new(2), new(3) };
            var id = ctx.SomeEntities
                .Where(e => list.Contains(e.ValueConvertedScalar))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(2, id);
            AssertSql(
                @"@__list_0='System.Int32[]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""ValueConvertedScalar"" = ANY (@__list_0)
LIMIT 2");
        }

        [Fact]
        public void Array_column_Contains_value_converted_param()
        {
            using var ctx = CreateContext();
            var item = new IntWrapper(8);
            var id = ctx.SomeEntities
                .Where(e => e.ValueConvertedArray.Contains(item))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"@__item_0='8' (Nullable = true)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""ValueConvertedArray"" @> ARRAY[@__item_0]::integer[]
LIMIT 2");
        }

        [Fact]
        public void List_column_Contains_value_converted_param()
        {
            using var ctx = CreateContext();
            var item = new IntWrapper(8);
            var id = ctx.SomeEntities
                .Where(e => e.ValueConvertedList.Contains(item))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"@__item_0='8' (Nullable = true)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""ValueConvertedList"" @> ARRAY[@__item_0]::integer[]
LIMIT 2");
        }

        [Fact]
        public void Array_param_Contains_value_converted_array_column()
        {
            using var ctx = CreateContext();
            var p = new IntWrapper[] { new(8), new(9) };
            var id = ctx.SomeEntities
                .Where(e => e.ValueConvertedArray.All(x => p.Contains(x)))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"@__p_0='System.Int32[]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""ValueConvertedArray"" <@ @__p_0
LIMIT 2");
        }

        [Fact]
        public void List_param_Contains_value_converted_list_column()
        {
            using var ctx = CreateContext();
            var p = new List<IntWrapper> { new(8), new(9) };
            var id = ctx.SomeEntities
                .Where(e => e.ValueConvertedList.All(x => p.Contains(x)))
                .Select(e => e.Id)
                .Single();

            Assert.Equal(1, id);
            AssertSql(
                @"@__p_0='System.Collections.Generic.List`1[System.Int32]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""ValueConvertedList"" <@ @__p_0
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
        public void Any_Contains_on_constant_array(bool list)
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
        public void Any_Contains_between_column_and_List(bool list)
        {
            using var ctx = CreateContext();

            var ints = new List<int> { 2, 3 };
            var id = ctx.SomeEntities
                .Where(e => e.IntArray.Any(i => ints.Contains(i)))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();
            Assert.Equal(1, id);

            ints = new List<int> { 1, 2 };
            var count = ctx.SomeEntities
                .Where(e => e.IntArray.Any(i => ints.Contains(i)))
                .OverArrayOrList(list)
                .Count();
            Assert.Equal(0, count);

            AssertSql(list,
                @"@__ints_0='System.Collections.Generic.List`1[System.Int32]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntArray"" && @__ints_0
LIMIT 2",
                //
                @"@__ints_0='System.Collections.Generic.List`1[System.Int32]' (DbType = Object)

SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE s.""IntArray"" && @__ints_0");
        }

        [Theory]
        [MemberData(nameof(IsListData))]
        public void Any_Contains_between_column_and_array(bool list)
        {
            using var ctx = CreateContext();

            var ints = new[] { 2, 3 };
            var id = ctx.SomeEntities
                .Where(e => e.IntArray.Any(i => ints.Contains(i)))
                .Select(e => e.Id)
                .OverArrayOrList(list)
                .Single();
            Assert.Equal(1, id);

            ints = new[] { 1, 2 };
            var count = ctx.SomeEntities
                .Where(e => e.IntArray.Any(i => ints.Contains(i)))
                .OverArrayOrList(list)
                .Count();
            Assert.Equal(0, count);

            AssertSql(list,
                @"@__ints_0='System.Int32[]' (DbType = Object)

SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""IntArray"" && @__ints_0
LIMIT 2",
                //
                @"@__ints_0='System.Int32[]' (DbType = Object)

SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE s.""IntArray"" && @__ints_0");
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

        [Theory]
        [MemberData(nameof(IsListData))]
        public Task Any_like_column(bool list)
        {
            using var ctx = CreateContext();

            return AssertTranslationFailed(() => ctx.SomeEntities
                .Where(e => e.StringArray.Any(p => EF.Functions.Like(p, "3")))
                .OverArrayOrList(list)
                .ToListAsync());
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

        #region Nullability

        [ConditionalFact]
        public void Nullable_value_array_index_compare_to_null()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.NullableIntArray[3] == null)
                .Select(e => e.Id)
                .Count();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE (s.""NullableIntArray""[4] IS NULL)");
        }

        [ConditionalFact]
        public void Non_nullable_value_array_index_compare_to_null()
        {
            using var ctx = CreateContext();
            var count = ctx.SomeEntities
#pragma warning disable CS0472
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                .Where(e => e.IntArray[1] == null)
#pragma warning restore CS0472
                .Select(e => e.Id)
                .Count();

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE FALSE");
        }

        [ConditionalFact]
        public void Nullable_reference_array_index_compare_to_null()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.NullableStringArray[3] == null)
                .Select(e => e.Id)
                .Count();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE (s.""NullableStringArray""[4] IS NULL)");
        }

        [ConditionalFact]
        public void Non_nullable_reference_array_index_compare_to_null()
        {
            using var ctx = CreateContext();
            var count = ctx.SomeEntities
#pragma warning disable CS0472
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                .Where(e => e.StringArray[1] == null)
#pragma warning restore CS0472
                .Select(e => e.Id)
                .Count();

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE FALSE");
        }

        [ConditionalFact]
        public void Nullable_value_list_index_compare_to_null()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.NullableIntList[3] == null)
                .Select(e => e.Id)
                .Count();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE (s.""NullableIntList""[4] IS NULL)");
        }

        [ConditionalFact]
        public void Non_nullable_value_list_index_compare_to_null()
        {
            using var ctx = CreateContext();
            var count = ctx.SomeEntities
#pragma warning disable CS0472
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                .Where(e => e.IntList[1] == null)
#pragma warning restore CS0472
                .Select(e => e.Id)
                .Count();

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE FALSE");
        }

        [ConditionalFact]
        public void Nullable_reference_list_index_compare_to_null()
        {
            using var ctx = CreateContext();
            var id = ctx.SomeEntities
                .Where(e => e.NullableStringList[3] == null)
                .Select(e => e.Id)
                .Count();

            Assert.Equal(1, id);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE (s.""NullableStringList""[4] IS NULL)");
        }

        [ConditionalFact]
        public void Non_nullable_reference_list_index_compare_to_null()
        {
            using var ctx = CreateContext();
            var count = ctx.SomeEntities
#pragma warning disable CS0472
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                .Where(e => e.StringList[1] == null)
#pragma warning restore CS0472
                .Select(e => e.Id)
                .Count();

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE FALSE");
        }

        #endregion Nullability

        #region Support

        protected ArrayArrayQueryContext CreateContext() => Fixture.CreateContext();

        private void AssertSql(bool list, params string[] expected)
            => AssertSql(list
                ? expected.Select(e => e
                        .Replace(@"""IntArray""", @"""IntList""")
                        .Replace(@"""NullableIntArray""", @"""NullableIntList""")
                        .Replace(@"""StringArray""", @"""StringList""")
                        .Replace(@"""NullableStringArray""", @"""NullableStringList"""))
                    .ToArray()
                : expected);

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public static IEnumerable<object[]> IsListData = new[] { new object[] { false }, new object[] { true } };

        public class ArrayArrayQueryContext : PoolableDbContext
        {
            public DbSet<SomeArrayEntity> SomeEntities { get; set; }

            public ArrayArrayQueryContext(DbContextOptions options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<SomeArrayEntity>(
                    e =>
                    {
                        e.Property(ae => ae.ValueConvertedScalar)
                            .HasConversion(w => w.Value, v => new IntWrapper(v));

                        e.Property(ae => ae.ValueConvertedArray)
                            .HasPostgresArrayConversion(w => w.Value, v => new IntWrapper(v));

                        e.Property(ae => ae.ValueConvertedList)
                            .HasPostgresArrayConversion(w => w.Value, v => new IntWrapper(v));
                    });

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
                        NullableStringArray = new[] { "3", "4", null },
                        StringList = new List<string> { "3", "4" },
                        NullableStringList = new List<string> { "3", "4", null},
                        IntMatrix = new[,] { { 5, 6 }, { 7, 8 } },
                        NullableText = "foo",
                        NonNullableText = "foo",
                        ValueConvertedScalar = new IntWrapper(1),
                        ValueConvertedArray = new IntWrapper[] { new(8), new(9)},
                        ValueConvertedList = new List<IntWrapper> { new(8), new(9)},
                        Byte = 10
                    },
                    new SomeArrayEntity
                    {
                        Id = 2,
                        IntArray = new[] { 5, 6, 7, 8 },
                        IntList = new List<int> { 5, 6, 7, 8 },
                        NullableIntArray = new int?[] { 5, 6, 7, 8 },
                        NullableIntList = new List<int?> { 5, 6, 7, 8 },
                        Bytea = new byte[] { 5, 6, 7, 8 },
                        ByteArray = new byte[] { 5, 6, 7, 8 },
                        StringArray = new[] { "5", "6", "7", "8" },
                        NullableStringArray = new[] { "5", "6", "7", "8" },
                        StringList = new List<string> { "5", "6", "7", "8" },
                        NullableStringList = new List<string> { "5", "6", "7", "8" },
                        IntMatrix = new[,] { { 10, 11 }, { 12, 13 } },
                        NullableText = "bar",
                        NonNullableText = "bar",
                        ValueConvertedScalar = new IntWrapper(2),
                        ValueConvertedArray = new IntWrapper[] { new(9), new(10)},
                        ValueConvertedList = new List<IntWrapper> { new(9), new(10)},
                        Byte = 20
                    });
                context.SaveChanges();
            }
        }

        #nullable enable

        public class SomeArrayEntity
        {
            public int Id { get; set; }
            public int[] IntArray { get; set; } = null!;
            public List<int> IntList { get; set; } = null!;
            public int?[] NullableIntArray { get; set; } = null!;
            public List<int?> NullableIntList { get; set; } = null!;
            public int[,] IntMatrix { get; set; } = null!;
            public byte[] Bytea { get; set; } = null!;
            public byte[] ByteArray { get; set; } = null!;
            public string[] StringArray { get; set; } = null!;
            public List<string> StringList { get; set; } = null!;
            public string?[] NullableStringArray { get; set; } = null!;
            public List<string?> NullableStringList { get; set; } = null!;
            public string? NullableText { get; set; }
            public string NonNullableText { get; set; } = null!;
            public IntWrapper ValueConvertedScalar { get; set; } = null!;
            public IntWrapper[] ValueConvertedArray { get; set; } = null!;
            public List<IntWrapper> ValueConvertedList { get; set; } = null!;
            public byte Byte { get; set; }
        }

        public class IntWrapper
        {
            public IntWrapper(int value) => Value = value;
            public int Value { get; set; }
        }

        #nullable restore

        public class ArrayArrayQueryFixture : SharedStoreFixtureBase<ArrayArrayQueryContext>
        {
            protected override string StoreName => "ArrayArrayQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Ignore(CoreEventId.CollectionWithoutComparer));

            protected override void Seed(ArrayArrayQueryContext context) => ArrayArrayQueryContext.Seed(context);
        }

        protected static async Task AssertTranslationFailed(Func<Task> query)
            => Assert.Contains(
                CoreStrings.TranslationFailed("").Substring(48),
                (await Assert.ThrowsAsync<InvalidOperationException>(query))
                .Message);

        #endregion
    }
}

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ArrayTests
{
    using SomeArrayEntity = ArrayQueryTest.SomeArrayEntity;

    internal static class QueryableExtensions
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

    internal class ArrayToListReplacingExpressionVisitor : ExpressionVisitor
    {
        private static readonly PropertyInfo IntArray = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.IntArray));
        private static readonly PropertyInfo NullableIntArray = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.NullableIntArray));
        private static readonly PropertyInfo IntList = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.IntList));
        private static readonly PropertyInfo NullableIntList = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.NullableIntList));
        private static readonly PropertyInfo StringArray = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.StringArray));
        private static readonly PropertyInfo NullableStringArray = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.NullableStringArray));
        private static readonly PropertyInfo StringList = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.StringList));
        private static readonly PropertyInfo NullableStringList = typeof(SomeArrayEntity).GetProperty(nameof(SomeArrayEntity.NullableStringList));

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member == IntArray)
                return Expression.MakeMemberAccess(node.Expression, IntList);
            if (node.Member == NullableIntArray)
                return Expression.MakeMemberAccess(node.Expression, NullableIntList);
            if (node.Member == StringArray)
                return Expression.MakeMemberAccess(node.Expression, StringList);
            if (node.Member == NullableStringArray)
                return Expression.MakeMemberAccess(node.Expression, NullableStringList);
            return node;
        }
    }
}
