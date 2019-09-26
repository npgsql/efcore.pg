using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal
{
    public class NpgsqlAnnotationCodeGeneratorTest
    {
        [Fact]
        public void GenerateFluentApi_value_generation()
        {
            var generator = new NpgsqlAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
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

            var property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "IdentityByDefault");
            var annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.True(generator.IsHandledByConvention(property, annotation));
            var result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal(nameof(NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn), result.Method);
            Assert.Equal(0, result.Arguments.Count);

            property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "IdentityAlways");
            annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.False(generator.IsHandledByConvention(property, annotation));
            result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal(nameof(NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn), result.Method);
            Assert.Equal(0, result.Arguments.Count);

            property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "Serial");
            annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.True(generator.IsHandledByConvention(property, annotation));
            result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal(nameof(NpgsqlPropertyBuilderExtensions.UseSerialColumn), result.Method);
            Assert.Equal(0, result.Arguments.Count);
        }

        [Fact]
        public void GenerateFluentApi_identity_sequence_options()
        {
            var generator = new NpgsqlAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
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
                            isCyclic: true,
                            numbersToCache: 10);
                });

            var property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "Id");
            var annotation = property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions);
            Assert.False(generator.IsHandledByConvention(property, annotation));
            var result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal(nameof(NpgsqlPropertyBuilderExtensions.HasIdentityOptions), result.Method);
            Assert.Equal(5L, result.Arguments[0]);
            Assert.Equal(2L, result.Arguments[1]);
            Assert.Equal(3L, result.Arguments[2]);
            Assert.Equal(2000L, result.Arguments[3]);
            Assert.Equal(true, result.Arguments[4]);
            Assert.Equal(10L, result.Arguments[5]);
        }
    }
}
