namespace Microsoft.EntityFrameworkCore.TestModels.Array;

public class ArrayEntity
{
    public int Id { get; set; }
    public int[] IntArray { get; set; } = null!;
    public List<int> IntList { get; set; } = null!;
    public int?[] NullableIntArray { get; set; } = null!;
    public List<int?> NullableIntList { get; set; } = null!;
    public byte[] Bytea { get; set; } = null!;
    public byte[] ByteArray { get; set; } = null!;
    public string[] StringArray { get; set; } = null!;
    public List<string> StringList { get; set; } = null!;
    public string?[] NullableStringArray { get; set; } = null!;
    public List<string?> NullableStringList { get; set; } = null!;
    public string? NullableText { get; set; }
    public string NonNullableText { get; set; } = null!;
    public string Varchar10 { get; set; } = null!;
    public string Varchar15 { get; set; } = null!;
    public SomeEnum EnumConvertedToInt { get; set; }
    public SomeEnum EnumConvertedToString { get; set; }
    public SomeEnum? NullableEnumConvertedToString { get; set; }
    public SomeEnum? NullableEnumConvertedToStringWithNonNullableLambda { get; set; }
    public SomeEnum[] ValueConvertedArrayOfEnum { get; set; } = null!;
    public List<SomeEnum> ValueConvertedListOfEnum { get; set; } = null!;
    public string[] ArrayOfStringConvertedToDelimitedString { get; set; } = null!;
    public List<string> ListOfStringConvertedToDelimitedString { get; set; } = null!;
    public IList<int> IList { get; set; } = null!;
    public byte Byte { get; set; }
}
