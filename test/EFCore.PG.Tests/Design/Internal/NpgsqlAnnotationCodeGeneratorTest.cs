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
            Assert.Equal("UseIdentityByDefaultColumn", result.Method);
            Assert.Equal(0, result.Arguments.Count);

            property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "IdentityAlways");
            annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.False(generator.IsHandledByConvention(property, annotation));
            result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal("UseIdentityAlwaysColumn", result.Method);
            Assert.Equal(0, result.Arguments.Count);

            property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "Serial");
            annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.True(generator.IsHandledByConvention(property, annotation));
            result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal("UseSerialColumn", result.Method);
            Assert.Equal(0, result.Arguments.Count);
        }
    }
}
