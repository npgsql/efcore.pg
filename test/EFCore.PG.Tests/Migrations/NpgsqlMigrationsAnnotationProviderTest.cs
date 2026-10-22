using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

public class NpgsqlMigrationsAnnotationProviderTest
{
    private readonly NpgsqlMigrationsAnnotationProvider _migrationsAnnotationProvider = new(new MigrationsAnnotationProviderDependencies());

    [Fact]
    public virtual void ForRemove_returns_relational_model_annotations()
    {
        var modelBuilder = new ModelBuilder()
            .HasPostgresExtension("test_extension")
            .HasPostgresExtension("test_schema2", "test_extension2")
            .HasPostgresEnum("test_enum", ["A", "B", "C"])
            .HasPostgresEnum("test_schema2", "test_enum2", ["A", "B", "C"])
            .HasPostgresRange("test_range", "test_subtype")
            .HasPostgresRange("test_schema2", "test_range2", "test_subtype2")
            .HasCollation("test_collation", "test_locale")
            .HasCollation("test_schema2", "test_collation2", "test_locale2", "test_provider2", false);

        // Define a sequence, so we can verify that the annotation it creates is excluded from the ForRemove() result.
        modelBuilder.HasSequence("test_sequence");

        var model = new RelationalModel(modelBuilder.FinalizeModel());
        var allAnnotations = model.Model.GetAnnotations().ToList();

        var annotations = _migrationsAnnotationProvider.ForRemove(model).ToList();

        Assert.Equal(9, allAnnotations.Count);
        Assert.Equal(9, annotations.Count);
        Assert.Contains(annotations, a => a.Name == $"{NpgsqlAnnotationNames.PostgresExtensionPrefix}test_extension");
        Assert.Contains(annotations, a => a.Name == $"{NpgsqlAnnotationNames.PostgresExtensionPrefix}test_schema2.test_extension2");
        Assert.Contains(annotations, a => a.Name == $"{NpgsqlAnnotationNames.EnumPrefix}test_enum");
        Assert.Contains(annotations, a => a.Name == $"{NpgsqlAnnotationNames.EnumPrefix}test_schema2.test_enum2");
        Assert.Contains(annotations, a => a.Name == $"{NpgsqlAnnotationNames.RangePrefix}test_range");
        Assert.Contains(annotations, a => a.Name == $"{NpgsqlAnnotationNames.RangePrefix}test_schema2.test_range2");
        Assert.Contains(annotations, a => a.Name == $"{NpgsqlAnnotationNames.CollationDefinitionPrefix}test_collation");
        Assert.Contains(annotations, a => a.Name == $"{NpgsqlAnnotationNames.CollationDefinitionPrefix}test_schema2.test_collation2");
        Assert.Contains(annotations, a => a.Name == NpgsqlAnnotationNames.CollationSchemasResolved && a.Value is true);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("app", null, "app")]
    [InlineData("app.schema", null, "app.schema")]
    [InlineData("app", "other", "other")]
    public virtual void Collation_schemas_are_resolved_only_in_relational_metadata(
        string defaultSchema, string configuredSchema, string expectedSchema)
    {
        var model = new ModelBuilder()
            .HasCollation(configuredSchema, "case.insensitive", locale: "POSIX", provider: "libc")
            .HasDefaultSchema(defaultSchema)
            .FinalizeModel();
        var relationalModel = new RelationalModel(model);
        var annotationProvider = new NpgsqlAnnotationProvider(new RelationalAnnotationProviderDependencies());
        var annotations = annotationProvider.For(relationalModel, designTime: true).ToList();
        relationalModel.AddAnnotations(annotations);

        Assert.Equal(configuredSchema, Assert.Single(model.GetCollations()).Schema);
        Assert.Null(model.FindAnnotation(NpgsqlAnnotationNames.CollationSchemasResolved));
        Assert.Equal(expectedSchema, Assert.Single(PostgresCollation.GetCollations(relationalModel)).Schema);
        Assert.Contains(annotations, a => a.Name == NpgsqlAnnotationNames.CollationSchemasResolved && a.Value is true);
        Assert.Equal(
            annotations.Select(a => (a.Name, a.Value)),
            _migrationsAnnotationProvider.ForRemove(relationalModel).Select(a => (a.Name, a.Value)));
        Assert.Empty(annotationProvider.For(relationalModel, designTime: false));
    }

    [Fact]
    public virtual void ForRemove_handles_no_annotations()
    {
        var model = new RelationalModel(new Model());
        Assert.Empty(_migrationsAnnotationProvider.ForRemove(model));
    }
}
