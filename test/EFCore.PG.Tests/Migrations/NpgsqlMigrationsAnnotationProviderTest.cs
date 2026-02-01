using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

public class NpgsqlMigrationsAnnotationProviderTest
{
    private readonly NpgsqlMigrationsAnnotationProvider _migrationsAnnotationProvider = new(new MigrationsAnnotationProviderDependencies());

    [Fact]
    public virtual void ForRemove_returns_relational_model_annotations()
    {
        var modelBuilder = new ModelBuilder()
            .HasAnnotation("Npgsql:PostgresExtension:Foo1", "SomeValue")
            .HasAnnotation("Npgsql:Enum:Foo2", "SomeValue")
            .HasAnnotation("Npgsql:Range:Foo3", "SomeValue")
            .HasAnnotation("Npgsql:CollationDefinition:Foo4", "SomeValue")
            .HasAnnotation("Npgsql:SomethingElse:Foo5", "SomeValue") // Should be ignored by ForRemove()
            .HasAnnotation("Npgsql:Another", "SomeValue") // Should be ignored by ForRemove()
            .HasAnnotation("PostgresExtension:Foo1", "SomeValue") // Should be ignored by ForRemove()
            .HasAnnotation("Enum:Foo2", "SomeValue") // Should be ignored by ForRemove()
            .HasAnnotation("Range:Foo3", "SomeValue") // Should be ignored by ForRemove()
            .HasAnnotation("CollationDefinition:Foo4", "SomeValue"); // Should be ignored by ForRemove()

        var model = new RelationalModel(modelBuilder.FinalizeModel());

        var annotations = _migrationsAnnotationProvider.ForRemove(model).ToList();

        Assert.Equal(4, annotations.Count);
        Assert.Contains(annotations, a => a.Name == "Npgsql:PostgresExtension:Foo1");
        Assert.Contains(annotations, a => a.Name == "Npgsql:Enum:Foo2");
        Assert.Contains(annotations, a => a.Name == "Npgsql:Range:Foo3");
        Assert.Contains(annotations, a => a.Name == "Npgsql:CollationDefinition:Foo4");
    }

    [Fact]
    public virtual void ForRemove_handles_no_annotations()
    {
        var model = new RelationalModel(new Model());
        Assert.Empty(_migrationsAnnotationProvider.ForRemove(model));
    }
}
