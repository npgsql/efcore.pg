using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage;

public class NpgsqlArrayValueConverterTest
{
    private static readonly ValueConverter<Beatles[], int[]> EnumArrayToNumberArray
        = new NpgsqlArrayConverter<Beatles[], Beatles[], int[]>(new EnumToNumberConverter<Beatles, int>());

    [ConditionalFact]
    public void Can_convert_enum_arrays_to_number_arrays()
    {
        var converter = EnumArrayToNumberArray.ConvertToProviderExpression.Compile();

        Assert.Equal(new[] { 7 }, converter([Beatles.John]));
        Assert.Equal(new[] { 4 }, converter([Beatles.Paul]));
        Assert.Equal(new[] { 1 }, converter([Beatles.George]));
        Assert.Equal(new[] { -1 }, converter([Beatles.Ringo]));
        Assert.Equal(new[] { 77 }, converter([(Beatles)77]));
        Assert.Equal(new[] { 0 }, converter([default(Beatles)]));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_enum_arrays_to_number_arrays_object()
    {
        var converter = EnumArrayToNumberArray.ConvertToProvider;

        Assert.Equal(new[] { 7 }, converter(new[] { Beatles.John }));
        Assert.Equal(new[] { 4 }, converter(new[] { Beatles.Paul }));
        Assert.Equal(new[] { 1 }, converter(new[] { Beatles.George }));
        Assert.Equal(new[] { -1 }, converter(new[] { Beatles.Ringo }));
        Assert.Equal(new[] { 77 }, converter(new[] { (Beatles)77 }));
        Assert.Equal(new[] { 0 }, converter(new[] { default(Beatles) }));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_number_arrays_to_enum_arrays()
    {
        var converter = EnumArrayToNumberArray.ConvertFromProviderExpression.Compile();

        Assert.Equal([Beatles.John], converter([7]));
        Assert.Equal([Beatles.Paul], converter([4]));
        Assert.Equal([Beatles.George], converter([1]));
        Assert.Equal([Beatles.Ringo], converter([-1]));
        Assert.Equal([(Beatles)77], converter([77]));
        Assert.Equal([default(Beatles)], converter([0]));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_number_arrays_to_enum_arrays_object()
    {
        var converter = EnumArrayToNumberArray.ConvertFromProvider;

        Assert.Equal(new[] { Beatles.John }, converter(new[] { 7 }));
        Assert.Equal(new[] { Beatles.Paul }, converter(new[] { 4 }));
        Assert.Equal(new[] { Beatles.George }, converter(new[] { 1 }));
        Assert.Equal(new[] { Beatles.Ringo }, converter(new[] { -1 }));
        Assert.Equal(new[] { (Beatles)77 }, converter(new[] { 77 }));
        Assert.Equal(new[] { default(Beatles) }, converter(new[] { 0 }));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_non_nullable_arrays_with_nullable_element_converter()
    {
        var converter = new NpgsqlArrayConverter<int[], int[], string[]>(
            new ValueConverter<int?, string>(v => v.HasValue ? v.Value.ToString() : null, v => v == null ? null : int.Parse(v)));

        Assert.Equal(["1", "2"], converter.ConvertToProviderExpression.Compile()([1, 2]));
        Assert.Equal([1, 2], converter.ConvertFromProviderExpression.Compile()(["1", "2"]));
    }

    [ConditionalFact]
    public void Can_convert_non_nullable_lists_with_nullable_element_converter()
    {
        var converter = new NpgsqlArrayConverter<List<int>, List<int>, string[]>(
            new ValueConverter<int?, string>(v => v.HasValue ? v.Value.ToString() : null, v => v == null ? null : int.Parse(v)));

        Assert.Equal(["1", "2"], converter.ConvertToProviderExpression.Compile()([1, 2]));
        Assert.Equal([1, 2], converter.ConvertFromProviderExpression.Compile()(["1", "2"]));
    }

    [ConditionalFact]
    public void Can_convert_object_arrays_through_an_element_converter()
    {
        var elementConverter = new ValueConverter<int, long>(v => v, v => checked((int)v));
        var converter = new NpgsqlArrayConverter<object[], object[], long[]>(elementConverter);

        var toProvider = (Func<object[], long[]>)converter.ConvertToProviderExpression.Compile();
        Assert.Equal([1L, 2L], toProvider([1, 2]));
        Assert.Null(toProvider(null));

        var fromProvider = (Func<long[], object[]>)converter.ConvertFromProviderExpression.Compile();
        Assert.Equal(new object[] { 1, 2 }, fromProvider([1L, 2L]));
        Assert.Null(fromProvider(null));
    }

    [ConditionalFact]
    public void Can_convert_object_lists_through_an_element_converter()
    {
        var elementConverter = new ValueConverter<int, long>(v => v, v => checked((int)v));
        var converter = new NpgsqlArrayConverter<List<object>, List<object>, long[]>(elementConverter);

        var toProvider = (Func<List<object>, long[]>)converter.ConvertToProviderExpression.Compile();
        Assert.Equal([1L, 2L], toProvider([1, 2]));

        var fromProvider = (Func<long[], List<object>>)converter.ConvertFromProviderExpression.Compile();
        Assert.Equal(new List<object> { 1, 2 }, fromProvider([1L, 2L]));
    }

    [ConditionalFact]
    public void Can_convert_object_arrays_through_a_string_element_converter()
    {
        var elementConverter = new ValueConverter<int, string>(v => v.ToString(), v => int.Parse(v));
        var converter = new NpgsqlArrayConverter<object[], object[], string[]>(elementConverter);

        var toProvider = (Func<object[], string[]>)converter.ConvertToProviderExpression.Compile();
        Assert.Equal(new string[] { "1", null, "2" }, toProvider(new object[] { 1, null, 2 }));

        var fromProvider = (Func<string[], object[]>)converter.ConvertFromProviderExpression.Compile();
        Assert.Equal(new object[] { 1, null, 2 }, fromProvider(new string[] { "1", null, "2" }));
    }

    [ConditionalFact]
    public void Can_convert_object_lists_through_a_string_element_converter()
    {
        var elementConverter = new ValueConverter<int, string>(v => v.ToString(), v => int.Parse(v));
        var converter = new NpgsqlArrayConverter<List<object>, List<object>, string[]>(elementConverter);

        var toProvider = (Func<List<object>, string[]>)converter.ConvertToProviderExpression.Compile();
        Assert.Equal(new string[] { "1", null, "2" }, toProvider(new List<object> { 1, null, 2 }));

        var fromProvider = (Func<string[], List<object>>)converter.ConvertFromProviderExpression.Compile();
        Assert.Equal(new List<object> { 1, null, 2 }, fromProvider(new string[] { "1", null, "2" }));
    }

    [ConditionalFact]
    public void Can_convert_object_arrays_through_a_string_to_string_converter()
    {
        var elementConverter = new ValueConverter<string, string>(v => v, v => v);
        var converter = new NpgsqlArrayConverter<object[], object[], string[]>(elementConverter);

        var toProvider = (Func<object[], string[]>)converter.ConvertToProviderExpression.Compile();
        Assert.Equal(new string[] { "a", null }, toProvider(new object[] { "a", null }));
    }

    [ConditionalFact]
    public void Can_preserve_null_elements_in_object_arrays()
    {
        var elementConverter = new ValueConverter<int, long>(v => v, v => checked((int)v));
        var converter = new NpgsqlArrayConverter<object[], object[], long?[]>(elementConverter);

        var toProvider = (Func<object[], long?[]>)converter.ConvertToProviderExpression.Compile();
        Assert.Equal(new long?[] { null, 2L }, toProvider(new object[] { null, 2 }));

        var fromProvider = (Func<long?[], object[]>)converter.ConvertFromProviderExpression.Compile();
        Assert.Equal(new object[] { null, 2 }, fromProvider(new long?[] { null, 2L }));
    }

    [ConditionalFact]
    public void Can_preserve_null_elements_in_object_lists()
    {
        var elementConverter = new ValueConverter<int, long>(v => v, v => checked((int)v));
        var converter = new NpgsqlArrayConverter<List<object>, List<object>, long?[]>(elementConverter);

        var toProvider = (Func<List<object>, long?[]>)converter.ConvertToProviderExpression.Compile();
        Assert.Equal(new long?[] { null, 2L }, toProvider(new List<object> { null, 2 }));

        var fromProvider = (Func<long?[], List<object>>)converter.ConvertFromProviderExpression.Compile();
        Assert.Equal(new List<object> { null, 2 }, fromProvider(new long?[] { null, 2L }));
    }

    [ConditionalFact]
    public void Rejects_an_element_converter_whose_type_does_not_match_the_array()
    {
        var elementConverter = new ValueConverter<int, long>(v => v, v => checked((int)v));

        var exception = Assert.Throws<ArgumentException>(
            () => new NpgsqlArrayConverter<long[], long[], long[]>(elementConverter));

        Assert.Contains("doesn't match", exception.Message);
    }

    private enum Beatles
    {
        John = 7,
        Paul = 4,
        George = 1,
        Ringo = -1
    }
}
