// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlStringTrimTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _trim
            = typeof(string).GetRuntimeMethod(nameof(string.Trim), new Type[0]);

        static readonly MethodInfo _trimWithChars
            = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) });

        // The following exists as an optimization in netcoreapp20
        static readonly MethodInfo _trimWithSingleChar
            = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Equals(_trim))
            {
                // Note that PostgreSQL TRIM() does spaces only, not all whitespace, so we use a regex
                return new SqlFunctionExpression(
                    "REGEXP_REPLACE",
                    typeof(string),
                    new[]
                    {
                        methodCallExpression.Object,
                        Expression.Constant(@"^\s*(.*?)\s*$"),
                        Expression.Constant(@"\1")
                    });
            }

            if (methodCallExpression.Method.Equals(_trimWithChars))
            {
                var constantTrimChars = methodCallExpression.Arguments[0] as ConstantExpression;
                if (constantTrimChars == null)
                    return null;

                return new SqlFunctionExpression(
                    "BTRIM",
                    typeof(string),
                    new[]
                    {
                        methodCallExpression.Object,
                        Expression.Constant(new string((char[])constantTrimChars.Value))
                    });
            }

            if (methodCallExpression.Method.Equals(_trimWithSingleChar))
            {
                var constantTrimChar = methodCallExpression.Arguments[0] as ConstantExpression;
                if (constantTrimChar == null)
                    return null;

                return new SqlFunctionExpression(
                    "BTRIM",
                    typeof(string),
                    new[]
                    {
                        methodCallExpression.Object,
                        Expression.Constant(new string((char)constantTrimChar.Value, 1))
                    });
            }

            return null;
        }
    }
}
