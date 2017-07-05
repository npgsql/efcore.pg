namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class NpgsqlScaffoldingCodeGenerator : IScaffoldingProviderCodeGenerator
    {
        public virtual string GenerateUseProvider(string connectionString, string language)
            => language == "CSharp"
                ? $".{nameof(NpgsqlDbContextOptionsExtensions.UseNpgsql)}({GenerateVerbatimStringLiteral(connectionString)})"
                : null;

        static string GenerateVerbatimStringLiteral(string value) => "@\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
