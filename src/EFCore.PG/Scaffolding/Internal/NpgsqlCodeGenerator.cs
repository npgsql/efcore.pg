using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal
{
    /// <summary>
    /// The default configuration generator for Npgsql.
    /// </summary>
    public class NpgsqlConfigurationCodeGenerator : ProviderCodeGenerator
    {
        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlConfigurationCodeGenerator"/> class.
        /// </summary>
        /// <param name="dependencies">The dependencies.</param>
        public NpgsqlConfigurationCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
            : base(dependencies) {}

        /// <inheritdoc />
        public override MethodCallCodeFragment GenerateUseProvider(string connectionString)
            => new MethodCallCodeFragment(nameof(NpgsqlDbContextOptionsExtensions.UseNpgsql), connectionString);
    }
}
