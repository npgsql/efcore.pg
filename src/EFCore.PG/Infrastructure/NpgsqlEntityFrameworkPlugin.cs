using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.EvaluatableExpressionFilters.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure
{
    /// <summary>
    /// Represents a plugin to the Npgsql provider for Entity Framework Core.
    /// </summary>
    public abstract class NpgsqlEntityFrameworkPlugin
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        [NotNull]
        public abstract string Name { get; }

        /// <summary>
        /// The description of the plugin.
        /// </summary>
        [NotNull]
        public abstract string Description { get; }

        /// <summary>
        /// Adds plugin-specific type mappings to the <see cref="NpgsqlTypeMappingSource"/>.
        /// </summary>
        /// <param name="typeMappingSource">The default type mapping source for the Npgsql provider.</param>
        public virtual void AddMappings([NotNull] NpgsqlTypeMappingSource typeMappingSource) {}

        /// <summary>
        /// Adds plugin-specific method call translators to the <see cref="NpgsqlCompositeMethodCallTranslator"/>.
        /// </summary>
        /// <param name="compositeMethodCallTranslator"></param>
        public virtual void AddMethodCallTranslators([NotNull] NpgsqlCompositeMethodCallTranslator compositeMethodCallTranslator) {}

        /// <summary>
        /// Adds plugin-specific member translators to the <see cref="NpgsqlCompositeMemberTranslator"/>.
        /// </summary>
        /// <param name="compositeMemberTranslator"></param>
        public virtual void AddMemberTranslators([NotNull] NpgsqlCompositeMemberTranslator compositeMemberTranslator) {}

        /// <summary>
        /// Adds plugin-specific evaluatable expression filters to the <see cref="NpgsqlCompositeEvaluatableExpressionFilter"/>.
        /// </summary>
        /// <param name="compositeEvaluatableExpressionFilter"></param>
        public virtual void AddEvaluatableExpressionFilters([NotNull] NpgsqlCompositeEvaluatableExpressionFilter compositeEvaluatableExpressionFilter) {}
    }
}
