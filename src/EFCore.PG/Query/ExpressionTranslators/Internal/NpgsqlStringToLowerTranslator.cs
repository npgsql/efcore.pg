using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringToLowerTranslator : ParameterlessInstanceMethodCallTranslator
    {
        public NpgsqlStringToLowerTranslator()
            : base(typeof(string), "ToLower", "LOWER")
        {
        }
    }
}
