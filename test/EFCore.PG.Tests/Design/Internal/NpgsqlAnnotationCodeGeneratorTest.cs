using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal
{
    public class NpgsqlAnnotationCodeGeneratorTest
    {
        [Fact]
        public void GenerateFluentApi_value_generation()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(NpgsqlConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("IdentityByDefault").UseIdentityByDefaultColumn();
                    x.Property<int>("IdentityAlways").UseIdentityAlwaysColumn();
                    x.Property<int>("Serial").UseSerialColumn();
                });

            // Note: both serial and identity-by-default columns are considered by-convention - we don't want
            // to assume that the PostgreSQL version of the scaffolded database necessarily determines the
            // version of the database that the scaffolded model will target. This makes life difficult for
            // models with mixed strategies but that's an edge case.

            var entity = modelBuilder.Model.FindEntityType("Post");

            var property = entity.GetProperties().Single(p => p.Name == "IdentityByDefault");
            var annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
            generator.RemoveAnnotationsHandledByConventions(property, annotations);
            Assert.Empty(annotations);
            var result = generator.GenerateFluentApiCalls(property, property.GetAnnotations().ToDictionary(a => a.Name, a => a))
                .Single();
            Assert.Equal(nameof(NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn), result.Method);
            Assert.Equal(0, result.Arguments.Count);

            property = entity.GetProperties().Single(p => p.Name == "IdentityAlways");
            annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
            generator.RemoveAnnotationsHandledByConventions(property, annotations);
            Assert.Contains(annotations, kv => kv.Key == NpgsqlAnnotationNames.ValueGenerationStrategy);
            result = generator.GenerateFluentApiCalls(property, annotations).Single();
            Assert.Equal(nameof(NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn), result.Method);
            Assert.Equal(0, result.Arguments.Count);

            property = entity.GetProperties().Single(p => p.Name == "Serial");
            annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
            generator.RemoveAnnotationsHandledByConventions(property, annotations);
            Assert.Empty(annotations);
            result = generator.GenerateFluentApiCalls(property, property.GetAnnotations().ToDictionary(a => a.Name, a => a))
                .Single();
            Assert.Equal(nameof(NpgsqlPropertyBuilderExtensions.UseSerialColumn), result.Method);
            Assert.Equal(0, result.Arguments.Count);
        }

        [Fact]
        public void GenerateFluentApi_identity_sequence_options()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(NpgsqlConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id")
                        .UseIdentityByDefaultColumn()
                        .HasIdentityOptions(
                            startValue: 5,
                            incrementBy: 2,
                            minValue: 3,
                            maxValue: 2000,
                            cyclic: true,
                            numbersToCache: 10);
                });

            var property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "Id");
            var annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
            generator.RemoveAnnotationsHandledByConventions(property, annotations);
            Assert.Contains(annotations, kv => kv.Key == NpgsqlAnnotationNames.IdentityOptions);
            var result = generator.GenerateFluentApiCalls(property, annotations).Single();
            Assert.Equal(nameof(NpgsqlPropertyBuilderExtensions.HasIdentityOptions), result.Method);
            Assert.Equal(5L, result.Arguments[0]);
            Assert.Equal(2L, result.Arguments[1]);
            Assert.Equal(3L, result.Arguments[2]);
            Assert.Equal(2000L, result.Arguments[3]);
            Assert.Equal(true, result.Arguments[4]);
            Assert.Equal(10L, result.Arguments[5]);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IModel_works_with_HiLo()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(NpgsqlConventionSetBuilder.Build());
            modelBuilder.UseHiLo("HiLoIndexName", "HiLoIndexSchema");

            var annotations = modelBuilder.Model.GetAnnotations().ToDictionary(a => a.Name, a => a);
            var result = generator.GenerateFluentApiCalls(modelBuilder.Model, annotations).Single();

            Assert.Equal("UseHiLo", result.Method);

            Assert.Collection(
                result.Arguments,
                name => Assert.Equal("HiLoIndexName", name),
                schema => Assert.Equal("HiLoIndexSchema", schema));
        }

        [ConditionalFact]
        public void GenerateFluentApi_IProperty_works_with_HiLo()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(NpgsqlConventionSetBuilder.Build());
            modelBuilder.Entity("Post", x => x.Property<int>("Id").UseHiLo("HiLoIndexName", "HiLoIndexSchema"));
            var property = modelBuilder.Model.FindEntityType("Post").FindProperty("Id");

            var annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
            var result = generator.GenerateFluentApiCalls(property, annotations).Single();

            Assert.Equal("UseHiLo", result.Method);

            Assert.Collection(
                result.Arguments,
                name => Assert.Equal("HiLoIndexName", name),
                schema => Assert.Equal("HiLoIndexSchema", schema));
        }

        NpgsqlAnnotationCodeGenerator CreateGenerator()
            => new NpgsqlAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies(
                new NpgsqlTypeMappingSource(
                    new TypeMappingSourceDependencies(
                        new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                        Array.Empty<ITypeMappingSourcePlugin>()
                    ),
                    new RelationalTypeMappingSourceDependencies(Array.Empty<IRelationalTypeMappingSourcePlugin>()),
                    new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()))));
    }
}
