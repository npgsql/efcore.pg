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

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionVisitors
{
    /// <summary>
    /// The default factory for creating instances of <see cref="NpgsqlSqlTranslatingExpressionVisitor"/> for Npgsql.
    /// </summary>
    public class NpgsqlSqlTranslatingExpressionVisitorFactory : SqlTranslatingExpressionVisitorFactory
    {
        /// <summary>
        /// The <see cref="INpgsqlOptions"/> configued for the current context.
        /// </summary>
        [NotNull] readonly INpgsqlOptions _npgsqlOptions;

        /// <summary>
        /// Creates a new instance of the <see cref="SqlTranslatingExpressionVisitorFactory" /> class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        /// <param name="npgsqlOptions">The options configured for the current context.</param>
        public NpgsqlSqlTranslatingExpressionVisitorFactory(
            [NotNull] SqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] INpgsqlOptions npgsqlOptions)
            : base(dependencies)
            => _npgsqlOptions = npgsqlOptions;

        /// <summary>
        /// Creates a new instance of the <see cref="NpgsqlSqlTranslatingExpressionVisitor"/> class.
        /// </summary>
        /// <param name="queryModelVisitor">The query model visitor.</param>
        /// <param name="targetSelectExpression">The target select expression.</param>
        /// <param name="topLevelPredicate">The top level predicate.</param>
        /// <param name="inProjection">True if we are translating a projection; otherwise, false.</param>
        /// <returns>
        /// A new instance of the <see cref="NpgsqlSqlTranslatingExpressionVisitor"/> class.
        /// </returns>
        public override SqlTranslatingExpressionVisitor Create(
            RelationalQueryModelVisitor queryModelVisitor,
            SelectExpression targetSelectExpression = null,
            Expression topLevelPredicate = null,
            bool inProjection = false)
            => new NpgsqlSqlTranslatingExpressionVisitor(
                Dependencies,
                queryModelVisitor,
                _npgsqlOptions.Compatibility,
                targetSelectExpression,
                topLevelPredicate,
                inProjection);
    }
}
