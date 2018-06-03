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
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.Id == 1);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                Assert.Equal(new List<int> { 3, 4 }, x.SomeList);
            }
        }

        [Fact]
        public void List_Roundtrip()
        {
            using (var ctx = CreateContext())
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
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeArray[0] == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE e.""SomeArray""[1] = 3");
            }
        }

        [Fact]
        public void List_Index_with_constant()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeList[0] == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE e.""SomeList""[1] = 3");
            }
        }

        [Fact]
        public void Array_Index_bytea_with_constant()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeBytea[0] == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE get_byte(e.""SomeBytea"", 0) = 3");
            }
        }

        [Fact]
        public void String_Index_text_with_constant_char_as_int()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeString[0] == 'T').ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE ascii(substr(e.""SomeString"", 1, 1)) = 84");
            }
        }

        [Fact]
        public void String_Index_text_with_constant_string()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeString[0].ToString() == "T").ToList();
                AssertContainsInSql(@"WHERE CAST(ascii(substr(e.""SomeString"", 1, 1)) AS text) = 'T'");
            }
        }

        [Fact]
        public void Array_ElementAt_with_constant()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeArray.ElementAt(0) == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE e.""SomeArray""[1] = 3");
            }
        }

        [Fact]
        public void List_ElementAt_with_constant()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeList.ElementAt(0) == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE e.""SomeList""[1] = 3");
            }
        }

        [Fact]
        public void Array_ElementAt_bytea_with_constant()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeBytea.ElementAt(0) == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE get_byte(e.""SomeBytea"", 0) = 3");
            }
        }

        [Fact]
        public void String_ElementAt_text_with_constant_char_as_int()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeString.ElementAt(0) == 'T').ToList();
                AssertContainsInSql(@"WHERE ascii(substr(e.""SomeString"", 1, 1)) = 84");
            }
        }

        [Fact]
        public void String_ElementAt_text_with_constant_string()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeString.ElementAt(0).ToString() == "T").ToList();
                AssertContainsInSql(@"WHERE CAST(ascii(substr(e.""SomeString"", 1, 1)) AS text) = 'T'");
            }
        }

        [Fact]
        public void Array_Index_with_non_constant()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeArray[x] == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE e.""SomeArray""[@__x_0 + 1] = 3");
            }
        }

        [Fact]
        public void List_Index_with_non_constant()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeList[x] == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE e.""SomeList""[@__x_0 + 1] = 3");
            }
        }

        [Fact]
        public void Array_Index_bytea_with_non_constant()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeBytea[x] == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE get_byte(e.""SomeBytea"", @__x_0) = 3");
            }
        }

        [Fact]
        public void String_Index_text_with_non_constant_char_as_int()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeString[x] == 'T').ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE ascii(substr(e.""SomeString"", @__x_0 + 1, 1)) = 84");
            }
        }

        [Fact]
        public void String_Index_text_with_non_constant_string()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var _ = ctx.SomeEntities.Where(e => e.SomeString[x].ToString() == "T").ToList();
                AssertContainsInSql(@"WHERE CAST(ascii(substr(e.""SomeString"", @__x_0 + 1, 1)) AS text) = 'T'");
            }
        }

        [Fact]
        public void Array_ElementAt_with_non_constant()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeArray.ElementAt(x) == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE e.""SomeArray""[@__x_0 + 1] = 3");
            }
        }

        [Fact]
        public void List_ElementAt_with_non_constant()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeList.ElementAt(x) == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE e.""SomeList""[@__x_0 + 1] = 3");
            }
        }

        [Fact]
        public void Array_ElementAt_bytea_with_non_constant()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeBytea.ElementAt(x) == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE get_byte(e.""SomeBytea"", @__x_0) = 3");
            }
        }

        [Fact]
        public void String_ElementAt_text_with_non_constant_char_as_int()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeString.ElementAt(x) == 'T').ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE ascii(substr(e.""SomeString"", @__x_0 + 1, 1)) = 84");
            }
        }

        [Fact]
        public void String_ElementAt_text_with_non_constant_sting()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var _ = ctx.SomeEntities.Where(e => e.SomeString.ElementAt(x).ToString() == "T").ToList();
                AssertContainsInSql(@"WHERE CAST(ascii(substr(e.""SomeString"", @__x_0 + 1, 1)) AS text) = 'T'");
            }
        }

        [Fact]
        public void Array_Index_multidimensional()
        {
            using (var ctx = CreateContext())
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
            using (var ctx = CreateContext())
            {
                var array = new[] { 3, 4 };
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Equals(array));
                Assert.Equal(array, x.SomeArray);
                AssertContainsInSql(@"WHERE e.""SomeArray"" = @__array_0");
            }
        }

        [Fact]
        public void List_Equal_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                var list = new List<int> { 3, 4 };
                var x = ctx.SomeEntities.Single(e => e.SomeList.Equals(list));
                Assert.Equal(list, x.SomeList);
                AssertContainsInSql(@"WHERE e.""SomeList"" = @__list_0");
            }
        }

        [Fact]
        public void Array_SequenceEqual_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                var array = new[] { 3, 4 };
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Equals(array));
                Assert.Equal(array, x.SomeArray);
                AssertContainsInSql(@"WHERE e.""SomeArray"" = @__array_0");
            }
        }

        [Fact]
        public void List_SequenceEqual_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                var list = new List<int> { 3, 4 };
                var x = ctx.SomeEntities.Single(e => e.SomeList.SequenceEqual(list));
                Assert.Equal(list, x.SomeList);
                AssertContainsInSql(@"WHERE e.""SomeList"" = @__list_0");
            }
        }

        [Fact]
        public void Array_SequenceEqual_with_literal()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.SequenceEqual(new[] { 3, 4 }));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE e.""SomeArray"" = ARRAY[3,4]::integer");
            }
        }

        [Fact]
        public void List_SequenceEqual_with_literal()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeList.SequenceEqual(new List<int> { 3, 4 }));
                Assert.Equal(new List<int> { 3, 4 }, x.SomeList);
                AssertContainsInSql(@"WHERE e.""SomeList"" = ARRAY[3,4]");
            }
        }

        #endregion

        #region Containment

        [Fact]
        public void Array_Contains_with_literal()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Contains(3));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE 3 = ANY (e.""SomeArray"")");
            }
        }

        [Fact]
        public void List_Contains_with_literal()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeList.Contains(3));
                Assert.Equal(new[] { 3, 4 }, x.SomeList);
                AssertContainsInSql(@"WHERE 3 = ANY (e.""SomeList"")");
            }
        }

        [Fact]
        public void Array_Contains_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var p = 3;
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Contains(p));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE @__p_0 = ANY (e.""SomeArray"")");
            }
        }

        [Fact]
        public void List_Contains_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var p = 3;
                var x = ctx.SomeEntities.Single(e => e.SomeList.Contains(p));
                Assert.Equal(new[] { 3, 4 }, x.SomeList);
                AssertContainsInSql(@"WHERE @__p_0 = ANY (e.""SomeList"")");
            }
        }

        [Fact]
        public void Array_Contains_with_column()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Contains(e.Id + 2));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE e.""Id"" + 2 = ANY (e.""SomeArray"")");
            }
        }

        [Fact]
        public void List_Contains_with_column()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeList.Contains(e.Id + 2));
                Assert.Equal(new List<int> { 3, 4 }, x.SomeList);
                AssertContainsInSql(@"WHERE e.""Id"" + 2 = ANY (e.""SomeList"")");
            }
        }

        [Fact]
        public void Array_All_Contains_List()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeList.All(y => x.SomeArray.Contains(y))).ToList();
                AssertContainsInSql(@"WHERE (x.""SomeList"" <@ x.""SomeArray"") = TRUE");
            }
        }

        [Fact]
        public void List_All_Contains_Array()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeArray.All(y => x.SomeList.Contains(y))).ToList();
                AssertContainsInSql(@"WHERE (x.""SomeArray"" <@ x.""SomeList"") = TRUE");
            }
        }

        [Fact]
        public void Array_Any_Contains_List()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeArray.Any(y => x.SomeList.Contains(y))).ToList();
                AssertContainsInSql(@"WHERE (x.""SomeArray"" && x.""SomeList"") = TRUE");
            }
        }

        [Fact]
        public void List_Any_Contains_Array()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeList.Any(y => x.SomeArray.Contains(y))).ToList();
                AssertContainsInSql(@"WHERE (x.""SomeList"" && x.""SomeArray"") = TRUE");
            }
        }

        #endregion

        #region Count

        [Fact]
        public void Array_Length()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Length == 2);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE array_length(e.""SomeArray"", 1) = 2");
            }
        }

        [Fact]
        public void List_Length()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.SomeList.Count == 0).ToArray();
                AssertContainsInSql(@"WHERE array_length(e.""SomeList"", 1) = 0");
            }
        }

        [Fact]
        public void Array_Count()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once UseCollectionCountProperty
                var _ = ctx.SomeEntities.Where(e => e.SomeArray.Count() == 1).ToArray();
                AssertContainsInSql(@"WHERE array_length(e.""SomeArray"", 1) = 1");
            }
        }

        [Fact]
        public void List_Count()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once UseCollectionCountProperty
                var _ = ctx.SomeEntities.Where(e => e.SomeList.Count() == 1).ToArray();
                AssertContainsInSql(@"WHERE array_length(e.""SomeList"", 1) = 1");
            }
        }

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/9242")]
        public void Array_Length_on_EF_Property()
        {
            using (var ctx = CreateContext())
            {
                // TODO: This fails
                var x = ctx.SomeEntities.Single(e => EF.Property<int[]>(e, nameof(SomeArrayEntity.SomeArray)).Length == 2);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE array_length(e.""SomeArray"", 1) = 2");
            }
        }

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/9242")]
        public void List_Length_on_EF_Property()
        {
            using (var ctx = CreateContext())
            {
                // TODO: This fails
                var x = ctx.SomeEntities.Single(e => EF.Property<List<int>>(e, nameof(SomeArrayEntity.SomeList)).Count == 2);
                Assert.Equal(new List<int> { 3, 4 }, x.SomeList);
                AssertContainsInSql(@"WHERE array_length(e.""SomeList"", 1) = 2");
            }
        }

        [Fact]
        public void Array_Length_on_literal_not_translated()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => new[] { 1, 2, 3 }.Length == e.Id).ToList();
                AssertContainsInSql(@"WHERE 3 = e.""Id""");
                AssertDoesNotContainInSql("array_length");
            }
        }

        [Fact]
        public void List_Length_on_literal_not_translated()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => new List<int> { 1, 2, 3 }.Count == e.Id).ToList();
                AssertContainsInSql(@"WHERE @__Count_0 = e.""Id""");
                AssertDoesNotContainInSql("array_length");
            }
        }

        #endregion

        #region Concatenation

        [Fact]
        public void Array_Concat_with_array_column()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeArray.Concat(e.SomeArray)).ToList();
                AssertContainsInSql(@"SELECT (e.""SomeArray"" || e.""SomeArray"")");
            }
        }

        [Fact]
        public void List_Concat_with_list_column()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeList.Concat(e.SomeList)).ToList();
                AssertContainsInSql(@"SELECT (e.""SomeList"" || e.""SomeList"")");
            }
        }

        [Fact]
        public void Array_Concat_with_list_column()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeArray.Concat(e.SomeList)).ToList();
                AssertContainsInSql(@"SELECT (e.""SomeArray"" || e.""SomeList"")");
            }
        }

        [Fact]
        public void List_Concat_with_array_column()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeList.Concat(e.SomeArray)).ToList();
                AssertContainsInSql(@"SELECT (e.""SomeList"" || e.""SomeArray"")");
            }
        }

