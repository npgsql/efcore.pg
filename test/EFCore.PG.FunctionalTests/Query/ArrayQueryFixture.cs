using Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Array;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public abstract class ArrayQueryFixture : SharedStoreFixtureBase<ArrayQueryContext>, IQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    private ArrayQueryData _expectedData;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Ignore(CoreEventId.CollectionWithoutComparer));

    protected override void Seed(ArrayQueryContext context)
        => ArrayQueryContext.Seed(context);

    public Func<DbContext> GetContextCreator()
        => CreateContext;

    public ISetSource GetExpectedData()
        => _expectedData ??= new ArrayQueryData();

    public IReadOnlyDictionary<Type, object> EntitySorters
        => new Dictionary<Type, Func<object, object>>
        {
            { typeof(ArrayEntity), e => ((ArrayEntity)e)?.Id }, { typeof(ArrayContainerEntity), e => ((ArrayContainerEntity)e)?.Id }
        }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters
        => new Dictionary<Type, Action<object, object>>
        {
            {
                typeof(ArrayEntity), (e, a) =>
                {
                    Assert.Equal(e is null, a is null);
                    if (a is not null)
                    {
                        var ee = (ArrayEntity)e;
                        var aa = (ArrayEntity)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.IntArray, ee.IntArray);
                        Assert.Equal(ee.IntList, ee.IntList);
                        Assert.Equal(ee.NullableIntArray, ee.NullableIntArray);
                        Assert.Equal(ee.Bytea, ee.Bytea);
                        Assert.Equal(ee.ByteArray, ee.ByteArray);
                        Assert.Equal(ee.StringArray, ee.StringArray);
                        Assert.Equal(ee.StringList, ee.StringList);
                        Assert.Equal(ee.NullableStringArray, ee.NullableStringArray);
                        Assert.Equal(ee.NullableStringList, ee.NullableStringList);
                        Assert.Equal(ee.NullableText, ee.NullableText);
                        Assert.Equal(ee.NonNullableText, ee.NonNullableText);
                        Assert.Equal(ee.EnumConvertedToInt, ee.EnumConvertedToInt);
                        Assert.Equal(ee.ValueConvertedArray, ee.ValueConvertedArray);
                        Assert.Equal(ee.ValueConvertedList, ee.ValueConvertedList);
                        Assert.Equal(ee.IList, ee.IList);
                        Assert.Equal(ee.Byte, ee.Byte);
                    }
                }
            },
            {
                typeof(ArrayContainerEntity), (e, a) =>
                {
                    Assert.Equal(e is null, a is null);
                    if (a is not null)
                    {
                        var ee = (ArrayContainerEntity)e;
                        var aa = (ArrayContainerEntity)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.ArrayEntities, ee.ArrayEntities);
                    }
                }
            }
        }.ToDictionary(e => e.Key, e => (object)e.Value);
}
