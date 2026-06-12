using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;

public class NpgsqlValueGenerationStrategyConventionTest
{
    [Fact]
    public void Annotations_are_added_when_conventional_model_builder_is_used()
    {
        var model = NpgsqlTestHelpers.Instance.CreateConventionBuilder().Model;

        var annotations = model.GetAnnotations().OrderBy(a => a.Name).ToList();
        Assert.Equal(3, annotations.Count);

        Assert.Equal(NpgsqlAnnotationNames.ValueGenerationStrategy, annotations.First().Name);
        Assert.Equal(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn, annotations.First().Value);
    }

    [Fact]
    public void Annotations_are_added_when_conventional_model_builder_is_used_with_sequences()
    {
        var model = NpgsqlTestHelpers.Instance.CreateConventionBuilder()
            .UseHiLo()
            .Model;

        model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);

        var annotations = model.GetAnnotations().OrderBy(a => a.Name).ToList();
        Assert.Equal(4, annotations.Count);

        Assert.Equal(NpgsqlAnnotationNames.HiLoSequenceName, annotations[0].Name);
        Assert.Equal(NpgsqlModelExtensions.DefaultHiLoSequenceName, annotations[0].Value);

        Assert.Equal(NpgsqlAnnotationNames.ValueGenerationStrategy, annotations[1].Name);
        Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, annotations[1].Value);

        Assert.Equal(RelationalAnnotationNames.MaxIdentifierLength, annotations[2].Name);

        Assert.Equal(
            RelationalAnnotationNames.Sequences,
            annotations[3].Name);
        Assert.NotNull(annotations[3].Value);
    }
}
