using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure
{
    public interface IEntityFrameworkNpgsqlPlugin
    {
        string Name { get; }
        string Description { get; }

        void AddMappings(NpgsqlTypeMappingSource typeMappingSource);
        void AddMethodCallTranslators(NpgsqlCompositeMethodCallTranslator compositeMethodCallTranslator);
        void AddMemberTranslators(NpgsqlCompositeMemberTranslator compositeMemberTranslator);
    }
}
