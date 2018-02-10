using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class SqlServerCodeGeneratorTest
    {
        [Fact]
        public virtual void Use_provider_method_is_generated_correctly()
        {
            var codeGenerator = new NpgsqlCodeGenerator(new ProviderCodeGeneratorDependencies());

            Assert.Equal("UseNpgsql", codeGenerator.UseProviderMethod);
        }
    }
}
