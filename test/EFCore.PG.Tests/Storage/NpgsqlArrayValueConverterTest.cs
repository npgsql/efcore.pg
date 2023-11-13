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

        Assert.Equal(new[] { 7 }, converter(new[] { Beatles.John }));
        Assert.Equal(new[] { 4 }, converter(new[] { Beatles.Paul }));
        Assert.Equal(new[] { 1 }, converter(new[] { Beatles.George }));
        Assert.Equal(new[] { -1 }, converter(new[] { Beatles.Ringo }));
        Assert.Equal(new[] { 77 }, converter(new[] { (Beatles)77 }));
        Assert.Equal(new[] { 0 }, converter(new[] { default(Beatles) }));
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

        Assert.Equal(new[] { Beatles.John }, converter(new[] { 7 }));
        Assert.Equal(new[] { Beatles.Paul }, converter(new[] { 4 }));
        Assert.Equal(new[] { Beatles.George }, converter(new[] { 1 }));
        Assert.Equal(new[] { Beatles.Ringo }, converter(new[] { -1 }));
        Assert.Equal(new[] { (Beatles)77 }, converter(new[] { 77 }));
        Assert.Equal(new[] { default(Beatles) }, converter(new[] { 0 }));
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

    private enum Beatles
    {
        John = 7,
        Paul = 4,
        George = 1,
        Ringo = -1
    }
}
