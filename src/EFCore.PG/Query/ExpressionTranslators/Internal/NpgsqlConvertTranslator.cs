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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates methods defined on <see cref="T:System.Convert"/> into PostgreSQL CAST expressions.
    /// </summary>
    public class NpgsqlConvertTranslator : IMethodCallTranslator
    {
        static readonly Dictionary<string, DbType> TypeMapping = new Dictionary<string, DbType>
        {
            [nameof(Convert.ToByte)] = DbType.Byte,
            [nameof(Convert.ToDecimal)] = DbType.Decimal,
            [nameof(Convert.ToDouble)] = DbType.Double,
            [nameof(Convert.ToInt16)] = DbType.Int16,
            [nameof(Convert.ToInt32)] = DbType.Int32,
            [nameof(Convert.ToInt64)] = DbType.Int64,
            [nameof(Convert.ToString)] = DbType.String
        };

        static readonly List<Type> SupportedTypes = new List<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(string)
        };

        static readonly IEnumerable<MethodInfo> SupportedMethods =
            TypeMapping.Keys
                .SelectMany(t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                    .Where(m => m.GetParameters().Length == 1
                                && SupportedTypes.Contains(m.GetParameters().First().ParameterType)));

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => SupportedMethods.Contains(methodCallExpression.Method)
                ? new ExplicitCastExpression(methodCallExpression.Arguments[0], methodCallExpression.Type)
                : null;
    }
}
