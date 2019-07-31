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
            Assert.Equal(3, annotations.Count);

            // TODO for PG9.6 testing: make this conditional
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

            Assert.Contains(annotations, a => a.Name == RelationalAnnotationNames.MaxIdentifierLength);
            Assert.Contains(annotations, a => a.Name == RelationalAnnotationNames.SequencePrefix + "." + NpgsqlModelExtensions.DefaultHiLoSequenceName);
            Assert.Contains(annotations, a =>
                a.Name == NpgsqlAnnotationNames.HiLoSequenceName &&
                a.Value.Equals(NpgsqlModelExtensions.DefaultHiLoSequenceName));

            Assert.Contains(annotations, a =>
                a.Name == NpgsqlAnnotationNames.ValueGenerationStrategy &&
                a.Value.Equals(NpgsqlValueGenerationStrategy.SequenceHiLo));
        }
    }
}
