using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringToUpperTranslator : ParameterlessInstanceMethodCallTranslator
    {
        public NpgsqlStringToUpperTranslator()
            : base(typeof(string), "ToUpper", "UPPER")
        {
        }
    }
}
