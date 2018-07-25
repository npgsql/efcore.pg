#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

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

        /// <summary>
        /// Adds additional dispatches to the translators list.
        /// </summary>
        /// <param name="translators">The translators.</param>
        public new virtual void AddTranslators([NotNull] IEnumerable<IExpressionFragmentTranslator> translators)
            => base.AddTranslators(translators);
    }
}
