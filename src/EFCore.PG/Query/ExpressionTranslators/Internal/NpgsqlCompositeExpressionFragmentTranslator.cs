#if PREVIEW7
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// A composite expression fragment translator that dispatches to multiple specialized translators specific to Npgsql.
    /// </summary>
    public class NpgsqlCompositeExpressionFragmentTranslator : RelationalCompositeExpressionFragmentTranslator
    {
        /// <summary>
        /// The default expression fragment translators registered by the Npgsql provider.
        /// </summary>
        [NotNull] [ItemNotNull] static readonly IExpressionFragmentTranslator[] ExpressionFragmentTranslators =
            {};

        /// <inheritdoc />
        public NpgsqlCompositeExpressionFragmentTranslator(
            [NotNull] RelationalCompositeExpressionFragmentTranslatorDependencies dependencies)
            : base(dependencies)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(ExpressionFragmentTranslators);
        }

        // ReSharper disable once MemberCanBeProtected.Global
        /// <summary>
        /// Adds additional dispatches to the translators list.
        /// </summary>
        /// <param name="translators">The translators.</param>
        public new virtual void AddTranslators([NotNull] IEnumerable<IExpressionFragmentTranslator> translators)
            => base.AddTranslators(translators);
    }
}
#endif
