#nullable enable

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestModels.Array;

public class ArrayContainerEntity
{
    public int Id { get; set; }
    public List<ArrayEntity> ArrayEntities { get; set; } = null!;
}
