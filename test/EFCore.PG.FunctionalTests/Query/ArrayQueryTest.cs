using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ArrayQueryTest : IClassFixture<ArrayQueryTest.ArrayFixture>
    {
        #region ArrayTests

        #region Roundtrip

        [Fact]
        public void Array_Roundtrip()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.Id == 1);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                Assert.Equal(new List<int> { 3, 4 }, x.SomeList);
            }
        }

        [Fact]
        public void List_Roundtrip()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.Id == 1);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                Assert.Equal(new List<int> { 3, 4 }, x.SomeList);
            }
        }

        #endregion

        #region Indexers

        [Fact]
        public void Array_Index_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeArray[0] == 3).ToList();
                AssertContainsInSql("WHERE (e.\"SomeArray\"[1]) = 3");
            }
        }

        [Fact]
        public void List_Index_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeList[0] == 3).ToList();
                AssertContainsInSql("WHERE e.\"SomeList\"[1] = 3");
            }
        }

        [Fact]
        public void Array_Index_bytea_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeBytea[0] == 3).ToList();
                AssertContainsInSql("WHERE (get_byte(e.\"SomeBytea\", 0)) = 3");
            }
        }

        [Fact]
        public void Array_ElementAt_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.ElementAt(0) == 3).ToList();
                AssertContainsInSql("WHERE (e.\"SomeArray\"[1]) = 3");
            }
        }

        [Fact]
        public void List_ElementAt_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeList.ElementAt(0) == 3).ToList();
                AssertContainsInSql("WHERE e.\"SomeList\"[1] = 3");
            }
        }

        [Fact]
        public void Array_ElementAt_bytea_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeBytea.ElementAt(0) == 3).ToList();
                AssertContainsInSql("WHERE (get_byte(e.\"SomeBytea\", 0)) = 3");
            }
        }

        [Fact]
        public void Array_Index_with_non_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var _ = ctx.SomeEntities.Where(e => e.SomeArray[x] == 3).ToList();
                AssertContainsInSql("WHERE (e.\"SomeArray\"[@__x_0 + 1]) = 3");
            }
        }

        [Fact]
        public void List_Index_with_non_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var _ = ctx.SomeEntities.Where(e => e.SomeList[x] == 3).ToList();
                AssertContainsInSql("WHERE e.\"SomeList\"[@__x_0 + 1] = 3");
            }
        }

        [Fact]
        public void Array_Index_bytea_with_non_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var _ = ctx.SomeEntities.Where(e => e.SomeBytea[x] == 3).ToList();
                AssertContainsInSql("WHERE (get_byte(e.\"SomeBytea\", @__x_0)) = 3");
            }
        }

        [Fact]
        public void Array_ElementAt_with_non_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.ElementAt(x) == 3).ToList();
                AssertContainsInSql("WHERE (e.\"SomeArray\"[@__x_0 + 1]) = 3");
            }
        }

        [Fact]
        public void List_ElementAt_with_non_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var _ = ctx.SomeEntities.Where(e => e.SomeList.ElementAt(x) == 3).ToList();
                AssertContainsInSql("WHERE e.\"SomeList\"[@__x_0 + 1] = 3");
            }
        }

        [Fact]
        public void Array_ElementAt_bytea_with_non_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var _ = ctx.SomeEntities.Where(e => e.SomeBytea.ElementAt(x) == 3).ToList();
                AssertContainsInSql("WHERE (get_byte(e.\"SomeBytea\", @__x_0)) = 3");
            }
        }

        [Fact]
        public void Array_Index_multidimensional()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // Operations on multidimensional arrays aren't mapped to SQL yet
                var actual = ctx.SomeEntities.Where(e => e.SomeMatrix[0, 0] == 5).ToList();
                Assert.Equal(1, actual.Count);
            }
        }

        #endregion

        #region Equality

        [Fact]
        public void Array_Equal_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var array = new[] { 3, 4 };
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.Equals(array)).ToList();
                AssertContainsInSql("WHERE e.\"SomeArray\" = @__array_0");
            }
        }

        [Fact]
        public void List_Equal_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var list = new List<int> { 3, 4 };
                var _ = ctx.SomeEntities.Where(e => e.SomeList.Equals(list)).ToList();
                AssertContainsInSql("WHERE e.\"SomeList\" = @__list_0");
            }
        }

        [Fact]
        public void Array_SequenceEqual_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var array = new[] { 3, 4 };
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.Equals(array)).ToList();
                AssertContainsInSql("WHERE e.\"SomeArray\" = @__array_0");
            }
        }

        [Fact]
        public void List_SequenceEqual_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var list = new List<int> { 3, 4 };
                var _ = ctx.SomeEntities.Where(e => e.SomeList.SequenceEqual(list)).ToList();
                AssertContainsInSql("WHERE e.\"SomeList\" = @__list_0");
            }
        }

        [Fact]
        public void Array_SequenceEqual_with_literal()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.SequenceEqual(new[] { 3, 4 })).ToList();
                AssertContainsInSql("WHERE e.\"SomeArray\" = ARRAY[3,4]::integer");
            }
        }

        [Fact]
        public void List_SequenceEqual_with_literal()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeList.SequenceEqual(new List<int> { 3, 4 })).ToList();
                AssertContainsInSql("WHERE e.\"SomeList\" = ARRAY[3,4]");
            }
        }

        #endregion

        #region value = ANY (array)

        [Fact]
        public void Array_Contains_with_literal()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.Contains(3)).ToList();
                AssertContainsInSql("WHERE 3 = ANY (e.\"SomeArray\")");
            }
        }

        [Fact]
        public void List_Contains_with_literal()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeList.Contains(3)).ToList();
                AssertContainsInSql("WHERE 3 = ANY (e.\"SomeList\")");
            }
        }

        [Fact]
        public void Array_Contains_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var p = 3;
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.Contains(p)).ToList();
                AssertContainsInSql("WHERE @__p_0 = ANY (e.\"SomeArray\")");
            }
        }

        [Fact]
        public void List_Contains_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var p = 3;
                var _ = ctx.SomeEntities.Where(e => e.SomeList.Contains(p)).ToList();
                AssertContainsInSql("WHERE @__p_0 = ANY (e.\"SomeList\")");
            }
        }

        [Fact]
        public void Array_Contains_with_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.Contains(e.Id + 2)).ToList();
                AssertContainsInSql("WHERE e.\"Id\" + 2 = ANY (e.\"SomeArray\")");
            }
        }

        [Fact]
        public void List_Contains_with_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeList.Contains(e.Id + 2)).ToList();
                AssertContainsInSql("WHERE e.\"Id\" + 2 = ANY (e.\"SomeList\")");
            }
        }

        #endregion

        #region @>

        [Fact]
        public void Array_Contains_Array()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.Contains(x.SomeArray, x.SomeArray)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeArray\" @> x.\"SomeArray\") = TRUE");
            }
        }

        [Fact]
        public void Array_Contains_List()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.Contains(x.SomeArray, x.SomeList)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeArray\" @> x.\"SomeList\") = TRUE");
            }
        }

        [Fact]
        public void List_Contains_Array()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.Contains(x.SomeList, x.SomeArray)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeList\" @> x.\"SomeArray\") = TRUE");
            }
        }

        [Fact]
        public void List_Contains_List()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.Contains(x.SomeList, x.SomeList)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeList\" @> x.\"SomeList\") = TRUE");
            }
        }

        #endregion

        #region <@

        [Fact]
        public void Array_ContainedBy_Array()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.ContainedBy(x.SomeArray, x.SomeArray)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeArray\" <@ x.\"SomeArray\") = TRUE");
            }
        }

        [Fact]
        public void Array_ContainedBy_List()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.ContainedBy(x.SomeArray, x.SomeList)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeArray\" <@ x.\"SomeList\") = TRUE");
            }
        }

        [Fact]
        public void List_ContainedBy_Array()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.ContainedBy(x.SomeList, x.SomeArray)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeList\" <@ x.\"SomeArray\") = TRUE");
            }
        }

        [Fact]
        public void List_ContainedBy_List()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.ContainedBy(x.SomeList, x.SomeList)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeList\" <@ x.\"SomeList\") = TRUE");
            }
        }

        #endregion

        #region &&

        [Fact]
        public void Array_Overlaps_Array()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.Overlaps(x.SomeArray, x.SomeArray)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeArray\" && x.\"SomeArray\") = TRUE");
            }
        }

        [Fact]
        public void Array_Overlaps_List()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.Overlaps(x.SomeArray, x.SomeList)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeArray\" && x.\"SomeList\") = TRUE");
            }
        }

        [Fact]
        public void List_Overlaps_Array()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.Overlaps(x.SomeList, x.SomeArray)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeList\" && x.\"SomeArray\") = TRUE");
            }
        }

        [Fact]
        public void List_Overlaps_List()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => EF.Functions.Overlaps(x.SomeList, x.SomeList)).ToList();
                AssertContainsInSql("WHERE (x.\"SomeList\" && x.\"SomeList\") = TRUE");
            }
        }

        #endregion

        #region ||

        [Fact]
        public void Array_Concat_with_array_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeArray.Concat(e.SomeArray)).ToList();
                AssertContainsInSql("SELECT (e.\"SomeArray\" || e.\"SomeArray\")");
            }
        }

        [Fact]
        public void List_Concat_with_list_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeList.Concat(e.SomeList)).ToList();
                AssertContainsInSql("SELECT (e.\"SomeList\" || e.\"SomeList\")");
            }
        }

        [Fact]
        public void Array_Concat_with_list_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeArray.Concat(e.SomeList)).ToList();
                AssertContainsInSql("SELECT (e.\"SomeArray\" || e.\"SomeList\")");
            }
        }

        [Fact]
        public void List_Concat_with_array_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeList.Concat(e.SomeArray)).ToList();
                AssertContainsInSql("SELECT (e.\"SomeList\" || e.\"SomeArray\")");
            }
        }

