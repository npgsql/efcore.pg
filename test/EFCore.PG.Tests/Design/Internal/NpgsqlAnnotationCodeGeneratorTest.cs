using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
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
