using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal
{
    /// <summary>
    /// The default code generator for Npgsql.
    /// </summary>
    public class NpgsqlCodeGenerator : ProviderCodeGenerator
    {
        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlCodeGenerator"/> class.
        /// </summary>
        /// <param name="dependencies">The dependencies.</param>
        public NpgsqlCodeGenerator([NotNull] ProviderCodeGeneratorDependencies dependencies)
            : base(dependencies) {}

        public override MethodCallCodeFragment GenerateUseProvider(
            string connectionString,
            MethodCallCodeFragment providerOptions)
            => new MethodCallCodeFragment(
                nameof(NpgsqlDbContextOptionsBuilderExtensions.UseNpgsql),
                providerOptions == null
                    ? new object[] { connectionString }
                    : new object[] { connectionString, new NestedClosureCodeFragment("x", providerOptions) });
    }
}
