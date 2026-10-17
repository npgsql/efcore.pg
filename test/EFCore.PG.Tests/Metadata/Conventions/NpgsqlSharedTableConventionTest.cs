using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

public class NpgsqlSharedTableConventionTest
{
    [Fact]
    public void Keys_are_not_uniquified_across_schemas()
    {
        var modelBuilder = NpgsqlTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<BoardMembersChair>().ToTable("chair", "board_members");
        modelBuilder.Entity<FurnitureChair>().ToTable("chair", "furniture");

        var model = modelBuilder.FinalizeModel();

        Assert.Equal("PK_chair", model.FindEntityType(typeof(BoardMembersChair)).FindPrimaryKey().GetName());
        Assert.Equal("PK_chair", model.FindEntityType(typeof(FurnitureChair)).FindPrimaryKey().GetName());
    }

    private class BoardMembersChair
    {
        public int Id { get; set; }
    }

    private class FurnitureChair
    {
        public int Id { get; set; }
    }
}
