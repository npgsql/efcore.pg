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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionVisitors;
using Remotion.Linq;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    /// <summary>
    /// The default relational LINQ query optimizer for Npgsql.
    /// </summary>
    public class NpgsqlQueryOptimizer : QueryOptimizer
    {
        /// <summary>
        /// The default expression visitors registered by the Npgsql provider.
        /// </summary>
        static readonly ExpressionVisitor[] ExpressionVisitors =
        {
            new NpgsqlExistsToAnyRewritingExpressionVisitor()
        };

        /// <inheritdoc />
        public override void Optimize(QueryCompilationContext queryCompilationContext, QueryModel queryModel)
        {
            base.Optimize(queryCompilationContext, queryModel);

            for (int i = 0; i < ExpressionVisitors.Length; i++)
            {
                queryModel.TransformExpressions(ExpressionVisitors[i].Visit);
            }
        }
    }
}
