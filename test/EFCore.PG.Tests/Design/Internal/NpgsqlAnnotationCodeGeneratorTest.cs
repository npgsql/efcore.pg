using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal
{
    public class NpgsqlAnnotationCodeGeneratorTest
    {
        [Fact]
        public void GenerateFluentApi_identity()
        {
            var generator = new NpgsqlAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
            var modelBuilder = new ModelBuilder(NpgsqlConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id").UseNpgsqlIdentityAlwaysColumn();
                });
            var property = modelBuilder.Model.FindEntityType("Post").GetProperties().Single();
            var annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);

            var result = generator.GenerateFluentApi(property, annotation);

            Assert.Equal("UseNpgsqlIdentityAlwaysColumn", result.Method);

            Assert.Equal(0, result.Arguments.Count);
        }
    }
}
