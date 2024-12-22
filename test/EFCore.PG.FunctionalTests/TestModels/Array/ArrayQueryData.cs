namespace Microsoft.EntityFrameworkCore.TestModels.Array;

public class ArrayQueryData : ISetSource
{
    public IReadOnlyList<ArrayEntity> ArrayEntities { get; } = CreateArrayEntities();
    public IReadOnlyList<ArrayContainerEntity> ContainerEntities { get; } = CreateContainerEntities();

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
        =>
        [
            new()
            {
                Id = 1,
                IntArray = [3, 4],
                IntList = [3, 4],
                NullableIntArray = [3, 4, null],
                NullableIntList = [3, 4, null],
                Bytea = [3, 4],
                ByteArray = [3, 4],
                StringArray = ["3", "4"],
                NullableStringArray = ["3", "4", null],
                StringList = ["3", "4"],
                NullableStringList = ["3", "4", null],
                NullableText = "foo",
                NonNullableText = "foo",
                Varchar10 = "foo",
                Varchar15 = "foo",
                EnumConvertedToInt = SomeEnum.One,
                EnumConvertedToString = SomeEnum.One,
                NullableEnumConvertedToString = SomeEnum.One,
                NullableEnumConvertedToStringWithNonNullableLambda = SomeEnum.One,
                ValueConvertedArrayOfEnum = [SomeEnum.Eight, SomeEnum.Nine],
                ValueConvertedListOfEnum = [SomeEnum.Eight, SomeEnum.Nine],
                ArrayOfStringConvertedToDelimitedString = ["3", "4"],
                ListOfStringConvertedToDelimitedString = ["3", "4"],
                IList = [8, 9],
                Byte = 10
            },
            new()
            {
                Id = 2,
                IntArray = [5, 6, 7, 8],
                IntList = [5, 6, 7, 8],
                NullableIntArray = [5, 6, 7, 8],
                NullableIntList = [5, 6, 7, 8],
                Bytea = [5, 6, 7, 8],
                ByteArray = [5, 6, 7, 8],
                StringArray = ["5", "6", "7", "8"],
                NullableStringArray = ["5", "6", "7", "8"],
                StringList = ["5", "6", "7", "8"],
                NullableStringList = ["5", "6", "7", "8"],
                NullableText = "bar",
                NonNullableText = "bar",
                Varchar10 = "bar",
                Varchar15 = "bar",
                EnumConvertedToInt = SomeEnum.Two,
                EnumConvertedToString = SomeEnum.Two,
                NullableEnumConvertedToString = SomeEnum.Two,
                NullableEnumConvertedToStringWithNonNullableLambda = SomeEnum.Two,
                ValueConvertedArrayOfEnum = [SomeEnum.Nine, SomeEnum.Ten],
                ValueConvertedListOfEnum = [SomeEnum.Nine, SomeEnum.Ten],
                ArrayOfStringConvertedToDelimitedString = ["5", "6", "7", "8"],
                ListOfStringConvertedToDelimitedString = ["5", "6", "7", "8"],
                IList = [9, 10],
                Byte = 20
            }
        ];

    public static IReadOnlyList<ArrayContainerEntity> CreateContainerEntities()
        => [new ArrayContainerEntity { Id = 1, ArrayEntities = CreateArrayEntities().ToList() }];
}
