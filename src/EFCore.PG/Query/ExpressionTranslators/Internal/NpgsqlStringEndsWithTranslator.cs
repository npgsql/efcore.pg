﻿#region License

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
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates <see cref="M:string.EndsWith(string)"/> to 'RIGHT(text, int) = text'.
    /// </summary>
    public class NpgsqlStringEndsWithTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo MethodInfo =
            typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression e)
        {
            var instance = e.Object;

            if (instance == null || !ReferenceEquals(e.Method, MethodInfo))
                return null;

            return
                Expression.Equal(
                    new SqlFunctionExpression(
                        "RIGHT",
                        instance.Type,
                        new[]
                        {
                            instance,
                            new SqlFunctionExpression("LENGTH", typeof(int), new[] { e.Arguments[0] })
                        }),
                    e.Arguments[0]);
        }
    }
}
