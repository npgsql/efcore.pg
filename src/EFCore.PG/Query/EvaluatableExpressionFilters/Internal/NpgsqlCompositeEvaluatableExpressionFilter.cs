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
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.EvaluatableExpressionFilters.Internal
{
    /// <summary>
    /// A composite evaluatable expression filter that dispatches to multiple specialized filters specific to Npgsql.
    /// </summary>
    public class NpgsqlCompositeEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
    {
        /// <summary>
        /// The collection of registered evaluatable expression filters.
        /// </summary>
        [NotNull] [ItemNotNull] readonly List<IEvaluatableExpressionFilter> _filters =
            new List<IEvaluatableExpressionFilter>
            {
                new NpgsqlFullTextSearchEvaluatableExpressionFilter()
            };

        /// <inheritdoc />
        public NpgsqlCompositeEvaluatableExpressionFilter([NotNull] IModel model, [NotNull] INpgsqlOptions npgsqlOptions) : base(model)
        {
            var versionDependentFilters =
                new IEvaluatableExpressionFilter[]
                    {};

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddFilters(versionDependentFilters);

            foreach (var plugin in npgsqlOptions.Plugins)
                plugin.AddEvaluatableExpressionFilters(this);
        }

        /// <inheritdoc />
        public override bool IsEvaluatableMember(MemberExpression expression)
            => _filters.All(x => x.IsEvaluatableMember(expression)) && base.IsEvaluatableMember(expression);

        /// <inheritdoc />
        public override bool IsEvaluatableMethodCall(MethodCallExpression expression)
            => _filters.All(x => x.IsEvaluatableMethodCall(expression)) && base.IsEvaluatableMethodCall(expression);

        /// <summary>
        /// Adds additional dispatches to the filters list.
        /// </summary>
        /// <param name="filters">The filters.</param>
        public virtual void AddFilters([NotNull] [ItemNotNull] IEnumerable<IEvaluatableExpressionFilter> filters)
            => _filters.InsertRange(0, filters);
    }
}
