namespace Microsoft.EntityFrameworkCore.TestModels.Array;

public class ArrayContainerEntity
{
    public int Id { get; set; }
    public List<ArrayEntity> ArrayEntities { get; set; } = null!;
}
