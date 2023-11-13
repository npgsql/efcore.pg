namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Array;

public class ArrayQueryData : ISetSource
{
    public IReadOnlyList<ArrayEntity> ArrayEntities { get; }
    public IReadOnlyList<ArrayContainerEntity> ContainerEntities { get; }

    public ArrayQueryData()
    {
        (ArrayEntities, ContainerEntities) = (CreateArrayEntities(), CreateContainerEntities());
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(ArrayEntity))
        {
            return (IQueryable<TEntity>)ArrayEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ArrayContainerEntity))
        {
            return (IQueryable<TEntity>)ContainerEntities.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    public static IReadOnlyList<ArrayEntity> CreateArrayEntities()
        => new ArrayEntity[]
        {
            new()
            {
                Id = 1,
                IntArray = new[] { 3, 4 },
                IntList = new List<int> { 3, 4 },
                NullableIntArray = new int?[] { 3, 4, null },
                NullableIntList = new List<int?>
                {
                    3,
                    4,
                    null
                },
                Bytea = new byte[] { 3, 4 },
                ByteArray = new byte[] { 3, 4 },
                StringArray = new[] { "3", "4" },
                NullableStringArray = new[] { "3", "4", null },
                StringList = new List<string> { "3", "4" },
                NullableStringList = new List<string>
                {
                    "3",
                    "4",
                    null
                },
                NullableText = "foo",
                NonNullableText = "foo",
                Varchar10 = "foo",
                Varchar15 = "foo",
                EnumConvertedToInt = SomeEnum.One,
                EnumConvertedToString = SomeEnum.One,
                NullableEnumConvertedToString = SomeEnum.One,
                NullableEnumConvertedToStringWithNonNullableLambda = SomeEnum.One,
                ValueConvertedArray = new[] { SomeEnum.Eight, SomeEnum.Nine },
                ValueConvertedList = new List<SomeEnum> { SomeEnum.Eight, SomeEnum.Nine },
                IList = new[] { 8, 9 },
                Byte = 10
            },
            new()
            {
                Id = 2,
                IntArray = new[] { 5, 6, 7, 8 },
                IntList = new List<int>
                {
                    5,
                    6,
                    7,
                    8
                },
                NullableIntArray = new int?[] { 5, 6, 7, 8 },
                NullableIntList = new List<int?>
                {
                    5,
                    6,
                    7,
                    8
                },
                Bytea = new byte[] { 5, 6, 7, 8 },
                ByteArray = new byte[] { 5, 6, 7, 8 },
                StringArray = new[] { "5", "6", "7", "8" },
                NullableStringArray = new[] { "5", "6", "7", "8" },
                StringList = new List<string>
                {
                    "5",
                    "6",
                    "7",
                    "8"
                },
                NullableStringList = new List<string>
                {
                    "5",
                    "6",
                    "7",
                    "8"
                },
                NullableText = "bar",
                NonNullableText = "bar",
                Varchar10 = "bar",
                Varchar15 = "bar",
                EnumConvertedToInt = SomeEnum.Two,
                EnumConvertedToString = SomeEnum.Two,
                NullableEnumConvertedToString = SomeEnum.Two,
                NullableEnumConvertedToStringWithNonNullableLambda = SomeEnum.Two,
                ValueConvertedArray = new[] { SomeEnum.Nine, SomeEnum.Ten },
                ValueConvertedList = new List<SomeEnum> { SomeEnum.Nine, SomeEnum.Ten },
                IList = new[] { 9, 10 },
                Byte = 20
            }
        };

    public static IReadOnlyList<ArrayContainerEntity> CreateContainerEntities()
        => new[] { new ArrayContainerEntity { Id = 1, ArrayEntities = CreateArrayEntities().ToList() } };
}