// .NET 4.6.1 doesn't include the Enumerable.Append and Enumerable.Prepend functions...
#if !NET461
        [Fact]
        public void Array_Append_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeArray.Append(0)).ToList();
                AssertContainsInSql(@"SELECT (e.""SomeArray"" || 0)");
            }
        }

        [Fact]
        public void List_Append_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeList.Append(0)).ToList();
                AssertContainsInSql(@"SELECT (e.""SomeList"" || 0)");
            }
        }

        [Fact]
        public void Array_Prepend_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeArray.Prepend(0)).ToList();
                AssertContainsInSql(@"SELECT (0 || e.""SomeArray"")");
            }
        }

        [Fact]
        public void List_Prepend_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeList.Prepend(0)).ToList();
                AssertContainsInSql(@"SELECT (0 || e.""SomeList"")");
            }
        }

#endif

        #endregion

        #region IndexOf

        [Fact]
        public void Array_IndexOf_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => Array.IndexOf(e.SomeArray, 0)).ToList();
                AssertContainsInSql(@"SELECT COALESCE(array_position(e.""SomeArray"", 0), -1)");
            }
        }

        [Fact]
        public void List_IndexOf_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => e.SomeList.IndexOf(0)).ToList();
                AssertContainsInSql(@"SELECT COALESCE(array_position(e.""SomeList"", 0), -1)");
            }
        }

        #endregion

        #region StringConversion

        [Fact]
        public void Array_ArrayToString()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayToString(e.SomeArray, ",")).ToList();
                AssertContainsInSql(@"SELECT array_to_string(e.""SomeArray"", ',')");
            }
        }

        [Fact]
        public void List_ArrayToString()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayToString(e.SomeList, ",")).ToList();
                AssertContainsInSql(@"SELECT array_to_string(e.""SomeList"", ',')");
            }
        }

        [Fact]
        public void Array_ArrayToString_with_null()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayToString(e.SomeArray, ",", "*")).ToList();
                AssertContainsInSql(@"SELECT array_to_string(e.""SomeArray"", ',', '*')");
            }
        }

        [Fact]
        public void List_ArrayToString_with_null()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => EF.Functions.ArrayToString(e.SomeList, ",", "*")).ToList();
                AssertContainsInSql(@"SELECT array_to_string(e.""SomeList"", ',', '*')");
            }
        }

        [Fact]
        public void Array_StringToArray()
        {
            using (var ctx = CreateContext())
            {
                var _ =
                    ctx.SomeEntities
                       .Select(e => EF.Functions.ArrayToString(e.SomeArray, ",", "*"))
                       .Select(e => EF.Functions.StringToArray<string>(e, ",", "*")).ToList();

                AssertContainsInSql(@"SELECT string_to_array(array_to_string(e.""SomeArray"", ',', '*'), ',', '*')");
            }
        }

        [Fact]
        public void List_StringToList()
        {
            using (var ctx = CreateContext())
            {
                var _ =
                    ctx.SomeEntities
                       .Select(e => EF.Functions.ArrayToString(e.SomeList, ",", "*"))
                       .Select(e => EF.Functions.StringToList<string>(e, ",", "*")).ToList();

                AssertContainsInSql(@"SELECT string_to_array(array_to_string(e.""SomeList"", ',', '*'), ',', '*')");
            }
        }

        #endregion

        #region Exists

        [Fact]
        public void Array_Exists_equals_with_literal_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => Array.Exists(x.SomeArray, y => y == 1)).ToList();
                AssertContainsInSql(@"WHERE 1 = ANY (x.""SomeArray"") = TRUE");
            }
        }

        [Fact]
        public void List_Exists_equals_with_literal_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeList.Exists(y => y == 1)).ToList();
                AssertContainsInSql(@"WHERE 1 = ANY (x.""SomeList"") = TRUE");
            }
        }

        [Fact]
        public void Array_Exists_not_equal_with_literal_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => Array.Exists(x.SomeArray, y => y != 1)).ToList();
                AssertContainsInSql(@"WHERE 1 <> ANY (x.""SomeArray"") = TRUE");
            }
        }

        [Fact]
        public void List_Exists_not_equal_with_literal_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeList.Exists(y => y != 1)).ToList();
                AssertContainsInSql(@"WHERE 1 <> ANY (x.""SomeList"") = TRUE");
            }
        }

        [Fact]
        public void Array_Exists_equals_with_parameter_array_and_column_array_element()
        {
            using (var ctx = CreateContext())
            {
                var array = new[] { 0, 1, 2 };
                var _ = ctx.SomeEntities.Where(x => Array.Exists(array, y => y == x.SomeArray[0])).ToList();
                AssertContainsInSql(@"WHERE x.""SomeArray""[1] IN (0, 1, 2)");
            }
        }

        [Fact]
        public void List_Exists_equals_with_parameter_array_and_column_list_element()
        {
            using (var ctx = CreateContext())
            {
                var list = new List<int> { 0, 1, 2 };
                var _ = ctx.SomeEntities.Where(x => list.Exists(y => y == x.SomeList[0])).ToList();
                AssertContainsInSql(@"WHERE x.""SomeList""[1] IN (0, 1, 2)");
            }
        }

        [Fact]
        public void Array_Exists_equals_with_parameter_array_and_column_array_element_flipped()
        {
            using (var ctx = CreateContext())
            {
                var array = new[] { 0, 1, 2 };
                var _ = ctx.SomeEntities.Where(x => Array.Exists(array, y => x.SomeArray[0] == y)).ToList();
                AssertContainsInSql(@"WHERE x.""SomeArray""[1] IN (0, 1, 2)");
            }
        }

        [Fact]
        public void List_Exists_equals_with_parameter_array_and_column_list_element_flipped()
        {
            using (var ctx = CreateContext())
            {
                var list = new List<int> { 0, 1, 2 };
                var _ = ctx.SomeEntities.Where(x => list.Exists(y => x.SomeList[0] == y)).ToList();
                AssertContainsInSql(@"WHERE x.""SomeList""[1] IN (0, 1, 2)");
            }
        }

        [Fact]
        public void Array_Exists_equals_with_literal_constant_flipped()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => Array.Exists(x.SomeArray, y => 1 == y)).ToList();
                AssertContainsInSql(@"WHERE 1 = ANY (x.""SomeArray"") = TRUE");
            }
        }

        [Fact]
        public void List_Exists_equals_with_literal_constant_flipped()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeList.Exists(y => 1 == y)).ToList();
                AssertContainsInSql(@"WHERE 1 = ANY (x.""SomeList"") = TRUE");
            }
        }

        [Fact]
        public void Array_Exists_less_than_with_literal_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => Array.Exists(x.SomeArray, y => y < 1)).ToList();
                AssertContainsInSql(@"WHERE 1 > ANY (x.""SomeArray"") = TRUE");
            }
        }

        [Fact]
        public void List_Exists_less_than_with_literal_constant()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeList.Exists(y => y < 1)).ToList();
                AssertContainsInSql(@"WHERE 1 > ANY (x.""SomeList"") = TRUE");
            }
        }

        [Fact]
        public void Array_Exists_less_than_with_literal_constant_flipped()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => Array.Exists(x.SomeArray, y => 1 > y)).ToList();
                AssertContainsInSql(@"WHERE 1 > ANY (x.""SomeArray"") = TRUE");
            }
        }

        [Fact]
        public void List_Exists_less_than_with_literal_constant_flipped()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeList.Exists(y => 1 > y)).ToList();
                AssertContainsInSql(@"WHERE 1 > ANY (x.""SomeList"") = TRUE");
            }
        }

        [Fact]
        public void Array_Exists_equals_with_column_list_element()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => Array.Exists(x.SomeArray, y => y == x.SomeList[0])).ToList();
                AssertContainsInSql(@"WHERE x.""SomeList""[1] = ANY (x.""SomeArray"") = TRUE");
            }
        }

        [Fact]
        public void List_Exists_equals_with_column_array_element()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeList.Exists(y => y == x.SomeArray[0])).ToList();
                AssertContainsInSql(@"WHERE x.""SomeArray""[1] = ANY (x.""SomeList"") = TRUE");
            }
        }

        [Fact]
        public void Array_Exists_equals_with_column_list_element_flipped()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => Array.Exists(x.SomeArray, y => x.SomeList[0] == y)).ToList();
                AssertContainsInSql(@"WHERE x.""SomeList""[1] = ANY (x.""SomeArray"") = TRUE");
            }
        }

        [Fact]
        public void List_Exists_equals_with_column_array_element_flipped()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(x => x.SomeList.Exists(y => x.SomeArray[0] == y)).ToList();
                AssertContainsInSql(@"WHERE x.""SomeArray""[1] = ANY (x.""SomeList"") = TRUE");
            }
        }

        #endregion

        #endregion

        #region Support

        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        ArrayFixture Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture">The fixture of resources for testing.</param>
        public ArrayQueryTest(ArrayFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        /// <summary>
        /// Creates a new <see cref="ArrayContext"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="ArrayContext"/> for testing.
        /// </returns>
        ArrayContext CreateContext() => Fixture.CreateContext();

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="expected">The SQL statement or fragment to search for in the logs.</param>
        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        /// <summary>
        /// Asserts that the SQL fragment does not appear in the logs.
        /// </summary>
        /// <param name="expected">The SQL statement or fragment to search for in the logs.</param>
        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        #endregion Support

        #region Fixtures

        /// <summary>
        /// Represents a database suitable for testing operations with PostgreSQL arrays.
        /// </summary>
        public class ArrayContext : DbContext
        {
            public DbSet<SomeArrayEntity> SomeEntities { get; set; }
            public ArrayContext(DbContextOptions options) : base(options) {}
            protected override void OnModelCreating(ModelBuilder builder) {}
        }

        /// <summary>
        /// Represents an entity suitable for testing operations with PostgreSQL arrays.
        /// </summary>
        public class SomeArrayEntity
        {
            public int Id { get; set; }
            public int[] SomeArray { get; set; }
            public List<int> SomeList { get; set; }
            public int[,] SomeMatrix { get; set; }
            public byte[] SomeBytea { get; set; }
            public string SomeString { get; set; }
        }

        /// <summary>
        /// Represents a fixture suitable for testing operations with PostgreSQL arrays.
        /// </summary>
        public class ArrayFixture : IDisposable
        {
            readonly DbContextOptions _options;
            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public ArrayFixture()
            {
                _testStore = NpgsqlTestStore.CreateScratch();
                _options = new DbContextOptionsBuilder()
                           .UseNpgsql(_testStore.ConnectionString, b => b.ApplyConfiguration())
                           .UseInternalServiceProvider(
                               new ServiceCollection()
                                   .AddEntityFrameworkNpgsql()
                                   .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                                   .BuildServiceProvider())
                           .Options;

                using (var ctx = CreateContext())
                {
                    ctx.Database.EnsureCreated();
                    ctx.SomeEntities.Add(new SomeArrayEntity
                    {
                        Id = 1,
                        SomeArray = new[] { 3, 4 },
                        SomeBytea = new byte[] { 3, 4 },
                        SomeList = new List<int> { 3, 4 },
                        SomeMatrix = new[,] { { 5, 6 }, { 7, 8 } },
                        SomeString = "This_is_a_test"
                    });
                    ctx.SomeEntities.Add(new SomeArrayEntity
                    {
                        Id = 2,
                        SomeArray = new[] { 5, 6, 7 },
                        SomeBytea = new byte[] { 5, 6, 7 },
                        SomeList = new List<int> { 5, 6, 7 },
                        SomeMatrix = new[,] { { 10, 11 }, { 12, 13 } },
                        SomeString = "this_is_a_test"
                    });
                    ctx.SaveChanges();
                }
            }

            readonly NpgsqlTestStore _testStore;
            public ArrayContext CreateContext() => new ArrayContext(_options);
            public void Dispose() => _testStore.Dispose();
        }

        #endregion
    }
}
