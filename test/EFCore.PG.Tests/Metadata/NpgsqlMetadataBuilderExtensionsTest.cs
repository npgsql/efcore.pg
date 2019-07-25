using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Xunit;

#pragma warning disable EF1001

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
{
    public class NpgsqlInternalMetadataBuilderExtensionsTest
    {
        IConventionModelBuilder CreateBuilder() => new InternalModelBuilder(new Model());

        [ConditionalFact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            Assert.NotNull(builder
                .ForNpgsqlHasValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, builder.Metadata.GetNpgsqlValueGenerationStrategy());

            Assert.NotNull(builder
                    .ForNpgsqlHasValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn, fromDataAnnotation: true));
            Assert.Equal(
                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn, builder.Metadata.GetNpgsqlValueGenerationStrategy());

            Assert.Null(builder
                .ForNpgsqlHasValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn, builder.Metadata.GetNpgsqlValueGenerationStrategy());

            Assert.Equal(
                1, builder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(NpgsqlAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [ConditionalFact]
        public void Can_access_entity_type()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot));

            Assert.NotNull(typeBuilder.ForNpgsqlIsUnlogged());
            Assert.True(typeBuilder.Metadata.GetNpgsqlIsUnlogged());

            Assert.NotNull(typeBuilder.ForNpgsqlIsUnlogged(false, fromDataAnnotation: true));
            Assert.False(typeBuilder.Metadata.GetNpgsqlIsUnlogged());

            Assert.Null(typeBuilder.ForNpgsqlIsUnlogged(true));
            Assert.False(typeBuilder.Metadata.GetNpgsqlIsUnlogged());

            Assert.Equal(
                1, typeBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(NpgsqlAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [ConditionalFact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot))
                .Property(typeof(int), "Id");

            Assert.NotNull(propertyBuilder.ForNpgsqlHasHiLoSequence("Splew", null));
            Assert.Equal("Splew", propertyBuilder.Metadata.GetNpgsqlHiLoSequenceName());

            Assert.NotNull(propertyBuilder.ForNpgsqlHasHiLoSequence("Splow", null, fromDataAnnotation: true));
            Assert.Equal("Splow", propertyBuilder.Metadata.GetNpgsqlHiLoSequenceName());

            Assert.Null(propertyBuilder.ForNpgsqlHasHiLoSequence("Splod", null));
            Assert.Equal("Splow", propertyBuilder.Metadata.GetNpgsqlHiLoSequenceName());

            Assert.Equal(
                1, propertyBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(NpgsqlAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [ConditionalFact]
        public void Throws_setting_sequence_generation_for_invalid_type()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot))
                .Property(typeof(string), "Name");

            Assert.Equal(
                NpgsqlStrings.SequenceBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => propertyBuilder.ForNpgsqlHasValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo)).Message);

            Assert.Equal(
                NpgsqlStrings.SequenceBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => new PropertyBuilder((IMutableProperty)propertyBuilder.Metadata).ForNpgsqlUseSequenceHiLo()).Message);
        }

        [ConditionalFact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot));
            var idProperty = entityTypeBuilder.Property(typeof(int), "Id").Metadata;
            var indexBuilder = entityTypeBuilder.HasIndex(new[] { idProperty });

            Assert.NotNull(indexBuilder.ForNpgsqlHasMethod("gin"));
            Assert.Equal("gin", indexBuilder.Metadata.GetNpgsqlMethod());

            Assert.NotNull(indexBuilder.ForNpgsqlHasMethod("gist", fromDataAnnotation: true));
            Assert.Equal("gist", indexBuilder.Metadata.GetNpgsqlMethod());

            Assert.Null(indexBuilder.ForNpgsqlHasMethod("gin"));
            Assert.Equal("gist", indexBuilder.Metadata.GetNpgsqlMethod());

            Assert.Equal(
                1, indexBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(NpgsqlAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [ConditionalFact]
        public void Can_access_relationship()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot));
            var idProperty = entityTypeBuilder.Property(typeof(int), "Id").Metadata;
            var key = entityTypeBuilder.HasKey(new[] { idProperty }).Metadata;
            var relationshipBuilder = entityTypeBuilder.HasRelationship(entityTypeBuilder.Metadata, key);

            Assert.NotNull(relationshipBuilder.HasConstraintName("Splew"));
            Assert.Equal("Splew", relationshipBuilder.Metadata.GetConstraintName());

            Assert.NotNull(relationshipBuilder.HasConstraintName("Splow", fromDataAnnotation: true));
            Assert.Equal("Splow", relationshipBuilder.Metadata.GetConstraintName());

            Assert.Null(relationshipBuilder.HasConstraintName("Splod"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.GetConstraintName());

            Assert.Equal(
                1, relationshipBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(RelationalAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        private class Splot
        {
        }
    }
}
