using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions
{
    public class NpgsqlValueGenerationStrategyConventionTest
    {
        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used()
        {
            var model = NpgsqlTestHelpers.Instance.CreateConventionBuilder().Model;

            var annotations = model.GetAnnotations().OrderBy(a => a.Name).ToList();
            Assert.Equal(2, annotations.Count);

            Assert.Equal(NpgsqlAnnotationNames.ValueGenerationStrategy, annotations.First().Name);
            Assert.Equal(NpgsqlValueGenerationStrategy.SerialColumn, annotations.First().Value);
        }

        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used_with_sequences()
        {
            var model = NpgsqlTestHelpers.Instance.CreateConventionBuilder()
                .ForNpgsqlUseSequenceHiLo()
                .Model;

            var annotations = model.GetAnnotations().OrderBy(a => a.Name).ToList();
            Assert.Equal(4, annotations.Count);

            // Note that the annotation order is different with Npgsql than the SqlServer (N vs. S...)
            Assert.Equal(NpgsqlAnnotationNames.HiLoSequenceName, annotations.ElementAt(0).Name);
            Assert.Equal(NpgsqlModelAnnotations.DefaultHiLoSequenceName, annotations.ElementAt(0).Value);

            Assert.Equal(NpgsqlAnnotationNames.ValueGenerationStrategy, annotations.ElementAt(1).Name);
            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, annotations.ElementAt(1).Value);

            Assert.Equal(RelationalAnnotationNames.MaxIdentifierLength, annotations[2].Name);

            Assert.Equal(
                RelationalAnnotationNames.SequencePrefix +
                "." +
                NpgsqlModelAnnotations.DefaultHiLoSequenceName,
                annotations.ElementAt(3).Name);
            Assert.NotNull(annotations.ElementAt(3).Value);
        }
    }
}
