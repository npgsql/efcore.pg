using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure
{
    public abstract class NpgsqlEntityFrameworkPlugin
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public virtual void AddMappings(NpgsqlTypeMappingSource typeMappingSource) {}
        public virtual void AddMethodCallTranslators(NpgsqlCompositeMethodCallTranslator compositeMethodCallTranslator) {}
        public virtual void AddMemberTranslators(NpgsqlCompositeMemberTranslator compositeMemberTranslator) {}
    }
}
