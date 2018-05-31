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
        static readonly IExpressionFragmentTranslator[] ExpressionFragmentTranslators =
        {
            new NpgsqlArrayFragmentTranslator()
        };

        /// <inheritdoc />
        public NpgsqlCompositeExpressionFragmentTranslator(
            [NotNull] RelationalCompositeExpressionFragmentTranslatorDependencies dependencies)
            : base(dependencies)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(ExpressionFragmentTranslators);
        }

        /// <summary>
        /// Adds additional dispatches to the translators list.
        /// </summary>
        /// <param name="translators">The translators.</param>
        public new virtual void AddTranslators([NotNull] IEnumerable<IExpressionFragmentTranslator> translators)
            => base.AddTranslators(translators);
    }
}
