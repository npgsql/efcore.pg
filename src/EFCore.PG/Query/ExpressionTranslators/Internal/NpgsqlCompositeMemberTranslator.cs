using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// A composite member translator that dispatches to multiple specialized member translators specific to Npgsql.
    /// </summary>
    public class NpgsqlCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        /// <summary>
        /// The default member translators registered by the Npgsql provider.
        /// </summary>
        [NotNull] [ItemNotNull] static readonly IMemberTranslator[] MemberTranslators =
        {
            new NpgsqlStringLengthTranslator(),
            new NpgsqlDateTimeMemberTranslator(),
            new NpgsqlRangeTranslator()
        };

        /// <inheritdoc />
        public NpgsqlCompositeMemberTranslator(
            [NotNull] RelationalCompositeMemberTranslatorDependencies dependencies,
            [NotNull] INpgsqlOptions npgsqlOptions)
            : base(dependencies)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            AddTranslators(MemberTranslators);
        }

        /// <summary>
        /// Adds additional dispatches to the translators list.
        /// </summary>
        /// <param name="translators">The translators.</param>
        public new virtual void AddTranslators([NotNull] IEnumerable<IMemberTranslator> translators)
            => base.AddTranslators(translators);
    }
}