// .NET 4.6.1 doesn't include the Enumerable.Append and Enumerable.Prepend functions...
#if !NET461
        [Fact]
        
        public void Array_Append_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeArray.Append(0)).ToList();
                AssertContainsInSql("SELECT (e.\"SomeArray\" || 0)");
            }
        }

        [Fact]
        
        public void List_Append_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeList.Append(0)).ToList();
                AssertContainsInSql("SELECT (e.\"SomeList\" || 0)");
            }
        }

        [Fact]
        
        public void Array_Prepend_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeArray.Prepend(0)).ToList();
                AssertContainsInSql("SELECT (0 || e.\"SomeArray\")");
            }
        }

        [Fact]
        
        public void List_Prepend_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeList.Prepend(0)).ToList();
                AssertContainsInSql("SELECT (0 || e.\"SomeList\")");
            }
        }

#endif

        #endregion

        #region array_fill

        [Fact]
        public void Array_ArrayFill_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayFill(e.Id, new[] { 2 })).ToList();
                AssertContainsInSql("SELECT array_fill(e.\"Id\", ARRAY[2]::integer[])");
            }
        }

        [Fact]
        public void List_ListFill_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ListFill(e.Id, new[] { 3 })).ToList();
                AssertContainsInSql("SELECT array_fill(e.\"Id\", ARRAY[3]::integer[])");
            }
        }

        #endregion

        #region array_dims

        [Fact]
        public void Array_ArrayDimensions_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayDimensions(e.SomeArray)).ToList();
                AssertContainsInSql("SELECT array_dims(e.\"SomeArray\")");
            }
        }

        [Fact]
        public void List_ArrayDimensions_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayDimensions(e.SomeList)).ToList();
                AssertContainsInSql("SELECT array_dims(e.\"SomeList\")");
            }
        }

        #endregion

        #region array_length

        [Fact]
        public void Array_Length()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.Length == 2).ToList();
                AssertContainsInSql("WHERE COALESCE(array_length(e.\"SomeArray\", 1), 0) = 2");
            }
        }

        [Fact]
        public void List_Length()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeList.Count == 0).ToList();
                AssertContainsInSql("WHERE COALESCE(array_length(e.\"SomeList\", 1), 0) = 0");
            }
        }

        [Fact]
        public void Array_GetLength()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Single(e => e.SomeArray.GetLength(0) == 2);
                AssertContainsInSql("WHERE COALESCE(array_length(e.\"SomeArray\", 1), 0) = 2");
            }
        }

        [Fact]
        public void Array_Count()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once UseCollectionCountProperty
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.Count() == 1).ToArray();
                AssertContainsInSql("WHERE COALESCE(array_length(e.\"SomeArray\", 1), 0) = 1");
            }
        }

        [Fact]
        public void List_Count()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once UseCollectionCountProperty
                var _ = ctx.SomeEntities.Where(e => e.SomeList.Count() == 1).ToArray();
                AssertContainsInSql("WHERE COALESCE(array_length(e.\"SomeList\", 1), 0) = 1");
            }
        }

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/9242")]
        public void Array_Length_on_EF_Property()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // TODO: This fails
                var x = ctx.SomeEntities.Single(e => EF.Property<int[]>(e, nameof(SomeArrayEntity.SomeArray)).Length == 2);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql("WHERE array_length(e.\"SomeArray\", 1) = 2");
            }
        }

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/9242")]
        public void List_Length_on_EF_Property()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // TODO: This fails
                var x = ctx.SomeEntities.Single(e => EF.Property<List<int>>(e, nameof(SomeArrayEntity.SomeList)).Count == 2);
                Assert.Equal(new List<int> { 3, 4 }, x.SomeList);
                AssertContainsInSql("WHERE array_length(e.\"SomeList\", 1) = 2");
            }
        }

        [Fact]
        public void Array_Length_on_literal_not_translated()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => new[] { 1, 2, 3 }.Length == e.Id).ToList();
                AssertContainsInSql("WHERE 3 = e.\"Id\"");
                AssertDoesNotContainInSql("array_length");
                AssertDoesNotContainInSql("cardinality");
            }
        }

        [Fact]
        public void List_Length_on_literal_not_translated()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => new List<int> { 1, 2, 3 }.Count == e.Id).ToList();
                AssertContainsInSql("WHERE @__Count_0 = e.\"Id\"");
                AssertDoesNotContainInSql("array_length");
                AssertDoesNotContainInSql("cardinality");
            }
        }

        [Fact]
        public void Array_GetLength_on_literal_not_translated()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => new[] { 1, 2, 3 }.GetLength(0) == e.Id).ToList();
                AssertContainsInSql("WHERE @__GetLength_0 = e.\"Id\"");
                AssertDoesNotContainInSql("array_length");
                AssertDoesNotContainInSql("cardinality");
            }
        }

        #endregion

        #region array_lower

        [Fact]
        public void Array_ArrayLower()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.GetLowerBound(0) == 2).ToList();
                AssertContainsInSql("WHERE (COALESCE(array_lower(e.\"SomeArray\", 1), 0) - 1) = 2");
            }
        }

        #endregion

        #region array_ndims

        [Fact]
        public void Array_Rank()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.Rank == 2).ToList();
                AssertContainsInSql("WHERE COALESCE(array_ndims(e.\"SomeArray\"), 1) = 2");
            }
        }

        #endregion

        #region array_position

        [Theory]
        [InlineData(9, 5)]
        public void Array_IndexOf_constant(int major, int minor)
        {
            using (var ctx = Fixture.CreateContext(new Version(major, minor)))
            {
                var _ = ctx.SomeEntities.Select(e => Array.IndexOf(e.SomeArray, 0)).ToList();
                AssertContainsInSql("SELECT COALESCE(array_position(e.\"SomeArray\", 0), 0) - 1");
            }
        }

        [Theory]
        [InlineData(9, 5)]
        public void List_IndexOf_constant(int major, int minor)
        {
            using (var ctx = Fixture.CreateContext(new Version(major, minor)))
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeList.IndexOf(0)).ToList();
                AssertContainsInSql("SELECT COALESCE(array_position(e.\"SomeList\", 0), 0) - 1");
            }
        }

        #endregion

        #region array_positions

        [Theory]
        [InlineData(9, 5)]
        public void Array_ArrayPositions_column(int major, int minor)
        {
            using (var ctx = Fixture.CreateContext(new Version(major, minor)))
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayPositions(e.SomeArray, 0)).ToList();
                AssertContainsInSql("SELECT array_positions(e.\"SomeArray\", 0)");
            }
        }

        [Theory]
        [InlineData(9, 5)]
        public void List_ListPositions_column(int major, int minor)
        {
            using (var ctx = Fixture.CreateContext(new Version(major, minor)))
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayPositions(e.SomeList, 0)).ToList();
                AssertContainsInSql("SELECT array_positions(e.\"SomeList\", 0)");
            }
        }

        #endregion

        #region array_remove

        [Fact]
        public void Array_ArrayRemove_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayRemove(e.SomeArray, 0)).ToList();
                AssertContainsInSql("SELECT array_remove(e.\"SomeArray\", 0)");
            }
        }

        [Fact]
        public void List_ArrayRemove_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayRemove(e.SomeList, 0)).ToList();
                AssertContainsInSql("SELECT array_remove(e.\"SomeList\", 0)");
            }
        }

        #endregion

        #region array_replace

        [Fact]
        public void Array_ArrayReplace_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayReplace(e.SomeArray, 0, 1)).ToList();
                AssertContainsInSql("SELECT array_replace(e.\"SomeArray\", 0, 1)");
            }
        }

        [Fact]
        public void List_ArrayReplace_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayReplace(e.SomeList, 0, 1)).ToList();
                AssertContainsInSql("SELECT array_replace(e.\"SomeList\", 0, 1)");
            }
        }

        #endregion

        #region array_upper

        [Fact]
        public void Array_ArrayUpper()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.GetUpperBound(0) == 2).ToList();
                AssertContainsInSql("WHERE (COALESCE(array_upper(e.\"SomeArray\", 1), 0) - 1) = 2");
            }
        }

        #endregion

        #region array_to_string

        [Fact]
        public void Array_ArrayToString()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayToString(e.SomeArray, "*")).ToList();
                AssertContainsInSql("SELECT array_to_string(e.\"SomeArray\", '*')");
            }
        }

        [Fact]
        public void List_ArrayToString()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayToString(e.SomeList, "*")).ToList();
                AssertContainsInSql("SELECT array_to_string(e.\"SomeList\", '*')");
            }
        }

        [Fact]
        public void Array_ArrayToString_with_null()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayToString(e.SomeArray, "*", ";")).ToList();
                AssertContainsInSql("SELECT array_to_string(e.\"SomeArray\", '*', ';')");
            }
        }

        [Fact]
        public void List_ArrayToString_with_null()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayToString(e.SomeList, "*", ";")).ToList();
                AssertContainsInSql("SELECT array_to_string(e.\"SomeList\", '*', ';')");
            }
        }

        #endregion

        #region string_to_array

        [Fact]
        public void Array_StringToArray()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => EF.Functions.StringToArray<string>(e.SomeText, "*") != null).ToList();
                AssertContainsInSql("WHERE string_to_array(e.\"SomeText\", '*') IS NOT NULL");
            }
        }

        [Fact]
        public void List_StringToList()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => EF.Functions.StringToList<string>(e.SomeText, "*") != null).ToList();
                AssertContainsInSql("WHERE string_to_array(e.\"SomeText\", '*') IS NOT NULL");
            }
        }

        [Fact]
        public void Array_StringToArray_with_null_string()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => EF.Functions.StringToArray<string>(e.SomeText, "*", ";") != null).ToList();
                AssertContainsInSql("WHERE string_to_array(e.\"SomeText\", '*', ';') IS NOT NULL");
            }
        }

        [Fact]
        public void List_StringToList_with_null_string()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => EF.Functions.StringToList<string>(e.SomeText, "*", ";") != null).ToList();
                AssertContainsInSql("WHERE string_to_array(e.\"SomeText\", '*', ';') IS NOT NULL");
            }
        }

        #endregion

        #endregion

        #region Support

        ArrayFixture Fixture { get; }

        public ArrayQueryTest(ArrayFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        public class ArrayContext : DbContext
        {
            public DbSet<SomeArrayEntity> SomeEntities { get; set; }

            public ArrayContext(DbContextOptions options) : base(options) {}
        }

        public class SomeArrayEntity
        {
            public int Id { get; set; }
            public int[] SomeArray { get; set; }
            public int[,] SomeMatrix { get; set; }
            public List<int> SomeList { get; set; }
            public byte[] SomeBytea { get; set; }
            public string SomeText { get; set; }
        }

        public class ArrayFixture : IDisposable
        {
            readonly NpgsqlTestStore _testStore;
            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public ArrayFixture()
            {
                _testStore = NpgsqlTestStore.CreateScratch();

                using (var ctx = CreateContext())
                {
                    ctx.Database.EnsureCreated();
                    ctx.SomeEntities.AddRange(
                        new SomeArrayEntity
                        {
                            Id = 1,
                            SomeArray = new[] { 3, 4 },
                            SomeBytea = new byte[] { 3, 4 },
                            SomeMatrix = new[,] { { 5, 6 }, { 7, 8 } },
                            SomeList = new List<int> { 3, 4 }
                        },
                        new SomeArrayEntity
                        {
                            Id = 2,
                            SomeArray = new[] { 5, 6, 7 },
                            SomeBytea = new byte[] { 5, 6, 7 },
                            SomeMatrix = new[,] { { 10, 11 }, { 12, 13 } },
                            SomeList = new List<int> { 3, 4 }
                        });
                    ctx.SaveChanges();
                }
            }

            public ArrayContext CreateContext(Version postgresVersion = default)
                => new ArrayContext(CreateOptions(postgresVersion));

            public void Dispose() => _testStore.Dispose();

            DbContextOptions CreateOptions(Version postgresVersion = null)
                => new DbContextOptionsBuilder()
                   .UseNpgsql(_testStore.ConnectionString, b => b.ApplyConfiguration().SetPostgresVersion(postgresVersion))
                   .UseInternalServiceProvider(
                       new ServiceCollection()
                           .AddEntityFrameworkNpgsql()
                           .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                           .BuildServiceProvider())
                   .Options;
        }

        #endregion
    }
}
