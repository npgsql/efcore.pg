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
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal
{
    /// <summary>
    /// The default factory for creating instances of <see cref="NpgsqlQuerySqlGenerator"/> for Npgsql.
    /// </summary>
    public class NpgsqlQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        /// <summary>
        /// The <see cref="INpgsqlOptions"/> configued for the current context.
        /// </summary>
        [NotNull] readonly INpgsqlOptions _npgsqlOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlQuerySqlGeneratorFactory"/> class.
        /// </summary>
        /// <param name="dependencies">The dependencies to construct instances of <see cref="NpgsqlQuerySqlGenerator"/>.</param>
        /// <param name="npgsqlOptions">The options configued for the current context.</param>
        public NpgsqlQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] INpgsqlOptions npgsqlOptions)
            : base(dependencies)
            => _npgsqlOptions = npgsqlOptions;

        /// <inheritdoc />
        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new NpgsqlQuerySqlGenerator(
                Dependencies,
                Check.NotNull(selectExpression, nameof(selectExpression)),
                _npgsqlOptions.Backend,
                _npgsqlOptions.Compatibility,
                _npgsqlOptions.ReverseNullOrderingEnabled);
    }
}
