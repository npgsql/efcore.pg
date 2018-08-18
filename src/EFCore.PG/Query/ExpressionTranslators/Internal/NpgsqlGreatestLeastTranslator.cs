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

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for GREATEST(...) and LEAST(...).
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/static/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
    /// </remarks>
    public class NpgsqlGreatestLeastTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// The minimum backend version supported by this translator.
        /// </summary>
        [NotNull] static readonly Version MinimumSupportedVersion = new Version(8, 1);

        /// <summary>
        /// The backend version to target.
        /// </summary>
        [NotNull] readonly Version _postgresVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlGreatestLeastTranslator"/> class.
        /// </summary>
        /// <param name="postgresVersion">The backend version to target.</param>
        public NpgsqlGreatestLeastTranslator([CanBeNull] Version postgresVersion)
            => _postgresVersion = postgresVersion ?? MinimumSupportedVersion;

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression expression)
        {
            if (_postgresVersion < MinimumSupportedVersion)
                return null;

            var method = expression.Method;

            if (!method.IsStatic || method.DeclaringType != typeof(Math))
                return null;

            switch (method.Name)
            {
            case nameof(Math.Max):
                return new SqlFunctionExpression("GREATEST", expression.Type, expression.Arguments);

            case nameof(Math.Min):
                return new SqlFunctionExpression("LEAST", expression.Type, expression.Arguments);

            default:
                return null;
            }
        }
    }
}
