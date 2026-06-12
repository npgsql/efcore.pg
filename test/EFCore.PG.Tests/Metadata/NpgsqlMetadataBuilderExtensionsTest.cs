using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

public class NpgsqlInternalMetadataBuilderExtensionsTest
{
    private IConventionModelBuilder CreateBuilder()
        => new InternalModelBuilder(new Model());

    [ConditionalFact]
    public void Can_access_model()
    {
        var builder = CreateBuilder();

        Assert.NotNull(
            builder
                .HasValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo));
        Assert.Equal(NpgsqlValueGenerationStrategy.SequenceHiLo, builder.Metadata.GetValueGenerationStrategy());

        Assert.NotNull(
            builder
                .HasValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn, fromDataAnnotation: true));
        Assert.Equal(
            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn, builder.Metadata.GetValueGenerationStrategy());

        Assert.Null(
            builder
                .HasValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo));
        Assert.Equal(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn, builder.Metadata.GetValueGenerationStrategy());

        Assert.Equal(
            1, builder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(NpgsqlAnnotationNames.Prefix, StringComparison.Ordinal)));
    }

    [ConditionalFact]
    public void Can_access_entity_type()
    {
        var typeBuilder = CreateBuilder().Entity(typeof(Splot));

        Assert.NotNull(typeBuilder.IsUnlogged());
        Assert.True(typeBuilder.Metadata.GetIsUnlogged());

        Assert.NotNull(typeBuilder.IsUnlogged(false, fromDataAnnotation: true));
        Assert.False(typeBuilder.Metadata.GetIsUnlogged());

        Assert.Null(typeBuilder.IsUnlogged());
        Assert.False(typeBuilder.Metadata.GetIsUnlogged());

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

        Assert.NotNull(propertyBuilder.HasHiLoSequence("Splew", null));
        Assert.Equal("Splew", propertyBuilder.Metadata.GetHiLoSequenceName());

        Assert.NotNull(propertyBuilder.HasHiLoSequence("Splow", null, fromDataAnnotation: true));
        Assert.Equal("Splow", propertyBuilder.Metadata.GetHiLoSequenceName());

        Assert.Null(propertyBuilder.HasHiLoSequence("Splod", null));
        Assert.Equal("Splow", propertyBuilder.Metadata.GetHiLoSequenceName());

        Assert.Equal(
            1, propertyBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(NpgsqlAnnotationNames.Prefix, StringComparison.Ordinal)));
    }

    [ConditionalFact]
    public void Can_access_index()
    {
        var modelBuilder = CreateBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(Splot));
        var idProperty = entityTypeBuilder.Property(typeof(int), "Id").Metadata;
        var indexBuilder = entityTypeBuilder.HasIndex([idProperty]);

        Assert.NotNull(indexBuilder.HasMethod("gin"));
        Assert.Equal("gin", indexBuilder.Metadata.GetMethod());

        Assert.NotNull(indexBuilder.HasMethod("gist", fromDataAnnotation: true));
        Assert.Equal("gist", indexBuilder.Metadata.GetMethod());

        Assert.Null(indexBuilder.HasMethod("gin"));
        Assert.Equal("gist", indexBuilder.Metadata.GetMethod());

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
        var key = entityTypeBuilder.HasKey([idProperty]).Metadata;
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

    private class Splot;
}
