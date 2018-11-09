using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.Scaffolding.Internal
{
    public class NpgsqlNodaTimeCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
    {
        public override MethodCallCodeFragment GenerateProviderOptions()
            => new MethodCallCodeFragment(nameof(NpgsqlNodaTimeDbContextOptionsBuilderExtensions.UseNodaTime));
    }
}
