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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlFullTextSearchMethodTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _tsQueryParse = typeof(NpgsqlTsQuery).GetMethod(
            nameof(NpgsqlTsQuery.Parse),
            BindingFlags.Public | BindingFlags.Static);

        static readonly MethodInfo _tsVectorParse = typeof(NpgsqlTsVector).GetMethod(
            nameof(NpgsqlTsVector.Parse),
            BindingFlags.Public | BindingFlags.Static);

        static readonly IReadOnlyDictionary<string, string> _sqlNameByMethodName =
            new Dictionary<string, string>
            {
                [nameof(NpgsqlFullTextSearchDbFunctionsExtensions.ArrayToTsVector)] = "array_to_tsvector",
                [nameof(NpgsqlFullTextSearchDbFunctionsExtensions.ToTsVector)] = "to_tsvector",
                [nameof(NpgsqlFullTextSearchDbFunctionsExtensions.PlainToTsQuery)] = "plainto_tsquery",
                [nameof(NpgsqlFullTextSearchDbFunctionsExtensions.PhraseToTsQuery)] = "phraseto_tsquery",
                [nameof(NpgsqlFullTextSearchDbFunctionsExtensions.ToTsQuery)] = "to_tsquery"
            };

        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method == _tsQueryParse || methodCallExpression.Method == _tsVectorParse)
                return new ExplicitCastExpression(
                    methodCallExpression.Arguments[0],
                    methodCallExpression.Method.ReturnType);

            if (methodCallExpression.Method.DeclaringType == typeof(NpgsqlFullTextSearchDbFunctionsExtensions)
                && _sqlNameByMethodName.TryGetValue(methodCallExpression.Method.Name, out var sqlFunctionName))
                return new SqlFunctionExpression(
                    sqlFunctionName,
                    methodCallExpression.Method.ReturnType,
                    methodCallExpression.Arguments.Skip(1));

            if (methodCallExpression.Method.DeclaringType == typeof(NpgsqlFullTextSearchLinqExtensions))
                return TryTranslateOperator(methodCallExpression) ?? TryTranslateFunction(methodCallExpression);

            return null;
        }

        static Expression TryTranslateOperator(MethodCallExpression e)
        {
            switch (e.Method.Name)
            {
            case nameof(NpgsqlFullTextSearchLinqExtensions.And):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "&&", typeof(NpgsqlTsQuery));

            case nameof(NpgsqlFullTextSearchLinqExtensions.Or):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "||", typeof(NpgsqlTsQuery));

            case nameof(NpgsqlFullTextSearchLinqExtensions.ToNegative):
                return new CustomUnaryExpression(e.Arguments[0], "!!", typeof(NpgsqlTsQuery));

            case nameof(NpgsqlFullTextSearchLinqExtensions.Contains):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "@>", typeof(bool));

            case nameof(NpgsqlFullTextSearchLinqExtensions.IsContainedIn):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "<@", typeof(bool));

            case nameof(NpgsqlFullTextSearchLinqExtensions.Matches):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "@@", typeof(bool));

            case nameof(NpgsqlFullTextSearchLinqExtensions.Concat):
                return new CustomBinaryExpression(e.Arguments[0], e.Arguments[1], "||", typeof(NpgsqlTsVector));

            default:
                return null;
            }
        }

        static Expression TryTranslateFunction(MethodCallExpression methodCallExpression)
        {
            switch (methodCallExpression.Method.Name)
            {
            case nameof(NpgsqlFullTextSearchLinqExtensions.GetNodeCount):
                return new SqlFunctionExpression(
                    "numnode",
                    methodCallExpression.Method.ReturnType,
                    methodCallExpression.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.GetQueryTree):
                return new SqlFunctionExpression(
                    "querytree",
                    methodCallExpression.Method.ReturnType,
                    methodCallExpression.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.GetResultHeadline):
                var tsheadlineFunctionName = "ts_headline";
                switch (methodCallExpression.Arguments.Count)
                {
                case 2:
                    return new SqlFunctionExpression(
                        tsheadlineFunctionName,
                        methodCallExpression.Method.ReturnType,
                        methodCallExpression.Arguments.Reverse());

                case 3:
                    return new SqlFunctionExpression(
                        tsheadlineFunctionName,
                        methodCallExpression.Method.ReturnType,
                        new[]
                        {
                            methodCallExpression.Arguments[1],
                            methodCallExpression.Arguments[0],
                            methodCallExpression.Arguments[2]
                        });

                case 4:
                    return new SqlFunctionExpression(
                        tsheadlineFunctionName,
                        methodCallExpression.Method.ReturnType,
                        new[]
                        {
                            methodCallExpression.Arguments[1],
                            methodCallExpression.Arguments[2],
                            methodCallExpression.Arguments[0],
                            methodCallExpression.Arguments[3]
                        });

                default:
                    throw new ArgumentException(
                        $"Invalid method overload for {tsheadlineFunctionName}",
                        nameof(methodCallExpression));
                }

            case nameof(NpgsqlFullTextSearchLinqExtensions.Rewrite):
                return new SqlFunctionExpression(
                    "ts_rewrite",
                    methodCallExpression.Method.ReturnType,
                    methodCallExpression.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.ToPhrase):
                return new SqlFunctionExpression(
                    "tsquery_phrase",
                    methodCallExpression.Method.ReturnType,
                    methodCallExpression.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.SetWeight):
                var arguments = methodCallExpression.Arguments.ToArray();

                if (arguments[1].Type == typeof(NpgsqlTsVector.Lexeme.Weight))
                {
                    if (!(arguments[1] is ConstantExpression weightExpression))
                    {
                        throw new ArgumentException(
                            "Enum 'weight' argument for 'SetWeight' must be a constant expression.");
                    }

                    arguments[1] = Expression.Constant(weightExpression.Value.ToString()[0]);
                }

                return new SqlFunctionExpression(
                    "setweight",
                    methodCallExpression.Method.ReturnType,
                    arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.Delete):
                return new SqlFunctionExpression(
                    "ts_delete",
                    methodCallExpression.Method.ReturnType,
                    methodCallExpression.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.Filter):
                return new SqlFunctionExpression(
                    "ts_filter",
                    methodCallExpression.Method.ReturnType,
                    new[]
                    {
                        methodCallExpression.Arguments[0],
                        new ExplicitStoreTypeCastExpression(methodCallExpression.Arguments[1], typeof(char[]), "\"char\"[]")
                    });

            case nameof(NpgsqlFullTextSearchLinqExtensions.GetLength):
                return new SqlFunctionExpression(
                    "length",
                    methodCallExpression.Method.ReturnType,
                    methodCallExpression.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.ToStripped):
                return new SqlFunctionExpression(
                    "strip",
                    methodCallExpression.Method.ReturnType,
                    methodCallExpression.Arguments);

            case nameof(NpgsqlFullTextSearchLinqExtensions.Rank):
            case nameof(NpgsqlFullTextSearchLinqExtensions.RankCoverDensity):
                var rankFunctionName = methodCallExpression.Method.Name == nameof(NpgsqlFullTextSearchLinqExtensions.Rank)
                    ? "ts_rank"
                    : "ts_rank_cd";

                switch (methodCallExpression.Arguments.Count)
                {
                case 2:
                    return new SqlFunctionExpression(
                        rankFunctionName,
                        methodCallExpression.Method.ReturnType,
                        methodCallExpression.Arguments);

                case 3:
                    var firstArgument = methodCallExpression.Arguments[0];
                    var secondArgument = methodCallExpression.Arguments[1];
                    if (methodCallExpression.Arguments[1].Type == typeof(float[]))
                    {
                        var temp = firstArgument;
                        firstArgument = secondArgument;
                        secondArgument = temp;
                    }

                    return new SqlFunctionExpression(
                        rankFunctionName,
                        methodCallExpression.Method.ReturnType,
                        new[]
                        {
                            firstArgument,
                            secondArgument,
                            methodCallExpression.Arguments[2]
                        });

                case 4:
                    return new SqlFunctionExpression(
                        rankFunctionName,
                        methodCallExpression.Method.ReturnType,
                        new[]
                        {
                            methodCallExpression.Arguments[1],
                            methodCallExpression.Arguments[0],
                            methodCallExpression.Arguments[2],
                            methodCallExpression.Arguments[3]
                        });

                default:
                    throw new ArgumentException(
                        $"Invalid method overload for {rankFunctionName}",
                        nameof(methodCallExpression));
                }

            default:
                return null;
            }
        }
    }
}
