// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates <see cref="M:string.Trim()"/>.
    /// </summary>
    public class NpgsqlStringTrimTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo Trim =
            typeof(string).GetRuntimeMethod(nameof(string.Trim), Type.EmptyTypes);

        static readonly MethodInfo TrimWithChars =
            typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) });

        // The following exists as an optimization in netcoreapp20
        static readonly MethodInfo TrimWithSingleChar =
            typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char) });

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression e)
        {
            if (e.Method.Equals(Trim))
            {
                // Note that PostgreSQL TRIM() does spaces only, not all whitespace, so we use a regex
                return new SqlFunctionExpression(
                    "REGEXP_REPLACE",
                    typeof(string),
                    new[]
                    {
                        e.Object,
                        Expression.Constant(@"^\s*(.*?)\s*$"),
                        Expression.Constant(@"\1")
                    });
            }

            if (e.Method.Equals(TrimWithChars))
            {
                if (!(e.Arguments[0] is ConstantExpression constantTrimChars))
                    return null;

                return new SqlFunctionExpression(
                    "BTRIM",
                    typeof(string),
                    new[]
                    {
                        e.Object,
                        Expression.Constant(new string((char[])constantTrimChars.Value))
                    });
            }

            if (!e.Method.Equals(TrimWithSingleChar))
                return null;

            if (!(e.Arguments[0] is ConstantExpression constantTrimChar))
                return null;

            return new SqlFunctionExpression(
                "BTRIM",
                typeof(string),
                new[]
                {
                    e.Object,
                    Expression.Constant(new string((char)constantTrimChar.Value, 1))
                });
        }
    }
}
