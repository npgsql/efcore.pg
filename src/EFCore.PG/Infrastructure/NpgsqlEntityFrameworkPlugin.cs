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
