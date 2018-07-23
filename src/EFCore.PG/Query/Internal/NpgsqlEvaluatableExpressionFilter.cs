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
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NpgsqlTypes;

// TODO: https://github.com/aspnet/EntityFrameworkCore/issues/11466 may provide a better way for implementing
// this (we're currently replacing an internal service
namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    /// <summary>
    /// Represents an Npgsql-specific filter to identify methods that are evaluatable.
    /// </summary>
    public class NpgsqlEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
    {
        /// <summary>
        /// The static method info for <see cref="NpgsqlTsQuery.Parse(string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo TsQueryParse =
            typeof(NpgsqlTsQuery).GetRuntimeMethod(nameof(NpgsqlTsQuery.Parse), new[] { typeof(string) });

        /// <summary>
        /// The static method info for <see cref="NpgsqlTsVector.Parse(string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo TsVectorParse =
            typeof(NpgsqlTsVector).GetRuntimeMethod(nameof(NpgsqlTsVector.Parse), new[] { typeof(string) });

        /// <inheritdoc />
        public NpgsqlEvaluatableExpressionFilter([NotNull] IModel model) : base(model) {}

        /// <inheritdoc />
        public override bool IsEvaluatableMethodCall(MethodCallExpression methodCallExpression)
            => methodCallExpression.Method != TsQueryParse &&
               methodCallExpression.Method != TsVectorParse &&
               methodCallExpression.Method.DeclaringType != typeof(NpgsqlFullTextSearchDbFunctionsExtensions) &&
               methodCallExpression.Method.DeclaringType != typeof(NpgsqlFullTextSearchLinqExtensions) &&
               base.IsEvaluatableMethodCall(methodCallExpression);
    }
}
